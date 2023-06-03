using System.Numerics;

namespace MVE.SalExt;
public struct Tag<T> : IEquatable<Tag<T>> where T : struct, IBinaryInteger<T>
{
    private T value = default;

    public Tag() { }
    public Tag(T value) { this.value = value; }

    public readonly bool Has(T flag)
        => (value & flag) != T.Zero;

    public readonly bool Hasnt(T flag)
        => (value & flag) == T.Zero;

    public void Set(T flag)
        => value = flag;

    public void Remove(T flag)
        => value &= ~flag;

    public void Remove(Tag<T> flag)
        => value &= ~flag.value;

    public void Add(T flag)
        => value |= flag;

    public void Add(Tag<T> flag)
        => value |= flag.value;

    public void Clear()
        => value = T.Zero;

    public readonly override int GetHashCode()
        => value.GetHashCode();

    public readonly override bool Equals(object? o)
        => o is Tag<T> t && t.value == value;

    public static bool operator ==(Tag<T> left, Tag<T> right)
        => left.value == right.value;

    public static bool operator !=(Tag<T> left, Tag<T> right)
        => left.value != right.value;

    public readonly bool Equals(Tag<T> other)
        => value == other.value;

    public static implicit operator Tag<T>(T v)
        => new(v);
}