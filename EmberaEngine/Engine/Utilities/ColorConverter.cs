using OpenTK.Mathematics;
using System;
using System.Globalization;

public static class ColorConverter
{
    /// <summary>
    /// Convert hex string (#RRGGBB or RRGGBB) to Color4.
    /// </summary>
    public static Color4 FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            throw new ArgumentException("Hex color string is null or empty.");

        hex = hex.TrimStart('#');

        if (hex.Length == 6)
        {
            // Parse RRGGBB
            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            return new Color4(r, g, b, 255);
        }
        else if (hex.Length == 8)
        {
            // Parse RRGGBBAA
            byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
            byte a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);
            return new Color4(r, g, b, a);
        }
        else
        {
            throw new FormatException("Hex color string must be 6 or 8 characters long.");
        }
    }

    /// <summary>
    /// Convert RGB or RGBA string "R,G,B" or "R,G,B,A" (0-255) to Color4.
    /// </summary>
    public static Color4 FromRgbString(string rgbString)
    {
        if (string.IsNullOrWhiteSpace(rgbString))
            throw new ArgumentException("RGB string is null or empty.");

        var parts = rgbString.Split(',');
        if (parts.Length < 3 || parts.Length > 4)
            throw new FormatException("RGB string must have 3 or 4 components.");

        byte r = byte.Parse(parts[0].Trim());
        byte g = byte.Parse(parts[1].Trim());
        byte b = byte.Parse(parts[2].Trim());
        byte a = parts.Length == 4 ? byte.Parse(parts[3].Trim()) : (byte)255;

        return new Color4(r, g, b, a);
    }

    /// <summary>
    /// Convert from 0-1 normalized floats (r,g,b,a) to Color4.
    /// </summary>
    public static Color4 FromNormalizedFloats(float r, float g, float b, float a = 1f)
    {
        return new Color4(r, g, b, a);
    }
}
