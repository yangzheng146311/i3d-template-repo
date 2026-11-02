using i3D;
using System;

public partial class AlertDetailPopup : Control
{
    public event Action<string> OnLocateDevice;
    public event Action OnClosed;

    public override void iStart()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        SetOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;
    }

    public void ShowAlert(SimDataService.AlertInfo alert)
    {
        foreach (var child in GetChildren())
            if (child is Item c) c.QueueFree();

        ColorRect overlay = new();
        overlay.SetAnchorsPreset(LayoutPreset.FullRect);
        overlay.SetOffsetsPreset(LayoutPreset.FullRect);
        overlay.Color = UIStyles.OVERLAY;
        overlay.MouseFilter = MouseFilterEnum.Stop;
        overlay.GuiInput += (InputEvent ev) =>
        {
            if (ev is InputEventMouseButton b && b.Pressed && b.ButtonIndex == MouseButton.Left)
                Close();
        };
        AddChild(overlay);

        float popupW = 380f;
        float popupH = 420f;
        PanelContainer popup = new();
        popup.SetAnchorsPreset(LayoutPreset.Center);
        popup.SetOffsetsPreset(LayoutPreset.Center);
        popup.OffsetLeft = -popupW * 0.5f;
        popup.OffsetRight = popupW * 0.5f;
        popup.OffsetTop = -popupH * 0.5f;
        popup.OffsetBottom = popupH * 0.5f;
        StyleBoxFlat style = new();
        style.SetBgColor(UIStyles.MakeStyleBox(UIStyles.PANEL_BG, UIStyles.ACCENT, 1, 8).BgColor);
        style.SetBorderColor(UIStyles.ACCENT);
        style.SetBorderWidthAll(1);
        style.SetCornerRadiusAll(8);
        style.SetContentMarginAll(0);
        popup.AddThemeStyleboxOverride("panel", style);
        AddChild(popup);

        MarginContainer margin = new();
        margin.SetAnchorsPreset(LayoutPreset.FullRect);
        margin.SetOffsetsPreset(LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_top", 16);
        margin.AddThemeConstantOverride("margin_bottom", 16);
        popup.AddChild(margin);

        VBoxContainer vbox = new();
        vbox.SetAnchorsPreset(LayoutPreset.FullRect);
        vbox.SetOffsetsPreset(LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 6);
        margin.AddChild(vbox);

        HBoxContainer titleRow = new();
        titleRow.AddThemeConstantOverride("separation", 8);

        Color levelColor = alert.Level switch { 2 => UIStyles.ERR, 1 => UIStyles.WARN, _ => UIStyles.OK };
        string levelText = alert.Level switch { 2 => "严重", 1 => "警告", _ => "提示" };

        titleRow.AddChild(UIStyles.MakeLabel("[" + levelText + "]", levelColor, UIStyles.FONT_SMALL));
        titleRow.AddChild(UIStyles.MakeLabel(alert.Title, Colors.White, 18));

        Control spacer = new();
        spacer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        titleRow.AddChild(spacer);

        Button closeBtn = new();
        closeBtn.Text = "×";
        closeBtn.AddThemeColorOverride("font_color", UIStyles.TEXT_DIM);
        closeBtn.AddThemeFontSizeOverride("font_size", 18);
        closeBtn.Pressed += Close;
        titleRow.AddChild(closeBtn);
        vbox.AddChild(titleRow);

        vbox.AddChild(UIStyles.MakeDivider());

        string statusText = alert.Status == "Active" ? "待处理" : "已确认";
        Color statusColor = alert.Status == "Active" ? UIStyles.WARN : UIStyles.OK;
        vbox.AddChild(UIBuilder.MakeInfoRow("告警状态", statusText));
        vbox.AddChild(UIBuilder.MakeInfoRow("告警时间", alert.Time.ToString("yyyy-MM-dd HH:mm:ss")));
        vbox.AddChild(UIBuilder.MakeInfoRow("所属区域", alert.Zone + "区"));
        vbox.AddChild(UIBuilder.MakeInfoRow("关联设备", alert.DeviceName));

        vbox.AddChild(UIStyles.MakeSpacer(4));
        vbox.AddChild(UIStyles.MakeSectionTitle("详细描述", UIStyles.FONT_SUBTITLE));
        Label descLabel = UIStyles.MakeLabel(alert.Description, UIStyles.TEXT_DIM, UIStyles.FONT_BODY);
        descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        descLabel.SetCustomMinimumSize(new Vector2(0, 40));
        vbox.AddChild(descLabel);

        vbox.AddChild(UIStyles.MakeSpacer(4));
        vbox.AddChild(UIStyles.MakeSectionTitle("处理建议", UIStyles.FONT_SUBTITLE));
        Label suggestLabel = UIStyles.MakeLabel(alert.Suggestion, UIStyles.ACCENT, UIStyles.FONT_SMALL);
        suggestLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        suggestLabel.SetCustomMinimumSize(new Vector2(0, 40));
        vbox.AddChild(suggestLabel);

        vbox.AddChild(UIStyles.MakeSpacer(12));
        HBoxContainer btnRow = new();
        btnRow.AddThemeConstantOverride("separation", 10);

        Button locateBtn = new();
        locateBtn.Text = "定位设备";
        locateBtn.SetCustomMinimumSize(new Vector2(0, 36));
        locateBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        locateBtn.AddThemeStyleboxOverride("normal", UIBuilder.MakeBtnStyle(UIStyles.ACCENT, UIStyles.ACCENT_BLUE, 6));
        locateBtn.AddThemeColorOverride("font_color", Colors.White);
        locateBtn.AddThemeFontSizeOverride("font_size", UIStyles.FONT_SMALL);
        locateBtn.Pressed += () => { OnLocateDevice?.Invoke(alert.DeviceName); Close(); };
        btnRow.AddChild(locateBtn);

        Button ackBtn = new();
        ackBtn.Text = "确认告警";
        ackBtn.SetCustomMinimumSize(new Vector2(0, 36));
        ackBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        ackBtn.AddThemeStyleboxOverride("normal",
            UIBuilder.MakeBtnStyle(UIStyles.WARN, new Color(0.2f, 0.12f, 0.02f), 6));
        ackBtn.AddThemeStyleboxOverride("hover",
            UIBuilder.MakeBtnStyle(new Color(1f, 0.7f, 0.2f), UIStyles.WARN, 6));
        ackBtn.AddThemeColorOverride("font_color", Colors.White);
        ackBtn.AddThemeColorOverride("font_hover_color", Colors.White);
        ackBtn.AddThemeFontSizeOverride("font_size", UIStyles.FONT_SMALL);
        btnRow.AddChild(ackBtn);

        Button dismissBtn = new();
        dismissBtn.Text = "关闭";
        dismissBtn.SetCustomMinimumSize(new Vector2(0, 36));
        dismissBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        dismissBtn.AddThemeStyleboxOverride("normal",
            UIBuilder.MakeBtnStyle(new Color(0.1f, 0.18f, 0.3f), UIStyles.DIVIDER, 6));
        dismissBtn.AddThemeColorOverride("font_color", UIStyles.TEXT_DIM);
        dismissBtn.AddThemeFontSizeOverride("font_size", UIStyles.FONT_SMALL);
        dismissBtn.Pressed += Close;
        btnRow.AddChild(dismissBtn);
        vbox.AddChild(btnRow);

        Visible = true;
    }

    private void Close()
    {
        Visible = false;
        OnClosed?.Invoke();
    }

    public override void OnInput(InputEvent e) { }
}
