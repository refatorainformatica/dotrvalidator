using System.Linq.Expressions;
using System.Reflection;

namespace DotRValidator.Internal;

/// <summary>
/// Resolves property paths and compiles accessors from LINQ expressions.
/// </summary>
/// <remarks>
/// Used by <see cref="AbstractValidator{T}"/> when registering rules and when formatting error messages.
/// </remarks>
internal static class PropertyNameResolver
{
    /// <summary>
    /// Extracts the property path from a typed property expression.
    /// </summary>
    /// <typeparam name="T">Root instance type.</typeparam>
    /// <typeparam name="TProperty">Selected property type.</typeparam>
    /// <param name="expression">Property selection expression.</param>
    /// <returns>Dot-separated property path.</returns>
    /// <remarks>
    /// Delegates to <see cref="GetPropertyPath"/> using the expression body.
    /// </remarks>
    internal static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression)
    {
        return GetPropertyPath(expression.Body);
    }

    /// <summary>
    /// Extracts the property path from an expression body.
    /// </summary>
    /// <param name="expression">Expression body to analyze.</param>
    /// <returns>Dot-separated property path.</returns>
    /// <remarks>
    /// Supports member access, unary conversion wrappers, and simple method-call receivers.
    /// </remarks>
    /// <exception cref="ArgumentException">When the expression is not a supported property selector.</exception>
    internal static string GetPropertyPath(Expression expression)
    {
        return expression switch
        {
            MemberExpression member => GetMemberPath(member),
            UnaryExpression { Operand: MemberExpression operand } => GetMemberPath(operand),
            MethodCallExpression { Object: not null } methodCall => GetMethodCallPath(methodCall),
            _ => throw new ArgumentException(
                "Invalid expression for property selection.",
                nameof(expression)
            ),
        };
    }

    /// <summary>
    /// Builds a property path from a member expression chain.
    /// </summary>
    /// <param name="expression">Leaf member expression.</param>
    /// <returns>Dot-separated path from root to member.</returns>
    /// <remarks>
    /// Walks parent expressions until the chain ends.
    /// </remarks>
    private static string GetMemberPath(MemberExpression expression)
    {
        var parts = new Stack<string>();
        var current = expression;

        while (current is not null)
        {
            parts.Push(current.Member.Name);
            current = current.Expression as MemberExpression;
        }

        return string.Join(".", parts);
    }

    /// <summary>
    /// Builds a property path from a method call whose receiver is a member chain.
    /// </summary>
    /// <param name="expression">Method call expression.</param>
    /// <returns>Dot-separated property path.</returns>
    /// <remarks>
    /// Used for selectors such as nested property access via intermediate calls.
    /// </remarks>
    /// <exception cref="ArgumentException">When the receiver is not a member expression.</exception>
    private static string GetMethodCallPath(MethodCallExpression expression)
    {
        if (expression.Object is MemberExpression member)
            return GetMemberPath(member);

        throw new ArgumentException(
            "Invalid expression for property selection.",
            nameof(expression)
        );
    }

    /// <summary>
    /// Compiles a property accessor from an expression.
    /// </summary>
    /// <typeparam name="T">Root instance type.</typeparam>
    /// <typeparam name="TProperty">Selected property type.</typeparam>
    /// <param name="expression">Property selection expression.</param>
    /// <returns>Compiled delegate that reads the property value.</returns>
    /// <remarks>
    /// Invoked on each validation to obtain the current property value.
    /// </remarks>
    internal static Func<T, TProperty> Compile<T, TProperty>(
        Expression<Func<T, TProperty>> expression
    )
    {
        return expression.Compile();
    }

    /// <summary>
    /// Formats an error message template with property metadata.
    /// </summary>
    /// <param name="template">Message template with optional placeholders.</param>
    /// <param name="propertyName">Resolved property name.</param>
    /// <param name="attemptedValue">Value that failed validation.</param>
    /// <returns>Formatted message string.</returns>
    /// <remarks>
    /// Replaces <c>{PropertyName}</c> and <c>{PropertyValue}</c> using ordinal comparison.
    /// </remarks>
    internal static string FormatMessage(
        string template,
        string propertyName,
        object? attemptedValue
    )
    {
        return template
            .Replace("{PropertyName}", propertyName, StringComparison.Ordinal)
            .Replace(
                "{PropertyValue}",
                attemptedValue?.ToString() ?? string.Empty,
                StringComparison.Ordinal
            );
    }
}
