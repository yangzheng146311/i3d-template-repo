using i3D;

public partial class WaterfallChart : Control
{
    private float _availability = 0.94f;
    private float _performance = 0.89f;
    private float _quality = 0.98f;
    private bool _dirty = true;

    public void SetData(float availability, float performance, float quality)
    {
        _availability = availability;
        _performance = performance;
        _quality = quality;
        _dirty = true;
    }

    public void MarkDirty() => _dirty = true;

    public override void Update(double delta)
    {
        if (_dirty)
        {
            QueueRedraw();
            _dirty = false;
        }
    }

    public override void _Draw()
    {
        Vector2 size = Size;
        if (size.X < 10 || size.Y < 10) return;

        float left = 20f;
        float right = size.X - 4f;
        float top = 4f;
        float bottom = size.Y - 28f;
        float chartW = right - left;
        float chartH = bottom - top;

        float oee = _availability * _performance * _quality;

        float[] vals = { 1f, _availability, _availability * _performance, oee };
        string[] labels = { "可用率", "×性能率", "×良品率", "OEE" };
        Color[] colors = { UIStyles.ACCENT, UIStyles.ACCENT, UIStyles.ACCENT, UIStyles.OK };

        int n = 4;
        float barGap = 12f;
        float barW = (chartW - barGap * (n + 1)) / n;

        for (int i = 0; i < n; i++)
        {
            float ratio = vals[i];
            float barH = chartH * ratio;
            float x = left + barGap + i * (barW + barGap);
            float y = bottom - barH;

            Color c = colors[i];
            DrawRect(new Rect2(x, y, barW, barH), c, true);
            DrawRect(new Rect2(x, y, barW, barH), new Color(1, 1, 1, 0.1f), false, 1f);

            string pctText = (i < 3)
                ? (vals[i] * 100f).ToString("F1") + "%"
                : (oee * 100f).ToString("F1") + "%";
            DrawString(GetThemeFont("font"), new Vector2(x + barW * 0.5f - 14f, y - 4f),
                pctText, HorizontalAlignment.Left, -1, 11, UIStyles.TEXT_WHITE);

            DrawString(GetThemeFont("font"), new Vector2(x + barW * 0.5f - 14f, bottom + 16f),
                labels[i], HorizontalAlignment.Left, -1, UIStyles.FONT_TINY, ChartUtils.LABEL_COLOR);
        }
    }
}
