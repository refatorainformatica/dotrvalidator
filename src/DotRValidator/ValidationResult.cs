namespace DotRValidator;

/// <summary>
/// Contains the result of a validation operation.
/// </summary>
/// <remarks>
/// Aggregates all failures from evaluated rules. An empty error list indicates success.
/// </remarks>
public class ValidationResult
{
    /// <summary>
    /// Initializes a successful validation result.
    /// </summary>
    /// <remarks>
    /// Creates a result with an empty <see cref="Errors"/> collection.
    /// </remarks>
    public ValidationResult()
    {
        Errors = [];
    }

    /// <summary>
    /// Initializes a validation result with failures.
    /// </summary>
    /// <param name="failures">Collection of validation failures.</param>
    /// <remarks>
    /// Failures are copied into a mutable list exposed via <see cref="Errors"/>.
    /// </remarks>
    public ValidationResult(IEnumerable<ValidationFailure> failures)
    {
        Errors = failures.ToList();
    }

    /// <summary>
    /// Indicates whether validation succeeded (no errors).
    /// </summary>
    /// <remarks>
    /// True when <see cref="Errors"/> is empty.
    /// </remarks>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// List of validation failures found.
    /// </summary>
    /// <remarks>
    /// Mutable list; callers may append failures when composing validation pipelines.
    /// </remarks>
    public List<ValidationFailure> Errors { get; }

    /// <summary>
    /// Throws <see cref="ValidationException"/> if validation failed.
    /// </summary>
    /// <exception cref="ValidationException">When validation errors exist.</exception>
    /// <remarks>
    /// No-op when <see cref="IsValid"/> is true.
    /// </remarks>
    public void ThrowIfInvalid()
    {
        if (!IsValid)
            throw new ValidationException(this);
    }

    /// <summary>
    /// Combines multiple validation results into a single result.
    /// </summary>
    /// <param name="results">Results to combine.</param>
    /// <returns>Combined result containing all failures.</returns>
    /// <remarks>
    /// Flattens errors from each input; order follows the parameter sequence.
    /// </remarks>
    public static ValidationResult Combine(params ValidationResult[] results)
    {
        var failures = results.SelectMany(r => r.Errors);
        return new ValidationResult(failures);
    }
}
