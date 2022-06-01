using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetCore_Model_View_Cortol_Created.Models;
using NetCore_Model_View_Cortol_Created.Security;

namespace NetCore_Model_View_Cortol_Created
{
    public class Startup
    {
        private IConfiguration config;

        public Startup(IConfiguration config)
        {
            this.config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages().AddRazorRuntimeCompilation();
            services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer(
                config.GetConnectionString("EmployeeBDConnection")));

            services.AddIdentity</*IdentityUser*/ ExtendIdentityUser, IdentityRole>(options => {
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequiredLength = 10;

                options.SignIn.RequireConfirmedEmail = true; // to login u have to confirm ur email
            }) // adding service with configuring Password property
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders(); // to make it possible to generate tokens for user regestration email confirmation

            //claim based authorization and role based authorization
            services.AddAuthorization(options => {
                // adding policy DeleteRolePolicy that requires claim of Delete Role
                options.AddPolicy("DeleteRolePolicy", policy => policy.RequireClaim("Delete Role","true"));
                // adding policy EditRolePolicy that requires claim of Edit Role
                options.AddPolicy("EditRolePolicy", policy => policy.RequireClaim("Edit Role" , "true"));
                // we cant make Authorize(Role= "Admin") in this way too - RequireRole not RequireClaim
                options.AddPolicy("AdminRolePolicy", policy => policy.RequireRole("Admin"));

                // we can use a custom requirement and handler 
                options.AddPolicy("EditRolePolicyUsingCustomRequirement",
                    policy => policy.AddRequirements(new ManageAdminRolesAndClaimsRequirement()));

            });

            // we register the handler of the ManageAdminRolesAndClaimsRequirement requiremet
            services.AddSingleton<IAuthorizationHandler, CanEditOnlyOtherAdminRolesAndClaimsHandler>();

            // adding external login providers like google, facebook, etc
            services.AddAuthentication()
                .AddFacebook(options => 
                {
                    options.ClientId = "3781257111908722";
                    options.ClientSecret = "bbc958df6fcb0d6876f158babe9e5a09";
                })
                .AddGoogle(options =>
                {
                    options.ClientId = "740761994086-3r7pgr6mm2hal02raqt710vi4aprcacc.apps.googleusercontent.com";
                    options.ClientSecret = "LJ5NiGKB64hnG_2HicuiuGfp";
                });

            // to change AccessDenied default route to a custom route

            services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = new PathString("/Administration/AccessDenied");
            });

            // use the service of Mvc
            services.AddMvc(options => {
                options.EnableEndpointRouting = false;
                //options =>
                //{
                //    var policy = new AuthorizationPolicyBuilder() // these lines of code make [Authorize] attribute for all Controllers in our application 
                //                        .RequireAuthenticatedUser().Build();
                //    options.Filters.Add(new AuthorizeFilter(policy));
                //}

                }).AddXmlSerializerFormatters(); // addxmlserializer to make it possible to read xml format like objects

            // to add the service of IEmployeeRepository that we created
            // we want each time we use the IEmployeeRepository, make an instance of MockEmploee class thats implements IEmployeeRepository
            // services.AddSingleton<IEmployeeRepository, MockEmployeeRepository>();
            services.AddScoped<IEmployeeRepository,SQLEmployeeRepository>(); // to deal with sql server
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
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }

           
            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=Home}/{action=index}/{id?}");
            });

            //app.UseMvc(); // we use attribute routing now inside the controller 

        }
    }
}
