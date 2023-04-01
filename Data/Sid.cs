﻿using System.Diagnostics;

namespace MVE;

/// <summary>
/// 游戏配置辨识符
/// </summary>
[DebuggerDisplay("\"{Area,nq}/{Id,nq}\"")]
public struct Sid
{
    public string Area { get; set; }

    public string Id { get; set; }

    public Sid(string id)
    {
        Area = "MVE";
        Id = id;
    }

    public Sid(string area, string id)
    {
        Area = area;
        Id = id;
    }

    public static bool operator ==(Sid sidA, Sid sidB)
        => sidA.Area == sidB.Area && sidA.Id == sidB.Id;

    public static bool operator !=(Sid sidA, Sid sidB)
        => !(sidA == sidB);

    public override bool Equals(object? obj)
        => obj is Sid sid && sid == this;

    public override int GetHashCode()
        => HashCode.Combine(Area, Id);

    public override string ToString()
        => $"{Area}/{Id}";

    public static Sid Parse(string str)
    {
        string[] strs = str.Split('/');
        return strs.Length switch
        {
            2 => new(strs[0], strs[1]),
            1 => new(strs[0]),
            _ => throw new ArgumentException($"{strs.Length} parts found in `str`, expect 2 or 1.", nameof(str))
        };
    }

    public static implicit operator Sid(string str)
        => Parse(str);

    public static implicit operator Sid((string area, string str) tuple)
        => new(tuple.area, tuple.str);
}