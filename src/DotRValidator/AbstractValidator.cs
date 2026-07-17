using System.Linq.Expressions;
using DotRValidator.Internal;

namespace DotRValidator;

/// <summary>
/// Abstract base class for creating fluent validators.
/// </summary>
/// <typeparam name="T">Type of the instance to validate.</typeparam>
/// <remarks>
/// Derive from this class and configure rules in the constructor using <see cref="RuleFor{TProperty}"/> and <see cref="RuleForEach{TElement}"/>.
/// Rules are evaluated in registration order unless cascade mode stops the chain.
/// </remarks>
public abstract class AbstractValidator<T> : IValidator<T>
{
    private readonly List<IValidationRule<T>> _rules = [];

    /// <summary>
    /// Defines a validation rule for a specific property.
    /// </summary>
    /// <typeparam name="TProperty">Property type.</typeparam>
    /// <param name="expression">Expression that selects the property.</param>
    /// <returns>Builder for rule chaining.</returns>
    /// <remarks>
    /// The property path is resolved from the expression and used in failure messages.
    /// </remarks>
    public IRuleBuilder<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        var propertyName = PropertyNameResolver.GetPropertyName(expression);
        var accessor = PropertyNameResolver.Compile(expression);
        var rule = new PropertyRule<T, TProperty>(propertyName, accessor);
        _rules.Add(rule);
        return new RuleBuilder<T, TProperty>(rule);
    }

    /// <summary>
    /// Defines validation rules for each element in a collection.
    /// </summary>
    /// <typeparam name="TElement">Collection element type.</typeparam>
    /// <param name="expression">Expression that selects the collection.</param>
    /// <returns>Builder for rule chaining.</returns>
    /// <remarks>
    /// Failures use indexed property names such as <c>Orders[0].ProductName</c>.
    /// Null collections are skipped without error.
    /// </remarks>
    public IRuleBuilderInitial<T, TElement> RuleForEach<TElement>(
        Expression<Func<T, IEnumerable<TElement>>> expression
    )
    {
        var propertyName = PropertyNameResolver.GetPropertyName(expression);
        var accessor = PropertyNameResolver.Compile(expression);
        var rule = new CollectionPropertyRule<T, TElement>(propertyName, accessor);
        _rules.Add(rule);
        return new CollectionRuleBuilder<T, TElement>(rule);
    }

    /// <inheritdoc />
    public ValidationResult Validate(T instance)
    {
        return Validate(new ValidationContext<T>(instance));
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateAsync(
        T instance,
        CancellationToken cancellation = default
    )
    {
        return await ValidateAsync(new ValidationContext<T>(instance), cancellation);
    }

    /// <inheritdoc />
    public ValidationResult Validate(ValidationContext<T> context)
    {
        var failures = _rules.SelectMany(rule => rule.Validate(context));
        return new ValidationResult(failures);
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ValidateAsync(
        ValidationContext<T> context,
        CancellationToken cancellation = default
    )
    {
        var allFailures = new List<ValidationFailure>();

        foreach (var rule in _rules)
        {
            var failures = await rule.ValidateAsync(context, cancellation);
            allFailures.AddRange(failures);
        }

        return new ValidationResult(allFailures);
    }

    /// <summary>
    /// Validates only the specified properties.
    /// </summary>
    /// <param name="instance">Instance to validate.</param>
    /// <param name="memberNames">Property names to validate.</param>
    /// <returns>Validation result containing failures for the selected members only.</returns>
    /// <remarks>
    /// Nested and indexed members are matched by prefix (for example, <c>Address</c> includes <c>Address.City</c>).
    /// </remarks>
    public ValidationResult Validate(T instance, params string[] memberNames)
    {
        var context = new ValidationContext<T>(instance) { MemberNames = memberNames };
        return Validate(context);
    }

    /// <summary>
    /// Validates and throws an exception if there are failures.
    /// </summary>
    /// <param name="instance">Instance to validate.</param>
    /// <remarks>
    /// Throws <see cref="ValidationException"/> when any rule fails.
    /// </remarks>
    public void ValidateAndThrow(T instance)
    {
        Validate(instance).ThrowIfInvalid();
    }

    /// <summary>
    /// Validates asynchronously and throws an exception if there are failures.
    /// </summary>
    /// <param name="instance">Instance to validate.</param>
    /// <param name="cancellation">Token used to cancel async rule evaluation.</param>
    /// <remarks>
    /// Throws <see cref="ValidationException"/> when any rule fails after async evaluation completes.
    /// </remarks>
    public async Task ValidateAndThrowAsync(T instance, CancellationToken cancellation = default)
    {
        (await ValidateAsync(instance, cancellation)).ThrowIfInvalid();
    }
}
