using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using MediatR;
using FluentValidation;
using WestcoastCars.Application.Common.Behaviors;
using WestcoastCars.Application.Interfaces;
using WestcoastCars.Application.Services;

namespace WestcoastCars.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient<IBlocketVehicleImportMapper, BlocketVehicleImportMapper>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
