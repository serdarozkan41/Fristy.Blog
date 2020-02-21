using Core.Data;
using Fristy.Blog.Application;
using Fristy.Blog.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Fristy.Blog.Application
{
	public static class DependencyHandler
	{

		private static bool isConfigured = false;

		/// <summary>
		/// Configure the Data context for both User and Transaction context.
		/// </summary>
		/// <param name="userDbOption">User DB Option</param>
		/// <param name="tranDbOption">Transaction DB Option</param>
		public static void ConfigureDataContext(this IServiceCollection services, Action<DbContextOptionsBuilder> userDbOption, Action<DbContextOptionsBuilder> tranDbOption)
		{

			if (isConfigured) return;

			ConfigureRepository(services);

			// Unit of work for Data operations via Core.Data library.
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			// Configure Login DB context. 
			services.AddDbContext<UserContext>(userDbOption);
			services.AddScoped<IdentityDbContext<AppUser>, UserContext>();

			// Configure Transaction DB context.
			if (tranDbOption == null)
				services.AddDbContext<BlogDbContext>();
			else
				services.AddDbContext<BlogDbContext>(tranDbOption);

			services.AddScoped<DbContext, BlogDbContext>();

			// Identity for authentication.
			services.AddIdentity<AppUser, IdentityRole>(opt =>
			{
				opt.User.RequireUniqueEmail = true;
				opt.Password.RequiredLength = 6;
				opt.Password.RequireNonAlphanumeric = false;
			})
			.AddEntityFrameworkStores<UserContext>()
			.AddDefaultTokenProviders();

			isConfigured = true;
		}

		#region Private Methods
		private static void ConfigureRepository(IServiceCollection services)
		{
			Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Assembly.GetExecutingAssembly().Location);

			foreach (var type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository")))
				services.Add(new ServiceDescriptor(type.GetInterfaces().First(i => i.Name.EndsWith(type.Name)), type, ServiceLifetime.Scoped));
		}

		#endregion

	}
}
