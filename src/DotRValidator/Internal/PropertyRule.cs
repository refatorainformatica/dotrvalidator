namespace DotRValidator.Internal;

/// <summary>
/// Validation rule that evaluates a single property on the root instance.
/// </summary>
/// <typeparam name="T">Type of the instance being validated.</typeparam>
/// <typeparam name="TProperty">Type of the property being validated.</typeparam>
/// <remarks>
/// Holds chained <see cref="RuleComponent{T, TProperty}"/> instances and an optional child validator.
/// </remarks>
internal sealed class PropertyRule<T, TProperty> : IValidationRule<T>
{
    /// <summary>
    /// Initializes a property rule with name and accessor.
    /// </summary>
    /// <param name="propertyName">Resolved dot-separated property path.</param>
    /// <param name="accessor">Delegate that reads the property value.</param>
    /// <remarks>
    /// <see cref="CascadeMode"/> defaults to <see cref="CascadeMode.Continue"/>.
    /// </remarks>
    internal PropertyRule(string propertyName, Func<T, TProperty> accessor)
    {
        PropertyName = propertyName;
        Accessor = accessor;
    }

    /// <summary>
    /// Resolved property path used in failure messages.
    /// </summary>
    internal string PropertyName { get; }

    /// <summary>
    /// Compiled accessor for the target property.
    /// </summary>
    internal Func<T, TProperty> Accessor { get; }

    /// <summary>
    /// Ordered list of validation components for this property.
    /// </summary>
    internal List<RuleComponent<T, TProperty>> Components { get; } = [];

    /// <summary>
    /// Cascade behavior when a component fails.
    /// </summary>
    internal CascadeMode CascadeMode { get; set; } = CascadeMode.Continue;

    /// <summary>
    /// Optional nested validator for complex property values.
    /// </summary>
    internal IValidator<TProperty>? ChildValidator { get; set; }

    /// <inheritdoc />
    public bool AppliesToMember(string memberName)
    {
        return PropertyName.Equals(memberName, StringComparison.Ordinal)
            || PropertyName.StartsWith(memberName + ".", StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public IEnumerable<ValidationFailure> Validate(ValidationContext<T> context)
    {
        if (context.MemberNames is { Length: > 0 } members && !members.Any(m => AppliesToMember(m)))
        {
            yield break;
        }

        var instance = context.InstanceToValidate;
        var value = Accessor(instance);
        var failures = new List<ValidationFailure>();

        foreach (var component in Components)
        {
            if (!component.ShouldExecute(instance))
                continue;

            if (component.SyncValidator is null)
                continue;

            if (!component.SyncValidator(instance, value, context))
            {
                failures.Add(CreateFailure(component, value, instance));

                if (CascadeMode == CascadeMode.Stop)
                    break;
            }
        }

        foreach (var failure in failures)
            yield return failure;

        if (ChildValidator is not null && value is not null)
        {
            var childResult = ChildValidator.Validate(value);
            foreach (var failure in childResult.Errors)
            {
                yield return new ValidationFailure(
                    string.IsNullOrEmpty(failure.PropertyName)
                        ? PropertyName
                        : $"{PropertyName}.{failure.PropertyName}",
                    failure.ErrorMessage
                )
                {
                    AttemptedValue = failure.AttemptedValue,
                    ErrorCode = failure.ErrorCode,
                    Severity = failure.Severity,
                    Instance = failure.Instance ?? instance,
                };
            }
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ValidationFailure>> ValidateAsync(
        ValidationContext<T> context,
        CancellationToken cancellation
    )
    {
        if (context.MemberNames is { Length: > 0 } members && !members.Any(m => AppliesToMember(m)))
        {
            return [];
        }

        var instance = context.InstanceToValidate;
        var value = Accessor(instance);
        var failures = new List<ValidationFailure>();

        foreach (var component in Components)
        {
            cancellation.ThrowIfCancellationRequested();

            if (!component.ShouldExecute(instance))
                continue;

            var isValid = component.AsyncValidator is not null
                ? await component.AsyncValidator(instance, value, context, cancellation)
                : component.SyncValidator?.Invoke(instance, value, context) ?? true;

            if (!isValid)
            {
                failures.Add(CreateFailure(component, value, instance));

                if (CascadeMode == CascadeMode.Stop)
                    break;
            }
        }

        if (ChildValidator is not null && value is not null)
        {
            var childResult = await ChildValidator.ValidateAsync(value, cancellation);
            foreach (var failure in childResult.Errors)
            {
                failures.Add(
                    new ValidationFailure(
                        string.IsNullOrEmpty(failure.PropertyName)
                            ? PropertyName
                            : $"{PropertyName}.{failure.PropertyName}",
                        failure.ErrorMessage
                    )
                    {
                        AttemptedValue = failure.AttemptedValue,
                        ErrorCode = failure.ErrorCode,
                        Severity = failure.Severity,
                        Instance = failure.Instance ?? instance,
                    }
                );
            }
        }

        return failures;
    }

    /// <summary>
    /// Creates a failure for a failed component.
    /// </summary>
    /// <param name="component">Component that failed.</param>
    /// <param name="value">Property value that was validated.</param>
    /// <param name="instance">Root instance being validated.</param>
    /// <returns>Configured validation failure.</returns>
    /// <remarks>
    /// Copies error code, severity, and attempted value from the component.
    /// </remarks>
    private ValidationFailure CreateFailure(
        RuleComponent<T, TProperty> component,
        TProperty value,
        T instance
    )
    {
        return new ValidationFailure(PropertyName, component.GetMessage(PropertyName, value))
        {
            AttemptedValue = value,
            ErrorCode = component.ErrorCode,
            Severity = component.Severity,
            Instance = instance,
        };
    }
}
