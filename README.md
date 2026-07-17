# DotRValidator

Fluent validation library for .NET inspired by [FluentValidation](https://docs.fluentvalidation.net/). Define strongly-typed validation rules using a fluent, readable, and maintainable API.

## Features

- Fluent API with `RuleFor` and `RuleForEach`
- Built-in validators (NotEmpty, EmailAddress, Length, GreaterThan, etc.)
- Conditional validation with `When` and `Unless`
- Cascade mode (`Continue` / `Stop`)
- Child validators with `SetValidator`
- Async validation with `MustAsync`
- Custom messages and error codes
- Partial property validation
- Dependency injection integration
- Full XML documentation

## Requirements

- .NET 8.0 or later

## Installation

Add a project reference:

```bash
dotnet add reference src/DotRValidator/DotRValidator.csproj
```

Or, after NuGet publication:

```bash
dotnet add package DotRValidator
```

## Quick start

### 1. Define the model

```csharp
public class Customer
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public int Age { get; set; }
}
```

### 2. Create the validator

```csharp
using DotRValidator;

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Please provide a valid email address.");

        RuleFor(x => x.Age)
            .InclusiveBetween(18, 120);
    }
}
```

### 3. Run validation

```csharp
var customer = new Customer { Name = "John", Email = "john@example.com", Age = 30 };
var validator = new CustomerValidator();
var result = validator.Validate(customer);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    }
}
```

## Validation with exceptions

```csharp
validator.ValidateAndThrow(customer);

// or async
await validator.ValidateAndThrowAsync(customer);
```

Throws `ValidationException` containing the full `ValidationResult`.

## Async validation

```csharp
RuleFor(x => x.Email)
    .MustAsync(async (email, cancellation) =>
    {
        await Task.Delay(100, cancellation);
        return !await EmailExistsInDatabase(email);
    })
    .WithMessage("Email is already registered.");
```

## Conditional validation

```csharp
RuleFor(x => x.Discount)
    .NotEqual(0m)
    .When(x => x.HasDiscount);

RuleFor(x => x.MiddleName)
    .NotEmpty()
    .Unless(x => x.UseSingleName);
```

## Child validators

```csharp
public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator());
    }
}
```

## Collection validation

```csharp
RuleForEach(x => x.Orders)
    .SetValidator(new OrderLineValidator());

RuleFor(x => x.Orders)
    .NotEmpty();
```

## Cascade mode

```csharp
RuleFor(x => x.FirstName)
    .Cascade(CascadeMode.Stop)
    .NotEmpty()
    .MinimumLength(2);
```

With `Stop`, execution stops after the first failure in the chain.

## Dependency injection

```csharp
using DotRValidator.DependencyInjection;

builder.Services.AddValidatorsFromAssemblyContaining<CustomerValidator>();

// Resolution
var validator = serviceProvider.GetRequiredService<IValidator<Customer>>();
```

## Built-in validators

| Method | Description |
|--------|-------------|
| `NotNull()` | Value must not be null |
| `Null()` | Value must be null |
| `NotEmpty()` | String/collection must not be empty |
| `Empty()` | String must be empty |
| `Equal(value)` | Value must be equal |
| `NotEqual(value)` | Value must not be equal |
| `Length(min, max)` | String length within range |
| `MinimumLength(n)` | Minimum length |
| `MaximumLength(n)` | Maximum length |
| `GreaterThan(value)` | Greater than |
| `GreaterThanOrEqualTo(value)` | Greater than or equal |
| `LessThan(value)` | Less than |
| `LessThanOrEqualTo(value)` | Less than or equal |
| `InclusiveBetween(from, to)` | Between inclusive bounds |
| `ExclusiveBetween(from, to)` | Between exclusive bounds |
| `EmailAddress()` | Valid email format |
| `Matches(pattern)` | Matches regex |
| `Must(predicate)` | Custom validation |
| `MustAsync(predicate)` | Async custom validation |

## Project structure

```
dotrvalidator/
├── src/DotRValidator/
│   ├── Abstractions/          # Public contracts (IValidator, IRuleBuilder)
│   ├── Enums/                 # CascadeMode, Severity
│   ├── Extensions/            # Built-in validator extensions
│   ├── DependencyInjection/   # DI registration helpers
│   └── Internal/              # Internal implementation
├── tests/DotRValidator.Tests/  # Unit tests (xUnit)
├── docs/                      # Detailed documentation
└── DotRValidator.slnx          # Solution
```

## Run tests

```bash
dotnet test
```

## Additional documentation

- [Getting started](docs/getting-started.md)
- [Built-in validators](docs/built-in-validators.md)
- [Advanced features](docs/advanced-features.md)

## License

See [LICENSE](LICENSE).
