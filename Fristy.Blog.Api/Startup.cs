using Fristy.Blog.Application;
using Fristy.Blog.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Fristy.Blog.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddOptions();

            ConfigureAuthentication(services);

            services.ConfigureDataContext(
                user => user.UseSqlServer(Configuration.GetConnectionString("FristyLoginConnection"), b => b.MigrationsAssembly("Fristy.Blog.Api")),
                blog => blog.UseSqlServer(Configuration.GetConnectionString("FristyBlogDbConnection"), b => b.MigrationsAssembly("Fristy.Blog.Api")));
            services.ConfigureServices(Configuration); 
            services.AddOpenApiDocument(document => { document.DocumentName = "Open Api"; });
            services.AddLogging(l => l.AddEventSourceLogger());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHsts();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors(builder =>
            {
                builder.AllowAnyHeader();
                builder.AllowCredentials();

                // For any origin access.
                //builder.AllowAnyOrigin(); 

                // Enable request for specific origin (ULR address) based on config file.
                builder.WithOrigins(Configuration.GetSection("AllowdOrigins").Get<string[]>());

                // Enable for specific type (GET,POST) based on config file.
                builder.WithMethods(Configuration.GetSection("AlloudMethods").Get<string[]>());

            });

            app.UseAuthentication();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseOpenApi(); // serve documents
            app.UseSwaggerUi3(s => { s.WithCredentials = true; }); // serve Swagger UI
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            // JWT token authentication.
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(op =>
            {
                op.RequireHttpsMetadata = false;
                op.SaveToken = true;
                op.IncludeErrorDetails = true;
                op.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    //ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidIssuers = Configuration.GetSection("Jwt:Issueres").Get<string[]>(),
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                    ClockSkew = System.TimeSpan.Zero // Remove delay of token when expire                
                };
            });

            //Authorization [policy based]
            services.AddAuthorization(auth =>
            {
                auth.AddPolicy(Constants.AdminAccess, p => { p.RequireRole(Role.Admin.ToString()); p.RequireAuthenticatedUser(); });
                auth.AddPolicy(Constants.AddEditDeleteAccess, p => { p.RequireRole(Role.Admin.ToString(), Role.AddEditDelete.ToString()); p.RequireAuthenticatedUser(); });
                auth.AddPolicy(Constants.AddEditAccess, p => { p.RequireRole(Role.AddEdit.ToString(), Role.Admin.ToString(), Role.AddEditDelete.ToString()); p.RequireAuthenticatedUser(); });
                auth.AddPolicy(Constants.ReadOnlyAccess, p => { p.RequireAuthenticatedUser(); });
            });
        }
    }
}
