using DotRValidator.Tests.Models;
using DotRValidator.Tests.Validators;

namespace DotRValidator.Tests;

/// <summary>
/// Tests for asynchronous validation via <see cref="DefaultValidatorExtensions.MustAsync{T, TProperty}"/>.
/// </summary>
/// <remarks>
/// Covers async pass, fail, and validate-and-throw scenarios.
/// </remarks>
public class AsyncValidationTests
{
    /// <summary>
    /// Verifies that async validation succeeds when the predicate passes.
    /// </summary>
    /// <remarks>
    /// Uses a non-blocked name after a short async delay.
    /// </remarks>
    [Fact]
    public async Task MustAsync_ValidValue_ShouldPass()
    {
        var validator = new AsyncCustomerValidator();
        var customer = new Customer { Name = "Maria", Email = "maria@example.com" };

        var result = await validator.ValidateAsync(customer);

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Verifies that async validation fails when the predicate returns false.
    /// </summary>
    /// <remarks>
    /// The name "blocked" is rejected by the async rule.
    /// </remarks>
    [Fact]
    public async Task MustAsync_InvalidValue_ShouldFail()
    {
        var validator = new AsyncCustomerValidator();
        var customer = new Customer { Name = "blocked", Email = "maria@example.com" };

        var result = await validator.ValidateAsync(customer);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    /// <summary>
    /// Verifies that <see cref="AbstractValidator{T}.ValidateAndThrowAsync"/> throws on failure.
    /// </summary>
    /// <remarks>
    /// Expects <see cref="ValidationException"/> when async validation fails.
    /// </remarks>
    [Fact]
    public async Task ValidateAndThrowAsync_Invalid_ShouldThrow()
    {
        var validator = new AsyncCustomerValidator();
        var customer = new Customer { Name = "blocked", Email = "maria@example.com" };

        await Assert.ThrowsAsync<ValidationException>(() =>
            validator.ValidateAndThrowAsync(customer)
        );
    }

    /// <summary>
    /// Test validator that rejects the name "blocked" via an async rule.
    /// </summary>
    /// <remarks>
    /// Simulates async I/O with a short delay before evaluating the predicate.
    /// </remarks>
    private sealed class AsyncCustomerValidator : AbstractValidator<Customer>
    {
        /// <summary>
        /// Registers the async name validation rule.
        /// </summary>
        /// <remarks>
        /// Uses <see cref="DefaultValidatorExtensions.MustAsync{T, TProperty}"/> with cancellation support.
        /// </remarks>
        public AsyncCustomerValidator()
        {
            RuleFor(x => x.Name)
                .MustAsync(
                    async (name, cancellation) =>
                    {
                        await Task.Delay(10, cancellation);
                        return name != "blocked";
                    }
                )
                .WithMessage("Name is blocked.");
        }
    }
}
