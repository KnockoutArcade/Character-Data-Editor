using Raylib_cs;

namespace CharacterDataEditor.Helpers
{
    public static class ShaderHelper
    {
        public static Shader shader;

        public static void InitShader(string vsh, string fsh) => shader = Raylib.LoadShaderFromMemory(vsh, fsh);
        public static void DeInitShader() => Raylib.UnloadShader(shader);
        public static void ShaderStartRender() => Raylib.BeginShaderMode(shader);
        public static void ShaderEndRender() => Raylib.EndShaderMode();

        public static void SetValue<T>(string variableName, T value, ShaderUniformDataType dataType) where T: unmanaged
        {
            var loc = Raylib.GetShaderLocation(shader, variableName);
            Raylib.SetShaderValue<T>(shader, loc, value, dataType);
        }

        public static void SetValue<T>(string variableName, T[] values, ShaderUniformDataType dataType) where T: unmanaged
        {
            var loc = Raylib.GetShaderLocation(shader, variableName);
            Raylib.SetShaderValue(shader, loc, values, dataType);
        }

        public static void SetValueTexture(string variableName, Texture2D texture)
        {
            var loc = Raylib.GetShaderLocation(shader, variableName);
            Raylib.SetShaderValueTexture(shader, loc, texture);
        }
    }
}
