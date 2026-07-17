using DotRValidator.Internal;

namespace DotRValidator;

/// <summary>
/// Fluent builder for configuring a single property rule.
/// </summary>
/// <typeparam name="T">Type of the instance being validated.</typeparam>
/// <typeparam name="TProperty">Type of the property being validated.</typeparam>
/// <remarks>
/// Internal implementation backing <see cref="AbstractValidator{T}.RuleFor{TProperty}"/>.
/// Mutates the underlying <see cref="PropertyRule{T, TProperty}"/>.
/// </remarks>
internal sealed class RuleBuilder<T, TProperty>
    : IRuleBuilder<T, TProperty>,
        IRuleBuilderOptions<T, TProperty>,
        IRuleBuilderInitial<T, TProperty>
{
    private readonly PropertyRule<T, TProperty> _rule;

    /// <summary>
    /// Initializes the builder for the given property rule.
    /// </summary>
    /// <param name="rule">Property rule to configure.</param>
    /// <remarks>
    /// Called when a rule is registered on the validator.
    /// </remarks>
    internal RuleBuilder(PropertyRule<T, TProperty> rule)
    {
        _rule = rule;
    }

    /// <summary>
    /// Sets the cascade mode for subsequent rules.
    /// </summary>
    /// <param name="cascadeMode">Desired cascade mode.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Affects components added after this call on the same property rule.
    /// </remarks>
    public IRuleBuilderOptions<T, TProperty> Cascade(CascadeMode cascadeMode)
    {
        _rule.CascadeMode = cascadeMode;
        return this;
    }

    /// <summary>
    /// Sets a custom error message for the previous rule.
    /// </summary>
    /// <param name="errorMessage">Error message. Supports {PropertyName} and {PropertyValue} placeholders.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Overrides the default message from built-in validators.
    /// </remarks>
    public IRuleBuilderOptions<T, TProperty> WithMessage(string errorMessage)
    {
        var last = _rule.Components.LastOrDefault();
        if (last is not null)
            last.CustomMessage = errorMessage;
        return this;
    }

    /// <summary>
    /// Sets a custom error code for the previous rule.
    /// </summary>
    /// <param name="errorCode">Identifying error code.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Stored on <see cref="ValidationFailure.ErrorCode"/> when the rule fails.
    /// </remarks>
    public IRuleBuilderOptions<T, TProperty> WithErrorCode(string errorCode)
    {
        var last = _rule.Components.LastOrDefault();
        if (last is not null)
            last.ErrorCode = errorCode;
        return this;
    }

    /// <summary>
    /// Sets the severity of the failure for the previous rule.
    /// </summary>
    /// <param name="severity">Failure severity.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Defaults to <see cref="Severity.Error"/> when not specified.
    /// </remarks>
    public IRuleBuilderOptions<T, TProperty> WithSeverity(Severity severity)
    {
        var last = _rule.Components.LastOrDefault();
        if (last is not null)
            last.Severity = severity;
        return this;
    }

    /// <summary>
    /// Executes the previous rule only when the condition is true.
    /// </summary>
    /// <param name="predicate">Condition for rule execution.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Evaluated against the root instance, not the property value.
    /// </remarks>
    public IRuleBuilderOptions<T, TProperty> When(Func<T, bool> predicate)
    {
        var last = _rule.Components.LastOrDefault();
        if (last is not null)
            last.WhenCondition = predicate;
        return this;
    }

    /// <summary>
    /// Executes the previous rule only when the condition is false.
    /// </summary>
    /// <param name="predicate">Condition that prevents rule execution.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Equivalent to skipping the rule when the predicate returns true.
    /// </remarks>
    public IRuleBuilderOptions<T, TProperty> Unless(Func<T, bool> predicate)
    {
        var last = _rule.Components.LastOrDefault();
        if (last is not null)
            last.UnlessCondition = predicate;
        return this;
    }

    /// <summary>
    /// Applies a child validator to the property.
    /// </summary>
    /// <typeparam name="TValidator">Type of the child validator.</typeparam>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Instantiates <typeparamref name="TValidator"/> with <c>new()</c>. Skipped when the property value is null.
    /// </remarks>
    public IRuleBuilderOptions<T, TProperty> SetValidator<TValidator>()
        where TValidator : IValidator<TProperty>, new()
    {
        return SetValidator(new TValidator());
    }

    /// <summary>
    /// Applies a child validator to the property.
    /// </summary>
    /// <param name="validator">Child validator instance.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Nested failures are prefixed with the parent property name. Skipped when the property value is null.
    /// </remarks>
    public IRuleBuilderOptions<T, TProperty> SetValidator(IValidator<TProperty> validator)
    {
        _rule.ChildValidator = validator;
        return this;
    }

    /// <summary>
    /// Appends a validation component to the underlying property rule.
    /// </summary>
    /// <param name="component">Component created by built-in extensions.</param>
    /// <remarks>
    /// Used internally by <see cref="DefaultValidatorExtensions"/>.
    /// </remarks>
    internal void AddComponent(RuleComponent<T, TProperty> component)
    {
        _rule.Components.Add(component);
    }
}

/// <summary>
/// Fluent builder for configuring rules on collection elements.
/// </summary>
/// <typeparam name="T">Type of the instance being validated.</typeparam>
/// <typeparam name="TElement">Type of each collection element.</typeparam>
/// <remarks>
/// Internal implementation backing <see cref="AbstractValidator{T}.RuleForEach{TElement}"/>.
/// </remarks>
internal sealed class CollectionRuleBuilder<T, TElement>
    : IRuleBuilder<T, TElement>,
        IRuleBuilderOptions<T, TElement>,
        IRuleBuilderInitial<T, TElement>
{
    private readonly CollectionPropertyRule<T, TElement> _rule;

    /// <summary>
    /// Initializes the builder for the given collection rule.
    /// </summary>
    /// <param name="rule">Collection property rule to configure.</param>
    /// <remarks>
    /// Called when a collection rule is registered on the validator.
    /// </remarks>
    internal CollectionRuleBuilder(CollectionPropertyRule<T, TElement> rule)
    {
        _rule = rule;
    }

    /// <summary>
    /// Sets the cascade mode for subsequent rules.
    /// </summary>
    /// <param name="cascadeMode">Desired cascade mode.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Affects components added after this call on the same property rule.
    /// </remarks>
    public IRuleBuilderOptions<T, TElement> Cascade(CascadeMode cascadeMode)
    {
        _rule.CascadeMode = cascadeMode;
        return this;
    }

    /// <summary>
    /// Sets a custom error message for the previous rule.
    /// </summary>
    /// <param name="errorMessage">Error message. Supports {PropertyName} and {PropertyValue} placeholders.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Overrides the default message from built-in validators.
    /// </remarks>
    public IRuleBuilderOptions<T, TElement> WithMessage(string errorMessage)
    {
        var last = _rule.Components.LastOrDefault();
        if (last is not null)
            last.CustomMessage = errorMessage;
        return this;
    }

    /// <summary>
    /// Sets a custom error code for the previous rule.
    /// </summary>
    /// <param name="errorCode">Identifying error code.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Stored on <see cref="ValidationFailure.ErrorCode"/> when the rule fails.
    /// </remarks>
    public IRuleBuilderOptions<T, TElement> WithErrorCode(string errorCode)
    {
        var last = _rule.Components.LastOrDefault();
        if (last is not null)
            last.ErrorCode = errorCode;
        return this;
    }

    /// <summary>
    /// Sets the severity of the failure for the previous rule.
    /// </summary>
    /// <param name="severity">Failure severity.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Defaults to <see cref="Severity.Error"/> when not specified.
    /// </remarks>
    public IRuleBuilderOptions<T, TElement> WithSeverity(Severity severity)
    {
        var last = _rule.Components.LastOrDefault();
        if (last is not null)
            last.Severity = severity;
        return this;
    }

    /// <summary>
    /// Executes the previous rule only when the condition is true.
    /// </summary>
    /// <param name="predicate">Condition for rule execution.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Evaluated against the root instance, not the element value.
    /// </remarks>
    public IRuleBuilderOptions<T, TElement> When(Func<T, bool> predicate)
    {
        var last = _rule.Components.LastOrDefault();
        if (last is not null)
            last.WhenCondition = predicate;
        return this;
    }

    /// <summary>
    /// Executes the previous rule only when the condition is false.
    /// </summary>
    /// <param name="predicate">Condition that prevents rule execution.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Equivalent to skipping the rule when the predicate returns true.
    /// </remarks>
    public IRuleBuilderOptions<T, TElement> Unless(Func<T, bool> predicate)
    {
        var last = _rule.Components.LastOrDefault();
        if (last is not null)
            last.UnlessCondition = predicate;
        return this;
    }

    /// <summary>
    /// Applies a child validator to the property.
    /// </summary>
    /// <typeparam name="TValidator">Type of the child validator.</typeparam>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Instantiates <typeparamref name="TValidator"/> with <c>new()</c>. Skipped when the element value is null.
    /// </remarks>
    public IRuleBuilderOptions<T, TElement> SetValidator<TValidator>()
        where TValidator : IValidator<TElement>, new()
    {
        return SetValidator(new TValidator());
    }

    /// <summary>
    /// Applies a child validator to the property.
    /// </summary>
    /// <param name="validator">Child validator instance.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Nested failures use indexed property names. Skipped when the element value is null.
    /// </remarks>
    public IRuleBuilderOptions<T, TElement> SetValidator(IValidator<TElement> validator)
    {
        _rule.ChildValidator = validator;
        return this;
    }

    /// <summary>
    /// Appends a validation component to the underlying collection rule.
    /// </summary>
    /// <param name="component">Component created by built-in extensions.</param>
    /// <remarks>
    /// Used internally by <see cref="DefaultValidatorExtensions"/>.
    /// </remarks>
    internal void AddComponent(RuleComponent<T, TElement> component)
    {
        _rule.Components.Add(component);
    }
}
