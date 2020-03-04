using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using starskyioc;

namespace starsky.foundation.ioc
{
    public static class ServiceCollectionExtensions
    {
	    public static void AddClassesWithServiceAttribute(this IServiceCollection serviceCollection, params string[] assemblyFilters)
        {
            var assemblies = GetAssemblies(assemblyFilters);
            serviceCollection.AddClassesWithServiceAttribute(assemblies);
        }

        public static void AddClassesWithServiceAttribute(this IServiceCollection serviceCollection, params Assembly[] assemblies)
        {
            var typesWithAttributes = assemblies
                .Where(assembly => !assembly.IsDynamic)
                .SelectMany(GetExportedTypes)
                .Where(type => !type.IsAbstract && !type.IsGenericTypeDefinition)
                .Select(type => new { Lifetime = type.GetCustomAttribute<ServiceAttribute>()?.IoCLifetime, ServiceType = type, ImplementationType = type.GetCustomAttribute<ServiceAttribute>()?.ServiceType })
                .Where(t => t.Lifetime != null);

            foreach (var type in typesWithAttributes)
            {
                if (type.ImplementationType == null)
                    serviceCollection.Add(type.ServiceType, type.Lifetime.Value);
                else
                    serviceCollection.Add(type.ImplementationType, type.ServiceType, type.Lifetime.Value);
            }
        }
        

        public static void Add(this IServiceCollection serviceCollection, IoCLifetime ioCLifetime, params Type[] types)
        {
	        if(types == null) throw new ArgumentNullException(types + nameof(types));

            foreach (var type in types)
            {
                serviceCollection.Add(type, ioCLifetime);
            }
        }

        public static void Add<T>(this IServiceCollection serviceCollection, IoCLifetime ioCLifetime)
        {
            serviceCollection.Add(typeof(T), ioCLifetime);
        }

        public static void Add(this IServiceCollection serviceCollection, Type type, IoCLifetime ioCLifetime)
        {
            switch (ioCLifetime)
            {
                case IoCLifetime.Singleton:
                    serviceCollection.AddSingleton(type);
                    break;
                case IoCLifetime.Transient:
                    serviceCollection.AddTransient(type);
                    break;
                case IoCLifetime.Scoped:
                    serviceCollection.AddScoped(type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ioCLifetime), ioCLifetime, null);
            }
        }

        public static void Add(this IServiceCollection serviceCollection, Type serviceType, Type implementationType, IoCLifetime ioCLifetime)
        {
            switch (ioCLifetime)
            {
                case IoCLifetime.Singleton:
                    serviceCollection.AddSingleton(serviceType, implementationType);
                    break;
                case IoCLifetime.Transient:
                    serviceCollection.AddTransient(serviceType, implementationType);
                    break;
                case IoCLifetime.Scoped:
                    serviceCollection.AddScoped(serviceType, implementationType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ioCLifetime), ioCLifetime, null);
            }
        }
        
        public static void AddTypesImplementing<T>(this IServiceCollection servicecollection, IoCLifetime ioCLifetime, params string[] assemblies)
        {
            servicecollection.AddTypesImplementing<T>(ioCLifetime, GetAssemblies(assemblies));
        }

        public static void AddTypesImplementing<T>(this IServiceCollection serviceCollection, IoCLifetime ioCLifetime, params Assembly[] assemblies)
        {
            var types = GetTypesImplementing(typeof(T), assemblies);
            serviceCollection.Add(ioCLifetime, types.ToArray());
        }

        /// <summary>
        /// Addes types that implement the generic interface
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="ioCLifetime"></param>
        /// <param name="abstractType">The generic type</param>
        /// <param name="assemblies">The assemblies with the types.</param>
        public static void AddTypesImplementingGenericInterface(this IServiceCollection serviceCollection, IoCLifetime ioCLifetime, Type abstractType, params Assembly[] assemblies)
        {
            var registrations =
                GetTypesImplementingGenericInterface(abstractType, assemblies)
                .Select(type => new
                {
                    Interfaces = type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == abstractType).ToList(),
                    Implementation = type
                });
            // register the types
            foreach (var reg in registrations)
            {
                foreach (var abstraction in reg.Interfaces)
                {
                    serviceCollection.Add(abstraction, reg.Implementation, IoCLifetime.Transient);
                }
            }
        }

        private static Assembly[] GetAssemblies(IEnumerable<string> assemblyFilters)
        {
            var assemblies = new List<Assembly>();
            foreach (var assemblyFilter in assemblyFilters)
            {
                assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(assembly => IsWildcardMatch(assembly.GetName().Name, assemblyFilter)).ToArray());
            }
            return assemblies.ToArray();
        }

        private static IEnumerable<Type> GetTypesImplementing(Type implementsType, IEnumerable<Assembly> assemblies, params string[] classFilter)
        {
            var types = GetTypesImplementing(implementsType, assemblies.ToArray());
            if (classFilter != null && classFilter.Any())
            {
                types = types.Where(type => classFilter.Any(filter => IsWildcardMatch(type.FullName, filter)));
            }
            return types;
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
                .Where(x => !x.IsInterface && !x.IsAbstract && x.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == implementsType))
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
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to load types from assembly {0}. {1}", assembly.FullName, ex.Message), ex);
            }
        }

        /// <summary>
        ///     Checks if a string matches a wildcard argument (using regex)
        /// </summary>
        private static bool IsWildcardMatch(string input, string wildcard)
        {
            return input == wildcard || Regex.IsMatch(input, "^" + Regex.Escape(wildcard).Replace("\\*", ".*").Replace("\\?", ".") + "$", RegexOptions.IgnoreCase);
        }

    }
}
