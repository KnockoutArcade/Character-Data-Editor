using CharacterDataEditor.Extensions;
using ImGuiNET;

namespace CharacterDataEditor.Helpers
{
    public static class ImguiDrawingHelper
    {
        public static void DrawIntInput(string label, ref int value)
        {
            ImGui.Columns(2);

            //draw the input label
            ImGui.Text(label.UpperCaseFirstLetter().AddSpacesToCamelCase());

            ImGui.NextColumn();

            //draw the input without a label
            ImGui.InputInt($"##{label}", ref value);

            ImGui.Columns(1);
        }

        public static void DrawBoolInput(string label, ref bool value)
        {
            ImGui.Columns(2);
            ImGui.Text(label.UpperCaseFirstLetter().AddSpacesToCamelCase());

            ImGui.NextColumn();
            ImGui.Checkbox($"##{label}", ref value);

            ImGui.Columns(1);
        }

        public static void DrawComboInput(string label, string[] comboItems, ref int value)
        {
            ImGui.Columns(2);
            ImGui.Text(label.UpperCaseFirstLetter().AddSpacesToCamelCase());

            ImGui.NextColumn();
            ImGui.Combo($"##{label}", ref value, comboItems, comboItems.Length);

            ImGui.Columns(1);
        }
    }
}
