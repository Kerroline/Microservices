using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MSAuth.DAL;
using MSAuth.Helpers;
using MSAuth.Models;
using MSAuth.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroservicesAuth
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddControllers().
                AddJsonOptions(x => x.JsonSerializerOptions.IgnoreNullValues = true);
               // .AddNewtonsoftJson(options =>
              // options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
           //);

            //services.AddDbContext<MSAuthContext>(options => options.UseNpgsql(_configuration.GetConnectionString("AuthUserPGDBlocal")));
            services.AddDbContext<MSAuthContext>(options => options.UseNpgsql(_configuration.GetConnectionString("AuthUserPGDBdocker")));

            services.AddDefaultIdentity<CustomUserModel>(options => options.SignIn.RequireConfirmedAccount = true).AddRoles<IdentityRole>()
              .AddEntityFrameworkStores<MSAuthContext>();
            //services.AddIdentity


            var appSettingsSection = _configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            var appSettings = appSettingsSection.Get<AppSettings>();

            var signingKey = new SigningSymmetricKey(appSettings.SecretKey);
            services.AddSingleton<IJwtSigningEncodingKey>(signingKey);

            

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            { 
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey.GetKey(),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJWTService, JWTService>();
            services.AddScoped<IUserService, UserService>();

        }

        private async Task CreateRoles(RoleManager<IdentityRole> RoleManager, UserManager<CustomUserModel> UserManager)
        {
            string[] roleList = { "Admin", "User"};

            IdentityResult roleResult;

            foreach (var roleName in roleList)
            {
                var roleExist = await RoleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    //create the roles and seed them to the DB
                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            //Find superuser in DB
            var _superuser = await UserManager.FindByEmailAsync("Admin@Admin");

            if (_superuser == null)
            {
                var poweruser = new CustomUserModel
                {

                    UserName = "Admin",
                    Email = "Admin@Admin",
                    EmailConfirmed = true,
                };
                string userPWD = "_Admin123";

                var createPowerUser = await UserManager.CreateAsync(poweruser, userPWD);
                if (createPowerUser.Succeeded)
                {

                    await UserManager.AddToRoleAsync(poweruser, "Admin");

                }
            }
            //Find testuser in DB
            var _testuser = await UserManager.FindByEmailAsync("Testuser@Testuser");

            if (_testuser == null)
            {
                var testuser = new CustomUserModel
                {

                    UserName = "Testuser",
                    Email = "Testuser@Testuser",
                    EmailConfirmed = true,
                };
                string userPWD = "_Testuser123";

                var createTestUser = await UserManager.CreateAsync(testuser, userPWD);
                if (createTestUser.Succeeded)
                {

                    await UserManager.AddToRoleAsync(testuser, "User");
                }
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            RoleManager<IdentityRole> roleManager,
            UserManager<CustomUserModel> userManager,
            MSAuthContext authContext
            )
        {

            //authContext.Database.Migrate();

            CreateRoles(roleManager, userManager).Wait();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .SetIsOriginAllowed(origin => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());


            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(x => x.MapControllers());
        }
    }
}
