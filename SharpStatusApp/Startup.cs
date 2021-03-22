using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Google.Cloud.SecretManager.V1;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SharpStatusApp.Data;

namespace SharpStatusApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationUserDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"), 
                npgsqlOptionsAction => npgsqlOptionsAction.MigrationsHistoryTable("__EFMigrationsHistory", "Organizations")));

            services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationUserDbContext>()
                .AddDefaultTokenProviders();

            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapGet("/secrets", async context =>
                {
                    var secret = new AccessSecretVersionSample().AccessSecretVersion();
                    await context.Response.WriteAsync(secret);
                });
            });
        }
    }

    public class AccessSecretVersionSample
    {
        public String AccessSecretVersion(
          string projectId = "twopeas", string secretId = "perfect-day", string secretVersionId = "1")
        {
            // Create the client.
            SecretManagerServiceClient client = SecretManagerServiceClient.Create();

            // Build the resource name.
            SecretVersionName secretVersionName = new SecretVersionName(projectId, secretId, secretVersionId);

            // Call the API.
            AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);

            // Convert the payload to a string. Payloads are bytes by default.
            String payload = result.Payload.Data.ToStringUtf8();
            return payload;
        }
    }

}
