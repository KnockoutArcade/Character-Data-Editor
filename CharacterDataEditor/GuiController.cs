using System;
using System.Numerics;
using ImGuiNET;
using Raylib_cs;

namespace CharacterDataEditor;

internal unsafe class GuiController
{
    private Vector2 _mousePosition;
    private Vector2 displaySize;
    private float delta;

    static double _previousFrameTime;
    static bool _fontAtlasLoaded;
    static Texture2D _fontAtlasTexture;

    static string GetClipboardText()
    {
        return Raylib.GetClipboardText_();
    }

    static void SetClipboardText(string text)
    {
        Raylib.SetClipboardText(text);
    }

    public GuiController()
    {
        Init();
    }

    private void Init()
    {
        var io = ImGui.GetIO();

        _mousePosition = new Vector2(0, 0);
        io.AddMousePosEvent(_mousePosition.X, _mousePosition.Y);

        // Use this space to add more fonts
        LoadDefaultFontAtlas();
    }

    public void Shutdown()
    {
        if (_fontAtlasLoaded)
        {
            ImGui.GetIO().Fonts.ClearFonts();
            ImGui.GetIO().Fonts.TexID = IntPtr.Zero;
            Raylib.UnloadTexture(_fontAtlasTexture);
            _fontAtlasTexture = default;
            _fontAtlasLoaded = false;
        }
        _previousFrameTime = 0.0;
    }

    private void UpdateMousePosAndButtons()
    {
        var io = ImGui.GetIO();

        if (io.WantSetMousePos)
        {
            Raylib.SetMousePosition((int)io.MousePos.X, (int)io.MousePos.Y);
        }

        io.AddMouseButtonEvent(0, Raylib.IsMouseButtonDown(MouseButton.Left));
        io.AddMouseButtonEvent(1, Raylib.IsMouseButtonDown(MouseButton.Right));
        io.AddMouseButtonEvent(2, Raylib.IsMouseButtonDown(MouseButton.Middle));

        if (!Raylib.IsWindowMinimized())
        {
            _mousePosition = new Vector2(Raylib.GetMouseX(), Raylib.GetMouseY());
        }

        io.AddMousePosEvent(_mousePosition.X, _mousePosition.Y);
    }

    private void UpdateMouseCursor()
    {
        var io = ImGui.GetIO();
        if (io.ConfigFlags.HasFlag(ImGuiConfigFlags.NoMouseCursorChange))
        {
            return;
        }

        var mouseCursor = ImGui.GetMouseCursor();
        if (io.MouseDrawCursor || mouseCursor == ImGuiMouseCursor.None)
        {
            Raylib.HideCursor();
        }
        else
        {
            Raylib.ShowCursor();
        }
    }

    public void NewFrame()
    {
        var io = ImGui.GetIO();

        displaySize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
        io.DisplaySize = displaySize;

        var currentTime = Raylib.GetTime();
        delta = _previousFrameTime > 0.0 ? (float)(currentTime - _previousFrameTime) : 1.0f / 60.0f;
        io.DeltaTime = delta > 0.0f ? delta : 1.0f / 60.0f;
        _previousFrameTime = currentTime;

        UpdateMousePosAndButtons();
        UpdateMouseCursor();

        var wheel = Raylib.GetMouseWheelMoveV();
        if (wheel != Vector2.Zero)
        {
            io.AddMouseWheelEvent(wheel.X, wheel.Y);
        }
    }

    public bool ProcessEvent()
    {
        var io = ImGui.GetIO();

        io.AddKeyEvent(ImGuiKey.ModCtrl, Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl));
        io.AddKeyEvent(ImGuiKey.ModShift, Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift));
        io.AddKeyEvent(ImGuiKey.ModAlt, Raylib.IsKeyDown(KeyboardKey.LeftAlt) || Raylib.IsKeyDown(KeyboardKey.RightAlt));
        io.AddKeyEvent(ImGuiKey.ModSuper, Raylib.IsKeyDown(KeyboardKey.LeftSuper) || Raylib.IsKeyDown(KeyboardKey.RightSuper));

        io.AddKeyEvent(ImGuiKey.Apostrophe, Raylib.IsKeyDown(KeyboardKey.Apostrophe));
        io.AddKeyEvent(ImGuiKey.Comma, Raylib.IsKeyDown(KeyboardKey.Comma));
        io.AddKeyEvent(ImGuiKey.Minus, Raylib.IsKeyDown(KeyboardKey.Minus));
        io.AddKeyEvent(ImGuiKey.Period, Raylib.IsKeyDown(KeyboardKey.Period));
        io.AddKeyEvent(ImGuiKey.Slash, Raylib.IsKeyDown(KeyboardKey.Slash));
        io.AddKeyEvent(ImGuiKey._0, Raylib.IsKeyDown(KeyboardKey.Zero));
        io.AddKeyEvent(ImGuiKey._1, Raylib.IsKeyDown(KeyboardKey.One));
        io.AddKeyEvent(ImGuiKey._2, Raylib.IsKeyDown(KeyboardKey.Two));
        io.AddKeyEvent(ImGuiKey._3, Raylib.IsKeyDown(KeyboardKey.Three));
        io.AddKeyEvent(ImGuiKey._4, Raylib.IsKeyDown(KeyboardKey.Four));
        io.AddKeyEvent(ImGuiKey._5, Raylib.IsKeyDown(KeyboardKey.Five));
        io.AddKeyEvent(ImGuiKey._6, Raylib.IsKeyDown(KeyboardKey.Six));
        io.AddKeyEvent(ImGuiKey._7, Raylib.IsKeyDown(KeyboardKey.Seven));
        io.AddKeyEvent(ImGuiKey._8, Raylib.IsKeyDown(KeyboardKey.Eight));
        io.AddKeyEvent(ImGuiKey._9, Raylib.IsKeyDown(KeyboardKey.Nine));
        io.AddKeyEvent(ImGuiKey.Semicolon, Raylib.IsKeyDown(KeyboardKey.Semicolon));
        io.AddKeyEvent(ImGuiKey.Equal, Raylib.IsKeyDown(KeyboardKey.Equal));
        io.AddKeyEvent(ImGuiKey.A, Raylib.IsKeyDown(KeyboardKey.A));
        io.AddKeyEvent(ImGuiKey.B, Raylib.IsKeyDown(KeyboardKey.B));
        io.AddKeyEvent(ImGuiKey.C, Raylib.IsKeyDown(KeyboardKey.C));
        io.AddKeyEvent(ImGuiKey.D, Raylib.IsKeyDown(KeyboardKey.D));
        io.AddKeyEvent(ImGuiKey.E, Raylib.IsKeyDown(KeyboardKey.E));
        io.AddKeyEvent(ImGuiKey.F, Raylib.IsKeyDown(KeyboardKey.F));
        io.AddKeyEvent(ImGuiKey.G, Raylib.IsKeyDown(KeyboardKey.G));
        io.AddKeyEvent(ImGuiKey.H, Raylib.IsKeyDown(KeyboardKey.H));
        io.AddKeyEvent(ImGuiKey.I, Raylib.IsKeyDown(KeyboardKey.I));
        io.AddKeyEvent(ImGuiKey.J, Raylib.IsKeyDown(KeyboardKey.J));
        io.AddKeyEvent(ImGuiKey.K, Raylib.IsKeyDown(KeyboardKey.K));
        io.AddKeyEvent(ImGuiKey.L, Raylib.IsKeyDown(KeyboardKey.L));
        io.AddKeyEvent(ImGuiKey.M, Raylib.IsKeyDown(KeyboardKey.M));
        io.AddKeyEvent(ImGuiKey.N, Raylib.IsKeyDown(KeyboardKey.N));
        io.AddKeyEvent(ImGuiKey.O, Raylib.IsKeyDown(KeyboardKey.O));
        io.AddKeyEvent(ImGuiKey.P, Raylib.IsKeyDown(KeyboardKey.P));
        io.AddKeyEvent(ImGuiKey.Q, Raylib.IsKeyDown(KeyboardKey.Q));
        io.AddKeyEvent(ImGuiKey.R, Raylib.IsKeyDown(KeyboardKey.R));
        io.AddKeyEvent(ImGuiKey.S, Raylib.IsKeyDown(KeyboardKey.S));
        io.AddKeyEvent(ImGuiKey.T, Raylib.IsKeyDown(KeyboardKey.T));
        io.AddKeyEvent(ImGuiKey.U, Raylib.IsKeyDown(KeyboardKey.U));
        io.AddKeyEvent(ImGuiKey.V, Raylib.IsKeyDown(KeyboardKey.V));
        io.AddKeyEvent(ImGuiKey.W, Raylib.IsKeyDown(KeyboardKey.W));
        io.AddKeyEvent(ImGuiKey.X, Raylib.IsKeyDown(KeyboardKey.X));
        io.AddKeyEvent(ImGuiKey.Y, Raylib.IsKeyDown(KeyboardKey.Y));
        io.AddKeyEvent(ImGuiKey.Z, Raylib.IsKeyDown(KeyboardKey.Z));
        io.AddKeyEvent(ImGuiKey.Space, Raylib.IsKeyDown(KeyboardKey.Space));
        io.AddKeyEvent(ImGuiKey.Escape, Raylib.IsKeyDown(KeyboardKey.Escape));
        io.AddKeyEvent(ImGuiKey.Enter, Raylib.IsKeyDown(KeyboardKey.Enter));
        io.AddKeyEvent(ImGuiKey.Tab, Raylib.IsKeyDown(KeyboardKey.Tab));
        io.AddKeyEvent(ImGuiKey.Backspace, Raylib.IsKeyDown(KeyboardKey.Backspace));
        io.AddKeyEvent(ImGuiKey.Insert, Raylib.IsKeyDown(KeyboardKey.Insert));
        io.AddKeyEvent(ImGuiKey.Delete, Raylib.IsKeyDown(KeyboardKey.Delete));
        io.AddKeyEvent(ImGuiKey.RightArrow, Raylib.IsKeyDown(KeyboardKey.Right));
        io.AddKeyEvent(ImGuiKey.LeftArrow, Raylib.IsKeyDown(KeyboardKey.Left));
        io.AddKeyEvent(ImGuiKey.DownArrow, Raylib.IsKeyDown(KeyboardKey.Down));
        io.AddKeyEvent(ImGuiKey.UpArrow, Raylib.IsKeyDown(KeyboardKey.Up));
        io.AddKeyEvent(ImGuiKey.PageUp, Raylib.IsKeyDown(KeyboardKey.PageUp));
        io.AddKeyEvent(ImGuiKey.PageDown, Raylib.IsKeyDown(KeyboardKey.PageDown));
        io.AddKeyEvent(ImGuiKey.Home, Raylib.IsKeyDown(KeyboardKey.Home));
        io.AddKeyEvent(ImGuiKey.End, Raylib.IsKeyDown(KeyboardKey.End));
        io.AddKeyEvent(ImGuiKey.CapsLock, Raylib.IsKeyDown(KeyboardKey.CapsLock));
        io.AddKeyEvent(ImGuiKey.ScrollLock, Raylib.IsKeyDown(KeyboardKey.ScrollLock));
        io.AddKeyEvent(ImGuiKey.NumLock, Raylib.IsKeyDown(KeyboardKey.NumLock));
        io.AddKeyEvent(ImGuiKey.PrintScreen, Raylib.IsKeyDown(KeyboardKey.PrintScreen));
        io.AddKeyEvent(ImGuiKey.Pause, Raylib.IsKeyDown(KeyboardKey.Pause));
        io.AddKeyEvent(ImGuiKey.F1, Raylib.IsKeyDown(KeyboardKey.F1));
        io.AddKeyEvent(ImGuiKey.F2, Raylib.IsKeyDown(KeyboardKey.F2));
        io.AddKeyEvent(ImGuiKey.F3, Raylib.IsKeyDown(KeyboardKey.F3));
        io.AddKeyEvent(ImGuiKey.F4, Raylib.IsKeyDown(KeyboardKey.F4));
        io.AddKeyEvent(ImGuiKey.F5, Raylib.IsKeyDown(KeyboardKey.F5));
        io.AddKeyEvent(ImGuiKey.F6, Raylib.IsKeyDown(KeyboardKey.F6));
        io.AddKeyEvent(ImGuiKey.F7, Raylib.IsKeyDown(KeyboardKey.F7));
        io.AddKeyEvent(ImGuiKey.F8, Raylib.IsKeyDown(KeyboardKey.F8));
        io.AddKeyEvent(ImGuiKey.F9, Raylib.IsKeyDown(KeyboardKey.F9));
        io.AddKeyEvent(ImGuiKey.F10, Raylib.IsKeyDown(KeyboardKey.F10));
        io.AddKeyEvent(ImGuiKey.F11, Raylib.IsKeyDown(KeyboardKey.F11));
        io.AddKeyEvent(ImGuiKey.F12, Raylib.IsKeyDown(KeyboardKey.F12));
        io.AddKeyEvent(ImGuiKey.LeftShift, Raylib.IsKeyDown(KeyboardKey.LeftShift));
        io.AddKeyEvent(ImGuiKey.LeftCtrl, Raylib.IsKeyDown(KeyboardKey.LeftControl));
        io.AddKeyEvent(ImGuiKey.LeftAlt, Raylib.IsKeyDown(KeyboardKey.LeftAlt));
        io.AddKeyEvent(ImGuiKey.LeftSuper, Raylib.IsKeyDown(KeyboardKey.LeftSuper));
        io.AddKeyEvent(ImGuiKey.RightShift, Raylib.IsKeyDown(KeyboardKey.RightShift));
        io.AddKeyEvent(ImGuiKey.RightCtrl, Raylib.IsKeyDown(KeyboardKey.RightControl));
        io.AddKeyEvent(ImGuiKey.RightAlt, Raylib.IsKeyDown(KeyboardKey.RightAlt));
        io.AddKeyEvent(ImGuiKey.RightSuper, Raylib.IsKeyDown(KeyboardKey.RightSuper));
        io.AddKeyEvent(ImGuiKey.Menu, Raylib.IsKeyDown(KeyboardKey.KeyboardMenu) || Raylib.IsKeyDown(KeyboardKey.Menu));
        io.AddKeyEvent(ImGuiKey.LeftBracket, Raylib.IsKeyDown(KeyboardKey.LeftBracket));
        io.AddKeyEvent(ImGuiKey.Backslash, Raylib.IsKeyDown(KeyboardKey.Backslash));
        io.AddKeyEvent(ImGuiKey.RightBracket, Raylib.IsKeyDown(KeyboardKey.RightBracket));
        io.AddKeyEvent(ImGuiKey.GraveAccent, Raylib.IsKeyDown(KeyboardKey.Grave));
        io.AddKeyEvent(ImGuiKey.Keypad0, Raylib.IsKeyDown(KeyboardKey.Kp0));
        io.AddKeyEvent(ImGuiKey.Keypad1, Raylib.IsKeyDown(KeyboardKey.Kp1));
        io.AddKeyEvent(ImGuiKey.Keypad2, Raylib.IsKeyDown(KeyboardKey.Kp2));
        io.AddKeyEvent(ImGuiKey.Keypad3, Raylib.IsKeyDown(KeyboardKey.Kp3));
        io.AddKeyEvent(ImGuiKey.Keypad4, Raylib.IsKeyDown(KeyboardKey.Kp4));
        io.AddKeyEvent(ImGuiKey.Keypad5, Raylib.IsKeyDown(KeyboardKey.Kp5));
        io.AddKeyEvent(ImGuiKey.Keypad6, Raylib.IsKeyDown(KeyboardKey.Kp6));
        io.AddKeyEvent(ImGuiKey.Keypad7, Raylib.IsKeyDown(KeyboardKey.Kp7));
        io.AddKeyEvent(ImGuiKey.Keypad8, Raylib.IsKeyDown(KeyboardKey.Kp8));
        io.AddKeyEvent(ImGuiKey.Keypad9, Raylib.IsKeyDown(KeyboardKey.Kp9));
        io.AddKeyEvent(ImGuiKey.KeypadDecimal, Raylib.IsKeyDown(KeyboardKey.KpDecimal));
        io.AddKeyEvent(ImGuiKey.KeypadDivide, Raylib.IsKeyDown(KeyboardKey.KpDivide));
        io.AddKeyEvent(ImGuiKey.KeypadMultiply, Raylib.IsKeyDown(KeyboardKey.KpMultiply));
        io.AddKeyEvent(ImGuiKey.KeypadSubtract, Raylib.IsKeyDown(KeyboardKey.KpSubtract));
        io.AddKeyEvent(ImGuiKey.KeypadAdd, Raylib.IsKeyDown(KeyboardKey.KpAdd));
        io.AddKeyEvent(ImGuiKey.KeypadEnter, Raylib.IsKeyDown(KeyboardKey.KpEnter));
        io.AddKeyEvent(ImGuiKey.KeypadEqual, Raylib.IsKeyDown(KeyboardKey.KpEqual));
        io.AddKeyEvent(ImGuiKey.AppBack, Raylib.IsKeyDown(KeyboardKey.Back));

        int codepoint;
        while ((codepoint = Raylib.GetCharPressed()) > 0)
        {
            io.AddInputCharacter((uint)codepoint);
        }

        return true;
    }

    void LoadDefaultFontAtlas()
    {
        if (!_fontAtlasLoaded)
        {
            var io = ImGui.GetIO();
            Image image = new();

            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out var width, out var height, out _);
            image.Data = pixels;
            image.Width = width;
            image.Height = height;
            image.Mipmaps = 1;
            image.Format = PixelFormat.UncompressedR8G8B8A8;

            _fontAtlasTexture = Raylib.LoadTextureFromImage(image);
            if (_fontAtlasTexture.Id == 0)
            {
                throw new InvalidOperationException("failed to upload font atlas");
            }
            io.Fonts.TexID = (IntPtr)_fontAtlasTexture.Id;
            io.Fonts.ClearTexData();
            _fontAtlasLoaded = true;
        }
    }

    public void Render(ImDrawDataPtr drawData)
    {
        if (drawData.DisplaySize.X <= 0 || drawData.DisplaySize.Y <= 0)
        {
            return;
        }

        Rlgl.DisableBackfaceCulling();
        for (var n = 0; n < drawData.CmdListsCount; n++)
        {
            var drawList = drawData.CmdLists[n];
            uint indexBufferOffset = 0;
            for (var i = 0; i < drawList.CmdBuffer.Size; i++)
            {
                var drawCommand = drawList.CmdBuffer[i];
                var pos = drawData.DisplayPos;
                var rectX = (int)(drawCommand.ClipRect.X - pos.X);
                var rectY = (int)(drawCommand.ClipRect.Y - pos.Y);
                var rectW = (int)(drawCommand.ClipRect.Z - rectX);
                var rectH = (int)(drawCommand.ClipRect.W - rectY);
                Raylib.BeginScissorMode(rectX, rectY, rectW, rectH);
                {
                    var textureId = drawCommand.TextureId;
                    for (var j = 0; j <= (drawCommand.ElemCount - 3); j += 3)
                    {
                        if (drawCommand.ElemCount == 0)
                        {
                            break;
                        }

                        Rlgl.PushMatrix();
                        Rlgl.Begin(DrawMode.Triangles);
                        Rlgl.SetTexture((uint)textureId.ToInt32());

                        ImDrawVertPtr vertex;
                        ushort index;

                        index = drawList.IdxBuffer[(int)(j + indexBufferOffset)];
                        vertex = drawList.VtxBuffer[index];
                        DrawTriangleVertex(vertex);

                        index = drawList.IdxBuffer[(int)(j + 2 + indexBufferOffset)];
                        vertex = drawList.VtxBuffer[index];
                        DrawTriangleVertex(vertex);

                        index = drawList.IdxBuffer[(int)(j + 1 + indexBufferOffset)];
                        vertex = drawList.VtxBuffer[index];
                        DrawTriangleVertex(vertex);

                        Rlgl.DisableTexture();
                        Rlgl.End();
                        Rlgl.PopMatrix();
                    }
                }

                indexBufferOffset += drawCommand.ElemCount;
            }
        }

        Raylib.EndScissorMode();
        Rlgl.EnableBackfaceCulling();
    }

    void DrawTriangleVertex(ImDrawVertPtr vertex)
    {
        var color = new Color((byte)(vertex.col >> 0), (byte)(vertex.col >> 8), (byte)(vertex.col >> 16), (byte)(vertex.col >> 24));
        Rlgl.Color4ub(color.R, color.G, color.B, color.A);
        Rlgl.TexCoord2f(vertex.uv.X, vertex.uv.Y);
        Rlgl.Vertex2f(vertex.pos.X, vertex.pos.Y);
    }
}
