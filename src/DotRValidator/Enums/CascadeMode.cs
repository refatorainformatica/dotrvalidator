namespace DotRValidator;

/// <summary>
/// Defines how validation rules in a chain should be executed when a failure occurs.
/// </summary>
/// <remarks>
/// Configured per property via <see cref="IRuleBuilder{T, TProperty}.Cascade(CascadeMode)"/>.
/// Applies to components on the same rule, not across different properties.
/// </remarks>
public enum CascadeMode
{
    /// <summary>
    /// Continues executing all rules even after a failure.
    /// </summary>
    /// <remarks>
    /// Default mode. Collects every failure in the chain.
    /// </remarks>
    Continue,

    /// <summary>
    /// Stops executing subsequent rules in the same chain after the first failure.
    /// </summary>
    /// <remarks>
    /// Useful when later rules depend on earlier ones passing (for example, length checks after not-empty).
    /// </remarks>
    Stop,
}
