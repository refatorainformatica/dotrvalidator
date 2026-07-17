# Advanced Features

Documentation for advanced DotRValidator features for complex validation scenarios.

## Conditional validation

### When

Executes the rule only when the condition is true:

```csharp
RuleFor(x => x.Discount)
    .GreaterThan(0m)
    .When(x => x.HasDiscount);
```

### Unless

Executes the rule only when the condition is false:

```csharp
RuleFor(x => x.TaxId)
    .NotEmpty()
    .Unless(x => x.IsIndividual);
```

## Cascade mode

Controls behavior after a failure in the same rule chain:

```csharp
// Default: Continue — runs all rules
RuleFor(x => x.Name).NotEmpty().MinimumLength(2);

// Stop — stops at the first failure
RuleFor(x => x.Name)
    .Cascade(CascadeMode.Stop)
    .NotEmpty()
    .MinimumLength(2);
```

| Mode | Behavior |
|------|----------|
| `Continue` | Runs all rules and accumulates errors |
| `Stop` | Stops after the first failure in the chain |

## Child validators

For nested objects, create separate validators and apply them with `SetValidator`:

```csharp
public class AddressValidator : AbstractValidator<Address>
{
    public AddressValidator()
    {
        RuleFor(x => x.Street).NotEmpty();
        RuleFor(x => x.City).NotEmpty();
    }
}

public class CustomerValidator : AbstractValidator<Customer>
{
    public CustomerValidator()
    {
        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator());
    }
}
```

Child property errors are automatically prefixed:

```
Address.Street: 'Address.Street' must not be empty.
```

## Collection validation (RuleForEach)

Applies rules to each element in a collection:

```csharp
RuleForEach(x => x.OrderLines)
    .SetValidator(new OrderLineValidator());

// Inline rule per element
RuleForEach(x => x.Tags)
    .NotEmpty();
```

Errors are indexed:

```
OrderLines[0].Quantity: 'OrderLines[0].Quantity' must be greater than 0.
OrderLines[2].ProductName: 'OrderLines[2].ProductName' must not be empty.
```

## Partial validation

Validate only specific members using the overload or context:

```csharp
// Convenient overload
validator.Validate(customer, "Name", "Email");

// Via context
var context = new ValidationContext<Customer>(customer)
{
    MemberNames = ["Name"]
};
validator.Validate(context);
```

## Async validation

Use `ValidateAsync` when there are async rules:

```csharp
var result = await validator.ValidateAsync(customer, cancellationToken);
```

Sync and async rules can coexist in the same validator.

## Failure severity

Set the severity of a failure:

```csharp
RuleFor(x => x.MiddleName)
    .NotEmpty()
    .WithSeverity(Severity.Warning);
```

By default, all failures use `Severity.Error`.

## ValidationContext

The validation context provides additional data:

```csharp
public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        RuleFor(x => x.Total)
            .Must((order, total, context) =>
            {
                var maxAllowed = context.RootContextData.TryGetValue("MaxTotal", out var max)
                    ? (decimal)max!
                    : 10000m;
                return total <= maxAllowed;
            });
    }
}

// Usage
var context = new ValidationContext<Order>(order);
context.RootContextData["MaxTotal"] = 5000m;
validator.Validate(context);
```

## Combining results

```csharp
var result1 = nameValidator.Validate(model);
var result2 = addressValidator.Validate(model.Address);
var combined = ValidationResult.Combine(result1, result2);
```

## Validation exceptions

```csharp
try
{
    validator.ValidateAndThrow(customer);
}
catch (ValidationException ex)
{
    foreach (var error in ex.Result.Errors)
    {
        _logger.LogWarning("{Property}: {Message}", error.PropertyName, error.ErrorMessage);
    }
}
```

## Dependency injection

### Automatic assembly registration

```csharp
services.AddValidatorsFromAssemblyContaining<CustomerValidator>();
services.AddValidatorsFromAssembly(typeof(Startup).Assembly);
```

### Manual registration

```csharp
services.AddValidator<CustomerValidator, Customer>();
```

### Resolution

```csharp
public class CustomerService(IValidator<Customer> validator)
{
    public async Task CreateAsync(Customer customer)
    {
        await validator.ValidateAndThrowAsync(customer);
        // persist...
    }
}
```

## Internal architecture

```
AbstractValidator<T>
    └── RuleFor / RuleForEach
            └── RuleBuilder / CollectionRuleBuilder
                    └── RuleComponent (validator + conditions + message)
                            └── PropertyRule / CollectionPropertyRule
                                    └── ValidationResult

Project layout:
├── Abstractions/     IValidator, IRuleBuilder
├── Enums/            CascadeMode, Severity
├── Extensions/       DefaultValidatorExtensions
├── DependencyInjection/
└── Internal/         Rule builders, property rules, resolvers
```

## Comparison with FluentValidation

| Feature | FluentValidation | DotRValidator |
|---------|------------------|--------------|
| RuleFor / RuleForEach | ✅ | ✅ |
| Built-in validators | ✅ | ✅ |
| When / Unless | ✅ | ✅ |
| CascadeMode | ✅ | ✅ |
| SetValidator | ✅ | ✅ |
| MustAsync | ✅ | ✅ |
| DI Extensions | ✅ | ✅ |
| ASP.NET Auto-validation | ✅ | Manual |
| Localization | ✅ | Planned |
| TestHelper | ✅ | Via xUnit |

DotRValidator covers the most common fluent validation scenarios. For automatic ASP.NET pipeline integration, validate manually in endpoints or create a custom filter.
