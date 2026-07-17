namespace DotRValidator.Internal;

/// <summary>
/// Represents a single validator component within a property rule chain.
/// </summary>
/// <typeparam name="T">Type of the instance being validated.</typeparam>
/// <typeparam name="TProperty">Type of the property being validated.</typeparam>
/// <remarks>
/// Created by built-in extensions and executed in order by <see cref="PropertyRule{T, TProperty}"/>.
/// </remarks>
internal sealed class RuleComponent<T, TProperty>
{
    /// <summary>
    /// Initializes a synchronous rule component.
    /// </summary>
    /// <param name="validator">Sync validation delegate.</param>
    /// <param name="defaultMessage">Default failure message template.</param>
    /// <param name="errorCode">Optional error code.</param>
    /// <remarks>
    /// <see cref="AsyncValidator"/> remains null for sync-only components.
    /// </remarks>
    internal RuleComponent(
        Func<T, TProperty, ValidationContext<T>, bool> validator,
        string defaultMessage,
        string? errorCode = null
    )
    {
        SyncValidator = validator;
        DefaultMessage = defaultMessage;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes an asynchronous rule component.
    /// </summary>
    /// <param name="asyncValidator">Async validation delegate.</param>
    /// <param name="defaultMessage">Default failure message template.</param>
    /// <param name="errorCode">Optional error code.</param>
    /// <remarks>
    /// <see cref="SyncValidator"/> remains null for async-only components.
    /// </remarks>
    internal RuleComponent(
        Func<T, TProperty, ValidationContext<T>, CancellationToken, Task<bool>> asyncValidator,
        string defaultMessage,
        string? errorCode = null
    )
    {
        AsyncValidator = asyncValidator;
        DefaultMessage = defaultMessage;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Synchronous validation delegate, when configured.
    /// </summary>
    internal Func<T, TProperty, ValidationContext<T>, bool>? SyncValidator { get; }

    /// <summary>
    /// Asynchronous validation delegate, when configured.
    /// </summary>
    internal Func<
        T,
        TProperty,
        ValidationContext<T>,
        CancellationToken,
        Task<bool>
    >? AsyncValidator { get; }

    /// <summary>
    /// Default failure message template with placeholder tokens.
    /// </summary>
    internal string DefaultMessage { get; }

    /// <summary>
    /// Optional programmatic error code for failures.
    /// </summary>
    internal string? ErrorCode { get; set; }

    /// <summary>
    /// Optional custom message overriding <see cref="DefaultMessage"/>.
    /// </summary>
    internal string? CustomMessage { get; set; }

    /// <summary>
    /// Optional condition requiring a true root-instance predicate to run the rule.
    /// </summary>
    internal Func<T, bool>? WhenCondition { get; set; }

    /// <summary>
    /// Optional condition that skips the rule when the predicate is true.
    /// </summary>
    internal Func<T, bool>? UnlessCondition { get; set; }

    /// <summary>
    /// Severity assigned to failures from this component.
    /// </summary>
    internal Severity Severity { get; set; } = Severity.Error;

    /// <summary>
    /// Determines whether this component should execute for the given instance.
    /// </summary>
    /// <param name="instance">Root instance being validated.</param>
    /// <returns>True when the component should run.</returns>
    /// <remarks>
    /// <see cref="UnlessCondition"/> takes precedence over <see cref="WhenCondition"/>.
    /// </remarks>
    internal bool ShouldExecute(T instance)
    {
        if (UnlessCondition is not null && UnlessCondition(instance))
            return false;

        if (WhenCondition is not null)
            return WhenCondition(instance);

        return true;
    }

    /// <summary>
    /// Resolves the failure message for this component.
    /// </summary>
    /// <param name="propertyName">Property name to embed in the message.</param>
    /// <param name="attemptedValue">Value that failed validation.</param>
    /// <returns>Formatted error message.</returns>
    /// <remarks>
    /// Uses <see cref="CustomMessage"/> when set; otherwise <see cref="DefaultMessage"/>.
    /// </remarks>
    internal string GetMessage(string propertyName, object? attemptedValue)
    {
        var template = CustomMessage ?? DefaultMessage;
        return PropertyNameResolver.FormatMessage(template, propertyName, attemptedValue);
    }
}
