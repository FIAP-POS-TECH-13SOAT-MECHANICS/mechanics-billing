using FluentValidation;
using Mechanics.Application.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Mechanics.Infra.CrossCutting.IoC.Extensions;

public static class ValidatorExtensions
{
    public static IServiceCollection AddRequestValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<IAppService>();
        return services;
    }
}
