using CharacterDataEditor.Screens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Services
{
    public interface IScreenManager
    {
        public IScreen CurrentScreen { get; }
        public float ScreenScale { get; set; }
        public void NavigateTo(string screenName);
        public void NavigateTo(Type screenType);
        public void NavigateTo(string screenName, dynamic screenData);
        public void NavigateTo(Type screenType, dynamic screenData);
    }

    public class ScreenManager : IScreenManager
    {
        private readonly List<IScreen> _screens;
        public ScreenManager(IEnumerable<IScreen> screens)
        {
            _screens = screens.ToList();

            NavigateTo(typeof(MainScreen));
        }

        private IScreen _currentScreen;
        public IScreen CurrentScreen { get { return _currentScreen; } }

        public float ScreenScale { get; set; }

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
            var screenType = _screens.Where(x => x.GetType().Name == screenName).FirstOrDefault().GetType();

            NavigateTo(screenType, screenData);
        }

        public void NavigateTo(Type screenType, dynamic screenData)
        {
            var screen = _screens.Where(x => x.GetType() == screenType).FirstOrDefault();
            _currentScreen = screen;
            _currentScreen.Init(screenData);
        }
    }
}
