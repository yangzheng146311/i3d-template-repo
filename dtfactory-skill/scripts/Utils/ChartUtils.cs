using i3D;

public static class ChartUtils
{
    public static readonly Color BG_DARK = new(0.03f, 0.07f, 0.14f);
    public static readonly Color GRID_COLOR = new(0.1f, 0.2f, 0.32f);
    public static readonly Color LABEL_COLOR = new(0.545f, 0.639f, 0.78f);
    public static readonly Color ACCENT = new(0f, 0.83f, 1f);
    public static readonly Color OK = new(0f, 0.9f, 0.46f);
    public static readonly Color WARN = new(1f, 0.6f, 0f);
    public static readonly Color ERR = new(1f, 0.27f, 0.27f);

    public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        if (Mathf.Abs(fromMax - fromMin) < 0.0001f) return (toMin + toMax) * 0.5f;
        return toMin + (value - fromMin) / (fromMax - fromMin) * (toMax - toMin);
    }

    public static Color LerpColor(Color a, Color b, float t)
    {
        t = Mathf.Clamp(t, 0f, 1f);
        return new Color(
            a.R + (b.R - a.R) * t,
            a.G + (b.G - a.G) * t,
            a.B + (b.B - a.B) * t,
            a.A + (b.A - a.A) * t
        );
    }

    public static Color ColorForLevel(int level)
    {
        return level switch
        {
            0 => OK,
            1 => WARN,
            2 => ERR,
            _ => LABEL_COLOR
        };
    }

    public static Color ColorForValue(float ratio)
    {
        if (ratio < 0.5f) return LerpColor(OK, WARN, ratio * 2f);
        return LerpColor(WARN, ERR, (ratio - 0.5f) * 2f);
    }

    public static float FindMin(float[] data)
    {
        float min = float.MaxValue;
        for (int i = 0; i < data.Length; i++)
            if (data[i] < min) min = data[i];
        return min;
    }

    public static float FindMax(float[] data)
    {
        float max = float.MinValue;
        for (int i = 0; i < data.Length; i++)
            if (data[i] > max) max = data[i];
        return max;
    }
}
