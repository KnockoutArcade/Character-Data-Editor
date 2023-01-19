using CharacterDataEditor.Extensions;
using CharacterDataEditor.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;

namespace CharacterDataEditor
{
    public class Startup
    {
        public Startup()
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IRenderUI, RaylibImGui>();
            services.AddSingleton<IRecentFiles, RecentFiles>();

            //register screens
            services.RegisterScreens();
        }
    }
}