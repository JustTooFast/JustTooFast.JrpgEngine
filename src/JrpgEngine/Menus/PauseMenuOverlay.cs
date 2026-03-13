// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JustTooFast.JrpgEngine.Menus;

public sealed class PauseMenuOverlay
{
    private readonly Texture2D _pixel;

    private KeyboardState _previousKeyboardState;
    private int _selectedIndex;

    public PauseMenuOverlay(Texture2D pixel)
    {
        _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel));
    }

    public bool IsOpen { get; private set; }

    public PauseMenuResult Update(GameTime gameTime)
    {
        if (gameTime is null)
        {
            throw new ArgumentNullException(nameof(gameTime));
        }

        var keyboardState = Keyboard.GetState();

        if (!IsOpen)
        {
            if (WasKeyJustPressed(keyboardState, Keys.Escape))
            {
                Open();
            }

            _previousKeyboardState = keyboardState;
            return PauseMenuResult.None;
        }

        if (WasKeyJustPressed(keyboardState, Keys.Escape))
        {
            Close();
            _previousKeyboardState = keyboardState;
            return PauseMenuResult.Resumed;
        }

        if (WasKeyJustPressed(keyboardState, Keys.Up) || WasKeyJustPressed(keyboardState, Keys.W))
        {
            _selectedIndex = Math.Max(0, _selectedIndex - 1);
        }
        else if (WasKeyJustPressed(keyboardState, Keys.Down) || WasKeyJustPressed(keyboardState, Keys.S))
        {
            _selectedIndex = Math.Min(1, _selectedIndex + 1);
        }
        else if (WasKeyJustPressed(keyboardState, Keys.Enter) || WasKeyJustPressed(keyboardState, Keys.Space))
        {
            var result = _selectedIndex switch
            {
                0 => PauseMenuResult.Resumed,
                1 => PauseMenuResult.ReturnToTitle,
                _ => PauseMenuResult.None
            };

            if (result == PauseMenuResult.Resumed)
            {
                Close();
            }

            _previousKeyboardState = keyboardState;
            return result;
        }

        _previousKeyboardState = keyboardState;
        return PauseMenuResult.None;
    }

    public void Draw(SpriteBatch spriteBatch, Viewport viewport)
    {
        if (!IsOpen)
        {
            return;
        }

        if (spriteBatch is null)
        {
            throw new ArgumentNullException(nameof(spriteBatch));
        }

        var screenBounds = new Rectangle(0, 0, viewport.Width, viewport.Height);

        var panelWidth = 220;
        var panelHeight = 140;
        var panelBounds = new Rectangle(
            (viewport.Width - panelWidth) / 2,
            (viewport.Height - panelHeight) / 2,
            panelWidth,
            panelHeight);

        var resumeBounds = new Rectangle(panelBounds.X + 20, panelBounds.Y + 30, panelBounds.Width - 40, 32);
        var titleBounds = new Rectangle(panelBounds.X + 20, panelBounds.Y + 75, panelBounds.Width - 40, 32);

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone);

        spriteBatch.Draw(_pixel, screenBounds, Color.Black * 0.45f);
        spriteBatch.Draw(_pixel, panelBounds, Color.DarkSlateGray);
        DrawRectOutline(spriteBatch, panelBounds, 2, Color.White);

        spriteBatch.Draw(_pixel, resumeBounds, _selectedIndex == 0 ? Color.Goldenrod : Color.Gray);
        DrawRectOutline(spriteBatch, resumeBounds, 2, Color.Black);

        spriteBatch.Draw(_pixel, titleBounds, _selectedIndex == 1 ? Color.IndianRed : Color.Gray);
        DrawRectOutline(spriteBatch, titleBounds, 2, Color.Black);

        spriteBatch.End();
    }

    public void Reset()
    {
        IsOpen = false;
        _selectedIndex = 0;
        _previousKeyboardState = Keyboard.GetState();
    }

    private void Open()
    {
        IsOpen = true;
        _selectedIndex = 0;
    }

    private void Close()
    {
        IsOpen = false;
        _selectedIndex = 0;
    }

    private bool WasKeyJustPressed(KeyboardState currentKeyboardState, Keys key)
    {
        return currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
    }

    private void DrawRectOutline(
        SpriteBatch spriteBatch,
        Rectangle rect,
        int thickness,
        Color color)
    {
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Bottom - thickness, rect.Width, thickness), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Left, rect.Top, thickness, rect.Height), color);
        spriteBatch.Draw(_pixel, new Rectangle(rect.Right - thickness, rect.Top, thickness, rect.Height), color);
    }
}

public enum PauseMenuResult
{
    None = 0,
    Resumed = 1,
    ReturnToTitle = 2
}