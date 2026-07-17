using DotRValidator.Tests.Models;
using DotRValidator.Tests.Validators;

namespace DotRValidator.Tests;

/// <summary>
/// Tests for built-in validators and common rule combinations.
/// </summary>
/// <remarks>
/// Uses <see cref="CustomerValidator"/> as the primary system under test.
/// </remarks>
public class BuiltInValidatorsTests
{
    private readonly CustomerValidator _validator = new();

    /// <summary>
    /// Verifies that a fully valid customer passes all rules.
    /// </summary>
    /// <remarks>
    /// Expects an empty error collection.
    /// </remarks>
    [Fact]
    public void Validate_ValidCustomer_ShouldPass()
    {
        var customer = CreateValidCustomer();

        var result = _validator.Validate(customer);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Verifies that an empty name fails validation.
    /// </summary>
    /// <remarks>
    /// Exercises <see cref="DefaultValidatorExtensions.NotEmpty{T}(IRuleBuilder{T, string?})"/>.
    /// </remarks>
    [Fact]
    public void Validate_EmptyName_ShouldFail()
    {
        var customer = CreateValidCustomer();
        customer.Name = "";

        var result = _validator.Validate(customer);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    /// <summary>
    /// Verifies that an invalid email fails with a custom message.
    /// </summary>
    /// <remarks>
    /// Confirms <see cref="IRuleBuilderOptions{T, TProperty}.WithMessage(string)"/> overrides the default.
    /// </remarks>
    [Fact]
    public void Validate_InvalidEmail_ShouldFailWithCustomMessage()
    {
        var customer = CreateValidCustomer();
        customer.Email = "invalid-email";

        var result = _validator.Validate(customer);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            e =>
                e.PropertyName == "Email"
                && e.ErrorMessage == "Please provide a valid email address."
        );
    }

    /// <summary>
    /// Verifies that age outside the allowed range fails validation.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="DefaultValidatorExtensions.InclusiveBetween{T, TProperty}"/>.
    /// </remarks>
    [Fact]
    public void Validate_AgeOutOfRange_ShouldFail()
    {
        var customer = CreateValidCustomer();
        customer.Age = 10;

        var result = _validator.Validate(customer);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Age");
    }

    /// <summary>
    /// Verifies that conditional rules are skipped when the condition is false.
    /// </summary>
    /// <remarks>
    /// Discount rule runs only when <see cref="Customer.HasDiscount"/> is true.
    /// </remarks>
    [Fact]
    public void Validate_WhenCondition_ShouldOnlyValidateWhenTrue()
    {
        var customer = CreateValidCustomer();
        customer.HasDiscount = false;
        customer.Discount = 0;

        var result = _validator.Validate(customer);

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Verifies that conditional rules run when the condition is true.
    /// </summary>
    /// <remarks>
    /// Zero discount fails when a discount is expected.
    /// </remarks>
    [Fact]
    public void Validate_WhenConditionTrue_ShouldValidateDiscount()
    {
        var customer = CreateValidCustomer();
        customer.HasDiscount = true;
        customer.Discount = 0;

        var result = _validator.Validate(customer);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Discount");
    }

    /// <summary>
    /// Verifies that nested child validators produce prefixed property names.
    /// </summary>
    /// <remarks>
    /// Invalid postcode on address yields <c>Address.Postcode</c> failure.
    /// </remarks>
    [Fact]
    public void Validate_ChildValidator_ShouldValidateNestedObject()
    {
        var customer = CreateValidCustomer();
        customer.Address!.Postcode = "invalid";

        var result = _validator.Validate(customer);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Address.Postcode");
    }

    /// <summary>
    /// Verifies that validate-and-throw surfaces failures as an exception.
    /// </summary>
    /// <remarks>
    /// <see cref="ValidationException.Result"/> contains the failure list.
    /// </remarks>
    [Fact]
    public void ValidateAndThrow_InvalidCustomer_ShouldThrowValidationException()
    {
        var customer = CreateValidCustomer();
        customer.Name = null;

        var exception = Assert.Throws<ValidationException>(() =>
            _validator.ValidateAndThrow(customer)
        );

        Assert.NotEmpty(exception.Result.Errors);
    }

    /// <summary>
    /// Creates a customer instance that satisfies all validator rules.
    /// </summary>
    /// <returns>A valid test customer.</returns>
    /// <remarks>
    /// Shared baseline for positive and negative test cases.
    /// </remarks>
    private static Customer CreateValidCustomer() =>
        new()
        {
            Name = "John Smith",
            Email = "john@example.com",
            Age = 30,
            Address = new Address
            {
                Street = "Main Street",
                City = "New York",
                Postcode = "01310-100",
            },
        };
}
