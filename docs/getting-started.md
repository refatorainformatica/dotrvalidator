# Getting Started

This guide covers the core concepts of **DotRValidator** and shows how to build robust validators for your .NET applications.

## Core concepts

### Validator

A validator is a class that inherits from `AbstractValidator<T>`, where `T` is the type of object being validated. Rules are defined in the constructor using the fluent API.

### Rule (`RuleFor`)

Each rule is associated with a property via a lambda expression:

```csharp
RuleFor(customer => customer.Email).NotEmpty();
```

This is refactor-safe: renaming the property in the model automatically updates the reference in the validator (as long as the expression is used).

### Validation result

`ValidationResult` contains:

| Property | Description |
|----------|-------------|
| `IsValid` | `true` when there are no errors |
| `Errors` | List of `ValidationFailure` |

Each `ValidationFailure` includes:

- `PropertyName` — property that failed
- `ErrorMessage` — descriptive message
- `AttemptedValue` — submitted value
- `ErrorCode` — optional code for i18n or programmatic logic
- `Severity` — `Error`, `Warning`, or `Info`

## Complete example

```csharp
public class RegistrationRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}

public class RegistrationValidator : AbstractValidator<RegistrationRequest>
{
    public RegistrationValidator()
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(2);

        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password)
            .WithMessage("Passwords do not match.");
    }
}
```

## Customizing messages

Use `WithMessage` immediately after the validator:

```csharp
RuleFor(x => x.Email)
    .EmailAddress()
    .WithMessage("{PropertyName} is not a valid email address.");
```

Supported placeholders:

- `{PropertyName}` — property name
- `{PropertyValue}` — submitted value

## Customizing error codes

```csharp
RuleFor(x => x.Email)
    .EmailAddress()
    .WithErrorCode("INVALID_EMAIL");
```

## Partial validation

Validate only specific properties:

```csharp
var result = validator.Validate(customer, "Name", "Email");
```

Useful for multi-step forms or partial API PATCH operations.

## Usage with ASP.NET Core

Register validators in the container:

```csharp
builder.Services.AddValidatorsFromAssemblyContaining<RegistrationValidator>();
```

Validate manually in the endpoint or middleware:

```csharp
app.MapPost("/register", async (RegistrationRequest request, IValidator<RegistrationRequest> validator) =>
{
    var result = await validator.ValidateAsync(request);
    if (!result.IsValid)
        return Results.ValidationProblem(result.Errors.GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()));

    // process registration...
    return Results.Ok();
});
```

## Best practices

1. **One validator per model** — keep cohesive rules in a single class.
2. **Reuse child validators** — nested objects should have their own validators.
3. **Prefer `Must` for business logic** — complex rules stay isolated in private methods.
4. **Use `Cascade(Stop)` for dependent rules** — avoid redundant messages (e.g., NotEmpty before MinimumLength).
5. **Test your validators** — cover valid, invalid, and conditional scenarios.

## Next steps

- [Built-in validators](built-in-validators.md)
- [Advanced features](advanced-features.md)
