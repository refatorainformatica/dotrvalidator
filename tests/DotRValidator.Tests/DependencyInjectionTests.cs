using DotRValidator.DependencyInjection;
using DotRValidator.Tests.Validators;
using Microsoft.Extensions.DependencyInjection;

namespace DotRValidator.Tests;

/// <summary>
/// Tests for dependency injection registration extensions.
/// </summary>
/// <remarks>
/// Verifies assembly scanning and explicit validator registration.
/// </remarks>
public class DependencyInjectionTests
{
    /// <summary>
    /// Verifies that assembly scanning registers concrete validators.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="ServiceCollectionExtensions.AddValidatorsFromAssemblyContaining{T}"/>.
    /// </remarks>
    [Fact]
    public void AddValidatorsFromAssemblyContaining_ShouldRegisterValidators()
    {
        var services = new ServiceCollection();
        services.AddValidatorsFromAssemblyContaining<CustomerValidator>();

        var provider = services.BuildServiceProvider();
        var validators = provider.GetServices<IValidator<Models.Customer>>().ToList();

        Assert.Contains(validators, v => v is CustomerValidator);
    }

    /// <summary>
    /// Verifies that explicit registration resolves the configured validator.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="ServiceCollectionExtensions.AddValidator{TValidator, TModel}"/>.
    /// </remarks>
    [Fact]
    public void AddValidator_ShouldRegisterSpecificValidator()
    {
        var services = new ServiceCollection();
        services.AddValidator<RegistrationValidator, Models.RegistrationRequest>();

        var provider = services.BuildServiceProvider();
        var validator = provider.GetService<IValidator<Models.RegistrationRequest>>();

        Assert.NotNull(validator);
        Assert.IsType<RegistrationValidator>(validator);
    }
}
