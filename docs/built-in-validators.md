# Built-in Validators

Complete reference for validators available in the `DefaultValidatorExtensions` class.

## Null validation

### NotNull

Verifies the value is not `null`.

```csharp
RuleFor(x => x.Address).NotNull();
```

### Null

Verifies the value is `null`.

```csharp
RuleFor(x => x.MiddleName).Null();
```

## String validation

### NotEmpty

Verifies the string is not `null`, empty, or whitespace.

```csharp
RuleFor(x => x.Name).NotEmpty();
```

Also available for collections:

```csharp
RuleFor(x => x.Items).NotEmpty();
```

### Empty

Verifies the string is `null`, empty, or whitespace.

```csharp
RuleFor(x => x.Nickname).Empty();
```

### Length

```csharp
// Range
RuleFor(x => x.Code).Length(3, 10);

// Exact length
RuleFor(x => x.ZipCode).Length(8);
```

### MinimumLength / MaximumLength

```csharp
RuleFor(x => x.Password).MinimumLength(8);
RuleFor(x => x.Description).MaximumLength(500);
```

### EmailAddress

Validates basic email format.

```csharp
RuleFor(x => x.Email).EmailAddress();
```

### Matches

Validates against a regular expression.

```csharp
RuleFor(x => x.Postcode)
    .Matches(@"^\d{5}-?\d{3}$")
    .WithMessage("Invalid postal code.");
```

## Value comparison

### Equal / NotEqual

```csharp
RuleFor(x => x.Status).Equal("Active");
RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);
RuleFor(x => x.Discount).NotEqual(0m);
```

## Numeric comparison

Requires `TProperty : IComparable<TProperty>, IComparable`.

```csharp
RuleFor(x => x.Age).GreaterThan(0);
RuleFor(x => x.Age).GreaterThanOrEqualTo(18);
RuleFor(x => x.Stock).LessThan(1000);
RuleFor(x => x.Rating).LessThanOrEqualTo(5);
```

### InclusiveBetween / ExclusiveBetween

```csharp
RuleFor(x => x.Age).InclusiveBetween(18, 65);
RuleFor(x => x.Score).ExclusiveBetween(0, 100);
```

## Custom validation

### Must

```csharp
RuleFor(x => x.Name)
    .Must(name => name.All(char.IsLetter))
    .WithMessage("{PropertyName} must contain letters only.");

// With access to the full instance
RuleFor(x => x.EndDate)
    .Must((model, endDate) => endDate > model.StartDate)
    .WithMessage("End date must be after the start date.");
```

### MustAsync

```csharp
RuleFor(x => x.Username)
    .MustAsync(async (username, cancellation) =>
    {
        return !await _repository.ExistsAsync(username, cancellation);
    })
    .WithMessage("Username already exists.");
```

## Chaining

All validators return `IRuleBuilderOptions<T, TProperty>`, allowing chaining:

```csharp
RuleFor(x => x.Email)
    .NotEmpty()
    .EmailAddress()
    .MaximumLength(254)
    .WithMessage("Invalid email address.");
```

## Default messages

Each validator has a default English message. Examples:

| Validator | Default message |
|-----------|-----------------|
| NotNull | `'{PropertyName}' must not be null.` |
| NotEmpty | `'{PropertyName}' must not be empty.` |
| EmailAddress | `'{PropertyName}' is not a valid email address.` |
| Length | `'{PropertyName}' must be between {min} and {max} characters.` |
| GreaterThan | `'{PropertyName}' must be greater than {value}.` |

Customize with `.WithMessage("...")` after any validator.
