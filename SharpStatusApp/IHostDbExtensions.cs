using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using SharpStatusApp.Data;

namespace SharpStatusApp
{
    internal static class IHostDbExtensions
    {
        internal static IHost MigrateDatabases(this IHost host) =>
            host.MigrateOrganizationDb();

        internal static IHost MigrateOrganizationDb(this IHost host) => 
            host.MigrateDbContext<TenantsDbContext>();

        internal static IHost MigrateDbContext<TContext>(this IHost host) where TContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var logger = services.GetRequiredService<ILogger<TContext>>();
            using var dbCtx = services.GetRequiredService<TContext>();

            ValidateModelSnapshot(logger, dbCtx);

            RunMigrations(logger, dbCtx);

            return host;

            static void ValidateModelSnapshot(ILogger<TContext> logger, TContext dbCtx)
            {
                logger.LogInformation("Validating model snapshot for {DbContext}", typeof(TContext).Name);

                var migrationsAssembly = dbCtx.GetService<IMigrationsAssembly>();
                var snapshot = migrationsAssembly.ModelSnapshot.Model;

                var dependencies = dbCtx.GetService<ProviderConventionSetBuilderDependencies>();
                var relationalDependencies = dbCtx.GetService<RelationalConventionSetBuilderDependencies>();

                var typeMappingConvention = new TypeMappingConvention(dependencies);
                typeMappingConvention.ProcessModelFinalizing(((IConventionModel)snapshot).Builder, null);

                var relationalModelConvention = new RelationalModelConvention(dependencies, relationalDependencies);
                var sourceModel = relationalModelConvention.ProcessModelFinalized(snapshot);

                var modelDiffer = dbCtx.GetService<IMigrationsModelDiffer>();
                var hasDifferences = modelDiffer.HasDifferences(
                    sourceModel.GetRelationalModel(),
                    dbCtx.Model.GetRelationalModel());

                if (hasDifferences)
                {
                    logger.LogCritical("There are differences between the current database model for {DbContext} and the most recent migration.", typeof(TContext).Name);
                    throw new InvalidOperationException($"There are differences between the current database model for {typeof(TContext).Name} and the most recent migration.");
                }
            }

            static void RunMigrations(ILogger<TContext> logger, TContext dbCtx)
            {
                logger.LogInformation("Running database migrations for context {DbContext}", typeof(TContext).Name);
                var migrations = dbCtx.Database.GetPendingMigrations().ToList();

                if (migrations.Count == 0)
                {
                    logger.LogInformation("No pending migrations");
                }
                else
                {
                    var migrator = dbCtx.GetService<IMigrator>();

                    foreach (var migration in migrations)
                    {
                        logger.LogInformation("Applying migration {Migration}", migration);

                        migrator.Migrate(migration);
                    }
                }

                logger.LogInformation("Database migration complete");
            }
        }
    }
}
