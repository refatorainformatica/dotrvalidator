namespace DotRValidator;

/// <summary>
/// Validation context containing the instance being validated and additional metadata.
/// </summary>
/// <typeparam name="T">Type of the instance being validated.</typeparam>
/// <remarks>
/// Passed to rules during evaluation. Use <see cref="RootContextData"/> to share cross-rule state.
/// </remarks>
public class ValidationContext<T>
{
    /// <summary>
    /// Initializes a validation context for the specified instance.
    /// </summary>
    /// <param name="instanceToValidate">Instance to validate.</param>
    /// <remarks>
    /// Creates a context with an empty <see cref="RootContextData"/> dictionary.
    /// </remarks>
    public ValidationContext(T instanceToValidate)
    {
        InstanceToValidate = instanceToValidate;
    }

    /// <summary>
    /// Instance being validated.
    /// </summary>
    public T InstanceToValidate { get; }

    /// <summary>
    /// Dictionary of additional properties that can be used during validation.
    /// </summary>
    /// <remarks>
    /// Useful for passing services or flags into custom <see cref="DefaultValidatorExtensions.Must{T, TProperty}(IRuleBuilder{T, TProperty}, Func{T, TProperty, bool})"/> predicates.
    /// </remarks>
    public Dictionary<string, object?> RootContextData { get; } = new();

    /// <summary>
    /// Member selector for partial validation. When set, only the specified properties are validated.
    /// </summary>
    /// <remarks>
    /// When null or empty, all registered rules run. Set via <see cref="AbstractValidator{T}.Validate(T, string[])"/>.
    /// </remarks>
    public string[]? MemberNames { get; set; }
}
