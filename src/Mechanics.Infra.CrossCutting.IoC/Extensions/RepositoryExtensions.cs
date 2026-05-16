using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Mechanics.Infra.Data;
using Mechanics.Infra.Data.Options;
using Mechanics.Infra.Data.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mechanics.Infra.CrossCutting.IoC.Extensions;

public static class RepositoryExtensions
{
    public static IServiceCollection AddDataRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IAmazonDynamoDB>(_ =>
        {
            var options = configuration.GetSection("AwsCredentials").Get<AwsCredentialsOptions>()!;

            if (options.UseLocalstack)
                return new AmazonDynamoDBClient(
                    new BasicAWSCredentials("local", "empty-key"),
                    new AmazonDynamoDBConfig
                    {
                        RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region),
                        ServiceURL = options.LocalstackUrl,
                    });

            return new AmazonDynamoDBClient(
                new SessionAWSCredentials(options.AccessKey, options.SecretAccessKey, options.SessionToken),
                new AmazonDynamoDBConfig { RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region) });
        });

        services.AddSingleton<IDynamoDBContext, DynamoDBContext>();
        services.Configure<TableNames>(configuration.GetSection(nameof(TableNames)));

        services.AddRepositories();

        return services;
    }

    public static IServiceCollection AddDataRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>();
        services.AddSingleton<IDynamoDBContext, DynamoDBContext>();

        services.Configure<TableNames>(options =>
        {
            options.Payments = Environment.GetEnvironmentVariable("TableNames__Payments")!;
            options.Budgets = Environment.GetEnvironmentVariable("TableNames__Budgets")!;
        });

        services.AddRepositories();

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        var repositoryTypes = typeof(IRepository).Assembly.GetTypes()
            .Where(type => type is { IsInterface: false, IsAbstract: false } && type.GetInterfaces().Any(IsRepositoryInterface));

        foreach (var repositoryType in repositoryTypes)
        {
            var repositoryInterface = repositoryType.GetInterfaces().First(IsRepositoryInterface);
            services.AddScoped(repositoryInterface, repositoryType);
        }

        return services;

        static bool IsRepositoryInterface(Type type) => type != typeof(IRepository) && typeof(IRepository).IsAssignableFrom(type);
    }
}
