using CharacterDataEditor.Services;

namespace CharacterDataEditor.Screens
{
    public interface IScreen
    {
        public void Init(dynamic screenData);
        public void CheckForExit(IScreenManager screenManager);
        public void RenderImGui(IScreenManager screenManager);
        public void RenderAfterImGui(IScreenManager screenManager);
    }
}
