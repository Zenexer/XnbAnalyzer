using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace XnbAnalyzer.Xnb.Content;

[Serializable]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public readonly struct Color : IEquatable<Color>
{
    private readonly uint packedValue;

    public uint PackedValue => packedValue;

    public byte R => unchecked((byte)packedValue);
    public byte G => unchecked((byte)(packedValue >> 8));
    public byte B => unchecked((byte)(packedValue >> 16));
    public byte A => unchecked((byte)(packedValue >> 24));

    public Color(uint packedValue)
        => this.packedValue = packedValue;

    public Color(byte r, byte g, byte b, byte a)
        => packedValue = unchecked((uint)r | ((uint)g << 8) | ((uint)b << 16) | ((uint)a << 24));

    public Color(byte r, byte g, byte b)
        => packedValue = unchecked(0xff000000u | (uint)r | ((uint)g << 8) | ((uint)b << 16));

    public bool Equals(Color other) => packedValue == other.packedValue;
    public override bool Equals(object? obj) => obj is Color color && Equals(color);
    public static bool operator ==(Color left, Color right) => left.Equals(right);
    public static bool operator !=(Color left, Color right) => left == right;
    public override int GetHashCode() => packedValue.GetHashCode();

    public override string ToString() => $"#{R:xx}{G:xx}{B:xx}{A:xx}";
    private string GetDebuggerDisplay() => ToString();
}
