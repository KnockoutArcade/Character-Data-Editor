using CharacterDataEditor.Enums;
using CharacterDataEditor.Extensions;
using CharacterDataEditor.Models.CharacterData;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;
using static System.Formats.Asn1.AsnWriter;

namespace CharacterDataEditor.Helpers
{
    public static class ImguiDrawingHelper
    {
        public static bool DrawIntInput(string label, ref int value, int minValue = int.MinValue, int? maxValue = null)
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
            else if (value < minValue)
            {
                value = minValue;
            }

            var itemSelected = ImGui.IsItemFocused();

            ImGui.Columns(1);

            return itemSelected;
        }

        public static void DrawFlagsInput<T>(string label, ref T value, bool drawZeroValue = false) where T : Enum
        {
            var flagValues = Enum.GetValues(typeof(T));
            List<(string key, bool value, int flagValue)> flagValuesList = new List<(string key, bool value, int flagValue)>();

            foreach (T item in flagValues)
            {
                if (!drawZeroValue && (int)(object)item == 0)
                {
                    continue;
                }

                var valueStatus = ((int)(object)value & (int)(object)item) == (int)(object)item;
                flagValuesList.Add((item.ToString().AddSpacesToCamelCase(), valueStatus, (int)(object)item));
            }

            ImGui.Columns(2);

            for(int i = 0; i < flagValuesList.Count; i++)
            {
                var item = flagValuesList[i];

                bool status = item.value;
                ImGui.Text($"{item.key}");

                ImGui.NextColumn();

                ImGui.Checkbox($"##{i}#{label}", ref status);

                ImGui.NextColumn();

                item.value = status;

                flagValuesList[i] = item;
            }

            int refOutValue = 0;

            foreach (var item in flagValuesList)
            {
                if (item.value)
                {
                    refOutValue += item.flagValue;
                }
            }

            value = (T)(object)refOutValue;

            ImGui.Columns(1);
        }

        public static bool DrawDecimalInput(string label, ref float value, float step = 0.1f, float minValue = float.MinValue, float? maxValue = null)
        {
            ImGui.Columns(2);

            ImGui.Text(label.UpperCaseFirstLetter().AddSpacesToCamelCase());

            ImGui.NextColumn();

            ImGui.InputFloat($"##{label}", ref value, step);

            if (maxValue != null && value > maxValue.Value)
            {
                value = maxValue.Value;
            }
            else if (value < minValue)
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

        public static void DrawSelectableComboInput(string label, string[] comboItems, ref int value, Action<int> selectAction, Action<int> changeAction)
        {
            int initalValue = value;

            ImGui.Columns(2);
            if (ImGui.Selectable(label.UpperCaseFirstLetter().AddSpacesToCamelCase()))
            {
                selectAction(value);
            }

            ImGui.NextColumn();
            ImGui.Combo($"##{label}", ref value, comboItems, comboItems.Length);

            ImGui.Columns(1);

            if (initalValue != value)
            {
                changeAction(value);
            }
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
