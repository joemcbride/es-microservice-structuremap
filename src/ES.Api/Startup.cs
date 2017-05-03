﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ES.Api.Identity;
using StructureMap;

namespace ES.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var apiSettings = Configuration.Get<ApiSettings>();
            var tokenSettings = Configuration.Get<TokenSettings>();

            services.AddSingleton<IConfigurationRoot>(Configuration);
            services.AddSingleton(tokenSettings);
            services.AddSingleton<ICertificateLoader, StoreCertificateLoader>();

            if(tokenSettings.Mode == "Local")
            {
                services.AddSingleton<ICertificateLoader, LocalCertificateLoader>();
            }

            services.AddCors(options =>
            {
                options.AddPolicy(
                    "Origin",
                    builder => builder
                        .WithOrigins(apiSettings.AllowedOrigins)
                        .AllowAnyHeader()
                        .WithMethods("POST"));
            });

            services
              .AddAuthorization()
              .AddMvc();

            return configureContainer(services).GetInstance<IServiceProvider>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors("Origin");

            app.AddJwtAuthentication(app.ApplicationServices.GetService<ICertificateLoader>());

            app.UseMvc();
        }

        private IContainer configureContainer(IServiceCollection services)
        {
            var container = new Container(_ =>
            {
                _.Populate(services);
                _.Policies.OnMissingFamily<SettingsPolicy>();
            });
            return container;
        }
    }
}
