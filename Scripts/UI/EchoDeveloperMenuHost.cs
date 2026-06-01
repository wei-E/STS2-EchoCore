using EchoCore.Scripts.Config;
using EchoCore.Scripts.Services;
using Godot;
using MegaCrit.Sts2.Core.Nodes;

namespace EchoCore.Scripts.UI;

/// <summary>
/// 开发者菜单的挂载宿主。
/// 它只负责入口按钮与显隐，不承载具体的声骸实例生成逻辑。
/// </summary>
public sealed partial class EchoDeveloperMenuHost : Control
{
    private const string NodeName = "EchoCoreDeveloperMenuHost";

    private Button _openButton = null!;
    private EchoDeveloperMenu _menu = null!;

    public static void AttachTo(NRun run)
    {
        if (run.GetNodeOrNull<EchoDeveloperMenuHost>(NodeName) != null)
        {
            return;
        }

        var host = new EchoDeveloperMenuHost
        {
            Name = NodeName,
        };
        run.AddChild(host);
    }

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        ZIndex = 512;

        _openButton = new Button
        {
            Text = "声骸开发",
            TooltipText = "打开 EchoCore 开发者菜单",
            CustomMinimumSize = new Vector2(124f, 42f),
            MouseFilter = MouseFilterEnum.Stop,
            TopLevel = true,
        };
        _openButton.Pressed += () => _menu.Toggle();
        AddChild(_openButton);

        _menu = new EchoDeveloperMenu();
        AddChild(_menu);

        LayoutControls();
        RefreshVisibility();
    }

    public override void _Process(double delta)
    {
        RefreshVisibility();
        LayoutControls();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationResized)
        {
            LayoutControls();
        }
    }

    private void RefreshVisibility()
    {
        bool visible = EchoDeveloperConfig.EnableEchoDeveloperMenu && !EchoDeveloperService.IsCombatActive();
        _openButton.Visible = visible;
        if (!visible)
        {
            _menu.Close();
        }
    }

    private void LayoutControls()
    {
        if (_openButton == null)
        {
            return;
        }

        Vector2 viewportSize = GetViewportRect().Size;
        _openButton.Position = new Vector2(Math.Max(16f, viewportSize.X - 280f), 132f);
        _openButton.Size = new Vector2(124f, 42f);
    }
}
