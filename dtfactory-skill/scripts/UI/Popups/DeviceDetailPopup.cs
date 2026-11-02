using i3D;
using System;

public partial class DeviceDetailPopup : Control
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

    public void ShowDevice(SimDataService.DeviceInfo device, float[] tempHistory, SimDataService data)
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
        float popupH = 520f;
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

        ScrollContainer scroll = new();
        scroll.SetAnchorsPreset(LayoutPreset.FullRect);
        scroll.SetOffsetsPreset(LayoutPreset.FullRect);
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        margin.AddChild(scroll);

        VBoxContainer vbox = new();
        vbox.SetAnchorsPreset(LayoutPreset.FullRect);
        vbox.SetOffsetsPreset(LayoutPreset.FullRect);
        vbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        vbox.AddThemeConstantOverride("separation", 6);
        scroll.AddChild(vbox);

        HBoxContainer titleRow = new();
        titleRow.AddThemeConstantOverride("separation", 8);
        titleRow.AddChild(UIStyles.MakeLabel(device.Name, Colors.White, 18));
        string statusText = device.Online ? "[在线]" : "[离线]";
        Color statusColor = device.Online ? UIStyles.OK : UIStyles.ERR;
        titleRow.AddChild(UIStyles.MakeLabel(statusText, statusColor, UIStyles.FONT_SMALL));
        Control sp = new();
        sp.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        titleRow.AddChild(sp);
        Button closeBtn = new();
        closeBtn.Text = "×";
        closeBtn.AddThemeColorOverride("font_color", UIStyles.TEXT_DIM);
        closeBtn.AddThemeFontSizeOverride("font_size", 18);
        closeBtn.Pressed += Close;
        titleRow.AddChild(closeBtn);
        vbox.AddChild(titleRow);

        vbox.AddChild(UIStyles.MakeDivider());

        vbox.AddChild(UIBuilder.MakeInfoRow("所属区域", device.Zone + "区"));
        vbox.AddChild(UIBuilder.MakeInfoRow("3D坐标", $"({device.Position.X:F1}, {device.Position.Y:F1}, {device.Position.Z:F1})"));
        vbox.AddChild(UIBuilder.MakeInfoRow("累计运行", device.RunHours.ToString("F1") + " 小时"));

        vbox.AddChild(UIStyles.MakeSpacer(4));
        vbox.AddChild(UIStyles.MakeSectionTitle("健康评分", UIStyles.FONT_SUBTITLE));
        HealthBar hb = new HealthBar();
        hb.SetCustomMinimumSize(new Vector2(0, 32));
        hb.SetScore(device.HealthScore);
        vbox.AddChild(hb);

        vbox.AddChild(UIStyles.MakeSpacer(4));
        vbox.AddChild(UIStyles.MakeSectionTitle("实时参数", UIStyles.FONT_SUBTITLE));

        string[] pNames = { "温度", "湿度", "压力", "电流", "转速", "振动" };
        string[] pUnits = { "°C", "%", "kPa", "A", "rpm", "mm/s" };
        float[] pVals = { device.Temperature, device.Humidity, device.Pressure, device.Current, device.Rpm, device.Vibration };
        float[,] pRanges = { { 15f, 40f }, { 20f, 80f }, { 99f, 104f }, { 8f, 16f }, { 1400f, 1600f }, { 0f, 0.3f } };
        for (int i = 0; i < 6; i++)
        {
            ParamBar bar = new();
            bar.SetCustomMinimumSize(new Vector2(0, 22));
            bar.Setup(pNames[i], pUnits[i], pRanges[i, 0], pRanges[i, 1], pVals[i]);
            vbox.AddChild(bar);
        }

        vbox.AddChild(UIStyles.MakeSpacer(4));
        vbox.AddChild(UIStyles.MakeSectionTitle("温度趋势 (近5分钟)", UIStyles.FONT_SUBTITLE));
        LineChart miniChart = new LineChart();
        miniChart.SetCustomMinimumSize(new Vector2(0, 100));
        miniChart.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        miniChart.SetData(tempHistory);
        miniChart.SetColor(UIStyles.ACCENT);
        miniChart.SetLabelFormat("{0:F0}");
        vbox.AddChild(miniChart);

        vbox.AddChild(UIStyles.MakeSpacer(4));
        vbox.AddChild(UIStyles.MakeSectionTitle("维保信息", UIStyles.FONT_SUBTITLE));
        var remain = device.NextMaintenance - DateTime.Now;
        vbox.AddChild(UIBuilder.MakeInfoRow("下次保养", device.NextMaintenance.ToString("yyyy-MM-dd")));
        vbox.AddChild(UIBuilder.MakeInfoRow("剩余天数", remain.Days + " 天"));

        vbox.AddChild(UIStyles.MakeSpacer(8));
        HBoxContainer btnRow = new();
        btnRow.AddThemeConstantOverride("separation", 10);

        Button locateBtn = new();
        locateBtn.Text = "定位设备";
        locateBtn.SetCustomMinimumSize(new Vector2(0, 36));
        locateBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        locateBtn.AddThemeStyleboxOverride("normal", UIBuilder.MakeBtnStyle(UIStyles.ACCENT, UIStyles.ACCENT_BLUE, 6));
        locateBtn.AddThemeColorOverride("font_color", Colors.White);
        locateBtn.AddThemeFontSizeOverride("font_size", UIStyles.FONT_SMALL);
        locateBtn.Pressed += () => { OnLocateDevice?.Invoke(device.Name); Close(); };
        btnRow.AddChild(locateBtn);

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
