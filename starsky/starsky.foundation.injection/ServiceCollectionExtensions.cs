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
	    public static void AddClassesWithServiceAttribute(this IServiceCollection serviceCollection, params string[] assemblyFilters)
        {
            var assemblies = GetAssemblies(assemblyFilters);
            serviceCollection.AddClassesWithServiceAttribute(assemblies);
        }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="serviceCollection"></param>
	    /// <param name="assemblies"></param>
        public static void AddClassesWithServiceAttribute(this IServiceCollection serviceCollection, params Assembly[] assemblies)
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

        public static void Add(this IServiceCollection serviceCollection, Type serviceType, Type implementationType, InjectionLifetime ioCLifetime)
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
        
        public static void AddTypesImplementing<T>(this IServiceCollection servicecollection, InjectionLifetime ioCLifetime, params string[] assemblies)
        {
            servicecollection.AddTypesImplementing<T>(ioCLifetime, GetAssemblies(assemblies));
        }

        public static void AddTypesImplementing<T>(this IServiceCollection serviceCollection, InjectionLifetime ioCLifetime, params Assembly[] assemblies)
        {
            var types = GetTypesImplementing(typeof(T), assemblies);
            serviceCollection.Add(ioCLifetime, types.ToArray());
        }

        private static Assembly[] GetAssemblies(IEnumerable<string> assemblyFilters)
        {
            var assemblies = new List<Assembly>();
            foreach (var assemblyFilter in assemblyFilters)
            {
                assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(assembly => 
	                IsWildcardMatch(assembly.GetName().Name, assemblyFilter)).ToArray());
            }

            return GetReferencedAssemblies(assemblies, assemblyFilters);
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


        public static Type[] GetTypesImplementing<T>(params Assembly[] assemblies)
		{
			if (assemblies == null || assemblies.Length == 0)
			{
				return new Type[0];
			}

			var targetType = typeof(T);

			return assemblies
				.Where(assembly => !assembly.IsDynamic)
				.SelectMany(GetExportedTypes)
				.Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition && targetType.IsAssignableFrom(type))
				.ToArray();
		}

        private static IEnumerable<Type> GetTypesImplementing(Type implementsType, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                return new Type[0];
            }

            var targetType = implementsType;

            return assemblies
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(GetExportedTypes)
                .Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition && targetType.IsAssignableFrom(type))
                .ToArray();
        }

        private static IEnumerable<Type> GetTypesImplementingGenericInterface(Type implementsType, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
            {
                return new Type[0];
            }
            
            return assemblies
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(GetExportedTypes)
                .Where(x => !x.IsInterface && !x.IsAbstract && x.GetInterfaces().Any(i => i.IsGenericType
                                                                                          && i.GetGenericTypeDefinition() == implementsType))
                .ToArray();
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
