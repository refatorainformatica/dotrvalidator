namespace DotRValidator.Tests.Models;

/// <summary>
/// Sample customer model used in validation tests.
/// </summary>
/// <remarks>
/// Exercises nested objects, collections, and conditional rules.
/// </remarks>
public class Customer
{
    /// <summary>
    /// Customer display name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Customer email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Customer age in years.
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Optional mailing address.
    /// </summary>
    public Address? Address { get; set; }

    /// <summary>
    /// Order lines associated with the customer.
    /// </summary>
    public List<OrderLine> Orders { get; set; } = [];

    /// <summary>
    /// Indicates whether a discount applies.
    /// </summary>
    public bool HasDiscount { get; set; }

    /// <summary>
    /// Discount amount when <see cref="HasDiscount"/> is true.
    /// </summary>
    public decimal Discount { get; set; }
}

/// <summary>
/// Sample postal address model used in nested validation tests.
/// </summary>
/// <remarks>
/// Validated by <see cref="Validators.AddressValidator"/>.
/// </remarks>
public class Address
{
    /// <summary>
    /// Street name and number.
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// City name.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Postal or ZIP code.
    /// </summary>
    public string? Postcode { get; set; }
}

/// <summary>
/// Sample order line model used in collection validation tests.
/// </summary>
/// <remarks>
/// Validated per element via <see cref="Validators.OrderLineValidator"/>.
/// </remarks>
public class OrderLine
{
    /// <summary>
    /// Product name for the line item.
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Ordered quantity.
    /// </summary>
    public int Quantity { get; set; }
}

/// <summary>
/// Sample registration request model used in cascade and cross-field tests.
/// </summary>
/// <remarks>
/// Includes password confirmation for equality rule testing.
/// </remarks>
public class RegistrationRequest
{
    /// <summary>
    /// User first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Registration email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Chosen password.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Password confirmation field.
    /// </summary>
    public string? ConfirmPassword { get; set; }
}
