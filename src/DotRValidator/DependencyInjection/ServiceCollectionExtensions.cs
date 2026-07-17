using System.Reflection;

namespace DotRValidator.DependencyInjection;

/// <summary>
/// Extensions for registering validators in the dependency injection container.
/// </summary>
/// <remarks>
/// Scans assemblies for concrete types implementing <see cref="IValidator{T}"/> and registers them by interface.
/// </remarks>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all validators found in the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">Reference type used to locate the assembly.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="lifetime">Registered service lifetime.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Convenience overload for assembly scanning from a marker type in the target project.
    /// </remarks>
    public static IServiceCollection AddValidatorsFromAssemblyContaining<T>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
    {
        return services.AddValidatorsFromAssembly(typeof(T).Assembly, lifetime);
    }

    /// <summary>
    /// Registers all validators found in the specified assembly.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="assembly">Assembly to scan.</param>
    /// <param name="lifetime">Registered service lifetime.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Registers each validator against every <see cref="IValidator{T}"/> interface it implements.
    /// Abstract types and interfaces are excluded.
    /// </remarks>
    public static IServiceCollection AddValidatorsFromAssembly(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
    {
        var validatorTypes = assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && !type.IsInterface)
            .SelectMany(type =>
                type.GetInterfaces()
                    .Where(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)
                    )
                    .Select(i => new { ServiceType = i, ImplementationType = type })
            )
            .ToList();

        foreach (var validator in validatorTypes)
        {
            services.Add(
                new ServiceDescriptor(validator.ServiceType, validator.ImplementationType, lifetime)
            );
        }

        return services;
    }

    /// <summary>
    /// Registers a specific validator in the container.
    /// </summary>
    /// <typeparam name="TValidator">Validator type.</typeparam>
    /// <typeparam name="TModel">Validated model type.</typeparam>
    /// <param name="services">Service collection.</param>
    /// <param name="lifetime">Registered service lifetime.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Maps <see cref="IValidator{TModel}"/> to <typeparamref name="TValidator"/> explicitly when assembly scanning is not used.
    /// </remarks>
    public static IServiceCollection AddValidator<TValidator, TModel>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped
    )
        where TValidator : class, IValidator<TModel>
    {
        services.Add(
            new ServiceDescriptor(typeof(IValidator<TModel>), typeof(TValidator), lifetime)
        );
        return services;
    }
}
