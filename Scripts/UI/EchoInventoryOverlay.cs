using System.Globalization;
using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Registry;
using EchoCore.Scripts.Services;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

namespace EchoCore.Scripts.UI;

/// <summary>
/// 声骸 MVP 库存界面。先做一个轻量浮窗，后续再替换为正式美术 UI。
/// </summary>
public sealed partial class EchoInventoryOverlay : Control
{
    private const string NodeName = "EchoCoreInventoryOverlay";
    private const float ButtonWidth = 116f;
    private const float ButtonHeight = 44f;
    private const float ButtonRightMargin = 24f;
    private const float ButtonTopMargin = 88f;
    private const float PanelWidth = 640f;
    private const float PanelHeight = 520f;
    private const float PanelRightMargin = 24f;
    private const float PanelTopMargin = 140f;

    private Button _openButton = null!;
    private Button _activeSkillButton = null!;
    private PanelContainer _panel = null!;
    private VBoxContainer _content = null!;

    public static void AttachTo(NRun run)
    {
        if (run.GetNodeOrNull<EchoInventoryOverlay>(NodeName) != null)
        {
            return;
        }

        var overlay = new EchoInventoryOverlay
        {
            Name = NodeName,
        };
        run.AddChild(overlay);
    }

    public static void OpenForTuning()
    {
        var overlay = NRun.Instance?.GetNodeOrNull<EchoInventoryOverlay>(NodeName);
        overlay?.OpenPanelForTuning();
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsPreset(LayoutPreset.FullRect);
        ZIndex = 5000;
        BuildOpenButton();
        BuildActiveSkillButton();
        BuildPanel();
        LayoutOverlay();
        Refresh();
    }

    public override void _Process(double delta)
    {
        UpdateModeVisibility();
        UpdateActiveSkillButton();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            LayoutOverlay();
        }
    }

    private void BuildOpenButton()
    {
        _openButton = new Button
        {
            Text = "声骸",
            CustomMinimumSize = new Vector2(104f, 42f),
            MouseFilter = MouseFilterEnum.Stop,
            TooltipText = "打开声骸库存",
            TopLevel = true,
        };
        _openButton.Size = new Vector2(ButtonWidth, ButtonHeight);
        _openButton.Pressed += TogglePanel;
        AddChild(_openButton);
    }

    private void BuildActiveSkillButton()
    {
        _activeSkillButton = new Button
        {
            Text = "声骸技",
            CustomMinimumSize = new Vector2(104f, 42f),
            MouseFilter = MouseFilterEnum.Stop,
            TooltipText = "释放主声骸主动技",
            TopLevel = true,
            Visible = false,
        };
        _activeSkillButton.Size = new Vector2(ButtonWidth, ButtonHeight);
        _activeSkillButton.Pressed += async () =>
        {
            var player = GetLocalPlayer();
            if (player == null)
            {
                return;
            }

            await EchoActiveSkillService.TryActivate(player);
            UpdateActiveSkillButton();
        };
        AddChild(_activeSkillButton);
    }

    private void BuildPanel()
    {
        _panel = new PanelContainer
        {
            Visible = false,
            CustomMinimumSize = new Vector2(640f, 520f),
            MouseFilter = MouseFilterEnum.Stop,
            TopLevel = true,
        };
        _panel.Size = new Vector2(PanelWidth, PanelHeight);
        AddChild(_panel);

        var root = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(620f, 500f),
        };
        _panel.AddChild(root);

        var header = new HBoxContainer();
        root.AddChild(header);

        header.AddChild(new Label
        {
            Text = "声骸库存 / 装备",
            CustomMinimumSize = new Vector2(480f, 32f),
        });

        var close = new Button
        {
            Text = "关闭",
            CustomMinimumSize = new Vector2(80f, 32f),
        };
        close.Pressed += () => _panel.Visible = false;
        header.AddChild(close);

        var scroll = new ScrollContainer
        {
            CustomMinimumSize = new Vector2(620f, 450f),
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
        };
        root.AddChild(scroll);

        _content = new VBoxContainer();
        scroll.AddChild(_content);
    }

    private void TogglePanel()
    {
        _panel.Visible = !_panel.Visible;
        LayoutOverlay();
        if (_panel.Visible)
        {
            Refresh();
        }
    }

    private void OpenPanelForTuning()
    {
        _panel.Visible = true;
        LayoutOverlay();
        Refresh();
    }

    private void LayoutOverlay()
    {
        var viewportSize = GetViewportRect().Size;
        _openButton.Position = new Vector2(
            viewportSize.X - ButtonWidth - ButtonRightMargin,
            ButtonTopMargin);

        _activeSkillButton.Position = _openButton.Position;

        _panel.Position = new Vector2(
            viewportSize.X - PanelWidth - PanelRightMargin,
            PanelTopMargin);
    }

    private void UpdateModeVisibility()
    {
        bool inCombat = CombatManager.Instance.IsInProgress;
        _openButton.Visible = !inCombat;
        _activeSkillButton.Visible = inCombat;

        if (inCombat && _panel.Visible)
        {
            _panel.Visible = false;
        }
    }

    private void UpdateActiveSkillButton()
    {
        if (_activeSkillButton == null || !_activeSkillButton.Visible)
        {
            return;
        }

        var player = GetLocalPlayer();
        if (player == null)
        {
            _activeSkillButton.Text = "声骸技";
            _activeSkillButton.Disabled = true;
            _activeSkillButton.TooltipText = "当前没有可用玩家";
            return;
        }

        var status = EchoActiveSkillService.GetStatus(player);
        _activeSkillButton.Text = status.RemainingCooldown > 0
            ? $"CD {status.RemainingCooldown}"
            : "声骸技";
        _activeSkillButton.Disabled = !status.CanUse;
        _activeSkillButton.TooltipText = status.Definition == null
            ? status.Reason
            : $"{GetLocalizedTextOrFallback(status.Definition.NameKey)}：{status.Reason}";
    }

    private void Refresh()
    {
        if (_content == null)
        {
            return;
        }

        foreach (var child in _content.GetChildren())
        {
            child.QueueFree();
        }

        var player = GetLocalPlayer();
        if (player == null)
        {
            _content.AddChild(MakeMutedLabel("当前没有可用玩家。进入跑图或战斗后再打开。"));
            return;
        }

        AddTuningSection(player);
        AddEquipmentSection(player);
        AddSonataSection(player);
        AddInventorySection(player);
    }

    private static Player? GetLocalPlayer()
    {
        var state = RunManager.Instance?.DebugOnlyGetState();
        return state == null ? null : LocalContext.GetMe(state.Players);
    }

    private void AddEquipmentSection(Player player)
    {
        _content.AddChild(MakeSectionTitle("装备槽"));

        var slots = EchoInventory.GetEquippedInstanceIds(player);
        for (var i = 0; i < EchoInventory.MaxEquipSlots; i++)
        {
            var slotIndex = i;
            var instance = EchoInventory.FindByInstanceId(player, slots[i]);
            var row = new HBoxContainer
            {
                CustomMinimumSize = new Vector2(600f, 34f),
            };

            row.AddChild(new Label
            {
                Text = $"槽位 {i + 1}",
                CustomMinimumSize = new Vector2(72f, 30f),
            });

            row.AddChild(new Label
            {
                Text = instance == null ? "未装备" : GetEchoDisplayName(instance),
                CustomMinimumSize = new Vector2(390f, 30f),
            });

            var clear = new Button
            {
                Text = "卸下",
                Disabled = instance == null,
                CustomMinimumSize = new Vector2(72f, 30f),
            };
            clear.Pressed += () =>
            {
                EchoInventory.Unequip(player, slotIndex);
                Refresh();
            };
            row.AddChild(clear);

            _content.AddChild(row);
        }
    }

    private void AddInventorySection(Player player)
    {
        _content.AddChild(MakeSectionTitle("库存"));

        var inventory = EchoInventory.GetAll(player);
        if (inventory.Count == 0)
        {
            _content.AddChild(MakeMutedLabel("暂无声骸。战斗胜利奖励中领取声骸后会出现在这里。"));
            return;
        }

        foreach (var instance in inventory)
        {
            _content.AddChild(MakeInventoryRow(player, instance));
        }
    }

    private void AddTuningSection(Player player)
    {
        if (!EchoTuningService.IsTuningModeActive(player))
        {
            return;
        }

        _content.AddChild(MakeSectionTitle("调谐"));
        _content.AddChild(MakeMutedLabel("本次火堆调谐已开启。选择一个声骸，消耗金币重骰它的唯一词条。"));
    }

    private void AddSonataSection(Player player)
    {
        _content.AddChild(MakeSectionTitle("合鸣"));

        var summaries = EchoCombatEffectService.GetActiveSonataSummaries(player);
        if (summaries.Count == 0)
        {
            _content.AddChild(MakeMutedLabel("当前已装备声骸还没有激活合鸣效果。"));
            return;
        }

        foreach (var summary in summaries)
        {
            var sonataName = GetLocalizedTextOrFallback(summary.Definition.NameKey);
            var breakpointsText = string.Join(
                " / ",
                summary.ActiveBreakpoints.Select(value => GetSonataBreakpointDisplayText(summary, value)));
            _content.AddChild(new Label
            {
                Text = $"{sonataName}：{summary.EquippedCount} 件，{breakpointsText}",
                CustomMinimumSize = new Vector2(600f, 52f),
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            });
        }
    }

    private Control MakeInventoryRow(Player player, EchoInstance instance)
    {
        var row = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(600f, 92f),
        };

        var top = new HBoxContainer();
        row.AddChild(top);

        top.AddChild(new Label
        {
            Text = GetEchoDisplayName(instance),
            CustomMinimumSize = new Vector2(290f, 30f),
        });

        top.AddChild(new Label
        {
            Text = EchoInventory.IsEquipped(player, instance) ? "已装备" : "未装备",
            CustomMinimumSize = new Vector2(70f, 30f),
        });

        for (var i = 0; i < EchoInventory.MaxEquipSlots; i++)
        {
            var slotIndex = i;
            var equip = new Button
            {
                Text = (i + 1).ToString(CultureInfo.InvariantCulture),
                TooltipText = $"装备到槽位 {i + 1}",
                CustomMinimumSize = new Vector2(42f, 30f),
            };
            equip.Pressed += () =>
            {
                EchoInventory.Equip(player, instance, slotIndex);
                Refresh();
            };
            top.AddChild(equip);
        }

        if (EchoTuningService.IsTuningModeActive(player))
        {
            var tuningCost = EchoTuningService.GetTuningCost(instance);
            var tune = new Button
            {
                Text = "调谐",
                TooltipText = $"消耗 {tuningCost} 金币重骰唯一词条",
                CustomMinimumSize = new Vector2(64f, 30f),
                Disabled = !EchoTuningService.CanTune(player, instance),
            };
            tune.Pressed += async () =>
            {
                await EchoTuningService.TryTuneEcho(player, instance);
                Refresh();
            };
            top.AddChild(tune);
        }

        row.AddChild(new Label
        {
            Text = GetAffixSummary(instance),
            CustomMinimumSize = new Vector2(590f, 44f),
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        });

        row.AddChild(new Label
        {
            Text = GetSonataSummary(instance),
            CustomMinimumSize = new Vector2(590f, 26f),
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        });

        return row;
    }

    private static Label MakeSectionTitle(string text)
    {
        return new Label
        {
            Text = $"[{text}]",
            CustomMinimumSize = new Vector2(600f, 34f),
        };
    }

    private static Label MakeMutedLabel(string text)
    {
        return new Label
        {
            Text = text,
            CustomMinimumSize = new Vector2(600f, 34f),
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
    }

    private static string GetEchoDisplayName(EchoInstance instance)
    {
        if (!EchoRegistry.TryGetEcho(instance.DefinitionId, out var definition))
        {
            return instance.DefinitionId;
        }

        var shortId = instance.InstanceId.Length > 8 ? instance.InstanceId[^8..] : instance.InstanceId;
        var localizedName = GetLocalizedTextOrFallback(definition.NameKey);
        return $"{localizedName}  Lv.{instance.Level}  #{shortId}";
    }

    private static string GetAffixSummary(EchoInstance instance)
    {
        if (instance.Affixes.Count == 0)
        {
            return "词条：无";
        }

        var parts = instance.Affixes.Select(affix =>
        {
            var name = EchoRegistry.TryGetAffix(affix.AffixId, out var definition)
                ? GetLocalizedTextOrFallback(definition.NameKey)
                : affix.AffixId;
            return $"{name} +{affix.Value:0.#}（档位{affix.Tier}）";
        });

        return "词条：" + string.Join(" / ", parts);
    }

    private static string GetSonataSummary(EchoInstance instance)
    {
        if (string.IsNullOrWhiteSpace(instance.SelectedSonataId))
        {
            return "合鸣：无";
        }

        if (EchoRegistry.TryGetSonata(instance.SelectedSonataId, out var sonata))
        {
            return $"合鸣：{GetLocalizedTextOrFallback(sonata.NameKey)}";
        }

        return $"合鸣：{instance.SelectedSonataId}";
    }

    private static string GetSonataBreakpointDisplayText(EchoCombatEffectService.ActiveSonataSummary summary, int requiredCount)
    {
        var breakpoint = summary.Definition.Breakpoints.FirstOrDefault(item => item.RequiredCount == requiredCount);
        if (breakpoint == null)
        {
            return $"{requiredCount}件";
        }

        return GetLocalizedTextOrFallback(breakpoint.DescriptionKey);
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
            "ECHO_CORE_UNIVERSAL_RESONANCE.description" => "Echo Core MVP 使用的通用合鸣占位。",
            "ECHO_CORE_UNIVERSAL_RESONANCE.breakpoint_2" => "2件：开战获得4点格挡。",
            "ECHO_CORE_UNIVERSAL_RESONANCE.breakpoint_3" => "3件：额外获得1点力量。",
            "ECHO_CORE_UNIVERSAL_RESONANCE.breakpoint_5" => "5件：额外获得1点敏捷。",
            "ECHO_CORE_HIDDEN_LIGHT.name" => "隐世回光",
            "ECHO_CORE_HIDDEN_LIGHT.description" => "适合恢复与稳态收益的合鸣占位。",
            "ECHO_CORE_HIDDEN_LIGHT.breakpoint_2" => "2件：开战回复1点生命。",
            "ECHO_CORE_HIDDEN_LIGHT.breakpoint_3" => "3件：额外获得3点格挡。",
            "ECHO_CORE_HIDDEN_LIGHT.breakpoint_5" => "5件：额外获得1点敏捷。",
            _ => key,
        };
    }
}
