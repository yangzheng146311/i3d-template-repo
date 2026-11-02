using i3D;
using System;

public partial class SafetyPanel : VBoxContainer
{
    private SimDataService _data;

    private Label _personnelLabel;
    private Label _safetyEventLabel;
    private Label _safetyStatusLabel;
    private ParamBar[,] _gasBars = new ParamBar[3, 3];
    private CheckBox[] _fenceCheckboxes = new CheckBox[3];
    private VBoxContainer _eventListContainer;

    private static readonly string[] ZONE_NAMES = { "A区", "B区", "C区" };
    private static readonly string[] GAS_NAMES = { "CO", "CH₄", "可燃气体" };
    private static readonly string[] GAS_UNITS = { "ppm", "%", "%" };
    private static readonly float[,] GAS_RANGES = {
        { 0f, 50f }, { 0f, 0.5f }, { 0f, 0.3f }
    };

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

        AddChild(UIStyles.MakeSectionTitle("安全管理"));
        AddChild(UIStyles.MakeDivider());

        AddChild(UIStyles.MakeSectionTitle("安全态势", UIStyles.FONT_SUBTITLE));

        PanelContainer statusCard = new();
        statusCard.AddThemeStyleboxOverride("panel", UIStyles.MakeCardStyle(UIStyles.OK));
        statusCard.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        VBoxContainer statusVBox = new();
        statusVBox.AddThemeConstantOverride("separation", 4);
        statusCard.AddChild(statusVBox);

        HBoxContainer statusRow = new();
        statusRow.AddThemeConstantOverride("separation", 12);

        HBoxContainer personnelBox = new();
        personnelBox.AddThemeConstantOverride("separation", 6);
        _personnelLabel = UIStyles.MakeLabel("23", UIStyles.TEXT_WHITE, UIStyles.FONT_BIG);
        personnelBox.AddChild(_personnelLabel);
        personnelBox.AddChild(UIStyles.MakeLabel("厂区人员", UIStyles.TEXT_DIM, UIStyles.FONT_SMALL));
        statusRow.AddChild(personnelBox);

        HBoxContainer eventBox = new();
        eventBox.AddThemeConstantOverride("separation", 6);
        _safetyEventLabel = UIStyles.MakeLabel("0", UIStyles.OK, UIStyles.FONT_BIG);
        eventBox.AddChild(_safetyEventLabel);
        eventBox.AddChild(UIStyles.MakeLabel("安全事件", UIStyles.TEXT_DIM, UIStyles.FONT_SMALL));
        statusRow.AddChild(eventBox);

        Control spacer = new();
        spacer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        statusRow.AddChild(spacer);

        _safetyStatusLabel = UIStyles.MakeLabel("[安全]", UIStyles.OK, 16);
        statusRow.AddChild(_safetyStatusLabel);

        statusVBox.AddChild(statusRow);
        AddChild(statusCard);

        AddChild(UIStyles.MakeSpacer(8));
        AddChild(UIStyles.MakeSectionTitle("气体监测", UIStyles.FONT_SUBTITLE));
        AddChild(UIStyles.MakeDivider());

        for (int z = 0; z < 3; z++)
        {
            Label zoneLabel = UIStyles.MakeLabel(ZONE_NAMES[z], UIStyles.TEXT_DIM, UIStyles.FONT_SMALL);
            zoneLabel.SetCustomMinimumSize(new Vector2(0, 20));
            AddChild(zoneLabel);

            for (int g = 0; g < 3; g++)
            {
                ParamBar bar = new();
                bar.SetCustomMinimumSize(new Vector2(0, 22));
                bar.Setup("  " + GAS_NAMES[g], GAS_UNITS[g], GAS_RANGES[g, 0], GAS_RANGES[g, 1], 0);
                _gasBars[z, g] = bar;
                AddChild(bar);
            }

            if (z < 2) AddChild(UIStyles.MakeSpacer(2));
        }

        AddChild(UIStyles.MakeSpacer(8));
        AddChild(UIStyles.MakeSectionTitle("电子围栏", UIStyles.FONT_SUBTITLE));
        AddChild(UIStyles.MakeDivider());

        string[] fenceNames = { "高压区域", "化学品存储区", "重型设备区" };
        for (int i = 0; i < 3; i++)
        {
            HBoxContainer fenceRow = new();
            fenceRow.SetCustomMinimumSize(new Vector2(0, 30));
            fenceRow.AddThemeConstantOverride("separation", 8);

            CheckBox cb = new();
            cb.ButtonPressed = true;
            cb.AddThemeColorOverride("font_color", UIStyles.TEXT_WHITE);
            cb.AddThemeFontSizeOverride("font_size", UIStyles.FONT_SMALL);
            _fenceCheckboxes[i] = cb;
            fenceRow.AddChild(cb);

            fenceRow.AddChild(UIStyles.MakeLabel(fenceNames[i], UIStyles.TEXT_DIM, UIStyles.FONT_BODY));

            Control s = new();
            s.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            fenceRow.AddChild(s);

            fenceRow.AddChild(UIStyles.MakeLabel("[启用]", UIStyles.OK, UIStyles.FONT_TINY));
            AddChild(fenceRow);
        }

        Label fenceInfo = UIStyles.MakeLabel("危险区域: 3处 · 当前越界: 0人", UIStyles.TEXT_DIM, UIStyles.FONT_TINY);
        AddChild(fenceInfo);

        AddChild(UIStyles.MakeSpacer(8));
        AddChild(UIStyles.MakeSectionTitle("视频监控", UIStyles.FONT_SUBTITLE));
        AddChild(UIStyles.MakeDivider());

        GridContainer videoGrid = new();
        videoGrid.Columns = 2;
        videoGrid.AddThemeConstantOverride("separation", 4);

        for (int i = 0; i < 4; i++)
        {
            PanelContainer camPlaceholder = new();
            camPlaceholder.SetCustomMinimumSize(new Vector2(140, 90));
            StyleBoxFlat camStyle = UIStyles.MakeStyleBox(new Color(0.03f, 0.06f, 0.12f), UIStyles.DIVIDER, 1, 4);
            camStyle.SetContentMarginAll(4);
            camPlaceholder.AddThemeStyleboxOverride("panel", camStyle);

            VBoxContainer camVBox = new();
            camVBox.AddThemeConstantOverride("separation", 2);
            camPlaceholder.AddChild(camVBox);

            HBoxContainer camHeader = new();
            camHeader.AddThemeConstantOverride("separation", 4);
            ColorRect recDot = new();
            recDot.SetCustomMinimumSize(new Vector2(6, 6));
            recDot.Color = UIStyles.ERR;
            camHeader.AddChild(recDot);
            camHeader.AddChild(UIStyles.MakeLabel("CAM " + (i + 1), UIStyles.TEXT_DIM, UIStyles.FONT_TINY));
            Control cs = new();
            cs.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            camHeader.AddChild(cs);
            camHeader.AddChild(UIStyles.MakeLabel("REC", UIStyles.ERR, UIStyles.FONT_TINY));
            camVBox.AddChild(camHeader);

            Control camBody = new();
            camBody.SizeFlagsVertical = SizeFlags.ExpandFill;
            camVBox.AddChild(camBody);

            videoGrid.AddChild(camPlaceholder);
        }
        AddChild(videoGrid);

        AddChild(UIStyles.MakeSpacer(8));
        AddChild(UIStyles.MakeSectionTitle("今日安全事件", UIStyles.FONT_SUBTITLE));
        AddChild(UIStyles.MakeDivider());

        ScrollContainer scroll = new();
        scroll.SetCustomMinimumSize(new Vector2(0, 80));
        scroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
        AddChild(scroll);

        _eventListContainer = new VBoxContainer();
        _eventListContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _eventListContainer.AddThemeConstantOverride("separation", 3);
        scroll.AddChild(_eventListContainer);

        RefreshData();
    }

    public void RefreshData()
    {
        if (_data == null) return;

        if (_personnelLabel != null) _personnelLabel.Text = _data.PersonnelCount.ToString();

        int eventCount = _data.SafetyEvents.Count;
        if (_safetyEventLabel != null)
        {
            _safetyEventLabel.Text = eventCount.ToString();
            _safetyEventLabel.AddThemeColorOverride("font_color", eventCount > 0 ? UIStyles.WARN : UIStyles.OK);
        }
        if (_safetyStatusLabel != null)
        {
            _safetyStatusLabel.Text = eventCount > 0 ? "[注意]" : "[安全]";
            _safetyStatusLabel.AddThemeColorOverride("font_color", eventCount > 0 ? UIStyles.WARN : UIStyles.OK);
        }

        for (int z = 0; z < 3; z++)
        {
            if (_gasBars[z, 0] != null) _gasBars[z, 0].SetValue(_data.GetGasCO(z));
            if (_gasBars[z, 1] != null) _gasBars[z, 1].SetValue(_data.GetGasCH4(z));
            if (_gasBars[z, 2] != null) _gasBars[z, 2].SetValue(_data.GetGasFlammable(z));
        }

        RefreshEventList();
    }

    private void RefreshEventList()
    {
        if (_eventListContainer == null || _data == null) return;

        while (_eventListContainer.GetChildCount() > 0)
        {
            var child = _eventListContainer.GetChild(0);
            _eventListContainer.RemoveChild(child);
            child.QueueFree();
        }

        if (_data.SafetyEvents.Count == 0)
        {
            _eventListContainer.AddChild(
                UIStyles.MakeLabel("今日无安全事件", UIStyles.OK, UIStyles.FONT_SMALL));
            return;
        }

        for (int i = 0; i < _data.SafetyEvents.Count; i++)
        {
            var evt = _data.SafetyEvents[i];

            HBoxContainer row = new();
            row.SetCustomMinimumSize(new Vector2(0, 24));
            row.AddThemeConstantOverride("separation", 6);

            ColorRect typeDot = new();
            typeDot.SetCustomMinimumSize(new Vector2(4, 12));
            typeDot.Color = UIStyles.WARN;
            typeDot.SizeFlagsVertical = SizeFlags.ShrinkCenter;
            row.AddChild(typeDot);

            row.AddChild(UIStyles.MakeLabel(evt.Type, UIStyles.WARN, UIStyles.FONT_SMALL));
            Label desc = UIStyles.MakeLabel(evt.Description, UIStyles.TEXT_DIM, UIStyles.FONT_TINY);
            desc.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            desc.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            row.AddChild(desc);
            row.AddChild(UIStyles.MakeLabel(evt.Time.ToString("HH:mm"), UIStyles.TEXT_DIM, UIStyles.FONT_TINY));

            _eventListContainer.AddChild(row);
        }
    }
}
