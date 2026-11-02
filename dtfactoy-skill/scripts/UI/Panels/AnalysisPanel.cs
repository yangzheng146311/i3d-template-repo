using i3D;
using System;

public partial class AnalysisPanel : VBoxContainer
{
    private SimDataService _data;

    private BarChart _yieldBar;
    private BarChart _goodRateBar;
    private WaterfallChart _oeeChart;
    private LineChart _energyChart;
    private BarChart _zoneTempBar;

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

        AddChild(UIStyles.MakeSectionTitle("数据分析"));
        AddChild(UIStyles.MakeDivider());

        AddChild(UIStyles.MakeSectionTitle("产能对比 (各区域)", UIStyles.FONT_SUBTITLE));
        _yieldBar = new BarChart();
        _yieldBar.SetCustomMinimumSize(new Vector2(0, 120));
        AddChild(_yieldBar);

        AddChild(UIStyles.MakeSpacer(4));
        AddChild(UIStyles.MakeSectionTitle("良品率对比", UIStyles.FONT_SUBTITLE));
        _goodRateBar = new BarChart();
        _goodRateBar.SetCustomMinimumSize(new Vector2(0, 100));
        AddChild(_goodRateBar);

        AddChild(UIStyles.MakeSpacer(4));
        AddChild(UIStyles.MakeSectionTitle("OEE 瀑布图", UIStyles.FONT_SUBTITLE));
        _oeeChart = new WaterfallChart();
        _oeeChart.SetCustomMinimumSize(new Vector2(0, 130));
        AddChild(_oeeChart);

        AddChild(UIStyles.MakeSpacer(4));
        AddChild(UIStyles.MakeSectionTitle("能耗趋势", UIStyles.FONT_SUBTITLE));
        _energyChart = new LineChart();
        _energyChart.SetCustomMinimumSize(new Vector2(0, 100));
        _energyChart.SetLabelFormat("{0:F0}");
        AddChild(_energyChart);

        AddChild(UIStyles.MakeSpacer(4));
        AddChild(UIStyles.MakeSectionTitle("各区温度对比", UIStyles.FONT_SUBTITLE));
        _zoneTempBar = new BarChart();
        _zoneTempBar.SetCustomMinimumSize(new Vector2(0, 110));
        AddChild(_zoneTempBar);

        AddChild(UIStyles.MakeSpacer(8));

        HBoxContainer btnRow = new();
        btnRow.AddThemeConstantOverride("separation", 8);

        Button pdfBtn = new();
        pdfBtn.Text = "导出 PDF";
        pdfBtn.SetCustomMinimumSize(new Vector2(0, 34));
        pdfBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        pdfBtn.AddThemeStyleboxOverride("normal", UIBuilder.MakeBtnStyle(UIStyles.ACCENT, UIStyles.ACCENT_BLUE, 6));
        pdfBtn.AddThemeColorOverride("font_color", UIStyles.TEXT_WHITE);
        pdfBtn.AddThemeFontSizeOverride("font_size", UIStyles.FONT_SMALL);
        btnRow.AddChild(pdfBtn);

        Button excelBtn = new();
        excelBtn.Text = "导出 Excel";
        excelBtn.SetCustomMinimumSize(new Vector2(0, 34));
        excelBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        excelBtn.AddThemeStyleboxOverride("normal",
            UIBuilder.MakeBtnStyle(new Color(0.1f, 0.18f, 0.3f), UIStyles.DIVIDER, 6));
        excelBtn.AddThemeColorOverride("font_color", UIStyles.TEXT_DIM);
        excelBtn.AddThemeFontSizeOverride("font_size", UIStyles.FONT_SMALL);
        btnRow.AddChild(excelBtn);

        AddChild(btnRow);

        RefreshData();
    }

    public void RefreshData()
    {
        if (_data == null) return;

        string[] zoneLabels = { "A区", "B区", "C区" };
        Color[] zoneColors = { UIStyles.ACCENT, UIStyles.OK, UIStyles.WARN };

        if (_yieldBar != null)
        {
            float[] zoneYield = _data.GetZoneYield();
            _yieldBar.SetData(zoneYield, zoneLabels, zoneColors);
            _yieldBar.MarkDirty();
        }

        if (_goodRateBar != null)
        {
            float[] zoneGR = _data.GetZoneGoodRate();
            Color[] grColors = { UIStyles.OK, UIStyles.OK, UIStyles.OK };
            _goodRateBar.SetData(zoneGR, zoneLabels, grColors);
            _goodRateBar.MarkDirty();
        }

        if (_oeeChart != null)
        {
            _oeeChart.SetData(_data.GetAvailability(), _data.GetPerformance(), _data.GetQuality());
            _oeeChart.MarkDirty();
        }

        if (_energyChart != null)
        {
            float[] elecHistory = new float[_data.EnergyHistory.Length];
            float[] waterHistory = new float[_data.EnergyHistory.Length];
            float[] gasHistory = new float[_data.EnergyHistory.Length];
            for (int i = 0; i < elecHistory.Length; i++)
            {
                elecHistory[i] = _data.EnergyHistory[i];
                waterHistory[i] = 30f + Mathf.Sin(i * 0.5f) * 8f;
                gasHistory[i] = 15f + Mathf.Sin(i * 0.3f + 2f) * 5f;
            }
            _energyChart.SetMultiData(
                new float[][] { elecHistory, waterHistory, gasHistory },
                new Color[] { UIStyles.ACCENT, UIStyles.OK, UIStyles.WARN }
            );
        }

        if (_zoneTempBar != null)
        {
            float[] zoneAvg = _data.GetZoneAvgTemp();
            _zoneTempBar.SetData(zoneAvg, zoneLabels, zoneColors);
            _zoneTempBar.MarkDirty();
        }
    }
}
