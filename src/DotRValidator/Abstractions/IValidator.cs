namespace DotRValidator;

/// <summary>
/// Contract for validators that operate on a specific type.
/// </summary>
/// <typeparam name="T">Type of the instance to validate.</typeparam>
/// <remarks>
/// Implementations evaluate configured rules and return a <see cref="ValidationResult"/>.
/// Use <see cref="AbstractValidator{T}"/> for fluent rule definition.
/// </remarks>
public interface IValidator<T>
{
    /// <summary>
    /// Validates the specified instance synchronously.
    /// </summary>
    /// <param name="instance">Instance to validate.</param>
    /// <returns>Validation result containing any failures.</returns>
    /// <remarks>
    /// Runs all applicable rules without throwing. Call <see cref="ValidationResult.ThrowIfInvalid"/> to fail fast.
    /// </remarks>
    ValidationResult Validate(T instance);

    /// <summary>
    /// Validates the specified instance asynchronously.
    /// </summary>
    /// <param name="instance">Instance to validate.</param>
    /// <param name="cancellation">Token used to cancel async rule evaluation.</param>
    /// <returns>Validation result containing any failures.</returns>
    /// <remarks>
    /// Async rules are awaited in registration order. Cancellation is honored between rules.
    /// </remarks>
    Task<ValidationResult> ValidateAsync(T instance, CancellationToken cancellation = default);

    /// <summary>
    /// Validates using a validation context.
    /// </summary>
    /// <param name="context">Context with the instance and optional partial-validation metadata.</param>
    /// <returns>Validation result containing any failures.</returns>
    /// <remarks>
    /// Supports partial validation when <see cref="ValidationContext{T}.MemberNames"/> is set.
    /// </remarks>
    ValidationResult Validate(ValidationContext<T> context);

    /// <summary>
    /// Validates using a validation context asynchronously.
    /// </summary>
    /// <param name="context">Context with the instance and optional partial-validation metadata.</param>
    /// <param name="cancellation">Token used to cancel async rule evaluation.</param>
    /// <returns>Validation result containing any failures.</returns>
    /// <remarks>
    /// Combines context-based partial validation with async rule execution.
    /// </remarks>
    Task<ValidationResult> ValidateAsync(
        ValidationContext<T> context,
        CancellationToken cancellation = default
    );
}
