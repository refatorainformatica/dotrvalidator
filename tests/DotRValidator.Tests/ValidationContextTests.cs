using DotRValidator.Tests.Models;
using DotRValidator.Tests.Validators;

namespace DotRValidator.Tests;

/// <summary>
/// Tests for <see cref="ValidationContext{T}"/> and related validation utilities.
/// </summary>
/// <remarks>
/// Covers partial validation, result merging, and unless conditions.
/// </remarks>
public class ValidationContextTests
{
    /// <summary>
    /// Verifies that partial validation limits failures to selected members.
    /// </summary>
    /// <remarks>
    /// Only the Name property is validated; Email errors are excluded.
    /// </remarks>
    [Fact]
    public void Validate_SpecificMembers_ShouldOnlyValidateThoseMembers()
    {
        var validator = new CustomerValidator();
        var customer = new Customer
        {
            Name = "",
            Email = "invalid",
            Age = 30,
        };

        var result = validator.Validate(customer, "Name");

        Assert.NotEmpty(result.Errors);
        Assert.All(result.Errors, e => Assert.Equal("Name", e.PropertyName));
        Assert.DoesNotContain(result.Errors, e => e.PropertyName == "Email");
    }

    /// <summary>
    /// Verifies that combine merges errors from multiple results.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="ValidationResult.Combine(ValidationResult[])"/>.
    /// </remarks>
    [Fact]
    public void ValidationResult_Combine_ShouldMergeFailures()
    {
        var result1 = new ValidationResult([new ValidationFailure("A", "Error A")]);
        var result2 = new ValidationResult([new ValidationFailure("B", "Error B")]);

        var combined = ValidationResult.Combine(result1, result2);

        Assert.Equal(2, combined.Errors.Count);
    }

    /// <summary>
    /// Verifies that unless skips a rule when its predicate is true.
    /// </summary>
    /// <remarks>
    /// Empty name is allowed when age is at least 18.
    /// </remarks>
    [Fact]
    public void Unless_ShouldSkipRuleWhenConditionIsTrue()
    {
        var validator = new UnlessValidator();
        var customer = new Customer { Name = null!, Age = 30 };

        var result = validator.Validate(customer);

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Test validator demonstrating unless on a not-empty name rule.
    /// </summary>
    /// <remarks>
    /// Skips name validation for adult customers.
    /// </remarks>
    private sealed class UnlessValidator : AbstractValidator<Customer>
    {
        /// <summary>
        /// Registers name rule with an unless condition on age.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="IRuleBuilderOptions{T, TProperty}.Unless(Func{T, bool})"/>.
        /// </remarks>
        public UnlessValidator()
        {
            RuleFor(x => x.Name).NotEmpty().Unless(x => x.Age >= 18);
        }
    }
}
