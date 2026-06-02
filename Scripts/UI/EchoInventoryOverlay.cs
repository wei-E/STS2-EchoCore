using EchoCore.Scripts.Echoes;
using EchoCore.Scripts.Registry;
using EchoCore.Scripts.Services;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Runs;

namespace EchoCore.Scripts.UI;

/// <summary>
/// 声骸仓库 UI。
/// 当前版本优先复用已有素材，先把三栏结构和核心交互跑通，缺失的正式美术先用文字与纯色占位。
/// </summary>
public sealed partial class EchoInventoryOverlay : Control
{
    private const string NodeName = "EchoCoreInventoryOverlay";
    private const string LayoutBackgroundPath = "res://echo-core/ui/echoes/layout/inventory_bg.png";
    private const string LayoutSidebarPath = "res://echo-core/ui/echoes/layout/equipment_sidebar.png";
    private const string InventoryCardFramePath = "res://echo-core/ui/echoes/layout/inventory_card_frame.png";
    private const string DefaultIconPath = "res://echo-core/ui/echoes/icons/default_echo_icon.png";
    private const string Cost1IconPath = "res://echo-core/ui/echoes/icons/cost_1.png";
    private const string Cost3IconPath = "res://echo-core/ui/echoes/icons/cost_3.png";
    private const string Cost4IconPath = "res://echo-core/ui/echoes/icons/cost_4.png";
    private const float ButtonWidth = 116f;
    private const float ButtonHeight = 44f;
    private const float ButtonRightMargin = 24f;
    private const float ButtonTopMargin = 88f;
    private const float SidebarSourceWidth = 139f;
    private const float SidebarSourceHeight = 889f;
    private const float SidebarSafeLeft = 28f;
    private const float SidebarAvatarInset = 4f;
    private const float BackgroundSourceWidth = 977f;
    private const float BackgroundSourceHeight = 889f;
    private const float PanelWidthFactor = 0.90f;
    private const float PanelHeightFactor = 0.84f;
    private const float PanelMaxWidth = 1860f;
    private const float PanelMaxHeight = 980f;

    private static readonly Rect2 SidebarCostRect = new(25f, 65f, 85f, 29f);
    private static readonly Rect2[] SidebarSlotRects =
    [
        new Rect2(21f, 166f, 94f, 100f),
        new Rect2(33f, 328f, 70f, 68f),
        new Rect2(32f, 422f, 70f, 68f),
        new Rect2(32f, 514f, 70f, 68f),
        new Rect2(31f, 609f, 70f, 68f),
    ];

    private readonly List<Button> _slotButtons = [];
    private readonly Dictionary<string, Button> _inventoryButtonsByInstanceId = new(StringComparer.Ordinal);

    private Button _openButton = null!;
    private Button _activeSkillButton = null!;
    private Control _panel = null!;
    private PanelContainer _windowPanel = null!;
    private TextureRect _backgroundTexture = null!;
    private Control _layoutRoot = null!;
    private TextureRect _sidebarTexture = null!;
    private Label _costLabel = null!;
    private Label _selectionHintLabel = null!;
    private Label _feedbackLabel = null!;
    private GridContainer _inventoryGrid = null!;
    private Label _inventoryEmptyLabel = null!;
    private Label _detailTitleLabel = null!;
    private Label _detailMetaLabel = null!;
    private Label _detailClassLabel = null!;
    private Label _detailDescriptionLabel = null!;
    private Label _detailSkillLabel = null!;
    private Label _detailAffixLabel = null!;
    private Label _detailSonataLabel = null!;
    private Button _unequipButton = null!;
    private Button _tuneButton = null!;

    private string? _selectedInstanceId;
    private string? _feedbackMessage;

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
        // Godot CanvasItem 的 ZIndex 有上限，超过后会在读档进入 NRun 时直接报错。
        // 这里保持较高层级即可，不需要逼近引擎极限。
        ZIndex = 1024;
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
            TooltipText = "打开声骸仓库",
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
        _panel = new Control
        {
            Visible = false,
            MouseFilter = MouseFilterEnum.Stop,
            TopLevel = true,
        };
        _panel.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(_panel);

        _windowPanel = new PanelContainer
        {
            MouseFilter = MouseFilterEnum.Ignore,
            ClipContents = true,
        };
        _windowPanel.AddThemeStyleboxOverride("panel", CreateWindowChromePanel());
        _panel.AddChild(_windowPanel);

        _backgroundTexture = new TextureRect
        {
            Texture = LoadTexture(LayoutBackgroundPath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            Modulate = new Color(1f, 1f, 1f, 0.94f),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _windowPanel.AddChild(_backgroundTexture);

        var dim = new ColorRect
        {
            Color = new Color(0f, 0f, 0f, 0.12f),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _windowPanel.AddChild(dim);

        _layoutRoot = new Control
        {
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _windowPanel.AddChild(_layoutRoot);

        BuildSidebar();
        BuildInventoryColumn();
        BuildDetailsColumn();

        var closeButton = new Button
        {
            Text = "关闭",
            CustomMinimumSize = new Vector2(92f, 38f),
            MouseFilter = MouseFilterEnum.Stop,
            TopLevel = true,
        };
        closeButton.Pressed += () => _panel.Visible = false;
        _panel.AddChild(closeButton);
        closeButton.SetMeta("EchoCoreCloseButton", true);
    }

    private void BuildSidebar()
    {
        _sidebarTexture = new TextureRect
        {
            Texture = LoadTexture(LayoutSidebarPath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _layoutRoot.AddChild(_sidebarTexture);

        _costLabel = new Label
        {
            Text = "0/12",
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _costLabel.AddThemeColorOverride("font_color", new Color("f3df92"));
        _costLabel.AddThemeFontSizeOverride("font_size", 20);
        _layoutRoot.AddChild(_costLabel);

        _selectionHintLabel = new Label
        {
            Text = "选择中间声骸后，点击左侧槽位装备。",
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _selectionHintLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.78f));
        _selectionHintLabel.AddThemeFontSizeOverride("font_size", 13);
        _layoutRoot.AddChild(_selectionHintLabel);

        for (var i = 0; i < EchoInventory.MaxEquipSlots; i++)
        {
            var slotIndex = i;
            var slotButton = new Button
            {
                Text = string.Empty,
                MouseFilter = MouseFilterEnum.Stop,
                Flat = true,
                FocusMode = FocusModeEnum.None,
            };
            slotButton.Pressed += () => OnSlotPressed(slotIndex);
            _slotButtons.Add(slotButton);
            _layoutRoot.AddChild(slotButton);
        }
    }

    private void BuildInventoryColumn()
    {
        var title = CreateSectionTitle("声骸列表");
        title.Name = "InventoryTitle";
        _layoutRoot.AddChild(title);

        var scroll = new ScrollContainer
        {
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            MouseFilter = MouseFilterEnum.Stop,
        };
        scroll.Name = "InventoryScroll";
        _layoutRoot.AddChild(scroll);

        var inventoryWrap = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        scroll.AddChild(inventoryWrap);

        _inventoryEmptyLabel = new Label
        {
            Text = "暂无声骸。战斗胜利奖励中领取声骸后会出现在这里。",
            Visible = false,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        _inventoryEmptyLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.8f));
        _inventoryEmptyLabel.AddThemeFontSizeOverride("font_size", 16);
        inventoryWrap.AddChild(_inventoryEmptyLabel);

        _inventoryGrid = new GridContainer
        {
            Columns = 3,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _inventoryGrid.AddThemeConstantOverride("h_separation", 18);
        _inventoryGrid.AddThemeConstantOverride("v_separation", 18);
        inventoryWrap.AddChild(_inventoryGrid);
    }

    private void BuildDetailsColumn()
    {
        var title = CreateSectionTitle("声骸详情");
        title.Name = "DetailTitle";
        _layoutRoot.AddChild(title);

        var detailPanel = new PanelContainer
        {
            MouseFilter = MouseFilterEnum.Pass,
        };
        detailPanel.Name = "DetailPanel";
        detailPanel.AddThemeStyleboxOverride("panel", CreateGlassPanel(new Color(0.06f, 0.09f, 0.14f, 0.72f)));
        _layoutRoot.AddChild(detailPanel);

        var detailScroll = new ScrollContainer
        {
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
            MouseFilter = MouseFilterEnum.Pass,
        };
        detailPanel.AddChild(detailScroll);

        var detailContent = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
        };
        detailContent.AddThemeConstantOverride("separation", 14);
        detailScroll.AddChild(detailContent);

        _detailTitleLabel = new Label
        {
            Text = "未选择声骸",
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        _detailTitleLabel.AddThemeColorOverride("font_color", new Color("f3df92"));
        _detailTitleLabel.AddThemeFontSizeOverride("font_size", 30);
        detailContent.AddChild(_detailTitleLabel);

        var costRow = new HBoxContainer();
        costRow.AddThemeConstantOverride("separation", 12);
        detailContent.AddChild(costRow);

        _detailMetaLabel = new Label
        {
            Text = "COST 0",
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _detailMetaLabel.SizeFlagsHorizontal = SizeFlags.Fill;
        _detailMetaLabel.AddThemeColorOverride("font_color", new Color("f3df92"));
        _detailMetaLabel.AddThemeFontSizeOverride("font_size", 28);
        costRow.AddChild(_detailMetaLabel);

        _detailClassLabel = new Label
        {
            Text = "普通",
            MouseFilter = MouseFilterEnum.Ignore,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        _detailClassLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _detailClassLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.85f));
        _detailClassLabel.AddThemeFontSizeOverride("font_size", 18);
        costRow.AddChild(_detailClassLabel);

        _detailDescriptionLabel = CreateBodyLabel();
        detailContent.AddChild(_detailDescriptionLabel);

        detailContent.AddChild(CreateSubsectionTitle("声骸技能"));
        _detailSkillLabel = CreateBodyLabel();
        detailContent.AddChild(_detailSkillLabel);

        detailContent.AddChild(CreateSubsectionTitle("词条"));
        _detailAffixLabel = CreateBodyLabel();
        detailContent.AddChild(_detailAffixLabel);

        detailContent.AddChild(CreateSubsectionTitle("合鸣效果"));
        _detailSonataLabel = CreateBodyLabel();
        detailContent.AddChild(_detailSonataLabel);

        var actionRow = new HBoxContainer();
        actionRow.AddThemeConstantOverride("separation", 12);
        detailContent.AddChild(actionRow);

        _unequipButton = new Button
        {
            Text = "卸下",
            CustomMinimumSize = new Vector2(110f, 40f),
            MouseFilter = MouseFilterEnum.Stop,
        };
        _unequipButton.Pressed += OnUnequipSelected;
        actionRow.AddChild(_unequipButton);

        _tuneButton = new Button
        {
            Text = "调谐",
            CustomMinimumSize = new Vector2(110f, 40f),
            MouseFilter = MouseFilterEnum.Stop,
        };
        _tuneButton.Pressed += async () => await OnTuneSelected();
        actionRow.AddChild(_tuneButton);

        _feedbackLabel = new Label
        {
            Text = string.Empty,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
        };
        _feedbackLabel.AddThemeColorOverride("font_color", new Color("f3df92"));
        _feedbackLabel.AddThemeFontSizeOverride("font_size", 15);
        detailContent.AddChild(_feedbackLabel);
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

        _panel.Size = viewportSize;
        Rect2 panelRect = CalculateCenteredPanelRect(viewportSize);
        _windowPanel.Position = panelRect.Position;
        _windowPanel.Size = panelRect.Size;
        _backgroundTexture.Position = Vector2.Zero;
        _backgroundTexture.Size = panelRect.Size;

        if (_windowPanel.GetChildOrNull<ColorRect>(1) is { } dim)
        {
            dim.Position = Vector2.Zero;
            dim.Size = panelRect.Size;
        }

        _layoutRoot.Position = Vector2.Zero;
        _layoutRoot.Size = panelRect.Size;

        float sidebarScale = panelRect.Size.Y / SidebarSourceHeight;
        float sidebarWidth = SidebarSourceWidth * sidebarScale;
        float safeLeft = SidebarSafeLeft;
        float safeTop = 24f;
        float safeBottom = 28f;
        float usableHeight = panelRect.Size.Y - safeTop - safeBottom;
        float rightWidth = MathF.Min(460f, panelRect.Size.X * 0.30f);
        float centerLeft = safeLeft + sidebarWidth + 36f;
        float centerWidth = MathF.Max(360f, panelRect.Size.X - centerLeft - rightWidth - 56f);
        float rightLeft = centerLeft + centerWidth + 28f;

        _sidebarTexture.Position = new Vector2(safeLeft, 0f);
        _sidebarTexture.Size = new Vector2(sidebarWidth, panelRect.Size.Y);

        _costLabel.Position = ScaleSidebarRect(SidebarCostRect, sidebarScale).Position;
        _costLabel.Size = ScaleSidebarRect(SidebarCostRect, sidebarScale).Size;
        _costLabel.AddThemeFontSizeOverride("font_size", Mathf.RoundToInt(20f * sidebarScale));
        _costLabel.Position += new Vector2(safeLeft, 0f);

        _selectionHintLabel.Position = ScaleSidebarPoint(10f, 810f, sidebarScale);
        _selectionHintLabel.Size = new Vector2(118f * sidebarScale, 58f * sidebarScale);
        _selectionHintLabel.AddThemeFontSizeOverride("font_size", Mathf.RoundToInt(13f * sidebarScale));
        _selectionHintLabel.Position += new Vector2(safeLeft, 0f);

        LayoutSlotButtons(sidebarScale, safeLeft);

        PositionNamedNode("InventoryTitle", new Vector2(centerLeft, safeTop + 18f), new Vector2(centerWidth, 42f));
        PositionNamedNode("InventoryScroll", new Vector2(centerLeft, safeTop + 72f), new Vector2(centerWidth, usableHeight - 78f));
        PositionNamedNode("DetailTitle", new Vector2(rightLeft, safeTop + 18f), new Vector2(rightWidth, 42f));
        PositionNamedNode("DetailPanel", new Vector2(rightLeft, safeTop + 72f), new Vector2(rightWidth, usableHeight - 78f));

        if (_panel.GetChildren().OfType<Button>().FirstOrDefault(node => node.HasMeta("EchoCoreCloseButton")) is { } closeButton)
        {
            closeButton.Position = new Vector2(panelRect.Size.X - 132f, safeTop + 2f);
            closeButton.Position += panelRect.Position;
        }
    }

    private void LayoutSlotButtons(float sidebarScale, float sidebarLeft)
    {
        for (var i = 0; i < _slotButtons.Count; i++)
        {
            Rect2 scaledRect = ScaleSidebarRect(SidebarSlotRects[i], sidebarScale);
            _slotButtons[i].Position = scaledRect.Position + new Vector2(sidebarLeft, 0f);
            _slotButtons[i].Size = scaledRect.Size;
        }
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
            : $"{EchoUiTextService.GetLocalizedTextOrFallback(status.Definition.NameKey)}：{status.Reason}";
    }

    private void Refresh()
    {
        var player = GetLocalPlayer();
        if (player == null)
        {
            _inventoryEmptyLabel.Visible = true;
            _inventoryEmptyLabel.Text = "当前没有可用玩家。进入跑图或战斗后再打开。";
            ClearContainer(_inventoryGrid);
            RefreshDetails(null, null);
            return;
        }

        var inventory = EchoInventory.GetAll(player);
        EnsureSelection(player, inventory);
        RefreshSidebar(player);
        RefreshInventory(player, inventory);
        RefreshDetails(player, GetSelectedInstance(player));
    }

    private static Player? GetLocalPlayer()
    {
        var state = RunManager.Instance?.DebugOnlyGetState();
        return state == null ? null : LocalContext.GetMe(state.Players);
    }

    private void EnsureSelection(Player player, IReadOnlyList<EchoInstance> inventory)
    {
        if (!string.IsNullOrWhiteSpace(_selectedInstanceId)
            && EchoInventory.FindByInstanceId(player, _selectedInstanceId) != null)
        {
            return;
        }

        string? selectedEquipped = EchoInventory.GetEquippedInstanceIds(player).FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));
        _selectedInstanceId = selectedEquipped ?? inventory.FirstOrDefault()?.InstanceId;
    }

    private void RefreshSidebar(Player player)
    {
        int currentCost = EchoInventory.GetEquippedCost(player);
        _costLabel.Text = $"{currentCost}/{EchoInventory.MaxTotalCost}";

        bool tuningActive = EchoTuningService.IsTuningModeActive(player);
        _selectionHintLabel.Text = tuningActive
            ? "火堆调谐中。先选声骸，再在右侧点击调谐。"
            : "先选中间声骸，再点击左侧槽位装备。";

        var slots = EchoInventory.GetEquippedInstanceIds(player);
        for (var i = 0; i < _slotButtons.Count; i++)
        {
            var button = _slotButtons[i];
            var instance = EchoInventory.FindByInstanceId(player, slots[i]);
            button.TooltipText = instance == null
                ? $"槽位 {i + 1}（空）"
                : $"槽位 {i + 1}：{EchoUiTextService.GetEchoDisplayName(instance)}";

            ApplySlotVisual(button, instance, i == 0, instance?.InstanceId == _selectedInstanceId);
        }
    }

    private void RefreshInventory(Player player, IReadOnlyList<EchoInstance> inventory)
    {
        ClearContainer(_inventoryGrid);
        _inventoryButtonsByInstanceId.Clear();

        _inventoryEmptyLabel.Visible = inventory.Count == 0;
        if (inventory.Count == 0)
        {
            return;
        }

        foreach (var instance in inventory)
        {
            var cardButton = CreateInventoryCard(player, instance);
            _inventoryButtonsByInstanceId[instance.InstanceId] = cardButton;
            _inventoryGrid.AddChild(cardButton);
        }
    }

    private Button CreateInventoryCard(Player player, EchoInstance instance)
    {
        var button = new Button
        {
            MouseFilter = MouseFilterEnum.Stop,
            Flat = true,
            FocusMode = FocusModeEnum.None,
            CustomMinimumSize = new Vector2(136f, 136f),
            Text = string.Empty,
            TooltipText = $"{EchoUiTextService.GetEchoDisplayName(instance)}\n{EchoUiTextService.GetAffixSummary(instance)}",
        };
        button.Pressed += () =>
        {
            _selectedInstanceId = instance.InstanceId;
            _feedbackMessage = null;
            Refresh();
        };

        var frame = new TextureRect
        {
            Texture = LoadTexture(InventoryCardFramePath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.Scale,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        frame.SetAnchorsPreset(LayoutPreset.FullRect);
        button.AddChild(frame);

        var portrait = new TextureRect
        {
            Texture = LoadTexture(GetDefinitionOrNull(instance)?.IconPath ?? DefaultIconPath) ?? LoadTexture(DefaultIconPath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore,
            Position = new Vector2(12f, 10f),
            Size = new Vector2(112f, 92f),
        };
        button.AddChild(portrait);

        var badge = new ColorRect
        {
            Color = instance.InstanceId == _selectedInstanceId
                ? new Color(0.94f, 0.84f, 0.52f, 0.28f)
                : new Color(0f, 0f, 0f, 0f),
            MouseFilter = MouseFilterEnum.Ignore,
            Position = new Vector2(5f, 5f),
            Size = new Vector2(126f, 126f),
        };
        button.AddChild(badge);

        if (EchoInventory.IsEquipped(player, instance))
        {
            var equippedLabel = new Label
            {
                Text = "已装备",
                Position = new Vector2(8f, 8f),
                Size = new Vector2(58f, 20f),
                MouseFilter = MouseFilterEnum.Ignore,
            };
            equippedLabel.AddThemeFontSizeOverride("font_size", 12);
            equippedLabel.AddThemeColorOverride("font_color", new Color("f3df92"));
            button.AddChild(equippedLabel);
        }

        var nameLabel = new Label
        {
            Text = GetInventoryCardName(instance),
            Position = new Vector2(10f, 94f),
            Size = new Vector2(98f, 32f),
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        nameLabel.AddThemeFontSizeOverride("font_size", 12);
        nameLabel.AddThemeColorOverride("font_color", Colors.White);
        button.AddChild(nameLabel);

        var costLabel = new Label
        {
            Text = GetDefinitionOrNull(instance)?.Cost.ToString() ?? "?",
            Position = new Vector2(108f, 96f),
            Size = new Vector2(20f, 24f),
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        costLabel.AddThemeFontSizeOverride("font_size", 18);
        costLabel.AddThemeColorOverride("font_color", new Color("f3df92"));
        button.AddChild(costLabel);

        return button;
    }

    private void RefreshDetails(Player? player, EchoInstance? instance)
    {
        if (player == null || instance == null)
        {
            _detailTitleLabel.Text = "未选择声骸";
            _detailMetaLabel.Text = "COST 0";
            _detailClassLabel.Text = "未选择";
            _detailDescriptionLabel.Text = "从中间列表选择一个声骸后，这里会显示它的说明、技能、词条与合鸣信息。";
            _detailSkillLabel.Text = "暂无";
            _detailAffixLabel.Text = "暂无";
            _detailSonataLabel.Text = "暂无";
            _unequipButton.Disabled = true;
            _tuneButton.Disabled = true;
            _feedbackLabel.Text = _feedbackMessage ?? string.Empty;
            return;
        }

        var definition = GetDefinitionOrNull(instance);
        _detailTitleLabel.Text = definition == null ? instance.DefinitionId : EchoUiTextService.GetEchoTitle(definition);
        _detailMetaLabel.Text = $"COST {definition?.Cost ?? 0}";
        _detailClassLabel.Text = GetClassDisplayText(definition?.Class);
        _detailDescriptionLabel.Text = definition == null
            ? "定义缺失。"
            : EchoUiTextService.GetEchoDescription(definition);
        _detailSkillLabel.Text = definition == null
            ? "未找到主动技定义。"
            : EchoUiTextService.GetSkillSummary(definition);
        _detailAffixLabel.Text = EchoUiTextService.GetAffixDetailSummary(instance);
        _detailSonataLabel.Text = EchoUiTextService.GetSonataDetailSummary(player, instance);
        _unequipButton.Disabled = !EchoInventory.IsEquipped(player, instance);
        _tuneButton.Disabled = !EchoTuningService.CanTune(player, instance);
        _tuneButton.Text = EchoTuningService.IsTuningModeActive(player)
            ? $"调谐 ({EchoTuningService.GetTuningCost(instance)})"
            : "调谐";
        _feedbackLabel.Text = _feedbackMessage ?? string.Empty;
    }

    private void OnSlotPressed(int slotIndex)
    {
        var player = GetLocalPlayer();
        if (player == null)
        {
            return;
        }

        var selected = GetSelectedInstance(player);
        if (selected == null)
        {
            _feedbackMessage = "先从中间列表选择一个声骸。";
            Refresh();
            return;
        }

        if (!EchoInventory.TryEquip(player, selected, slotIndex, out string failureReason))
        {
            _feedbackMessage = $"无法装备到槽位 {slotIndex + 1}。{failureReason}";
            Refresh();
            return;
        }

        _selectedInstanceId = selected.InstanceId;
        _feedbackMessage = $"已将 {GetInventoryCardName(selected)} 装备到槽位 {slotIndex + 1}。";
        Refresh();
    }

    private void OnUnequipSelected()
    {
        var player = GetLocalPlayer();
        if (player == null)
        {
            return;
        }

        var selected = GetSelectedInstance(player);
        if (selected == null)
        {
            return;
        }

        var slots = EchoInventory.GetEquippedInstanceIds(player);
        for (var i = 0; i < slots.Count; i++)
        {
            if (string.Equals(slots[i], selected.InstanceId, StringComparison.Ordinal))
            {
                EchoInventory.Unequip(player, i);
                _feedbackMessage = $"已卸下 {GetInventoryCardName(selected)}。";
                Refresh();
                return;
            }
        }

        _feedbackMessage = "当前选中的声骸未装备。";
        Refresh();
    }

    private async Task OnTuneSelected()
    {
        var player = GetLocalPlayer();
        if (player == null)
        {
            return;
        }

        var selected = GetSelectedInstance(player);
        if (selected == null)
        {
            return;
        }

        if (!EchoTuningService.IsTuningModeActive(player))
        {
            _feedbackMessage = "当前不在火堆调谐模式。";
            Refresh();
            return;
        }

        bool success = await EchoTuningService.TryTuneEcho(player, selected);
        _feedbackMessage = success
            ? $"已调谐 {GetInventoryCardName(selected)}。"
            : "调谐失败，可能是金币不足或当前声骸不可调谐。";
        Refresh();
    }

    private EchoInstance? GetSelectedInstance(Player player)
    {
        return string.IsNullOrWhiteSpace(_selectedInstanceId)
            ? null
            : EchoInventory.FindByInstanceId(player, _selectedInstanceId);
    }

    private static void ClearContainer(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            child.QueueFree();
        }
    }

    private static Label CreateSectionTitle(string text)
    {
        var label = new Label
        {
            Text = text,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeFontSizeOverride("font_size", 30);
        return label;
    }

    private static Label CreateSubsectionTitle(string text)
    {
        var label = new Label
        {
            Text = text,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        label.AddThemeColorOverride("font_color", new Color("f3df92"));
        label.AddThemeFontSizeOverride("font_size", 21);
        return label;
    }

    private static Label CreateBodyLabel()
    {
        var label = new Label
        {
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        label.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f, 0.92f));
        label.AddThemeFontSizeOverride("font_size", 17);
        return label;
    }

    private static StyleBoxFlat CreateGlassPanel(Color color)
    {
        var style = new StyleBoxFlat
        {
            BgColor = color,
            CornerRadiusTopLeft = 12,
            CornerRadiusTopRight = 12,
            CornerRadiusBottomLeft = 12,
            CornerRadiusBottomRight = 12,
            BorderWidthLeft = 1,
            BorderWidthTop = 1,
            BorderWidthRight = 1,
            BorderWidthBottom = 1,
            BorderColor = new Color(1f, 1f, 1f, 0.15f),
            ContentMarginLeft = 18,
            ContentMarginTop = 18,
            ContentMarginRight = 18,
            ContentMarginBottom = 18,
        };
        return style;
    }

    private static StyleBoxFlat CreateWindowChromePanel()
    {
        var style = new StyleBoxFlat
        {
            BgColor = new Color(0.02f, 0.04f, 0.07f, 0.88f),
            CornerRadiusTopLeft = 32,
            CornerRadiusTopRight = 32,
            CornerRadiusBottomLeft = 32,
            CornerRadiusBottomRight = 32,
            BorderWidthLeft = 3,
            BorderWidthTop = 3,
            BorderWidthRight = 3,
            BorderWidthBottom = 3,
            BorderColor = new Color(1f, 1f, 1f, 0.22f),
            ShadowColor = new Color(0f, 0f, 0f, 0.40f),
            ShadowSize = 24,
            ShadowOffset = new Vector2(0f, 8f),
            ContentMarginLeft = 0,
            ContentMarginTop = 0,
            ContentMarginRight = 0,
            ContentMarginBottom = 0,
        };
        return style;
    }

    private static void ApplySlotVisual(Button button, EchoInstance? instance, bool isMainSlot, bool isSelected)
    {
        foreach (var child in button.GetChildren())
        {
            child.QueueFree();
        }

        var border = new StyleBoxFlat
        {
            BgColor = isSelected
                ? new Color(0.92f, 0.84f, 0.55f, 0.22f)
                : new Color(0f, 0f, 0f, instance == null ? 0.22f : 0.08f),
            CornerRadiusTopLeft = 999,
            CornerRadiusTopRight = 999,
            CornerRadiusBottomLeft = 999,
            CornerRadiusBottomRight = 999,
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            BorderColor = isMainSlot
                ? new Color("f3df92")
                : new Color(1f, 1f, 1f, 0.18f),
        };
        button.AddThemeStyleboxOverride("normal", border);
        button.AddThemeStyleboxOverride("hover", border);
        button.AddThemeStyleboxOverride("pressed", border);
        button.AddThemeStyleboxOverride("focus", border);

        float inset = SidebarAvatarInset;
        var portrait = new TextureRect
        {
            Texture = LoadTexture(GetDefinitionOrNull(instance)?.IconPath ?? DefaultIconPath) ?? LoadTexture(DefaultIconPath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = MouseFilterEnum.Ignore,
            Position = new Vector2(inset, inset),
            Size = button.Size - new Vector2(inset * 2f, inset * 2f),
        };
        button.AddChild(portrait);

        if (instance != null)
        {
            var definition = GetDefinitionOrNull(instance);
            var cost = new Label
            {
                Text = definition?.Cost.ToString() ?? "?",
                Position = new Vector2(button.Size.X - 24f, 4f),
                Size = new Vector2(20f, 20f),
                HorizontalAlignment = HorizontalAlignment.Center,
                MouseFilter = MouseFilterEnum.Ignore,
            };
            cost.AddThemeFontSizeOverride("font_size", isMainSlot ? 22 : 18);
            cost.AddThemeColorOverride("font_color", new Color("f3df92"));
            button.AddChild(cost);
        }
    }

    private void PositionNamedNode(string name, Vector2 position, Vector2 size)
    {
        if (_layoutRoot.GetNodeOrNull<Control>(name) is not { } control)
        {
            return;
        }

        control.Position = position;
        control.Size = size;
    }

    private static EchoDefinition? GetDefinitionOrNull(EchoInstance? instance)
    {
        if (instance == null)
        {
            return null;
        }

        return EchoRegistry.TryGetEcho(instance.DefinitionId, out var definition)
            ? definition
            : null;
    }

    private static string GetInventoryCardName(EchoInstance instance)
    {
        var definition = GetDefinitionOrNull(instance);
        return definition == null ? instance.DefinitionId : EchoUiTextService.GetEchoTitle(definition);
    }

    private static string GetClassDisplayText(EchoClass? echoClass)
    {
        return echoClass switch
        {
            EchoClass.Common => "轻波级",
            EchoClass.Elite => "巨浪级",
            EchoClass.Overlord => "怒涛级",
            EchoClass.Calamity => "海啸级",
            _ => "未知",
        };
    }

    private static string GetCostIconPath(int cost)
    {
        return cost switch
        {
            1 => Cost1IconPath,
            3 => Cost3IconPath,
            4 => Cost4IconPath,
            _ => DefaultIconPath,
        };
    }

    private static Texture2D? LoadTexture(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !ResourceLoader.Exists(path))
        {
            return null;
        }

        return GD.Load<Texture2D>(path);
    }

    private static Vector2 ScaleSidebarPoint(float x, float y, float scale)
    {
        return new Vector2(x * scale, y * scale);
    }

    private static Rect2 ScaleSidebarRect(Rect2 rect, float scale)
    {
        return new Rect2(rect.Position * scale, rect.Size * scale);
    }

    private static Rect2 CalculateCenteredPanelRect(Vector2 viewportSize)
    {
        float targetWidth = MathF.Min(PanelMaxWidth, viewportSize.X * PanelWidthFactor);
        float targetHeight = MathF.Min(PanelMaxHeight, viewportSize.Y * PanelHeightFactor);
        float width = targetWidth;
        float height = targetHeight;
        float left = (viewportSize.X - width) * 0.5f;
        float top = (viewportSize.Y - height) * 0.5f;
        return new Rect2(left, top, width, height);
    }
}
