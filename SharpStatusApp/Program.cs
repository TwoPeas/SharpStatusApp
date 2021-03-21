using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SharpStatusApp.Data;

namespace SharpStatusApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            using var userDbCtx = services.GetRequiredService<ApplicationUserDbContext>();
            var migrationsAssembly = userDbCtx.GetService<IMigrationsAssembly>();

            var snapshot = migrationsAssembly.ModelSnapshot.Model;

            var dependencies = userDbCtx.GetService<ProviderConventionSetBuilderDependencies>();
            var relationalDependencies = userDbCtx.GetService<RelationalConventionSetBuilderDependencies>();

            var typeMappingConvention = new TypeMappingConvention(dependencies);
            typeMappingConvention.ProcessModelFinalizing(((IConventionModel)snapshot).Builder, null);

            var relationalModelConvention = new RelationalModelConvention(dependencies, relationalDependencies);
            var sourceModel = relationalModelConvention.ProcessModelFinalized(snapshot);

            var modelDiffer = userDbCtx.GetService<IMigrationsModelDiffer>();
            var hasDifferences = modelDiffer.HasDifferences(
                sourceModel.GetRelationalModel(),
                userDbCtx.Model.GetRelationalModel());

            if (hasDifferences)
            {
                throw new InvalidOperationException("There are differences between the current database model and the most recent migration.");
            }

            userDbCtx.Database.Migrate();

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
