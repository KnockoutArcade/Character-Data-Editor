using CharacterDataEditor.Services;

namespace CharacterDataEditor.Screens
{
    public interface IScreen
    {
        public void Init(dynamic screenData);
        public void Render(IScreenManager screenManager);
    }
}
