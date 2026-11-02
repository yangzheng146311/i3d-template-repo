using i3D;
using System;

public partial class OverviewPanel : VBoxContainer
{
    private SimDataService _data;

    private Label _statTotal;
    private Label _statOnlineRate;
    private Label _statOee;
    private Label _statAlerts;
    private LineChart _yieldChart;
    private Label _energyElec;
    private Label _energyWater;
    private Label _energyGas;
    private VBoxContainer _deviceListContainer;
    private VBoxContainer _alertTopContainer;

    public event Action<int> OnDeviceSelected;
    public event Action<int> OnAlertSelected;

    public override void iStart()
    {
        AddThemeConstantOverride("separation", 8);
    }

    public void SetData(SimDataService data)
    {
        _data = data;
        Build();
    }

    private void Build()
    {
        while (GetChildCount() > 0)
        {
            var c = GetChild(0);
            RemoveChild(c);
            c.QueueFree();
        }

        AddChild(UIStyles.MakeSectionTitle("系统概览"));
        AddChild(UIStyles.MakeDivider());

        HBoxContainer row1 = new();
        row1.AddThemeConstantOverride("separation", 8);
        row1.SetCustomMinimumSize(new Vector2(0, 70));
        _statTotal = UIBuilder.MakeStatValue("0", UIStyles.ACCENT);
        row1.AddChild(UIBuilder.MakeStatCard("设备总数", _statTotal, UIStyles.ACCENT));
        _statOnlineRate = UIBuilder.MakeStatValue("0%", UIStyles.OK);
        row1.AddChild(UIBuilder.MakeStatCard("在线率", _statOnlineRate, UIStyles.OK));
        AddChild(row1);

        HBoxContainer row2 = new();
        row2.AddThemeConstantOverride("separation", 8);
        row2.SetCustomMinimumSize(new Vector2(0, 70));
        _statOee = UIBuilder.MakeStatValue("0%", UIStyles.ACCENT);
        row2.AddChild(UIBuilder.MakeStatCard("OEE", _statOee, UIStyles.ACCENT));
        _statAlerts = UIBuilder.MakeStatValue("0", UIStyles.WARN);
        row2.AddChild(UIBuilder.MakeStatCard("活跃告警", _statAlerts, UIStyles.WARN));
        AddChild(row2);

        AddChild(UIStyles.MakeSpacer(8));
        AddChild(UIStyles.MakeSectionTitle("当日产量趋势", UIStyles.FONT_SUBTITLE));

        _yieldChart = new LineChart();
        _yieldChart.SetCustomMinimumSize(new Vector2(0, 100));
        _yieldChart.SetColor(UIStyles.OK);
        _yieldChart.SetLabelFormat("{0:F0}");
        AddChild(_yieldChart);

        AddChild(UIStyles.MakeSpacer(8));
        AddChild(UIStyles.MakeSectionTitle("能耗概况", UIStyles.FONT_SUBTITLE));

        HBoxContainer energyRow = new();
        energyRow.AddThemeConstantOverride("separation", 6);
        energyRow.SetCustomMinimumSize(new Vector2(0, 55));
        _energyElec = UIBuilder.MakeStatValue("0", UIStyles.ACCENT);
        energyRow.AddChild(UIBuilder.MakeStatCard("用电 kWh", _energyElec, UIStyles.ACCENT));
        _energyWater = UIBuilder.MakeStatValue("0", UIStyles.OK);
        energyRow.AddChild(UIBuilder.MakeStatCard("用水 t", _energyWater, UIStyles.OK));
        _energyGas = UIBuilder.MakeStatValue("0", UIStyles.WARN);
        energyRow.AddChild(UIBuilder.MakeStatCard("用气 m³", _energyGas, UIStyles.WARN));
        AddChild(energyRow);

        AddChild(UIStyles.MakeSpacer(8));
        AddChild(UIStyles.MakeSectionTitle("设备列表", UIStyles.FONT_SUBTITLE));
        AddChild(UIStyles.MakeDivider());

        _deviceListContainer = new VBoxContainer();
        _deviceListContainer.AddThemeConstantOverride("separation", 2);
        AddChild(_deviceListContainer);

        AddChild(UIStyles.MakeSpacer(8));
        AddChild(UIStyles.MakeSectionTitle("告警 TOP 5", UIStyles.FONT_SUBTITLE));
        AddChild(UIStyles.MakeDivider());

        _alertTopContainer = new VBoxContainer();
        _alertTopContainer.AddThemeConstantOverride("separation", 3);
        AddChild(_alertTopContainer);

        RefreshData();
    }

    public void RefreshData()
    {
        if (_data == null) return;

        if (_statTotal != null) _statTotal.Text = _data.Devices.Length.ToString();
        if (_statOnlineRate != null) _statOnlineRate.Text = _data.GetOnlineRate().ToString("F1") + "%";
        if (_statOee != null) _statOee.Text = _data.GetOverallOEE().ToString("F1") + "%";
        if (_statAlerts != null) _statAlerts.Text = _data.AlertCount.ToString();

        if (_yieldChart != null) _yieldChart.SetData(_data.DailyYieldHistory);

        if (_energyElec != null) _energyElec.Text = _data.GetTotalEnergy().ToString("F0");
        if (_energyWater != null) _energyWater.Text = "128";
        if (_energyGas != null) _energyGas.Text = "56";

        RefreshDeviceList();
        RefreshAlertTop();
    }

    private void RefreshDeviceList()
    {
        if (_deviceListContainer == null) return;

        while (_deviceListContainer.GetChildCount() > 0)
        {
            var child = _deviceListContainer.GetChild(0);
            _deviceListContainer.RemoveChild(child);
            child.QueueFree();
        }

        for (int i = 0; i < _data.Devices.Length; i++)
        {
            int idx = i;
            HBoxContainer row = new();
            row.SetCustomMinimumSize(new Vector2(0, 28));
            row.AddThemeConstantOverride("separation", 6);

            ColorRect dot = new();
            dot.SetCustomMinimumSize(new Vector2(8, 8));
            dot.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            dot.Color = _data.Devices[i].Online ? UIStyles.OK : UIStyles.ERR;
            row.AddChild(dot);

            Label nameLbl = UIStyles.MakeLabel(_data.Devices[i].Name,
                _data.Devices[i].Online ? UIStyles.TEXT_WHITE : UIStyles.TEXT_DIM, UIStyles.FONT_SMALL);
            nameLbl.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            nameLbl.VerticalAlignment = VerticalAlignment.Center;
            row.AddChild(nameLbl);

            Button locBtn = new();
            locBtn.Text = "定位";
            locBtn.AddThemeFontSizeOverride("font_size", UIStyles.FONT_SMALL);
            locBtn.AddThemeColorOverride("font_color", UIStyles.ACCENT);
            locBtn.AddThemeStyleboxOverride("normal",
                UIStyles.MakeStyleBox(Colors.Transparent, Colors.Transparent, 0, 3));
            locBtn.AddThemeStyleboxOverride("hover",
                UIStyles.MakeStyleBox(UIStyles.ACCENT_BLUE, UIStyles.ACCENT, 0, 3));
            locBtn.Pressed += () => OnDeviceSelected?.Invoke(idx);
            row.AddChild(locBtn);

            _deviceListContainer.AddChild(row);
        }
    }

    private void RefreshAlertTop()
    {
        if (_alertTopContainer == null) return;

        while (_alertTopContainer.GetChildCount() > 0)
        {
            var child = _alertTopContainer.GetChild(0);
            _alertTopContainer.RemoveChild(child);
            child.QueueFree();
        }

        int count = Mathf.Min(_data.AlertCount, 5);
        for (int i = 0; i < count; i++)
        {
            int idx = i;
            var alert = _data.Alerts[i];
            Color levelColor = alert.Level == 2 ? UIStyles.ERR : UIStyles.WARN;

            HBoxContainer row = new();
            row.SetCustomMinimumSize(new Vector2(0, 24));
            row.AddThemeConstantOverride("separation", 6);
            row.MouseFilter = MouseFilterEnum.Stop;
            row.GuiInput += (InputEvent e) =>
            {
                if (e is InputEventMouseButton btn && btn.Pressed && btn.ButtonIndex == MouseButton.Left)
                    OnAlertSelected?.Invoke(idx);
            };

            ColorRect levelDot = new();
            levelDot.SetCustomMinimumSize(new Vector2(4, 12));
            levelDot.Color = levelColor;
            levelDot.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            row.AddChild(levelDot);

            row.AddChild(UIStyles.MakeLabel(alert.Title, levelColor, UIStyles.FONT_SMALL));

            Label devLbl = UIStyles.MakeLabel(alert.DeviceName, UIStyles.TEXT_DIM, UIStyles.FONT_SMALL);
            devLbl.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            row.AddChild(devLbl);

            row.AddChild(UIStyles.MakeLabel(alert.Time.ToString("HH:mm"), UIStyles.TEXT_DIM, UIStyles.FONT_TINY));

            _alertTopContainer.AddChild(row);
        }
    }
}
