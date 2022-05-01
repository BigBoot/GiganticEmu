using System;
using System.Text.Json.Serialization;
using FluentEmail.MailKitSmtp;
using GiganticEmu.Shared.Backend;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MimeKit;

namespace GiganticEmu.Web;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var config = new BackendConfiguration();
        Configuration.GetSection(BackendConfiguration.GiganticEmu).Bind(config);

        services.Configure<BackendConfiguration>(Configuration.GetSection(BackendConfiguration.GiganticEmu));

        services.AddApplicationDatabase();

        services.AddIdentity<User, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDatabase>()
            .AddDefaultTokenProviders();

        var fromAddress = MailboxAddress.Parse(config.Email.From);
        services.AddFluentEmail(fromAddress.Address, fromAddress.Name)
            .AddMailKitSender(new SmtpClientOptions
            {
                Server = config.Email.SmtpServer,
                UseSsl = true,
                Port = config.Email.SmtpPort,
                RequiresAuthentication = true,
                User = config.Email.SmtpUsername,
                Password = config.Email.SmtpPassword,
            });

        services.Configure<IdentityOptions>(c =>
        {
            c.User.RequireUniqueEmail = false;
        });

        services.AddControllers()
            .AddJsonOptions(c =>
            {
                c.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web", Version = "v1" });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web v1"));
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            // app.UseHsts();
        }

        app.UseStaticFiles();

        app.UseRouting();

        //app.UseAuthentication(); 
        //app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

    }
}
