using CharacterDataEditor.Screens;
using CharacterDataEditor.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection RegisterScreens(this IServiceCollection services)
        {
            var screens = GetTypesImplementingInterface(typeof(IScreen));
            screens.ForEach(screen =>
            {
                services.AddScoped(typeof(IScreen), screen);
            });

            services.AddSingleton<IScreenManager, ScreenManager>();

            return services;
        }

        private static List<Type> GetTypesImplementingInterface(Type interfaceType)
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => 
                    interfaceType.IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsGenericTypeDefinition &&
                    !type.IsInterface)
                .ToList();
        }
    }
}
