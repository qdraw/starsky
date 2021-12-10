using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace starsky.foundation.injection
{
    public static class ServiceCollectionExtensions
    {
	    public static void AddClassesWithServiceAttribute(this IServiceCollection serviceCollection,
		    params string[] assemblyFilters)
        {
            var assemblies = GetAssemblies(assemblyFilters.ToList());
            serviceCollection.AddClassesWithServiceAttribute(assemblies);
        }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="serviceCollection"></param>
	    /// <param name="assemblies"></param>
        public static void AddClassesWithServiceAttribute(this IServiceCollection serviceCollection,
		    params Assembly[] assemblies)
        {
            var typesWithAttributes = assemblies
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(GetExportedTypes)
                .Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition)
                .Select(type => new { Lifetime = type.GetCustomAttribute<ServiceAttribute>()?.InjectionLifetime, ServiceType = type, 
	                ImplementationType = type.GetCustomAttribute<ServiceAttribute>()?.ServiceType })
                .Where(t => t.Lifetime != null);

            foreach (var type in typesWithAttributes)
            {
                if (type.ImplementationType == null)
                    serviceCollection.Add(type.ServiceType, type.Lifetime.Value);
                else
                    serviceCollection.Add(type.ImplementationType, type.ServiceType, type.Lifetime.Value);
            }
        }
        

        public static void Add(this IServiceCollection serviceCollection, InjectionLifetime ioCLifetime, params Type[] types)
        {
	        if(types == null) throw new ArgumentNullException(types + nameof(types));

            foreach (var type in types)
            {
                serviceCollection.Add(type, ioCLifetime);
            }
        }

        public static void Add<T>(this IServiceCollection serviceCollection, InjectionLifetime ioCLifetime)
        {
            serviceCollection.Add(typeof(T), ioCLifetime);
        }

        public static void Add(this IServiceCollection serviceCollection, Type type, InjectionLifetime ioCLifetime)
        {
            switch (ioCLifetime)
            {
                case InjectionLifetime.Singleton:
                    serviceCollection.AddSingleton(type);
                    break;
                case InjectionLifetime.Transient:
                    serviceCollection.AddTransient(type);
                    break;
                case InjectionLifetime.Scoped:
                    serviceCollection.AddScoped(type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ioCLifetime), ioCLifetime, null);
            }
        }

        public static void Add(this IServiceCollection serviceCollection, Type serviceType, 
	        Type implementationType, InjectionLifetime ioCLifetime)
        {
            switch (ioCLifetime)
            {
                case InjectionLifetime.Singleton:
                    serviceCollection.AddSingleton(serviceType, implementationType);
                    break;
                case InjectionLifetime.Transient:
                    serviceCollection.AddTransient(serviceType, implementationType);
                    break;
                case InjectionLifetime.Scoped:
                    serviceCollection.AddScoped(serviceType, implementationType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ioCLifetime), ioCLifetime, null);
            }
        }
        
        private static Assembly[] GetAssemblies(List<string> assemblyFilters)
        {
            var assemblies = new List<Assembly>();
            foreach (var assemblyFilter in assemblyFilters)
            {
                assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(assembly => 
	                IsWildcardMatch(assembly.GetName().Name, assemblyFilter)).ToArray());
            }

            assemblies = GetEntryAssemblyReferencedAssemblies(assemblies,
		            assemblyFilters);

            return GetReferencedAssemblies(assemblies, assemblyFilters);
        }

        private static List<Assembly> GetEntryAssemblyReferencedAssemblies(List<Assembly> assemblies, IEnumerable<string> assemblyFilters)
        {
	        var assemblyNames = Assembly
		        .GetEntryAssembly()?.GetReferencedAssemblies();
	        if ( assemblyNames == null )
	        {
		        return assemblies;
	        }

	        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
	        foreach ( var assemblyName in assemblyNames )
	        {
		        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
		        foreach ( var assemblyFilter in assemblyFilters )
		        {
			        var isThere = assemblies.Any(p => p.FullName == assemblyName.FullName);
			        if (IsWildcardMatch(assemblyName.Name, assemblyFilter) && !isThere )
			        {
				        assemblies.Add(AppDomain.CurrentDomain.Load(assemblyName));
			        }
		        }
	        }

	        return assemblies.OrderBy(p => p.FullName).ToList();
        }

        /// <summary>
        /// Get the assembly files that are referenced and match the pattern
        /// </summary>
        /// <param name="assemblies">current referenced assemblies</param>
        /// <param name="assemblyFilters">filters that need to be checked</param>
        /// <returns></returns>
        private static Assembly[] GetReferencedAssemblies(List<Assembly> assemblies, IEnumerable<string> assemblyFilters)
        {
	        // assemblies.ToList() to avoid Collection was modified; enumeration operation may not execute
	        foreach (var assemblyFilter in assemblyFilters.ToList())
	        {
		        foreach ( var assembly in assemblies.ToList())
		        {
			        foreach ( var referencedAssembly in assembly.GetReferencedAssemblies() )
			        {
				        if ( IsWildcardMatch(referencedAssembly.Name, assemblyFilter) 
				             && assemblies.All(p => p.FullName != referencedAssembly.FullName) )
				        {
					        assemblies.Add(Assembly.Load(referencedAssembly));
				        }
			        }
		        }
	        }
	        return assemblies.ToArray();
        }
        
        private static IEnumerable<Type> GetExportedTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (NotSupportedException)
            {
                // A type load exception would typically happen on an Anonymously Hosted DynamicMethods
                // Assembly and it would be safe to skip this exception.
                return Type.EmptyTypes;
            }
            catch (FileLoadException)
            {
                // The assembly points to a not found assembly - ignore and continue
                return Type.EmptyTypes;
            }
            catch (ReflectionTypeLoadException ex)
            {
                // Return the types that could be loaded. Types can contain null values.
                return ex.Types.Where(type => type != null);
            }
            catch (Exception ex)
            {
                // Throw a more descriptive message containing the name of the assembly.
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, 
	                "Unable to load types from assembly {0}. {1}", assembly.FullName, ex.Message), ex);
            }
        }

        /// <summary>
        ///     Checks if a string matches a wildcard argument (using regex)
        /// </summary>
        private static bool IsWildcardMatch(string input, string wildcard)
        {
            return input == wildcard || Regex.IsMatch(input, "^" + 
                                                             Regex.Escape(wildcard).Replace("\\*", ".*")
	                                                             .Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
        }

    }
}
