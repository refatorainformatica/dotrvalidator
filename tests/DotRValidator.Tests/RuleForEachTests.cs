using DotRValidator.Tests.Models;
using DotRValidator.Tests.Validators;

namespace DotRValidator.Tests;

/// <summary>
/// Tests for <see cref="AbstractValidator{T}.RuleForEach{TElement}"/> collection validation.
/// </summary>
/// <remarks>
/// Covers indexed property names and collection-level rules.
/// </remarks>
public class RuleForEachTests
{
    /// <summary>
    /// Verifies that child validators run for each collection element.
    /// </summary>
    /// <remarks>
    /// Second order line failures use index notation in property names.
    /// </remarks>
    [Fact]
    public void RuleForEach_WithChildValidator_ShouldValidateEachElement()
    {
        var validator = new OrderCollectionValidator();
        var customer = new Customer
        {
            Orders =
            [
                new OrderLine { ProductName = "Product A", Quantity = 2 },
                new OrderLine { ProductName = "", Quantity = 0 },
            ],
        };

        var result = validator.Validate(customer);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Orders[1].ProductName");
        Assert.Contains(result.Errors, e => e.PropertyName == "Orders[1].Quantity");
    }

    /// <summary>
    /// Verifies that not-empty fails when the collection has no elements.
    /// </summary>
    /// <remarks>
    /// Collection-level rule applies to the Orders property, not individual elements.
    /// </remarks>
    [Fact]
    public void RuleForEach_NotEmpty_ShouldFailWhenCollectionIsEmpty()
    {
        var validator = new OrderCollectionValidator();
        var customer = new Customer();

        var result = validator.Validate(customer);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Orders");
    }

    /// <summary>
    /// Test validator combining per-element and collection-level rules.
    /// </summary>
    /// <remarks>
    /// Applies <see cref="OrderLineValidator"/> to each order and requires a non-empty list.
    /// </remarks>
    private sealed class OrderCollectionValidator : AbstractValidator<Customer>
    {
        /// <summary>
        /// Registers collection element and collection property rules.
        /// </summary>
        /// <remarks>
        /// Uses both <see cref="AbstractValidator{T}.RuleForEach{TElement}"/> and <see cref="AbstractValidator{T}.RuleFor{TProperty}"/>.
        /// </remarks>
        public OrderCollectionValidator()
        {
            RuleForEach(x => x.Orders).SetValidator(new OrderLineValidator());

            RuleFor(x => x.Orders).NotEmpty();
        }
    }
}
