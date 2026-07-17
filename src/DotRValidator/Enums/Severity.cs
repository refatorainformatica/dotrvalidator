namespace DotRValidator;

/// <summary>
/// Indicates the severity of a validation failure.
/// </summary>
/// <remarks>
/// Assigned via <see cref="IRuleBuilderOptions{T, TProperty}.WithSeverity(Severity)"/>.
/// Consumers may filter or display failures differently by severity.
/// </remarks>
public enum Severity
{
    /// <summary>
    /// Error that prevents validation from succeeding.
    /// </summary>
    /// <remarks>
    /// Default severity for built-in validators.
    /// </remarks>
    Error,

    /// <summary>
    /// Warning that does not prevent validation from succeeding.
    /// </summary>
    /// <remarks>
    /// Included in <see cref="ValidationResult.Errors"/> but may be treated as non-blocking by callers.
    /// </remarks>
    Warning,

    /// <summary>
    /// Additional informational message about the validation.
    /// </summary>
    /// <remarks>
    /// For advisory feedback that should not block processing.
    /// </remarks>
    Info,
}
