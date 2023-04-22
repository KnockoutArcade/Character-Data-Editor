using CharacterDataEditor.Screens;
using Microsoft.Extensions.Logging;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Services
{
    public interface IScreenManager
    {
        public IScreen CurrentScreen { get; }
        public float ScreenScale { get; set; }
        public bool ExitWindow { get; set; }
        public Color BackgroundColor { get; set; }
        public void NavigateTo(string screenName);
        public void NavigateTo(Type screenType);
        public void NavigateTo(string screenName, dynamic screenData);
        public void NavigateTo(Type screenType, dynamic screenData);
    }

    public class ScreenManager : IScreenManager
    {
        private readonly List<IScreen> _screens;
        private readonly ILogger<IScreenManager> _logger;

        public ScreenManager(IEnumerable<IScreen> screens, ILogger<IScreenManager> logger)
        {
            _screens = screens.ToList();
            _logger = logger;
            ExitWindow = false;
        }

        private IScreen _currentScreen;
        public IScreen CurrentScreen { get { return _currentScreen; } }
        public float ScreenScale { get; set; }
        public bool ExitWindow { get; set; }
        public Color BackgroundColor { get; set; }

        public void NavigateTo(string screenName)
        {
            NavigateTo(screenName, null);
        }

        public void NavigateTo(Type screenType)
        {
            NavigateTo(screenType, null);
        }

        public void NavigateTo(string screenName, dynamic screenData)
        {
            var screenType = _screens.Where(x => x.GetType().Name == screenName).FirstOrDefault();

            if (screenType == null)
            {
                _logger.LogCritical($"Screen not found: {screenName}");
                return;
            }

            NavigateTo(screenType.GetType(), screenData);
        }

        public void NavigateTo(Type screenType, dynamic screenData)
        {
            _logger.LogInformation($"Navigating to: {screenType.Name}");

            var screen = _screens.Where(x => x.GetType() == screenType).FirstOrDefault();
            _currentScreen = screen;
            _currentScreen.Init(screenData);
        }
    }
}
