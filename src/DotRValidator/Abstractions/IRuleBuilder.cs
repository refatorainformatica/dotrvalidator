namespace DotRValidator;

/// <summary>
/// Base interface for building validation rules.
/// </summary>
/// <typeparam name="T">Type of the instance being validated.</typeparam>
/// <typeparam name="TProperty">Type of the property being validated.</typeparam>
/// <remarks>
/// Returned by <see cref="AbstractValidator{T}.RuleFor{TProperty}"/> and collection rule builders.
/// Chain built-in validators and options before adding the next rule.
/// </remarks>
public interface IRuleBuilder<T, TProperty>
{
    /// <summary>
    /// Sets the cascade mode for subsequent rules.
    /// </summary>
    /// <param name="cascadeMode">Desired cascade mode.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Affects components added after this call on the same property rule.
    /// </remarks>
    IRuleBuilderOptions<T, TProperty> Cascade(CascadeMode cascadeMode);

    /// <summary>
    /// Applies a child validator to the property.
    /// </summary>
    /// <typeparam name="TValidator">Type of the child validator.</typeparam>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Instantiates <typeparamref name="TValidator"/> with <c>new()</c>. Skipped when the property value is null.
    /// </remarks>
    IRuleBuilderOptions<T, TProperty> SetValidator<TValidator>()
        where TValidator : IValidator<TProperty>, new();

    /// <summary>
    /// Applies a child validator to the property.
    /// </summary>
    /// <param name="validator">Child validator instance.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Nested failures are prefixed with the parent property name. Skipped when the property value is null.
    /// </remarks>
    IRuleBuilderOptions<T, TProperty> SetValidator(IValidator<TProperty> validator);
}

/// <summary>
/// Interface for additional options after defining a validation rule.
/// </summary>
/// <typeparam name="T">Type of the instance being validated.</typeparam>
/// <typeparam name="TProperty">Type of the property being validated.</typeparam>
/// <remarks>
/// Options apply to the most recently added rule component on the current property.
/// </remarks>
public interface IRuleBuilderOptions<T, TProperty> : IRuleBuilder<T, TProperty>
{
    /// <summary>
    /// Sets a custom error message for the previous rule.
    /// </summary>
    /// <param name="errorMessage">Error message. Supports {PropertyName} and {PropertyValue} placeholders.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Overrides the default message from built-in validators.
    /// </remarks>
    IRuleBuilderOptions<T, TProperty> WithMessage(string errorMessage);

    /// <summary>
    /// Sets a custom error code for the previous rule.
    /// </summary>
    /// <param name="errorCode">Identifying error code.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Stored on <see cref="ValidationFailure.ErrorCode"/> when the rule fails.
    /// </remarks>
    IRuleBuilderOptions<T, TProperty> WithErrorCode(string errorCode);

    /// <summary>
    /// Sets the severity of the failure for the previous rule.
    /// </summary>
    /// <param name="severity">Failure severity.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Defaults to <see cref="Severity.Error"/> when not specified.
    /// </remarks>
    IRuleBuilderOptions<T, TProperty> WithSeverity(Severity severity);

    /// <summary>
    /// Executes the previous rule only when the condition is true.
    /// </summary>
    /// <param name="predicate">Condition for rule execution.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Evaluated against the root instance, not the property value.
    /// </remarks>
    IRuleBuilderOptions<T, TProperty> When(Func<T, bool> predicate);

    /// <summary>
    /// Executes the previous rule only when the condition is false.
    /// </summary>
    /// <param name="predicate">Condition that prevents rule execution.</param>
    /// <returns>Rule builder for chaining.</returns>
    /// <remarks>
    /// Equivalent to skipping the rule when the predicate returns true.
    /// </remarks>
    IRuleBuilderOptions<T, TProperty> Unless(Func<T, bool> predicate);
}

/// <summary>
/// Initial interface for building rules on collections.
/// </summary>
/// <typeparam name="T">Type of the instance being validated.</typeparam>
/// <typeparam name="TElement">Type of the collection elements.</typeparam>
/// <remarks>
/// Returned by <see cref="AbstractValidator{T}.RuleForEach{TElement}"/>.
/// Rules apply to each element independently with indexed property names.
/// </remarks>
public interface IRuleBuilderInitial<T, TElement> : IRuleBuilder<T, TElement> { }
