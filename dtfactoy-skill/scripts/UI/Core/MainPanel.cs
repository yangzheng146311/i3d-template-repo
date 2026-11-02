using i3D;
using System;

public partial class MainPanel : Control
{
    private NavPanel _navPanel;
    private ContentPanel _contentPanel;
    private Label _clockLabel;
    private Label _alertBadge;
    private Label _bottomOnline;
    private Label _bottomAlert;
    private Label _bottomOee;
    private Label _bottomYield;

    private SimDataService _data;
    private SpringArmCamera _camera;
    private DeviceMarker _marker;
    private bool _initDone;

    private OverviewPanel _overviewPanel;
    private MonitorPanel _monitorPanel;
    private AlertPanel _alertPanel;
    private AnalysisPanel _analysisPanel;
    private SafetyPanel _safetyPanel;

    private DeviceDetailPopup _devicePopup;
    private AlertDetailPopup _alertPopup;

    public override void iStart()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        SetOffsetsPreset(LayoutPreset.FullRect);

        CreateTopBar();
        CreateLeftNav();
        CreateRightPanel();
        CreateBottomBar();
        CreatePopups();

        FindDataAndCamera();
    }

    public override void Update(double delta)
    {
        if (_clockLabel != null)
            _clockLabel.Text = DateTime.Now.ToString("yyyy-MM-dd  HH:mm:ss");

        if (!_initDone)
        {
            if (_data == null || _camera == null)
                FindDataAndCamera();
            if (_data != null && _camera != null)
            {
                _initDone = true;
                S3.Print("[MainPanel] init done");
            }
        }

        if (_data != null && _marker == null)
        {
            var node = GetTree().GetFirstItemInGroup("device_marker");
            if (node is DeviceMarker m)
            {
                _marker = m;
                S3.Print("[MainPanel] found DeviceMarker");
            }
        }
    }

    private void FindDataAndCamera()
    {
        var tree = GetTree();

        if (_data == null)
        {
            var dataNode = tree.GetFirstItemInGroup("sim_data");
            if (dataNode is SimDataService ds)
            {
                _data = ds;
                _data.OnDataUpdated += OnDataRefresh;
                InitPanelsWithData();
                OnDataRefresh();
                S3.Print("[MainPanel] found SimDataService");
            }
        }

        if (_camera == null)
        {
            var camNode = tree.GetFirstItemInGroup("camera");
            if (camNode is SpringArmCamera cam)
            {
                _camera = cam;
                S3.Print("[MainPanel] found SpringArmCamera");
            }
        }
    }

    private void InitPanelsWithData()
    {
        if (_data == null) return;

        if (_overviewPanel != null) _overviewPanel.SetData(_data);
        if (_monitorPanel != null) _monitorPanel.SetData(_data);
        if (_alertPanel != null) _alertPanel.SetData(_data);
        if (_analysisPanel != null) _analysisPanel.SetData(_data);
        if (_safetyPanel != null) _safetyPanel.SetData(_data);
        if (_navPanel != null) _navPanel.SetData(_data);
    }

    private void OnDataRefresh()
    {
        if (_data == null) return;

        if (_overviewPanel != null) _overviewPanel.RefreshData();
        if (_monitorPanel != null) _monitorPanel.RefreshData();
        if (_alertPanel != null) _alertPanel.RefreshData();
        if (_analysisPanel != null) _analysisPanel.RefreshData();
        if (_safetyPanel != null) _safetyPanel.RefreshData();
        if (_navPanel != null) _navPanel.RefreshDeviceTree();

        if (_bottomOnline != null) _bottomOnline.Text = _data.GetOnlineCount().ToString();
        if (_bottomAlert != null) _bottomAlert.Text = _data.AlertCount.ToString();
        if (_bottomOee != null) _bottomOee.Text = _data.GetOverallOEE().ToString("F1") + "%";
        if (_bottomYield != null) _bottomYield.Text = ((int)_data.GetTotalYield()).ToString();
        if (_alertBadge != null) _alertBadge.Text = _data.AlertCount.ToString();
    }

    private void CreateTopBar()
    {
        Panel panel = new();
        panel.SetAnchorsPreset(LayoutPreset.TopWide);
        panel.SetOffsetsPreset(LayoutPreset.TopWide);
        panel.OffsetBottom = UIStyles.TOP_H;
        panel.AddThemeStyleboxOverride("panel", UIStyles.MakePanelStyle());
        AddChild(panel);

        HBoxContainer hbox = new();
        hbox.SetAnchorsPreset(LayoutPreset.FullRect);
        hbox.SetOffsetsPreset(LayoutPreset.FullRect);
        hbox.AddThemeConstantOverride("separation", 0);
        panel.AddChild(hbox);

        Control spacerL = new();
        spacerL.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        hbox.AddChild(spacerL);

        Label title = UIStyles.MakeLabel("深圳市龙岗区某某电厂 · 数字孪生平台", UIStyles.TEXT_WHITE, 22);
        title.VerticalAlignment = VerticalAlignment.Center;
        hbox.AddChild(title);

        Control spacerR = new();
        spacerR.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        hbox.AddChild(spacerR);

        Button alertBell = new();
        alertBell.Text = "🔔";
        alertBell.AddThemeFontSizeOverride("font_size", 18);
        alertBell.SetCustomMinimumSize(new Vector2(40, 0));
        alertBell.AddThemeStyleboxOverride("normal",
            UIStyles.MakeStyleBox(Colors.Transparent, Colors.Transparent, 0, 0));
        alertBell.Pressed += () =>
        {
            if (_navPanel != null) _navPanel.SelectModule(2);
            if (_contentPanel != null) _contentPanel.SelectPanel(2);
        };
        hbox.AddChild(alertBell);

        _alertBadge = UIStyles.MakeLabel("0", UIStyles.ERR, 10);
        _alertBadge.SetCustomMinimumSize(new Vector2(18, 18));
        _alertBadge.HorizontalAlignment = HorizontalAlignment.Center;
        _alertBadge.VerticalAlignment = VerticalAlignment.Center;
        StyleBoxFlat badgeStyle = UIStyles.MakeStyleBox(UIStyles.ERR, UIStyles.ERR, 0, 9);
        badgeStyle.SetContentMarginAll(2);
        _alertBadge.AddThemeStyleboxOverride("normal", badgeStyle);
        hbox.AddChild(_alertBadge);

        Control spacerBell = new();
        spacerBell.SetCustomMinimumSize(new Vector2(8, 0));
        hbox.AddChild(spacerBell);

        Label userIcon = UIStyles.MakeLabel("👤", UIStyles.TEXT_DIM, 16);
        userIcon.VerticalAlignment = VerticalAlignment.Center;
        hbox.AddChild(userIcon);

        Control spacerUser = new();
        spacerUser.SetCustomMinimumSize(new Vector2(16, 0));
        hbox.AddChild(spacerUser);

        _clockLabel = UIStyles.MakeLabel("", UIStyles.TEXT_DIM, 14);
        _clockLabel.SetCustomMinimumSize(new Vector2(180, 0));
        _clockLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _clockLabel.VerticalAlignment = VerticalAlignment.Center;
        hbox.AddChild(_clockLabel);

        Control spacerEnd = new();
        spacerEnd.SetCustomMinimumSize(new Vector2(12, 0));
        hbox.AddChild(spacerEnd);

        ColorRect line = new();
        line.SetAnchorsPreset(LayoutPreset.BottomWide);
        line.SetOffsetsPreset(LayoutPreset.BottomWide);
        line.OffsetTop = -1;
        line.OffsetBottom = 0;
        line.Color = UIStyles.ACCENT;
        panel.AddChild(line);
    }

    private void CreateLeftNav()
    {
        _navPanel = new NavPanel();
        _navPanel.AnchorLeft = 0f;
        _navPanel.AnchorRight = 0f;
        _navPanel.AnchorTop = 0f;
        _navPanel.AnchorBottom = 1f;
        _navPanel.OffsetLeft = 0f;
        _navPanel.OffsetRight = UIStyles.LEFT_W;
        _navPanel.OffsetTop = UIStyles.TOP_H;
        _navPanel.OffsetBottom = -UIStyles.BOTTOM_H;
        AddChild(_navPanel);

        _navPanel.OnModuleSelected += (idx) =>
        {
            if (_contentPanel != null) _contentPanel.SelectPanel(idx);
            if (idx == 1 && _monitorPanel != null && _data != null)
                _monitorPanel.SetDevice(0);
        };

        _navPanel.OnDeviceSelected += (devIdx) =>
        {
            if (_data == null || devIdx < 0 || devIdx >= _data.Devices.Length) return;
            LocateDevice(_data.Devices[devIdx].Name);
            if (_monitorPanel != null) _monitorPanel.SetDevice(devIdx);
            if (_navPanel != null) _navPanel.SelectModule(1);
            if (_contentPanel != null) _contentPanel.SelectPanel(1);
        };
    }

    private void CreateRightPanel()
    {
        _contentPanel = new ContentPanel();
        AddChild(_contentPanel);

        _overviewPanel = new OverviewPanel();
        _contentPanel.GetPanel(0).AddChild(_overviewPanel);
        _overviewPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _overviewPanel.OnDeviceSelected += (idx) => { if (_data != null) LocateDevice(_data.Devices[idx].Name); };
        _overviewPanel.OnAlertSelected += (idx) => ShowAlertDetail(idx);

        _monitorPanel = new MonitorPanel();
        _contentPanel.GetPanel(1).AddChild(_monitorPanel);
        _monitorPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        _alertPanel = new AlertPanel();
        _contentPanel.GetPanel(2).AddChild(_alertPanel);
        _alertPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _alertPanel.OnAlertSelected += (idx) => ShowAlertDetail(idx);
        _alertPanel.OnDeviceLocated += (name) => LocateDevice(name);

        _analysisPanel = new AnalysisPanel();
        _contentPanel.GetPanel(3).AddChild(_analysisPanel);
        _analysisPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        _safetyPanel = new SafetyPanel();
        _contentPanel.GetPanel(4).AddChild(_safetyPanel);
        _safetyPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        _contentPanel.SelectPanel(0);
    }

    private void CreateBottomBar()
    {
        Panel panel = new();
        panel.SetAnchorsPreset(LayoutPreset.BottomWide);
        panel.SetOffsetsPreset(LayoutPreset.BottomWide);
        panel.OffsetTop = -UIStyles.BOTTOM_H;
        panel.AddThemeStyleboxOverride("panel", UIStyles.MakePanelStyle());
        AddChild(panel);

        HBoxContainer hbox = new();
        hbox.SetAnchorsPreset(LayoutPreset.FullRect);
        hbox.SetOffsetsPreset(LayoutPreset.FullRect);
        hbox.AddThemeConstantOverride("separation", 0);
        panel.AddChild(hbox);

        hbox.AddChild(UIBuilder.MakeBottomStatChip("设备", _data?.Devices.Length.ToString() ?? "6", UIStyles.TEXT_WHITE));
        hbox.AddChild(UIBuilder.MakeBottomSeparator());
        _bottomOnline = UIStyles.MakeLabel("0", UIStyles.OK, 16);
        hbox.AddChild(UIBuilder.MakeBottomStatChipRef("在线", _bottomOnline, UIStyles.OK));
        hbox.AddChild(UIBuilder.MakeBottomSeparator());
        _bottomAlert = UIStyles.MakeLabel("0", UIStyles.WARN, 16);
        hbox.AddChild(UIBuilder.MakeBottomStatChipRef("告警", _bottomAlert, UIStyles.WARN));
        hbox.AddChild(UIBuilder.MakeBottomSeparator());
        _bottomOee = UIStyles.MakeLabel("--", UIStyles.ACCENT, 16);
        hbox.AddChild(UIBuilder.MakeBottomStatChipRef("OEE", _bottomOee, UIStyles.ACCENT));
        hbox.AddChild(UIBuilder.MakeBottomSeparator());
        _bottomYield = UIStyles.MakeLabel("--", UIStyles.OK, 16);
        hbox.AddChild(UIBuilder.MakeBottomStatChipRef("产量", _bottomYield, UIStyles.OK));

        ColorRect line = new();
        line.SetAnchorsPreset(LayoutPreset.TopWide);
        line.SetOffsetsPreset(LayoutPreset.TopWide);
        line.OffsetTop = 0;
        line.OffsetBottom = 1;
        line.Color = UIStyles.ACCENT;
        panel.AddChild(line);
    }

    private void CreatePopups()
    {
        _alertPopup = new AlertDetailPopup();
        _alertPopup.OnLocateDevice += LocateDevice;
        AddChild(_alertPopup);

        _devicePopup = new DeviceDetailPopup();
        _devicePopup.OnLocateDevice += LocateDevice;
        AddChild(_devicePopup);
    }

    private void LocateDevice(string deviceName)
    {
        if (_data == null || _camera == null) return;
        int idx = _data.FindDeviceIndex(deviceName);
        if (idx >= 0)
        {
            _camera.FlyTo(_data.Devices[idx].Position, 0.8f);
            _marker?.Highlight(idx);
            S3.Print("[MainPanel] FlyTo " + deviceName);
        }
    }

    private void ShowAlertDetail(int alertIdx)
    {
        if (_data == null || alertIdx >= _data.AlertCount) return;
        _alertPopup.ShowAlert(_data.Alerts[alertIdx]);
    }

    private void ShowDeviceDetail(int deviceIdx)
    {
        if (_data == null || deviceIdx >= _data.Devices.Length) return;
        _devicePopup.ShowDevice(_data.Devices[deviceIdx], _data.TempHistory[deviceIdx], _data);
    }
}
