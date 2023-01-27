using CharacterDataEditor.Extensions;
using CharacterDataEditor.Models;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

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

        public static void DrawStringInput(string label, ref string value, uint maxLength = 255)
        {
            ImGui.Columns(2);
            ImGui.Text(label.UpperCaseFirstLetter().AddSpacesToCamelCase());

            ImGui.NextColumn();
            ImGui.InputText($"##{label}", ref value, maxLength);

            ImGui.Columns(1);
        }

        public static void DrawPaletteEditor(ref List<RGBModel> palettes)
        {
            for (int i = 0; i < palettes.Count; i++)
            {
                //for shortness, grab the item we're manipulating
                var rgbPalette = palettes[i];

                //convert the values currently stored in 0-255 back to 0.0-1.0 range
                var selectedColors = new Vector3(
                    rgbPalette.Red / 255f,
                    rgbPalette.Green / 255f,
                    rgbPalette.Blue / 255f);

                //draw the actual control
                ImGui.Text($"Color {i}");
                ImGui.SameLine();
                ImGui.ColorEdit3($"##Color{i}", ref selectedColors);

                //convert the values back from 0.0-1.0 to 0-255
                palettes[i] = new RGBModel(selectedColors.X, selectedColors.Y, selectedColors.Z);
            }
        }
    }
}
