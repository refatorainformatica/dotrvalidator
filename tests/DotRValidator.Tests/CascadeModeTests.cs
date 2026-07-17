using DotRValidator.Tests.Models;
using DotRValidator.Tests.Validators;

namespace DotRValidator.Tests;

/// <summary>
/// Tests for <see cref="CascadeMode"/> behavior on rule chains.
/// </summary>
/// <remarks>
/// Compares stop versus continue cascade on the same property.
/// </remarks>
public class CascadeModeTests
{
    /// <summary>
    /// Verifies that stop cascade halts after the first failing component.
    /// </summary>
    /// <remarks>
    /// <see cref="RegistrationValidator"/> uses stop on first name; only one first-name error is expected.
    /// </remarks>
    [Fact]
    public void CascadeMode_Stop_ShouldStopAfterFirstFailure()
    {
        var validator = new RegistrationValidator();
        var request = new RegistrationRequest
        {
            FirstName = "",
            LastName = "",
            Email = "invalid",
            Password = "123",
            ConfirmPassword = "456",
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Single(result.Errors.Where(e => e.PropertyName == "FirstName"));
    }

    /// <summary>
    /// Verifies that continue cascade runs every component in the chain.
    /// </summary>
    /// <remarks>
    /// Empty first name fails both not-empty and minimum-length rules.
    /// </remarks>
    [Fact]
    public void CascadeMode_Continue_ShouldRunAllRules()
    {
        var validator = new ContinueCascadeValidator();
        var request = new RegistrationRequest { FirstName = "" };

        var result = validator.Validate(request);

        Assert.Equal(2, result.Errors.Count(e => e.PropertyName == "FirstName"));
    }

    /// <summary>
    /// Test validator that uses continue cascade on first name.
    /// </summary>
    /// <remarks>
    /// Contrasts with <see cref="RegistrationValidator"/> stop behavior.
    /// </remarks>
    private sealed class ContinueCascadeValidator : AbstractValidator<RegistrationRequest>
    {
        /// <summary>
        /// Registers first name rules with continue cascade.
        /// </summary>
        /// <remarks>
        /// Chains not-empty and minimum-length without stopping early.
        /// </remarks>
        public ContinueCascadeValidator()
        {
            RuleFor(x => x.FirstName).Cascade(CascadeMode.Continue).NotEmpty().MinimumLength(5);
        }
    }
}
