using System;
using System.Security.Claims;
using System.Threading.Tasks;
using IEvangelist.VideoChat.Abstractions;
using IEvangelist.VideoChat.Hubs;
using IEvangelist.VideoChat.Options;
using IEvangelist.VideoChat.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace IEvangelist.VideoChat
{
    public class Startup
    {
        readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options => 
                                 options.AddPolicy("DefaultPolicy", 
                                                   builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                    .AddCookie(ConfigureCookieOptions)
                    .AddMicrosoftAccount(ConfigureMicrosoftAccountOptions)
                    .AddTwitter(ConfigureTwitterAccountOptions);

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy =
                    new AuthorizationPolicyBuilder(CookieAuthenticationDefaults.AuthenticationScheme)
                       .RequireAuthenticatedUser()
                       .Build();
            });

            services.AddMvc(options =>
                     {
                         var policy =
                             new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
                         options.Filters.Add(new AuthorizeFilter(policy));
                     })
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.Configure<TwilioSettings>(_configuration.GetSection(nameof(TwilioSettings)))
                    .AddTransient<IVideoService, VideoService>()
                    .AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });

            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info { Title = "IEvangelist.VideoChat", Version = "v1" }); });

            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors("DefaultPolicy");
            app.UseSwagger()
               .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IEvangelist.VideoChat API v1"))
               .UseHttpsRedirection()
               .UseAuthentication()
               .UseStaticFiles()
               .UseSpaStaticFiles();

            app.UseSignalR(routes => routes.MapHub<NotificationHub>("/notificationHub"))
               .UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller}/{action=Index}/{id?}");
                })
               .UseSpa(spa =>
                {
                    // To learn more about options for serving an Angular SPA from ASP.NET Core,
                    // see https://go.microsoft.com/fwlink/?linkid=864501
                    spa.Options.SourcePath = "ClientApp";
                    if (env.IsDevelopment())
                    {
                        spa.UseAngularCliServer(npmScript: "start");
                    }
                });
        }

        static Action<CookieAuthenticationOptions> ConfigureCookieOptions => options =>
        {
            options.LoginPath =
                options.LogoutPath =
                    options.AccessDeniedPath = new PathString("/login");
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.SlidingExpiration = true;
        };

        Action<MicrosoftAccountOptions> ConfigureMicrosoftAccountOptions => options =>
        {
            options.ClientId = _configuration["Authentication:Microsoft:ApplicationId"];
            options.ClientSecret = _configuration["Authentication:Microsoft:Password"];
            options.Scope.Add("https://login.microsoftonline.com/common/oauth2/v2.0/authorize");
            options.SaveTokens = true;
            options.Events.OnCreatingTicket = async context =>
            {
                context.Identity.AddClaim(
                    new Claim("image", context.User.GetValue("image").SelectToken("url").ToString()));
                await Task.CompletedTask;
            };
        };

        Action<TwitterOptions> ConfigureTwitterAccountOptions => options =>
        {
            options.ConsumerKey = _configuration["Authentication:Twitter:ConsumerKey"];
            options.ConsumerSecret = _configuration["Authentication:Twitter:ConsumerSecret"];
            options.RetrieveUserDetails = true;
            options.ClaimActions.MapJsonKey("urn:twitter:profilepicture", "profile_image_url", ClaimTypes.Uri);
            options.SaveTokens = true;
            options.Events.OnCreatingTicket = async context =>
            {
                if (context.Principal.Identity is ClaimsIdentity identity)
                {
                    identity.AddClaim(
                        new Claim("image", context.User.GetValue("image").SelectToken("url").ToString()));
                }
                await Task.CompletedTask;
            };
        };
    }
}