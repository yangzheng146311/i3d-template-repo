using i3D;
using System;

public partial class MonitorPanel : VBoxContainer
{
    private SimDataService _data;
    private int _deviceIdx = 0;

    private Label _deviceNameLabel;
    private Label _statusBadge;
    private Label _zoneInfo;
    private HealthBar _healthBar;
    private GaugeChart _gaugeTemp;
    private GaugeChart _gaugeHumidity;
    private GaugeChart _gaugePressure;
    private ParamBar[] _paramBars = new ParamBar[6];
    private LineChart _trendChart;
    private TimeRangeSelector _timeRange;
    private Label _maintenanceLabel;
    private Label _runHoursLabel;

    public event Action<int> OnDeviceSelected;

    public override void iStart()
    {
        AddThemeConstantOverride("separation", 6);
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

        HBoxContainer titleRow = new();
        titleRow.AddThemeConstantOverride("separation", 8);
        _deviceNameLabel = UIStyles.MakeLabel("--", UIStyles.TEXT_WHITE, 18);
        titleRow.AddChild(_deviceNameLabel);
        _statusBadge = UIStyles.MakeLabel("", UIStyles.OK, UIStyles.FONT_SMALL);
        titleRow.AddChild(_statusBadge);
        Control spacer = new();
        spacer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        titleRow.AddChild(spacer);
        AddChild(titleRow);

        _zoneInfo = UIStyles.MakeLabel("", UIStyles.TEXT_DIM, UIStyles.FONT_SMALL);
        AddChild(_zoneInfo);

        AddChild(UIStyles.MakeDivider());

        _healthBar = new HealthBar();
        _healthBar.SetCustomMinimumSize(new Vector2(0, 32));
        AddChild(_healthBar);

        AddChild(UIStyles.MakeSpacer(4));
        AddChild(UIStyles.MakeSectionTitle("仪表盘", UIStyles.FONT_SUBTITLE));

        HBoxContainer gaugeRow = new();
        gaugeRow.AddThemeConstantOverride("separation", 6);
        gaugeRow.SetCustomMinimumSize(new Vector2(0, 100));

        _gaugeTemp = new GaugeChart();
        _gaugeTemp.SetLabel("温度", "°C");
        _gaugeTemp.SetRange(15f, 40f);
        _gaugeTemp.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        gaugeRow.AddChild(_gaugeTemp);

        _gaugeHumidity = new GaugeChart();
        _gaugeHumidity.SetLabel("湿度", "%");
        _gaugeHumidity.SetRange(20f, 80f);
        _gaugeHumidity.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        gaugeRow.AddChild(_gaugeHumidity);

        _gaugePressure = new GaugeChart();
        _gaugePressure.SetLabel("压力", "kPa");
        _gaugePressure.SetRange(99f, 104f);
        _gaugePressure.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        gaugeRow.AddChild(_gaugePressure);

        AddChild(gaugeRow);

        AddChild(UIStyles.MakeSpacer(4));
        AddChild(UIStyles.MakeSectionTitle("实时参数", UIStyles.FONT_SUBTITLE));

        string[] paramNames = { "温  度", "湿  度", "压  力", "电  流", "转  速", "振  动" };
        string[] paramUnits = { "°C", "%", "kPa", "A", "rpm", "mm/s" };
        float[,] paramRanges = {
            { 15f, 40f }, { 20f, 80f }, { 99f, 104f },
            { 8f, 16f }, { 1400f, 1600f }, { 0f, 0.3f }
        };

        for (int i = 0; i < 6; i++)
        {
            ParamBar bar = new();
            bar.SetCustomMinimumSize(new Vector2(0, 24));
            bar.Setup(paramNames[i], paramUnits[i], paramRanges[i, 0], paramRanges[i, 1], 0);
            _paramBars[i] = bar;
            AddChild(bar);
        }

        AddChild(UIStyles.MakeSpacer(4));
        AddChild(UIStyles.MakeSectionTitle("温度趋势", UIStyles.FONT_SUBTITLE));

        _trendChart = new LineChart();
        _trendChart.SetCustomMinimumSize(new Vector2(0, 90));
        _trendChart.SetColor(UIStyles.ACCENT);
        _trendChart.SetLabelFormat("{0:F0}");
        AddChild(_trendChart);

        _timeRange = new TimeRangeSelector();
        AddChild(_timeRange);

        AddChild(UIStyles.MakeSpacer(8));
        AddChild(UIStyles.MakeSectionTitle("维保信息", UIStyles.FONT_SUBTITLE));
        AddChild(UIStyles.MakeDivider());

        _maintenanceLabel = UIStyles.MakeLabel("", UIStyles.TEXT_DIM, UIStyles.FONT_BODY);
        AddChild(_maintenanceLabel);
        _runHoursLabel = UIStyles.MakeLabel("", UIStyles.TEXT_DIM, UIStyles.FONT_BODY);
        AddChild(_runHoursLabel);

        RefreshData();
    }

    public void SetDevice(int index)
    {
        if (_data == null || index < 0 || index >= _data.Devices.Length) return;
        _deviceIdx = index;
        RefreshData();
    }

    public void RefreshData()
    {
        if (_data == null) return;
        if (_deviceIdx < 0 || _deviceIdx >= _data.Devices.Length) return;

        var d = _data.Devices[_deviceIdx];

        if (_deviceNameLabel != null) _deviceNameLabel.Text = d.Name;
        if (_statusBadge != null)
        {
            _statusBadge.Text = d.Online ? "[在线]" : "[离线]";
            _statusBadge.AddThemeColorOverride("font_color", d.Online ? UIStyles.OK : UIStyles.ERR);
        }
        if (_zoneInfo != null) _zoneInfo.Text = d.Zone + "区 · 运行 " + d.RunHours.ToString("F0") + "h";

        if (_healthBar != null) _healthBar.SetScore(d.HealthScore);

        if (_gaugeTemp != null) _gaugeTemp.SetValue(d.Temperature);
        if (_gaugeHumidity != null) _gaugeHumidity.SetValue(d.Humidity);
        if (_gaugePressure != null) _gaugePressure.SetValue(d.Pressure);

        float[] values = { d.Temperature, d.Humidity, d.Pressure, d.Current, d.Rpm, d.Vibration };
        for (int i = 0; i < 6; i++)
        {
            if (_paramBars[i] != null) _paramBars[i].SetValue(values[i]);
        }

        if (_trendChart != null && _deviceIdx < _data.TempHistory.Length)
            _trendChart.SetData(_data.TempHistory[_deviceIdx]);

        if (_maintenanceLabel != null)
        {
            var remain = d.NextMaintenance - DateTime.Now;
            _maintenanceLabel.Text = "下次保养: " + d.NextMaintenance.ToString("yyyy-MM-dd") + "  (剩余 " + remain.Days + " 天)";
        }
        if (_runHoursLabel != null)
            _runHoursLabel.Text = "累计运行: " + d.RunHours.ToString("F1") + " 小时";
    }
}
