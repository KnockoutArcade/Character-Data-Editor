using System;
using System.Collections.Generic;
using System.Numerics;
using CharacterDataEditor.Extensions;
using CharacterDataEditor.Models.CharacterData;
using CharacterDataEditor.Models.ProjectileData;
using ImGuiNET;

namespace CharacterDataEditor.Helpers;

public static class ImguiDrawingHelper
{
    public static void DrawHelpMarker(string description, float scale = 2.0f)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.DelayShort) && ImGui.BeginTooltip())
        {

            ImGui.SetWindowFontScale(scale);

            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
            ImGui.TextUnformatted(description);
            ImGui.PopTextWrapPos();

            ImGui.EndTooltip();
        }
    }

    public static bool DrawIntInput(string label, ref int value, int minValue = int.MinValue, int? maxValue = null, string tooltip = "", string id = null)
    {
        ImGui.Columns(2);

        //draw the input label
        ImGui.Text(label.UpperCaseFirstLetter().AddSpacesToCamelCase());
        if (tooltip != "")
        {
            ImGui.SameLine();
            DrawHelpMarker(tooltip);
        }

        ImGui.NextColumn();

        //draw the input without a label
        ImGui.InputInt($"##{id ?? label}", ref value);

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

    public static void DrawFlagsInputListbox<T>(string label, ref T value, float scale, bool drawZeroValue = false) where T : Enum
    {
        var flagValues = Enum.GetValues(typeof(T));
        var flagValuesList = new List<(string key, bool value, int flagValue)>();

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

        ImGui.Text(label.UpperCaseFirstLetter().AddSpacesToCamelCase());
        ImGui.SameLine();
        DrawHelpMarker("Click to select, click again to deselect. An '*' appears on selected items.");

        ImGui.NextColumn();

        if (ImGui.BeginListBox($"##{label}#lb"))
        {
            for (var i = 0; i < flagValuesList.Count; i++)
            {
                var item = flagValuesList[i];

                var status = item.value;

                var itemText = item.value ? $"* {item.key}" : item.key;

                if (ImGui.Selectable(itemText, item.value))
                {
                    item.value = !item.value;
                }

                flagValuesList[i] = item;
            }

            ImGui.EndListBox();
        }

        var refOutValue = 0;

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

    public static void DrawFlagsInput<T>(string label, ref T value, bool drawZeroValue = false) where T : Enum
    {
        var flagValues = Enum.GetValues(typeof(T));
        var flagValuesList = new List<(string key, bool value, int flagValue)>();

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

        for (var i = 0; i < flagValuesList.Count; i++)
        {
            var item = flagValuesList[i];

            var status = item.value;
            ImGui.Text($"{item.key}");

            ImGui.NextColumn();

            ImGui.Checkbox($"##{i}#{label}", ref status);

            ImGui.NextColumn();

            item.value = status;

            flagValuesList[i] = item;
        }

        var refOutValue = 0;

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

    public static bool DrawDecimalInput(string label, ref float value, float step = 0.1f, float minValue = float.MinValue, float? maxValue = null, string id = null)
    {
        ImGui.Columns(2);

        ImGui.Text(label.UpperCaseFirstLetter().AddSpacesToCamelCase());

        ImGui.NextColumn();

        ImGui.InputFloat($"##{id ?? label}", ref value, step);

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

    public static void DrawBoolInput(string label, ref bool value, string tooltip = "", string id = null)
    {
        ImGui.Columns(2);
        ImGui.Text(label.UpperCaseFirstLetter().AddSpacesToCamelCase());
        if (tooltip != "")
        {
            ImGui.SameLine();
            DrawHelpMarker(tooltip);
        }
        ImGui.NextColumn();
        ImGui.Checkbox($"##{id ?? label}", ref value);

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
        var initalValue = value;

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

    public static void DrawStringInput(string label, ref string value, uint maxLength = 255, string tooltip = "")
    {
        ImGui.Columns(2);
        ImGui.Text(label.UpperCaseFirstLetter().AddSpacesToCamelCase());
        if (tooltip != "")
        {
            ImGui.SameLine();
            DrawHelpMarker(tooltip);
        }

        ImGui.NextColumn();
        ImGui.InputText($"##{label}", ref value, maxLength);

        ImGui.Columns(1);
    }

    public static void DrawPaletteEditor(ref List<RGBModel> palettes, float scale)
    {
        if (!ImGui.BeginTable("paletteEditor", 2, ImGuiTableFlags.NoBordersInBody))
        {
            return;
        }

        var tableFlags = ImGuiTableColumnFlags.NoSort;
        ImGui.TableSetupColumn("", tableFlags | ImGuiTableColumnFlags.WidthFixed, 100.0f * scale);
        ImGui.TableSetupColumn("", tableFlags | ImGuiTableColumnFlags.WidthStretch);

        for (var i = 0; i < palettes.Count; i++)
        {
            ImGui.TableNextColumn();
            //for shortness, grab the item we're manipulating
            var rgbPalette = palettes[i];

            //Put colors into a vector 3 for the imgui control
            var selectedColors = new Vector3(
                rgbPalette.Red,
                rgbPalette.Green,
                rgbPalette.Blue);

            //draw the actual control
            ImGui.Text($"Color {i}");

            ImGui.TableNextColumn();

            ImGui.ColorEdit3($"##Color{i}", ref selectedColors);

            //store values back into palette
            palettes[i] = new RGBModel(selectedColors.X, selectedColors.Y, selectedColors.Z);
        }

        ImGui.EndTable();
    }

    public static void DrawPaletteEditor(ref List<ProjectileRGBModel> palettes, float scale)
    {
        if (!ImGui.BeginTable("paletteEditor", 2, ImGuiTableFlags.NoBordersInBody))
        {
            return;
        }

        var tableFlags = ImGuiTableColumnFlags.NoSort;
        ImGui.TableSetupColumn("", tableFlags | ImGuiTableColumnFlags.WidthFixed, 100.0f * scale);
        ImGui.TableSetupColumn("", tableFlags | ImGuiTableColumnFlags.WidthStretch);

        for (var i = 0; i < palettes.Count; i++)
        {
            ImGui.TableNextColumn();
            //for shortness, grab the item we're manipulating
            var rgbPalette = palettes[i];

            //Put colors into a vector 3 for the imgui control
            var selectedColors = new Vector3(
                rgbPalette.Red,
                rgbPalette.Green,
                rgbPalette.Blue);

            //draw the actual control
            ImGui.Text($"Color {i}");

            ImGui.TableNextColumn();

            ImGui.ColorEdit3($"##Color{i}", ref selectedColors);

            //store values back into palette
            palettes[i] = new ProjectileRGBModel(selectedColors.X, selectedColors.Y, selectedColors.Z);
        }

        ImGui.EndTable();
    }

    public static void DrawVerticalSpacing(float scale, float unitsToMove)
    {
        var cursorPos = ImGui.GetCursorPos();
        cursorPos.Y += (unitsToMove * scale);
        ImGui.SetCursorPos(cursorPos);
        ImGui.Dummy(Vector2.Zero);
        ImGui.SetCursorPos(cursorPos);
    }

    public static bool DrawSelectableWithRemove(Action selectAction, Action duplicateAction, string label, bool selected, int id = -1)
    {
        if (!ImGui.BeginTable($"selectable##{label}${id}", 2, ImGuiTableFlags.NoBordersInBody))
        {
            return false;
        }

        var tableFlags = ImGuiTableColumnFlags.NoSort;
        var sizeOfButton = ImGui.CalcTextSize("Remove");

        ImGui.TableSetupColumn("", tableFlags | ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn("", tableFlags | ImGuiTableColumnFlags.WidthFixed, sizeOfButton.X * 1.3f);

        ImGui.TableNextColumn();

        if (ImGui.Selectable($"{label}##{id}", selected))
        {
            selectAction();
        }
        if (ImGui.BeginPopupContextItem())
        {
            selectAction();
            if (ImGui.Button("Create Duplicate"))
            {
                duplicateAction();
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }

        ImGui.TableNextColumn();

        if (ImGui.Button("Remove"))
        {
            ImGui.EndTable();
            return true;
        }

        ImGui.EndTable();
        return false;
    }

    public static void DrawSelectable(Action selectAction, string label, bool selected, int id = -1)
    {
        if (ImGui.Selectable($"{label}##{id}", selected))
        {
            selectAction();
        }
    }
}
