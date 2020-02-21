using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Fristy.Blog.Service
{
    public static class DependencyHandler
    {
        private static bool isConfigured = false;

        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {

            if (isConfigured) return;

            Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Assembly.GetExecutingAssembly().Location);

            foreach (var type in assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service")))
                services.Add(new ServiceDescriptor(type.GetInterfaces().First(i => i.Name.EndsWith(type.Name)), type, ServiceLifetime.Scoped));

            isConfigured = true;

        }

    }
}
