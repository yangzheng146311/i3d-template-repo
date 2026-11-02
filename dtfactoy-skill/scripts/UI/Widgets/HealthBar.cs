using i3D;

public partial class HealthBar : Control
{
    private int _score = 100;
    private bool _dirty = true;

    public void SetScore(int score)
    {
        _score = Mathf.Clamp(score, 0, 100);
        _dirty = true;
    }

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

        float barH = 10f;
        float barY = 4f;
        float barLeft = 0f;
        float barRight = size.X - 50f;

        DrawRect(new Rect2(barLeft, barY, barRight - barLeft, barH),
            new Color(0.08f, 0.15f, 0.25f), true);

        float ratio = _score / 100f;
        Color barColor;
        if (_score >= 80) barColor = UIStyles.OK;
        else if (_score >= 50) barColor = UIStyles.WARN;
        else barColor = UIStyles.ERR;

        float fillW = (barRight - barLeft) * ratio;
        DrawRect(new Rect2(barLeft, barY, fillW, barH), barColor, true);

        string scoreText = $"{_score}/100";
        DrawString(GetThemeFont("font"), new Vector2(barRight + 6f, barY + barH - 1),
            scoreText, HorizontalAlignment.Left, -1, UIStyles.FONT_BODY, barColor);

        float labelY = barY + barH + 14f;
        DrawString(GetThemeFont("font"), new Vector2(0, labelY),
            "健康评分", HorizontalAlignment.Left, -1, UIStyles.FONT_SMALL, UIStyles.TEXT_DIM);
    }
}
