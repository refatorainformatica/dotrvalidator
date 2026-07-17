namespace DotRValidator.Internal;

/// <summary>
/// Validation rule that evaluates each element of a collection property.
/// </summary>
/// <typeparam name="T">Type of the instance being validated.</typeparam>
/// <typeparam name="TElement">Type of each collection element.</typeparam>
/// <remarks>
/// Property names use index notation (for example, <c>Items[0]</c>). Null collections produce no failures.
/// </remarks>
internal sealed class CollectionPropertyRule<T, TElement> : IValidationRule<T>
{
    /// <summary>
    /// Initializes a collection property rule with name and accessor.
    /// </summary>
    /// <param name="propertyName">Resolved collection property path.</param>
    /// <param name="accessor">Delegate that reads the collection.</param>
    /// <remarks>
    /// <see cref="CascadeMode"/> defaults to <see cref="CascadeMode.Continue"/>.
    /// </remarks>
    internal CollectionPropertyRule(string propertyName, Func<T, IEnumerable<TElement>?> accessor)
    {
        PropertyName = propertyName;
        Accessor = accessor;
    }

    /// <summary>
    /// Resolved collection property path.
    /// </summary>
    internal string PropertyName { get; }

    /// <summary>
    /// Compiled accessor for the collection property.
    /// </summary>
    internal Func<T, IEnumerable<TElement>?> Accessor { get; }

    /// <summary>
    /// Ordered validation components applied to each element.
    /// </summary>
    internal List<RuleComponent<T, TElement>> Components { get; } = [];

    /// <summary>
    /// Cascade behavior when a component fails for an element.
    /// </summary>
    internal CascadeMode CascadeMode { get; set; } = CascadeMode.Continue;

    /// <summary>
    /// Optional nested validator run per non-null element.
    /// </summary>
    internal IValidator<TElement>? ChildValidator { get; set; }

    /// <inheritdoc />
    public bool AppliesToMember(string memberName)
    {
        return PropertyName.Equals(memberName, StringComparison.Ordinal)
            || PropertyName.StartsWith(memberName + "[", StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public IEnumerable<ValidationFailure> Validate(ValidationContext<T> context)
    {
        if (context.MemberNames is { Length: > 0 } members && !members.Any(m => AppliesToMember(m)))
        {
            yield break;
        }

        var instance = context.InstanceToValidate;
        var collection = Accessor(instance);

        if (collection is null)
            yield break;

        var index = 0;
        foreach (var element in collection)
        {
            var indexedPropertyName = $"{PropertyName}[{index}]";

            foreach (var component in Components)
            {
                if (!component.ShouldExecute(instance))
                    continue;

                if (component.SyncValidator is null)
                    continue;

                if (!component.SyncValidator(instance, element, context))
                {
                    yield return new ValidationFailure(
                        indexedPropertyName,
                        component.GetMessage(indexedPropertyName, element)
                    )
                    {
                        AttemptedValue = element,
                        ErrorCode = component.ErrorCode,
                        Severity = component.Severity,
                        Instance = instance,
                    };

                    if (CascadeMode == CascadeMode.Stop)
                        break;
                }
            }

            if (ChildValidator is not null && element is not null)
            {
                var childResult = ChildValidator.Validate(element);
                foreach (var failure in childResult.Errors)
                {
                    yield return new ValidationFailure(
                        string.IsNullOrEmpty(failure.PropertyName)
                            ? indexedPropertyName
                            : $"{indexedPropertyName}.{failure.PropertyName}",
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

            index++;
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
        var collection = Accessor(instance);
        var failures = new List<ValidationFailure>();

        if (collection is null)
            return failures;

        var index = 0;
        foreach (var element in collection)
        {
            cancellation.ThrowIfCancellationRequested();
            var indexedPropertyName = $"{PropertyName}[{index}]";

            foreach (var component in Components)
            {
                if (!component.ShouldExecute(instance))
                    continue;

                var isValid = component.AsyncValidator is not null
                    ? await component.AsyncValidator(instance, element, context, cancellation)
                    : component.SyncValidator?.Invoke(instance, element, context) ?? true;

                if (!isValid)
                {
                    failures.Add(
                        new ValidationFailure(
                            indexedPropertyName,
                            component.GetMessage(indexedPropertyName, element)
                        )
                        {
                            AttemptedValue = element,
                            ErrorCode = component.ErrorCode,
                            Severity = component.Severity,
                            Instance = instance,
                        }
                    );

                    if (CascadeMode == CascadeMode.Stop)
                        break;
                }
            }

            if (ChildValidator is not null && element is not null)
            {
                var childResult = await ChildValidator.ValidateAsync(element, cancellation);
                foreach (var failure in childResult.Errors)
                {
                    failures.Add(
                        new ValidationFailure(
                            string.IsNullOrEmpty(failure.PropertyName)
                                ? indexedPropertyName
                                : $"{indexedPropertyName}.{failure.PropertyName}",
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

            index++;
        }

        return failures;
    }
}
