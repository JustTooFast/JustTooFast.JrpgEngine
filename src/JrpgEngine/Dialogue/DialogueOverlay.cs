// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Dialogue;

public sealed class DialogueOverlay
{
    private readonly Texture2D _pixel;
    private readonly SpriteFont _font;

    public DialogueOverlay(Texture2D pixel, SpriteFont font)
    {
        _pixel = pixel ?? throw new ArgumentNullException(nameof(pixel));
        _font = font ?? throw new ArgumentNullException(nameof(font));
    }

    public void Draw(SpriteBatch spriteBatch, Viewport viewport, DialogueSession session)
    {
        if (spriteBatch is null)
        {
            throw new ArgumentNullException(nameof(spriteBatch));
        }

        if (session is null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        var screenBounds = new Rectangle(0, 0, viewport.Width, viewport.Height);

        var panelMargin = 16;
        var panelHeight = 112;
        var panelBounds = new Rectangle(
            panelMargin,
            viewport.Height - panelHeight - panelMargin,
            viewport.Width - (panelMargin * 2),
            panelHeight);

        var textPadding = 16;
        var textOrigin = new Vector2(
            panelBounds.X + textPadding,
            panelBounds.Y + textPadding);
        var textWidth = panelBounds.Width - (textPadding * 2);
        var wrappedText = WrapText(session.CurrentLine, textWidth);

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone);

        spriteBatch.Draw(_pixel, screenBounds, Color.Black * 0.25f);
        spriteBatch.Draw(_pixel, panelBounds, Color.MidnightBlue);
        DrawRectOutline(spriteBatch, panelBounds, 2, Color.White);

        spriteBatch.DrawString(_font, wrappedText, textOrigin, Color.White);

        spriteBatch.End();
    }

    private string WrapText(string text, float maxLineWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return string.Empty;
        }

        var lines = new List<string>();
        var currentLine = new StringBuilder();

        foreach (var word in words)
        {
            var candidate = currentLine.Length == 0
                ? word
                : $"{currentLine} {word}";

            if (_font.MeasureString(candidate).X <= maxLineWidth)
            {
                currentLine.Clear();
                currentLine.Append(candidate);
                continue;
            }

            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString());
                currentLine.Clear();
                currentLine.Append(word);
                continue;
            }

            lines.Add(word);
        }

        if (currentLine.Length > 0)
        {
            lines.Add(currentLine.ToString());
        }

        return string.Join('\n', lines);
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