using System.Runtime.CompilerServices;

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int PackRgb(float r, float g, float b)
    {
        r = Math.Clamp(r, 0.0f, 1.0f);
        g = Math.Clamp(g, 0.0f, 1.0f);
        b = Math.Clamp(b, 0.0f, 1.0f);

        var ri = (int)(r * 255.0f + 0.5f);
        var gi = (int)(g * 255.0f + 0.5f);
        var bi = (int)(b * 255.0f + 0.5f);

        return (ri << 16) | (gi << 8) | bi;
    }
}