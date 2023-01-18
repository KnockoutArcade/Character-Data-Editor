using CharacterDataEditor.Screens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CharacterDataEditor.Services
{
    public interface IScreenManager
    {
        public IScreen CurrentScreen { get; }
        public void NavigateTo(string screenName);
        public void NavigateTo(Type screenType);
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

        public void NavigateTo(string screenName)
        {
            var screenType = _screens.Where(x => x.GetType().Name == screenName).FirstOrDefault().GetType();
            
            NavigateTo(screenType);
        }

        public void NavigateTo(Type screenType)
        {
            var screen = _screens.Where(x => x.GetType() == screenType).FirstOrDefault();
            _currentScreen = screen;
            _currentScreen.Init();
        }
    }
}
