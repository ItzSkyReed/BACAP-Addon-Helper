namespace Core.Utils;

public static class ColorUtils
{
    /// <summary>
    /// Converts a normalized float RGB triplet into a packed 24-bit integer (<c>0xRRGGBB</c>).
    /// </summary>
    /// <param name="r">Red component in range [0.0, 1.0].</param>
    /// <param name="g">Green component in range [0.0, 1.0].</param>
    /// <param name="b">Blue component in range [0.0, 1.0].</param>
    /// <returns>The packed RGB integer value.</returns>
    public static int PackRgb(float r, float g, float b)
    {
        var ri = Math.Clamp((int)Math.Round(r * 255.0f), 0, 255);
        var gi = Math.Clamp((int)Math.Round(g * 255.0f), 0, 255);
        var bi = Math.Clamp((int)Math.Round(b * 255.0f), 0, 255);

        return (ri << 16) | (gi << 8) | bi;
    }
}