using EchoCore.Scripts.Affixes;
using EchoCore.Scripts.Developer;
using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Registry;
using EchoCore.Scripts.Services;
using Godot;

namespace EchoCore.Scripts.UI;

/// <summary>
/// 声骸开发者菜单。
/// 第一版只覆盖“自定义组合后添加到库存”，不耦合装备、战斗和仓库复杂交互。
/// </summary>
public sealed partial class EchoDeveloperMenu : Control
{
    private const float PanelWidth = 520f;
    private const float PanelHeight = 460f;

    private readonly List<EchoDefinition> _echoes = [];
    private readonly List<EchoAffixDefinition> _affixes = [];
    private readonly List<Sonata.SonataDefinition> _currentSonatas = [];
    private readonly List<EchoAffixTierDefinition> _currentTiers = [];

    private PanelContainer _panel = null!;
    private OptionButton _echoOption = null!;
    private OptionButton _sonataOption = null!;
    private OptionButton _affixOption = null!;
    private OptionButton _tierOption = null!;
    private Label _previewLabel = null!;
    private Label _feedbackLabel = null!;

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        BuildUi();
        LayoutPanel();
        ReloadOptions();
        Visible = false;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            LayoutPanel();
        }
    }

    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            LayoutPanel();
            ReloadOptions();
        }
    }

    public void Close()
    {
        Visible = false;
    }

    private void BuildUi()
    {
        var backdrop = new ColorRect
        {
            Color = new Color(0f, 0f, 0f, 0.52f),
            MouseFilter = MouseFilterEnum.Stop,
        };
        backdrop.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        backdrop.GuiInput += OnBackdropGuiInput;
        AddChild(backdrop);

        _panel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(PanelWidth, PanelHeight),
            MouseFilter = MouseFilterEnum.Stop,
        };
        _panel.SetAnchorsPreset(LayoutPreset.TopLeft);
        _panel.Size = new Vector2(PanelWidth, PanelHeight);
        AddChild(_panel);

        var margin = new MarginContainer();
        margin.AddThemeConstantOverride("margin_left", 20);
        margin.AddThemeConstantOverride("margin_top", 20);
        margin.AddThemeConstantOverride("margin_right", 20);
        margin.AddThemeConstantOverride("margin_bottom", 20);
        _panel.AddChild(margin);

        var content = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
        };
        content.AddThemeConstantOverride("separation", 12);
        margin.AddChild(content);

        content.AddChild(CreateHeaderRow());
        _echoOption = CreateDropdownRow(content, "声骸");
        _sonataOption = CreateDropdownRow(content, "合鸣");
        _affixOption = CreateDropdownRow(content, "词条");
        _tierOption = CreateDropdownRow(content, "档位");

        _previewLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            CustomMinimumSize = new Vector2(0f, 96f),
            VerticalAlignment = VerticalAlignment.Top,
        };
        content.AddChild(_previewLabel);

        _feedbackLabel = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            Modulate = new Color(1f, 0.94f, 0.55f),
        };
        content.AddChild(_feedbackLabel);

        var buttonRow = new HBoxContainer();
        buttonRow.AddThemeConstantOverride("separation", 12);
        content.AddChild(buttonRow);

        var addButton = new Button
        {
            Text = "添加到背包",
            CustomMinimumSize = new Vector2(180f, 40f),
        };
        addButton.Pressed += OnAddPressed;
        buttonRow.AddChild(addButton);

        var closeButton = new Button
        {
            Text = "关闭",
            CustomMinimumSize = new Vector2(100f, 40f),
        };
        closeButton.Pressed += Close;
        buttonRow.AddChild(closeButton);

        _echoOption.ItemSelected += _ =>
        {
            RefreshSonatas();
            UpdatePreview();
        };
        _sonataOption.ItemSelected += _ => UpdatePreview();
        _affixOption.ItemSelected += _ =>
        {
            RefreshTiers();
            UpdatePreview();
        };
        _tierOption.ItemSelected += _ => UpdatePreview();
    }

    private Control CreateHeaderRow()
    {
        var row = new HBoxContainer();
        row.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        var title = new Label
        {
            Text = "声骸开发者菜单",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        row.AddChild(title);

        return row;
    }

    private OptionButton CreateDropdownRow(VBoxContainer parent, string title)
    {
        var label = new Label { Text = title };
        parent.AddChild(label);

        var option = new OptionButton
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            FitToLongestItem = false,
            CustomMinimumSize = new Vector2(0f, 34f),
        };
        parent.AddChild(option);
        return option;
    }

    private void ReloadOptions()
    {
        _echoes.Clear();
        _echoes.AddRange(EchoDeveloperService.GetAvailableEchoes());

        _affixes.Clear();
        _affixes.AddRange(EchoDeveloperService.GetAvailableAffixes());

        RebuildEchoOptions();
        RebuildAffixOptions();
        RefreshSonatas();
        RefreshTiers();
        UpdatePreview();
        _feedbackLabel.Text = string.Empty;
    }

    private void RebuildEchoOptions()
    {
        _echoOption.Clear();
        for (int i = 0; i < _echoes.Count; i++)
        {
            _echoOption.AddItem(EchoUiTextService.GetEchoTitle(_echoes[i]), i);
        }
    }

    private void RebuildAffixOptions()
    {
        _affixOption.Clear();
        for (int i = 0; i < _affixes.Count; i++)
        {
            _affixOption.AddItem(EchoUiTextService.GetLocalizedTextOrFallback(_affixes[i].NameKey), i);
        }
    }

    private void RefreshSonatas()
    {
        _currentSonatas.Clear();
        _sonataOption.Clear();

        EchoDefinition? definition = GetSelectedEchoDefinition();
        if (definition == null)
        {
            _sonataOption.Disabled = true;
            return;
        }

        _currentSonatas.AddRange(EchoDeveloperService.GetAvailableSonatas(definition));
        if (_currentSonatas.Count == 0)
        {
            _sonataOption.Disabled = true;
            _sonataOption.AddItem("无", 0);
            return;
        }

        _sonataOption.Disabled = false;
        for (int i = 0; i < _currentSonatas.Count; i++)
        {
            _sonataOption.AddItem(EchoUiTextService.GetLocalizedTextOrFallback(_currentSonatas[i].NameKey), i);
        }
    }

    private void RefreshTiers()
    {
        _currentTiers.Clear();
        _tierOption.Clear();

        EchoAffixDefinition? affix = GetSelectedAffixDefinition();
        if (affix == null)
        {
            _tierOption.Disabled = true;
            return;
        }

        _currentTiers.AddRange(affix.Tiers.OrderBy(tier => tier.Tier));
        _tierOption.Disabled = _currentTiers.Count == 0;
        for (int i = 0; i < _currentTiers.Count; i++)
        {
            EchoAffixTierDefinition tier = _currentTiers[i];
            _tierOption.AddItem($"档位 {tier.Tier}  (+{tier.Value:0.#})", i);
        }
    }

    private void UpdatePreview()
    {
        EchoDefinition? definition = GetSelectedEchoDefinition();
        EchoAffixDefinition? affix = GetSelectedAffixDefinition();
        EchoAffixTierDefinition? tier = GetSelectedTierDefinition();

        if (definition == null || affix == null || tier == null)
        {
            _previewLabel.Text = "当前没有可用配置。";
            return;
        }

        string sonataText = "无";
        Sonata.SonataDefinition? sonata = GetSelectedSonataDefinition();
        if (sonata != null)
        {
            sonataText = EchoUiTextService.GetLocalizedTextOrFallback(sonata.NameKey);
        }

        _previewLabel.Text =
            $"声骸：{EchoUiTextService.GetEchoTitle(definition)}\n" +
            $"合鸣：{sonataText}\n" +
            $"词条：{EchoUiTextService.GetLocalizedTextOrFallback(affix.NameKey)} +{tier.Value:0.#}";
    }

    private void OnAddPressed()
    {
        EchoDefinition? definition = GetSelectedEchoDefinition();
        EchoAffixDefinition? affix = GetSelectedAffixDefinition();
        EchoAffixTierDefinition? tier = GetSelectedTierDefinition();
        if (definition == null || affix == null || tier == null)
        {
            _feedbackLabel.Text = "当前选择不完整，无法添加。";
            return;
        }

        var request = new EchoDeveloperGrantRequest(
            definition.Id,
            GetSelectedSonataDefinition()?.Id,
            affix.Id,
            tier.Tier);

        bool success = EchoDeveloperService.TryGrantToLocalPlayer(request, out string message);
        _feedbackLabel.Text = message;
        _feedbackLabel.Modulate = success ? new Color(0.75f, 1f, 0.75f) : new Color(1f, 0.7f, 0.7f);
    }

    private void OnBackdropGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton button && button.Pressed && button.ButtonIndex == MouseButton.Left)
        {
            Close();
        }
    }

    private void LayoutPanel()
    {
        if (_panel == null)
        {
            return;
        }

        // 开发者菜单固定按屏幕中心摆放，避免受父节点布局和锚点预设影响而漂移到左上角。
        Vector2 panelSize = new(PanelWidth, PanelHeight);
        Vector2 viewportSize = GetViewportRect().Size;
        _panel.Size = panelSize;
        _panel.Position = (viewportSize - panelSize) * 0.5f;
    }

    private EchoDefinition? GetSelectedEchoDefinition()
    {
        int index = _echoOption.Selected;
        return index >= 0 && index < _echoes.Count ? _echoes[index] : null;
    }

    private Sonata.SonataDefinition? GetSelectedSonataDefinition()
    {
        int index = _sonataOption.Selected;
        return index >= 0 && index < _currentSonatas.Count ? _currentSonatas[index] : null;
    }

    private EchoAffixDefinition? GetSelectedAffixDefinition()
    {
        int index = _affixOption.Selected;
        return index >= 0 && index < _affixes.Count ? _affixes[index] : null;
    }

    private EchoAffixTierDefinition? GetSelectedTierDefinition()
    {
        int index = _tierOption.Selected;
        return index >= 0 && index < _currentTiers.Count ? _currentTiers[index] : null;
    }
}
