using System.Diagnostics;

namespace MVE;

/// <summary>
/// 游戏配置辨识符
/// </summary>
[DebuggerDisplay("\"{Area,nq}/{Id,nq}\"")]
public struct Sid : IEquatable<Sid>
{
    public const string MVE = "MVE";

    public string Area { get; set; }

    public string Id { get; set; }

    public Sid(string area, string id)
        => (Area, Id) = (area, id);

    public static bool operator ==(Sid sidA, Sid sidB)
        => sidA.Area == sidB.Area && sidA.Id == sidB.Id;

    public static bool operator !=(Sid sidA, Sid sidB)
        => !(sidA == sidB);

    public readonly override bool Equals(object? obj)
        => obj is Sid sid && sid == this;

    public readonly override int GetHashCode()
        => HashCode.Combine(Area, Id);

    public readonly override string ToString()
        => $"{Area}/{Id}";

    public static Sid Parse(string str)
    {
        string[] strs = str.Split('/');
        return strs.Length switch
        {
            2 => new(strs[0], strs[1]),
            1 => new(MVE, strs[0]),
            _ => throw new ArgumentException($"{strs.Length} parts found in `str`, expect 2 or 1.", nameof(str))
        };
    }

    public static implicit operator Sid(string id)
        => new(MVE, id);

    public static implicit operator Sid((string area, string str) tuple)
        => new(tuple.area, tuple.str);

    readonly bool IEquatable<Sid>.Equals(Sid other)
        => this == other;
}