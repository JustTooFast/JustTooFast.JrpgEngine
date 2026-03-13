// Copyright 2026 Matthew Yancer
// SPDX-License-Identifier: Apache-2.0

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JustTooFast.JrpgEngine.Rendering;

public sealed class PresentationSurface : IDisposable
{
    public const int InternalWidth = 320;
    public const int InternalHeight = 176;
    public const int UiWidth = InternalWidth * 2;
    public const int UiHeight = InternalHeight * 2;
    public const int WindowedScale = 2;

    private readonly GraphicsDevice _graphicsDevice;

    private RenderTarget2D? _worldRenderTarget;
    private RenderTarget2D? _uiRenderTarget;
    private bool _disposed;

    public PresentationSurface(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        EnsureRenderTargets();
    }

    public Rectangle CalculateWorldDestinationRectangle(Viewport viewport)
    {
        ThrowIfDisposed();

        return CalculateDestinationRectangle(viewport, UiWidth, UiHeight);
    }

    public Rectangle CalculateUiDestinationRectangle(Viewport viewport)
    {
        ThrowIfDisposed();

        return CalculateDestinationRectangle(viewport, UiWidth, UiHeight);
    }

    private static Rectangle CalculateDestinationRectangle(Viewport viewport, int sourceWidth, int sourceHeight)
    {
        var scaleX = viewport.Width / sourceWidth;
        var scaleY = viewport.Height / sourceHeight;
        var scale = Math.Max(1, Math.Min(scaleX, scaleY));

        var width = sourceWidth * scale;
        var height = sourceHeight * scale;
        var x = viewport.X + ((viewport.Width - width) / 2);
        var y = viewport.Y + ((viewport.Height - height) / 2);

        return new Rectangle(x, y, width, height);
    }

    public void BeginWorldScene()
    {
        ThrowIfDisposed();
        EnsureRenderTargets();

        _graphicsDevice.SetRenderTarget(_worldRenderTarget);
        _graphicsDevice.Clear(Color.Black);
    }

    public void EndWorldScene()
    {
        ThrowIfDisposed();
        _graphicsDevice.SetRenderTarget(null);
    }

    public void BeginUiScene()
    {
        ThrowIfDisposed();
        EnsureRenderTargets();

        _graphicsDevice.SetRenderTarget(_uiRenderTarget);
        _graphicsDevice.Clear(Color.Transparent);
    }

    public void EndUiScene()
    {
        ThrowIfDisposed();
        _graphicsDevice.SetRenderTarget(null);
    }

    public void Present(SpriteBatch spriteBatch)
    {
        if (spriteBatch is null)
        {
            throw new ArgumentNullException(nameof(spriteBatch));
        }

        ThrowIfDisposed();
        EnsureRenderTargets();

        _graphicsDevice.SetRenderTarget(null);
        _graphicsDevice.Clear(Color.Black);

        var worldDestination = CalculateWorldDestinationRectangle(_graphicsDevice.Viewport);
        var uiDestination = CalculateUiDestinationRectangle(_graphicsDevice.Viewport);

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone);

        spriteBatch.Draw(_worldRenderTarget!, worldDestination, Color.White);
        spriteBatch.Draw(_uiRenderTarget!, uiDestination, Color.White);

        spriteBatch.End();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _worldRenderTarget?.Dispose();
        _worldRenderTarget = null;

        _uiRenderTarget?.Dispose();
        _uiRenderTarget = null;

        _disposed = true;
    }

    private void EnsureRenderTargets()
    {
        ThrowIfDisposed();

        if (_worldRenderTarget is null ||
            _worldRenderTarget.IsDisposed ||
            _worldRenderTarget.Width != InternalWidth ||
            _worldRenderTarget.Height != InternalHeight)
        {
            _worldRenderTarget?.Dispose();
            _worldRenderTarget = new RenderTarget2D(
                _graphicsDevice,
                InternalWidth,
                InternalHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.DiscardContents);
        }

        if (_uiRenderTarget is null ||
            _uiRenderTarget.IsDisposed ||
            _uiRenderTarget.Width != UiWidth ||
            _uiRenderTarget.Height != UiHeight)
        {
            _uiRenderTarget?.Dispose();
            _uiRenderTarget = new RenderTarget2D(
                _graphicsDevice,
                UiWidth,
                UiHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.DiscardContents);
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}