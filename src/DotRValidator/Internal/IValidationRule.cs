namespace DotRValidator.Internal;

/// <summary>
/// Internal contract for a single validation rule on a model type.
/// </summary>
/// <typeparam name="T">Type of the instance being validated.</typeparam>
/// <remarks>
/// Implemented by property and collection rules registered on <see cref="AbstractValidator{T}"/>.
/// </remarks>
internal interface IValidationRule<T>
{
    /// <summary>
    /// Validates synchronously using the provided context.
    /// </summary>
    /// <param name="context">Validation context with instance and optional member filter.</param>
    /// <returns>Failures produced by this rule.</returns>
    /// <remarks>
    /// Returns empty when the rule is excluded by partial member selection.
    /// </remarks>
    IEnumerable<ValidationFailure> Validate(ValidationContext<T> context);

    /// <summary>
    /// Validates asynchronously using the provided context.
    /// </summary>
    /// <param name="context">Validation context with instance and optional member filter.</param>
    /// <param name="cancellation">Token used to cancel async component evaluation.</param>
    /// <returns>Failures produced by this rule.</returns>
    /// <remarks>
    /// Falls back to sync validators when no async component is defined.
    /// </remarks>
    Task<IEnumerable<ValidationFailure>> ValidateAsync(
        ValidationContext<T> context,
        CancellationToken cancellation
    );

    /// <summary>
    /// Determines whether this rule applies to the given member name.
    /// </summary>
    /// <param name="memberName">Member name from partial validation.</param>
    /// <returns>True when the rule should run for the member.</returns>
    /// <remarks>
    /// Supports nested paths and collection index prefixes.
    /// </remarks>
    bool AppliesToMember(string memberName);
}
