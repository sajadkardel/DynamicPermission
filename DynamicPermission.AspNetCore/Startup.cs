using System;
using DynamicPermission.AspNetCore.Configurations.Identity;
using DynamicPermission.AspNetCore.Configurations.Identity.PermissionManager;
using DynamicPermission.AspNetCore.Context;
using DynamicPermission.AspNetCore.Repository.AppSetting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DynamicPermission.AspNetCore
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(new AuthorizeFilter("DynamicPermission"));
            });

            #region DataBase Context

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("AppDbContext"));
            });

            #endregion

            #region Authentication

            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // SignIn settings.
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;

            }).AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders()
                .AddErrorDescriber<PersianIdentityErrorDescriber>();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath="/Account/Login";
                options.LogoutPath="/Account/LogOut";
                options.Cookie.Name = "IdentityCookie";
                //options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
            });

            services.Configure<SecurityStampValidatorOptions>(option =>
            {
                option.ValidationInterval = TimeSpan.FromMinutes(20);
            });

            services.AddAuthorization(option =>
            {
                option.AddPolicy("DynamicPermission", policy => policy.Requirements.Add(new PermissionRequirement()));
            });

            #endregion

            #region Services

            services.AddMemoryCache();
            services.AddHttpContextAccessor();
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();
            services.AddScoped<IAppSettingService, AppSettingService>();
            
            #endregion

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapGet("/", async context =>
                //{
                //    await context.Response.WriteAsync("Hello World!");
                //});

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");


            });
        }
    }
}
