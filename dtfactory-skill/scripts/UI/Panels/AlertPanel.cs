using i3D;
using System;
using System.Collections.Generic;

public partial class AlertPanel : VBoxContainer
{
    private SimDataService _data;
    private int _filterLevel = -1;
    private int _filterTime = 0;

    private Label _countCritical;
    private Label _countWarning;
    private Label _countInfo;
    private HBoxContainer _levelButtons;
    private HBoxContainer _timeButtons;
    private LineChart _trendChart;
    private VBoxContainer _alertListContainer;
    private Label _alertCountLabel;
    private Button[] _levelBtns = new Button[4];
    private Button[] _timeBtns = new Button[3];

    public event Action<int> OnAlertSelected;
    public event Action<string> OnDeviceLocated;

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

        AddChild(UIStyles.MakeSectionTitle("告警统计"));
        AddChild(UIStyles.MakeDivider());

        HBoxContainer statsRow = new();
        statsRow.AddThemeConstantOverride("separation", 8);
        statsRow.SetCustomMinimumSize(new Vector2(0, 50));
        _countCritical = UIBuilder.MakeStatValue("0", UIStyles.ERR);
        statsRow.AddChild(UIBuilder.MakeStatCard("严重", _countCritical, UIStyles.ERR));
        _countWarning = UIBuilder.MakeStatValue("0", UIStyles.WARN);
        statsRow.AddChild(UIBuilder.MakeStatCard("警告", _countWarning, UIStyles.WARN));
        _countInfo = UIBuilder.MakeStatValue("0", UIStyles.ACCENT);
        statsRow.AddChild(UIBuilder.MakeStatCard("提示", _countInfo, UIStyles.ACCENT));
        AddChild(statsRow);

        AddChild(UIStyles.MakeSpacer(4));

        _levelButtons = new HBoxContainer();
        _levelButtons.AddThemeConstantOverride("separation", 4);
        string[] levelLabels = { "全部", "严重", "警告", "提示" };
        int[] levelVals = { -1, 2, 1, 0 };
        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            Button btn = new();
            btn.Text = levelLabels[i];
            btn.SetCustomMinimumSize(new Vector2(0, 28));
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            btn.AddThemeFontSizeOverride("font_size", UIStyles.FONT_SMALL);
            btn.Pressed += () => { _filterLevel = levelVals[idx]; ApplyLevelStyles(); RefreshAlertList(); };
            _levelBtns[i] = btn;
            _levelButtons.AddChild(btn);
        }
        AddChild(_levelButtons);

        _timeButtons = new HBoxContainer();
        _timeButtons.AddThemeConstantOverride("separation", 4);
        string[] timeLabels = { "今日", "本周", "本月" };
        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            Button btn = new();
            btn.Text = timeLabels[i];
            btn.SetCustomMinimumSize(new Vector2(0, 26));
            btn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            btn.AddThemeFontSizeOverride("font_size", UIStyles.FONT_SMALL);
            btn.Pressed += () => { _filterTime = idx; ApplyTimeStyles(); RefreshAlertList(); };
            _timeBtns[i] = btn;
            _timeButtons.AddChild(btn);
        }
        AddChild(_timeButtons);

        AddChild(UIStyles.MakeSpacer(4));
        AddChild(UIStyles.MakeSectionTitle("告警趋势 (7天)", UIStyles.FONT_SUBTITLE));

        _trendChart = new LineChart();
        _trendChart.SetCustomMinimumSize(new Vector2(0, 80));
        _trendChart.SetColor(UIStyles.WARN);
        _trendChart.SetLabelFormat("{0:F0}");
        float[] trendData = new float[7];
        for (int i = 0; i < 7; i++) trendData[i] = 2 + Mathf.Sin(i * 0.9f) * 3f + i * 0.3f;
        _trendChart.SetData(trendData);
        AddChild(_trendChart);

        AddChild(UIStyles.MakeSpacer(4));
        AddChild(UIStyles.MakeSectionTitle("告警列表", UIStyles.FONT_SUBTITLE));
        AddChild(UIStyles.MakeDivider());

        ScrollContainer scroll = new();
        scroll.SetCustomMinimumSize(new Vector2(0, 200));
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        AddChild(scroll);

        _alertListContainer = new VBoxContainer();
        _alertListContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _alertListContainer.AddThemeConstantOverride("separation", 4);
        scroll.AddChild(_alertListContainer);

        _alertCountLabel = UIStyles.MakeLabel("", UIStyles.TEXT_DIM, UIStyles.FONT_SMALL);
        AddChild(_alertCountLabel);

        ApplyLevelStyles();
        ApplyTimeStyles();
        RefreshData();
    }

    private void ApplyLevelStyles()
    {
        string[] levelLabels = { "全部", "严重", "警告", "提示" };
        int[] levelVals = { -1, 2, 1, 0 };
        for (int i = 0; i < _levelBtns.Length; i++)
        {
            bool sel = levelVals[i] == _filterLevel;
            _levelBtns[i].AddThemeStyleboxOverride("normal",
                UIStyles.MakeStyleBox(sel ? UIStyles.ACCENT_BLUE : new Color(0.03f, 0.07f, 0.14f),
                    sel ? UIStyles.ACCENT : UIStyles.DIVIDER, 0, 4));
            _levelBtns[i].AddThemeColorOverride("font_color", sel ? UIStyles.TEXT_WHITE : UIStyles.TEXT_DIM);
        }
    }

    private void ApplyTimeStyles()
    {
        for (int i = 0; i < _timeBtns.Length; i++)
        {
            bool sel = i == _filterTime;
            _timeBtns[i].AddThemeStyleboxOverride("normal",
                UIStyles.MakeStyleBox(sel ? UIStyles.ACCENT_BLUE : new Color(0.03f, 0.07f, 0.14f),
                    sel ? UIStyles.ACCENT : UIStyles.DIVIDER, 0, 4));
            _timeBtns[i].AddThemeColorOverride("font_color", sel ? UIStyles.TEXT_WHITE : UIStyles.TEXT_DIM);
        }
    }

    public void RefreshData()
    {
        if (_data == null) return;

        if (_countCritical != null) _countCritical.Text = _data.GetActiveAlertCount(2).ToString();
        if (_countWarning != null) _countWarning.Text = _data.GetActiveAlertCount(1).ToString();
        if (_countInfo != null) _countInfo.Text = _data.GetActiveAlertCount(0).ToString();

        RefreshAlertList();
    }

    private void RefreshAlertList()
    {
        if (_alertListContainer == null || _data == null) return;

        while (_alertListContainer.GetChildCount() > 0)
        {
            var child = _alertListContainer.GetChild(0);
            _alertListContainer.RemoveChild(child);
            child.QueueFree();
        }

        var filtered = _data.GetAlertsByLevel(_filterLevel);
        int count = Mathf.Min(filtered.Count, 20);

        for (int i = 0; i < count; i++)
        {
            int idx = i;
            var alert = filtered[i];
            Color levelColor = alert.Level == 2 ? UIStyles.ERR : alert.Level == 1 ? UIStyles.WARN : UIStyles.OK;

            PanelContainer card = new();
            card.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            StyleBoxFlat style = UIStyles.MakeStyleBox(new Color(0.04f, 0.08f, 0.16f, 0.9f), levelColor, 1, 4);
            style.SetContentMarginAll(8);
            style.SetBorderWidthAll(1);
            card.AddThemeStyleboxOverride("panel", style);
            card.MouseFilter = MouseFilterEnum.Stop;

            VBoxContainer cardVBox = new();
            cardVBox.AddThemeConstantOverride("separation", 3);
            card.AddChild(cardVBox);

            HBoxContainer topRow = new();
            topRow.AddThemeConstantOverride("separation", 6);

            ColorRect levelBar = new();
            levelBar.SetCustomMinimumSize(new Vector2(3, 16));
            levelBar.Color = levelColor;
            topRow.AddChild(levelBar);

            topRow.AddChild(UIStyles.MakeLabel(alert.Title, levelColor, UIStyles.FONT_BODY));
            Label devLbl = UIStyles.MakeLabel(alert.DeviceName + " · " + alert.Zone + "区", UIStyles.TEXT_DIM, UIStyles.FONT_SMALL);
            devLbl.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            topRow.AddChild(devLbl);
            topRow.AddChild(UIStyles.MakeLabel(alert.Time.ToString("HH:mm"), UIStyles.TEXT_DIM, UIStyles.FONT_TINY));
            cardVBox.AddChild(topRow);

            string statusText = alert.Status == "Active" ? "待处理" : "已确认";
            Color statusColor = alert.Status == "Active" ? UIStyles.WARN : UIStyles.OK;
            cardVBox.AddChild(UIStyles.MakeLabel("  " + alert.Description, UIStyles.TEXT_DIM, UIStyles.FONT_TINY));
            HBoxContainer statusRow = new();
            statusRow.AddThemeConstantOverride("separation", 6);
            statusRow.AddChild(UIStyles.MakeLabel("[" + statusText + "]", statusColor, UIStyles.FONT_TINY));
            Control s = new();
            s.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            statusRow.AddChild(s);
            cardVBox.AddChild(statusRow);

            card.GuiInput += (InputEvent e) =>
            {
                if (e is InputEventMouseButton btn && btn.Pressed && btn.ButtonIndex == MouseButton.Left)
                    OnAlertSelected?.Invoke(idx);
            };

            _alertListContainer.AddChild(card);
        }

        if (_alertCountLabel != null)
            _alertCountLabel.Text = $"共 {filtered.Count} 条告警 — 点击查看详情";
    }
}
