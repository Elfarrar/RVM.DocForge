using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RVM.DocForge.Domain.Interfaces;
using RVM.DocForge.Infrastructure.Data;
using RVM.DocForge.Infrastructure.Repositories;

namespace RVM.DocForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DocForgeDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IDocumentationProjectRepository, DocumentationProjectRepository>();
        services.AddScoped<IProjectSnapshotRepository, ProjectSnapshotRepository>();
        services.AddScoped<IGeneratedDocumentRepository, GeneratedDocumentRepository>();

        return services;
    }
}
