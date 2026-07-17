namespace DotRValidator;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
/// <remarks>
/// Thrown by <see cref="ValidationResult.ThrowIfInvalid"/> and validate-and-throw helpers.
/// The exception message lists all failure messages, one per line.
/// </remarks>
public class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance with the validation result.
    /// </summary>
    /// <param name="result">Validation result containing the failures.</param>
    /// <remarks>
    /// The exception message is built from each failure's <see cref="ValidationFailure.ToString"/> output.
    /// </remarks>
    public ValidationException(ValidationResult result)
        : base(BuildMessage(result))
    {
        Result = result;
    }

    /// <summary>
    /// Validation result associated with the exception.
    /// </summary>
    /// <remarks>
    /// Preserves structured failure data for logging or API error mapping.
    /// </remarks>
    public ValidationResult Result { get; }

    /// <summary>
    /// Builds the exception message from validation failures.
    /// </summary>
    /// <param name="result">Validation result containing failures.</param>
    /// <returns>Multiline message with one failure per line.</returns>
    /// <remarks>
    /// Uses the platform newline separator between messages.
    /// </remarks>
    private static string BuildMessage(ValidationResult result)
    {
        var messages = result.Errors.Select(e => e.ToString());
        return string.Join(Environment.NewLine, messages);
    }
}
