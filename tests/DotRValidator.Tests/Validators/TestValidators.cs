using DotRValidator.Tests.Models;

namespace DotRValidator.Tests.Validators;

/// <summary>
/// Validator for <see cref="Customer"/> used across integration-style tests.
/// </summary>
/// <remarks>
/// Demonstrates built-in rules, conditional validation, and nested validators.
/// </remarks>
public class CustomerValidator : AbstractValidator<Customer>
{
    /// <summary>
    /// Configures validation rules for customer properties.
    /// </summary>
    /// <remarks>
    /// Address validation runs only when the address is not null.
    /// </remarks>
    public CustomerValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Please provide a valid email address.");

        RuleFor(x => x.Age).InclusiveBetween(18, 120);

        RuleFor(x => x.Discount).NotEqual(0m).When(x => x.HasDiscount);

        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator())
            .When(x => x.Address is not null);
    }
}

/// <summary>
/// Validator for <see cref="Address"/> nested objects.
/// </summary>
/// <remarks>
/// Applies Brazilian-style postcode format via regex.
/// </remarks>
public class AddressValidator : AbstractValidator<Address>
{
    /// <summary>
    /// Configures validation rules for address fields.
    /// </summary>
    /// <remarks>
    /// Postcode must match five digits, optional hyphen, and three digits.
    /// </remarks>
    public AddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Postcode)
            .NotEmpty()
            .Matches(@"^\d{5}-?\d{3}$")
            .WithMessage("Invalid postal code.");
    }
}

/// <summary>
/// Validator for individual <see cref="OrderLine"/> elements.
/// </summary>
/// <remarks>
/// Used with <see cref="AbstractValidator{T}.RuleForEach{TElement}"/> in collection tests.
/// </remarks>
public class OrderLineValidator : AbstractValidator<OrderLine>
{
    /// <summary>
    /// Configures validation rules for order line fields.
    /// </summary>
    /// <remarks>
    /// Quantity must be strictly positive.
    /// </remarks>
    public OrderLineValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

/// <summary>
/// Validator for <see cref="RegistrationRequest"/> registration flows.
/// </summary>
/// <remarks>
/// First name uses stop cascade to demonstrate early chain termination.
/// </remarks>
public class RegistrationValidator : AbstractValidator<RegistrationRequest>
{
    /// <summary>
    /// Configures validation rules for registration fields.
    /// </summary>
    /// <remarks>
    /// Confirm password must equal the password property.
    /// </remarks>
    public RegistrationValidator()
    {
        RuleFor(x => x.FirstName).Cascade(CascadeMode.Stop).NotEmpty().MinimumLength(2);

        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match.");
    }
}
