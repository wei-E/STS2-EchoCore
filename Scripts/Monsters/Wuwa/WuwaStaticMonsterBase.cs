using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using System;
using System.Collections.Generic;

namespace EchoCore.Scripts.Monsters.Wuwa;

/// <summary>
/// 鸣潮小怪共用的静态立绘基类。
/// 第一版先用静态图跑通战斗链路，后续再替换为正式动画资源。
/// </summary>
public abstract class WuwaStaticMonsterBase : CustomMonsterModel
{
    private const string SharedVisualScenePath = "res://scenes/creature_visuals/echo_core_wuwa_monster_visuals.tscn";
    private static readonly Dictionary<string, Rect2> OpaqueRectCache = [];

    protected abstract string TexturePath { get; }

    protected virtual Vector2 VisualPosition => new(0f, -36f);

    protected virtual Vector2 VisualScale => new(0.68f, 0.68f);

    protected virtual Rect2 BoundsRect => new(-240f, -360f, 480f, 440f);

    protected virtual bool UseOpaqueContentBounds => true;

    protected virtual float BoundsPaddingX => 10f;

    protected virtual float BoundsPaddingTop => 8f;

    protected virtual float BoundsPaddingBottom => 6f;

    protected virtual Vector2 CenterPosition => new(0f, -86f);

    protected virtual Vector2 IntentPosition => new(0f, -250f);

    protected virtual Vector2 TalkPosition => new(0f, -220f);

    public override string? CustomVisualPath => SharedVisualScenePath;

    public override bool HasDeathSfx => false;

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        NCreatureVisuals visuals = NodeFactory<NCreatureVisuals>.CreateFromScene(SharedVisualScenePath);
        ApplyVisual(visuals);
        return visuals;
    }

    public override Task AfterAddedToRoom()
    {
        ApplyVisual(NCombatRoom.Instance?.GetCreatureNode(Creature)?.Visuals);
        return Task.CompletedTask;
    }

    private void ApplyVisual(NCreatureVisuals? visuals)
    {
        if (visuals == null)
        {
            return;
        }

        Sprite2D? sprite = visuals.FindChild("Visuals", true, false) as Sprite2D;
        if (sprite != null)
        {
            Texture2D? texture = ResourceLoader.Load<Texture2D>(TexturePath);
            sprite.Texture = texture;
            sprite.Position = VisualPosition;
            sprite.Scale = VisualScale;
        }

        if (visuals.FindChild("Bounds", true, false) is Control bounds)
        {
            Rect2 resolvedBounds = ResolveBoundsRect(sprite);
            bounds.OffsetLeft = resolvedBounds.Position.X;
            bounds.OffsetTop = resolvedBounds.Position.Y;
            bounds.OffsetRight = resolvedBounds.Position.X + resolvedBounds.Size.X;
            bounds.OffsetBottom = resolvedBounds.Position.Y + resolvedBounds.Size.Y;
        }

        if (visuals.FindChild("CenterPos", true, false) is Marker2D centerPos)
        {
            centerPos.Position = CenterPosition;
        }

        if (visuals.FindChild("IntentPos", true, false) is Marker2D intentPos)
        {
            intentPos.Position = IntentPosition;
        }

        if (visuals.FindChild("TalkPos", true, false) is Marker2D talkPos)
        {
            talkPos.Position = TalkPosition;
        }
    }

    private Rect2 ResolveBoundsRect(Sprite2D? sprite)
    {
        if (!UseOpaqueContentBounds || sprite?.Texture == null)
        {
            return BoundsRect;
        }

        Texture2D texture = sprite.Texture;
        Vector2 textureSize = texture.GetSize();
        if (textureSize.X <= 0f || textureSize.Y <= 0f)
        {
            return BoundsRect;
        }

        Rect2 opaqueRect = GetOpaquePixelRect(texture);
        float absScaleX = Math.Abs(sprite.Scale.X);
        float absScaleY = Math.Abs(sprite.Scale.Y);

        Vector2 topLeft = sprite.Position - new Vector2(textureSize.X * 0.5f * absScaleX, textureSize.Y * 0.5f * absScaleY);
        Vector2 contentTopLeft = topLeft + new Vector2(opaqueRect.Position.X * absScaleX, opaqueRect.Position.Y * absScaleY);
        Vector2 contentSize = new Vector2(opaqueRect.Size.X * absScaleX, opaqueRect.Size.Y * absScaleY);

        Rect2 contentRect = new(contentTopLeft, contentSize);
        return ExpandRect(contentRect, BoundsPaddingX, BoundsPaddingTop, BoundsPaddingBottom);
    }

    private static Rect2 ExpandRect(Rect2 rect, float paddingX, float paddingTop, float paddingBottom)
    {
        return new Rect2(
            rect.Position.X - paddingX,
            rect.Position.Y - paddingTop,
            rect.Size.X + paddingX * 2f,
            rect.Size.Y + paddingTop + paddingBottom);
    }

    private static Rect2 GetOpaquePixelRect(Texture2D texture)
    {
        string key = string.IsNullOrWhiteSpace(texture.ResourcePath)
            ? $"{texture.GetRid().Id}:{texture.GetSize().X}x{texture.GetSize().Y}"
            : texture.ResourcePath;

        if (OpaqueRectCache.TryGetValue(key, out Rect2 cached))
        {
            return cached;
        }

        Rect2 result;
        try
        {
            Image image = texture.GetImage();
            int width = image.GetWidth();
            int height = image.GetHeight();
            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (image.GetPixel(x, y).A <= 0.01f)
                    {
                        continue;
                    }

                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }

            result = maxX >= minX && maxY >= minY
                ? new Rect2(minX, minY, maxX - minX + 1, maxY - minY + 1)
                : new Rect2(0f, 0f, width, height);
        }
        catch
        {
            Vector2 fallbackSize = texture.GetSize();
            result = new Rect2(0f, 0f, fallbackSize.X, fallbackSize.Y);
        }

        OpaqueRectCache[key] = result;
        return result;
    }
}
