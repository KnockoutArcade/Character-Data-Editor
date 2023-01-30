using CharacterDataEditor.Extensions;
using CharacterDataEditor.Models;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using static System.Formats.Asn1.AsnWriter;

namespace CharacterDataEditor.Helpers
{
    public static class ImguiDrawingHelper
    {
        public static bool DrawIntInput(string label, ref int value, int minValue = 0, int? maxValue = null)
        {
            ImGui.Columns(2);

            //draw the input label
            ImGui.Text(label.UpperCaseFirstLetter().AddSpacesToCamelCase());

            ImGui.NextColumn();

            //draw the input without a label
            ImGui.InputInt($"##{label}", ref value);

            if (maxValue != null && value > maxValue.Value)
            {
                value = maxValue.Value;
            }

            if (value < minValue)
            {
                value = minValue;
            }

            var itemSelected = ImGui.IsItemFocused();

            ImGui.Columns(1);

            return itemSelected;
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

        public static void DrawPaletteEditor(ref List<RGBModel> palettes, float scale)
        {
            ImGui.BeginTable("paletteEditor", 2, ImGuiTableFlags.NoBordersInBody);

            var tableFlags = ImGuiTableColumnFlags.NoSort;
            ImGui.TableSetupColumn("", tableFlags, 100.0f * scale);
            ImGui.TableSetupColumn("", tableFlags | ImGuiTableColumnFlags.WidthStretch);

            for (int i = 0; i < palettes.Count; i++)
            {
                ImGui.TableNextColumn();
                //for shortness, grab the item we're manipulating
                var rgbPalette = palettes[i];

                //convert the values currently stored in 0-255 back to 0.0-1.0 range
                var selectedColors = new Vector3(
                    rgbPalette.Red / 255f,
                    rgbPalette.Green / 255f,
                    rgbPalette.Blue / 255f);

                //draw the actual control
                ImGui.Text($"Color {i}");

                ImGui.TableNextColumn();

                ImGui.ColorEdit3($"##Color{i}", ref selectedColors);

                //convert the values back from 0.0-1.0 to 0-255
                palettes[i] = new RGBModel(selectedColors.X, selectedColors.Y, selectedColors.Z);
            }

            ImGui.EndTable();
        }

        public static void DrawVerticalSpacing(float scale, float unitsToMove)
        {
            var cursorPos = ImGui.GetCursorPos();
            cursorPos.Y += (unitsToMove * scale);
            ImGui.SetCursorPos(cursorPos);
        }

        public static bool DrawSelectableWithRemove(Action selectAction, string label, bool selected, int id = -1)
        {
            ImGui.BeginTable($"selectable##{label}${id}", 2, ImGuiTableFlags.NoBordersInBody);
            
            var tableFlags = ImGuiTableColumnFlags.NoSort;
            var sizeOfButton = ImGui.CalcTextSize("Remove");

            ImGui.TableSetupColumn("", tableFlags | ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("", tableFlags, sizeOfButton.X * 1.3f);

            ImGui.TableNextColumn();

            if (ImGui.Selectable($"{label}##{id}", selected))
            {
                selectAction();
            }

            ImGui.TableNextColumn();

            if (ImGui.Button("Remove"))
            {
                return true;
            }

            ImGui.EndTable();
            return false;
        }
    }
}
