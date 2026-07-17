using System.Text.RegularExpressions;
using DotRValidator.Internal;

namespace DotRValidator.Extensions;

/// <summary>
/// Fluent extension methods with built-in validators for rule construction.
/// </summary>
/// <remarks>
/// Extend <see cref="IRuleBuilder{T, TProperty}"/> to add common validation predicates.
/// Each method registers a <see cref="RuleComponent{T, TProperty}"/> on the current rule.
/// </remarks>
public static partial class DefaultValidatorExtensions
{
    /// <summary>
    /// Validates that the property is not null.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Fails when the property value is null. Reference types and nullable value types only.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> NotNull<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => value is not null,
            "'{PropertyName}' must not be null."
        );
    }

    /// <summary>
    /// Validates that the property is null.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Fails when the property value is not null.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> Null<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => value is null,
            "'{PropertyName}' must be null."
        );
    }

    /// <summary>
    /// Validates that the string is not null, empty, or whitespace.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Uses <see cref="string.IsNullOrWhiteSpace(string?)"/> for the check.
    /// </remarks>
    public static IRuleBuilderOptions<T, string?> NotEmpty<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => !string.IsNullOrWhiteSpace(value),
            "'{PropertyName}' must not be empty."
        );
    }

    /// <summary>
    /// Validates that the string is null, empty, or whitespace.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Inverse of <see cref="NotEmpty{T}(IRuleBuilder{T, string?})"/>.
    /// </remarks>
    public static IRuleBuilderOptions<T, string?> Empty<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => string.IsNullOrWhiteSpace(value),
            "'{PropertyName}' must be empty."
        );
    }

    /// <summary>
    /// Validates that the collection is not null or empty.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TElement">Element type of the collection.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Requires at least one element via <see cref="Enumerable.Any{TSource}(IEnumerable{TSource})"/>.
    /// </remarks>
    public static IRuleBuilderOptions<T, IEnumerable<TElement>> NotEmpty<T, TElement>(
        this IRuleBuilder<T, IEnumerable<TElement>> ruleBuilder
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => value is not null && value.Any(),
            "'{PropertyName}' must not be empty."
        );
    }

    /// <summary>
    /// Validates that the collection is not null or empty.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TElement">Element type of the collection.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Uses <see cref="ICollection{T}.Count"/> for the emptiness check.
    /// </remarks>
    public static IRuleBuilderOptions<T, ICollection<TElement>> NotEmpty<T, TElement>(
        this IRuleBuilder<T, ICollection<TElement>> ruleBuilder
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => value is not null && value.Count > 0,
            "'{PropertyName}' must not be empty."
        );
    }

    /// <summary>
    /// Validates that the list is not null or empty.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TElement">Element type of the list.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Specialized overload for <see cref="List{T}"/> properties.
    /// </remarks>
    public static IRuleBuilderOptions<T, List<TElement>> NotEmpty<T, TElement>(
        this IRuleBuilder<T, List<TElement>> ruleBuilder
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => value is not null && value.Count > 0,
            "'{PropertyName}' must not be empty."
        );
    }

    /// <summary>
    /// Validates that the value equals the specified value.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="expected">Expected value.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Uses default equality comparer for <typeparamref name="TProperty"/>.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> Equal<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        TProperty expected
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => EqualityComparer<TProperty>.Default.Equals(value, expected),
            "'{PropertyName}' must be equal to '{PropertyValue}'."
        );
    }

    /// <summary>
    /// Validates that the value equals the value returned by the function.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="expectedProvider">Function that supplies the expected value from the instance.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Useful for cross-property checks such as password confirmation.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> Equal<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Func<T, TProperty> expectedProvider
    )
    {
        return ruleBuilder.SetValidator(
            (instance, value, _) =>
                EqualityComparer<TProperty>.Default.Equals(value, expectedProvider(instance)),
            "'{PropertyName}' must be equal to the expected value."
        );
    }

    /// <summary>
    /// Validates that the value is not equal to the specified value.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="expected">Value that must not match.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Uses default equality comparer for <typeparamref name="TProperty"/>.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> NotEqual<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        TProperty expected
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => !EqualityComparer<TProperty>.Default.Equals(value, expected),
            "'{PropertyName}' must not be equal to '{PropertyValue}'."
        );
    }

    /// <summary>
    /// Validates that the string length is within the specified range.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="min">Minimum inclusive length.</param>
    /// <param name="max">Maximum inclusive length.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Fails when the value is null or outside the inclusive bounds.
    /// </remarks>
    public static IRuleBuilderOptions<T, string?> Length<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        int min,
        int max
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => value is not null && value.Length >= min && value.Length <= max,
            $"'{{PropertyName}}' must be between {min} and {max} characters."
        );
    }

    /// <summary>
    /// Validates that the string length equals the specified value.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="exactLength">Required exact length.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Fails when the value is null or length differs from <paramref name="exactLength"/>.
    /// </remarks>
    public static IRuleBuilderOptions<T, string?> Length<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        int exactLength
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => value is not null && value.Length == exactLength,
            $"'{{PropertyName}}' must be exactly {exactLength} characters."
        );
    }

    /// <summary>
    /// Validates the minimum string length.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="minLength">Minimum inclusive length.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Fails when the value is null or shorter than <paramref name="minLength"/>.
    /// </remarks>
    public static IRuleBuilderOptions<T, string?> MinimumLength<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        int minLength
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => value is not null && value.Length >= minLength,
            $"'{{PropertyName}}' must be at least {minLength} characters."
        );
    }

    /// <summary>
    /// Validates the maximum string length.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="maxLength">Maximum inclusive length.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Null values pass; only non-null strings are length-checked.
    /// </remarks>
    public static IRuleBuilderOptions<T, string?> MaximumLength<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        int maxLength
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => value is null || value.Length <= maxLength,
            $"'{{PropertyName}}' must be at most {maxLength} characters."
        );
    }

    /// <summary>
    /// Validates that the value is greater than the specified value.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Comparable property type.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="valueToCompare">Exclusive lower bound reference value.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Fails when the property value is null or not greater than the comparison value.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> GreaterThan<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        TProperty valueToCompare
    )
        where TProperty : IComparable<TProperty>, IComparable
    {
        return ruleBuilder.SetValidator(
            (_, value, _) =>
                value is not null && Comparer<TProperty>.Default.Compare(value, valueToCompare) > 0,
            $"'{{PropertyName}}' must be greater than {valueToCompare}."
        );
    }

    /// <summary>
    /// Validates that the value is greater than or equal to the specified value.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Comparable property type.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="valueToCompare">Inclusive lower bound reference value.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Fails when the property value is null or less than the comparison value.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> GreaterThanOrEqualTo<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        TProperty valueToCompare
    )
        where TProperty : IComparable<TProperty>, IComparable
    {
        return ruleBuilder.SetValidator(
            (_, value, _) =>
                value is not null
                && Comparer<TProperty>.Default.Compare(value, valueToCompare) >= 0,
            $"'{{PropertyName}}' must be greater than or equal to {valueToCompare}."
        );
    }

    /// <summary>
    /// Validates that the value is less than the specified value.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Comparable property type.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="valueToCompare">Exclusive upper bound reference value.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Fails when the property value is null or not less than the comparison value.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> LessThan<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        TProperty valueToCompare
    )
        where TProperty : IComparable<TProperty>, IComparable
    {
        return ruleBuilder.SetValidator(
            (_, value, _) =>
                value is not null && Comparer<TProperty>.Default.Compare(value, valueToCompare) < 0,
            $"'{{PropertyName}}' must be less than {valueToCompare}."
        );
    }

    /// <summary>
    /// Validates that the value is less than or equal to the specified value.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Comparable property type.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="valueToCompare">Inclusive upper bound reference value.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Fails when the property value is null or greater than the comparison value.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> LessThanOrEqualTo<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        TProperty valueToCompare
    )
        where TProperty : IComparable<TProperty>, IComparable
    {
        return ruleBuilder.SetValidator(
            (_, value, _) =>
                value is not null
                && Comparer<TProperty>.Default.Compare(value, valueToCompare) <= 0,
            $"'{{PropertyName}}' must be less than or equal to {valueToCompare}."
        );
    }

    /// <summary>
    /// Validates that the value is between the inclusive bounds.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Comparable property type.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="from">Inclusive lower bound.</param>
    /// <param name="to">Inclusive upper bound.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Fails when the property value is null or outside the closed interval.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> InclusiveBetween<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        TProperty from,
        TProperty to
    )
        where TProperty : IComparable<TProperty>, IComparable
    {
        return ruleBuilder.SetValidator(
            (_, value, _) =>
            {
                if (value is null)
                    return false;
                return Comparer<TProperty>.Default.Compare(value, from) >= 0
                    && Comparer<TProperty>.Default.Compare(value, to) <= 0;
            },
            $"'{{PropertyName}}' must be between {from} and {to}."
        );
    }

    /// <summary>
    /// Validates that the value is between the exclusive bounds.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Comparable property type.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="from">Exclusive lower bound.</param>
    /// <param name="to">Exclusive upper bound.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Fails when the property value is null or on either boundary.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> ExclusiveBetween<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        TProperty from,
        TProperty to
    )
        where TProperty : IComparable<TProperty>, IComparable
    {
        return ruleBuilder.SetValidator(
            (_, value, _) =>
            {
                if (value is null)
                    return false;
                return Comparer<TProperty>.Default.Compare(value, from) > 0
                    && Comparer<TProperty>.Default.Compare(value, to) < 0;
            },
            $"'{{PropertyName}}' must be between {from} and {to} (exclusive)."
        );
    }

    /// <summary>
    /// Validates that the string is a valid email address.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Uses a compiled source-generated regex for basic format validation.
    /// </remarks>
    public static IRuleBuilderOptions<T, string?> EmailAddress<T>(
        this IRuleBuilder<T, string?> ruleBuilder
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => value is not null && EmailRegex().IsMatch(value),
            "'{PropertyName}' is not a valid email address."
        );
    }

    /// <summary>
    /// Validates that the string matches the specified regular expression.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="pattern">Regular expression pattern.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Compiles the regex once per rule registration. Null values fail.
    /// </remarks>
    public static IRuleBuilderOptions<T, string?> Matches<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        string pattern
    )
    {
        var regex = new Regex(pattern, RegexOptions.Compiled);
        return ruleBuilder.SetValidator(
            (_, value, _) => value is not null && regex.IsMatch(value),
            "'{PropertyName}' is not in the expected format."
        );
    }

    /// <summary>
    /// Validates using a custom predicate.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="predicate">Predicate that must return true for a valid value.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Receives only the property value. Use the two-parameter overload for access to the root instance.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> Must<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Func<TProperty, bool> predicate
    )
    {
        return ruleBuilder.SetValidator(
            (_, value, _) => predicate(value),
            "'{PropertyName}' failed the custom validation."
        );
    }

    /// <summary>
    /// Validates using a custom predicate with access to the instance.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="predicate">Predicate that must return true for a valid value.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Enables cross-field validation using both root instance and property value.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> Must<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Func<T, TProperty, bool> predicate
    )
    {
        return ruleBuilder.SetValidator(
            (instance, value, _) => predicate(instance, value),
            "'{PropertyName}' failed the custom validation."
        );
    }

    /// <summary>
    /// Validates using an async custom predicate.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="predicate">Async predicate that must return true for a valid value.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Honours <see cref="CancellationToken"/> during async validation.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> MustAsync<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Func<TProperty, CancellationToken, Task<bool>> predicate
    )
    {
        return ruleBuilder.SetAsyncValidator(
            async (_, value, _, cancellation) => await predicate(value, cancellation),
            "'{PropertyName}' failed the custom validation."
        );
    }

    /// <summary>
    /// Validates using an async custom predicate with access to the instance.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="predicate">Async predicate that must return true for a valid value.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Combines async I/O with access to the full root instance.
    /// </remarks>
    public static IRuleBuilderOptions<T, TProperty> MustAsync<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Func<T, TProperty, CancellationToken, Task<bool>> predicate
    )
    {
        return ruleBuilder.SetAsyncValidator(
            async (instance, value, _, cancellation) =>
                await predicate(instance, value, cancellation),
            "'{PropertyName}' failed the custom validation."
        );
    }

    /// <summary>
    /// Registers a synchronous validator component on the current rule.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="validator">Sync validation delegate.</param>
    /// <param name="defaultMessage">Default failure message template.</param>
    /// <param name="errorCode">Optional error code.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Dispatches to <see cref="RuleBuilder{T, TProperty}"/> or <see cref="CollectionRuleBuilder{T, TElement}"/>.
    /// </remarks>
    private static IRuleBuilderOptions<T, TProperty> SetValidator<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Func<T, TProperty, ValidationContext<T>, bool> validator,
        string defaultMessage,
        string? errorCode = null
    )
    {
        var component = new RuleComponent<T, TProperty>(validator, defaultMessage, errorCode);

        switch (ruleBuilder)
        {
            case RuleBuilder<T, TProperty> builder:
                builder.AddComponent(component);
                break;
            case CollectionRuleBuilder<T, TProperty> collectionBuilder:
                collectionBuilder.AddComponent(component);
                break;
        }

        return (IRuleBuilderOptions<T, TProperty>)ruleBuilder;
    }

    /// <summary>
    /// Registers an asynchronous validator component on the current rule.
    /// </summary>
    /// <typeparam name="T">Type of the instance being validated.</typeparam>
    /// <typeparam name="TProperty">Type of the property being validated.</typeparam>
    /// <param name="ruleBuilder">Rule builder to extend.</param>
    /// <param name="validator">Async validation delegate.</param>
    /// <param name="defaultMessage">Default failure message template.</param>
    /// <param name="errorCode">Optional error code.</param>
    /// <returns>Rule builder options for further chaining.</returns>
    /// <remarks>
    /// Dispatches to property or collection rule builders based on runtime type.
    /// </remarks>
    private static IRuleBuilderOptions<T, TProperty> SetAsyncValidator<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        Func<T, TProperty, ValidationContext<T>, CancellationToken, Task<bool>> validator,
        string defaultMessage,
        string? errorCode = null
    )
    {
        var component = new RuleComponent<T, TProperty>(validator, defaultMessage, errorCode);

        switch (ruleBuilder)
        {
            case RuleBuilder<T, TProperty> builder:
                builder.AddComponent(component);
                break;
            case CollectionRuleBuilder<T, TProperty> collectionBuilder:
                collectionBuilder.AddComponent(component);
                break;
        }

        return (IRuleBuilderOptions<T, TProperty>)ruleBuilder;
    }

    /// <summary>
    /// Source-generated regex for basic email format validation.
    /// </summary>
    /// <returns>Compiled regex instance.</returns>
    /// <remarks>
    /// Pattern requires local part, @, domain, and TLD segments without whitespace.
    /// </remarks>
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}
