using System.Globalization;
using EchoCore.Scripts.Affixes;
using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Registry;
using EchoCore.Scripts.Services;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Rewards;

namespace EchoCore.Scripts.Rewards;

/// <summary>
/// 奖励界面中的声骸奖励。点击后把声骸实例放进 EchoCore 临时库存。
/// </summary>
public sealed class EchoReward : Reward
{
    private bool _wasTaken;

    private readonly EchoDefinition _definition;
    private readonly EchoInstance _instance;

    protected override RewardType RewardType => RewardType.None;

    public override int RewardsSetIndex => 4;

    public EchoInstance Instance => _instance;

    public override LocString Description => new("monsters", _definition.NameKey);

    public override bool IsPopulated => true;

    protected override string? IconPath => _definition.IconPath;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => CreateHoverTips();

    public EchoReward(EchoDefinition definition, EchoInstance instance, Player player)
        : base(player)
    {
        _definition = definition;
        _instance = instance;
    }

    public override Task Populate()
    {
        return Task.CompletedTask;
    }

    public override Control? CreateIcon()
    {
        var icon = new TextureRect
        {
            Texture = PreloadManager.Cache.GetCompressedTexture2D(_definition.IconPath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
        };
        icon.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        return icon;
    }

    protected override Task<bool> OnSelect()
    {
        EchoInventory.Add(Player, _instance);
        _wasTaken = true;

        Log.Info($"[EchoCore] Obtained echo {_instance.DefinitionId} instance={_instance.InstanceId}; inventory={EchoInventory.GetAll(Player).Count}");
        return Task.FromResult(true);
    }

    public override void OnSkipped()
    {
        if (!_wasTaken)
        {
            Log.Info($"[EchoCore] Skipped echo {_instance.DefinitionId} instance={_instance.InstanceId}");
        }
    }

    public override void MarkContentAsSeen()
    {
        // 声骸不是原版 ModelDb 内容，暂时没有 Seen/FTUE 记录可写。
    }

    private IEnumerable<IHoverTip> CreateHoverTips()
    {
        var lines = new List<string>
        {
            new LocString("monsters", _definition.DescriptionKey).GetFormattedText(),
            $"COST {_definition.Cost} | {_definition.Class}",
            "点击后加入 EchoCore 临时库存。当前库存暂未持久化，后续阶段会接入保存与装备界面。",
        };

        if (!string.IsNullOrWhiteSpace(_instance.SelectedSonataId) && EchoRegistry.TryGetSonata(_instance.SelectedSonataId, out var sonata))
        {
            lines.Add($"合鸣：{GetLocalizedTextOrFallback(sonata.NameKey)}");
        }

        foreach (var affix in _instance.Affixes)
        {
            lines.Add(FormatAffixLine(affix));
        }

        yield return new HoverTip(new LocString("monsters", _definition.NameKey), string.Join("\n", lines));
    }

    private static string FormatAffixLine(EchoAffixInstance affix)
    {
        var value = affix.Value.ToString("0.#", CultureInfo.InvariantCulture);
        if (EchoRegistry.TryGetAffix(affix.AffixId, out var definition))
        {
            var name = GetLocalizedTextOrFallback(definition.NameKey);
            return $"{name} +{value}（档位 {affix.Tier} / {affix.TierRarity}）";
        }

        return $"{affix.AffixId} +{value}（档位 {affix.Tier} / {affix.TierRarity}）";
    }

    private static string GetLocalizedTextOrFallback(string key)
    {
        var localized = new LocString("monsters", key).GetFormattedText();
        if (!string.IsNullOrWhiteSpace(localized) && !string.Equals(localized, $"monsters.{key}", StringComparison.Ordinal))
        {
            return localized;
        }

        return key switch
        {
            "ECHO_CORE_UNIVERSAL_RESONANCE.name" => "基础残响",
            "ECHO_CORE_HIDDEN_LIGHT.name" => "隐世回光",
            _ => key,
        };
    }
}
