namespace DotRValidator;

/// <summary>
/// Represents an individual validation failure.
/// </summary>
/// <remarks>
/// Created by rules when a validator returns false. Optional fields support error codes, severity, and diagnostics.
/// </remarks>
public class ValidationFailure
{
    /// <summary>
    /// Initializes a new instance of <see cref="ValidationFailure"/>.
    /// </summary>
    /// <param name="propertyName">Name of the property that failed validation.</param>
    /// <param name="errorMessage">Descriptive error message.</param>
    /// <remarks>
    /// <see cref="Severity"/> defaults to <see cref="Severity.Error"/>.
    /// </remarks>
    public ValidationFailure(string propertyName, string errorMessage)
    {
        PropertyName = propertyName;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Name of the property that failed validation.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Descriptive error message.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Value that was submitted for validation when the failure occurred.
    /// </summary>
    /// <remarks>
    /// Populated by internal rules from the evaluated property or collection element.
    /// </remarks>
    public object? AttemptedValue { get; set; }

    /// <summary>
    /// Optional error code for programmatic identification of the failure.
    /// </summary>
    /// <remarks>
    /// Set via <see cref="IRuleBuilderOptions{T, TProperty}.WithErrorCode(string)"/>.
    /// </remarks>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Severity of the validation failure.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Severity.Error"/>. Warnings and info do not affect <see cref="ValidationResult.IsValid"/> by themselves.
    /// </remarks>
    public Severity Severity { get; set; } = Severity.Error;

    /// <summary>
    /// Instance of the object being validated.
    /// </summary>
    /// <remarks>
    /// References the root instance or the validated child object for nested failures.
    /// </remarks>
    public object? Instance { get; set; }

    /// <summary>
    /// Text representation of the validation failure.
    /// </summary>
    /// <returns>A string in the form <c>PropertyName: ErrorMessage</c>.</returns>
    /// <remarks>
    /// Used when building <see cref="ValidationException"/> messages.
    /// </remarks>
    public override string ToString() => $"{PropertyName}: {ErrorMessage}";
}
