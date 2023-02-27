namespace MVE;

public static class Extensions
{
    public static T Cast<T>(this object obj)
    {
        return (T)obj;
    }

    public static T? AsCast<T>(this object obj) where T : class
    {
        return obj as T;
    }

    public static Vector3 ToVec3WithZ0(this Vector2 vector2)
    {
        return new Vector3(vector2.X, vector2.Y, 0);
    }

    public static Vector3 ToVec3WithZ(this Vector2 vector2, float z)
    {
        return new Vector3(vector2.X, vector2.Y, z);
    }

    public static T? FindParent<T>(this Node self) where T : Node
    {
        var p = self.GetParent();
        return p is null ? null : p is T target ? target : FindParent<T>(p);
    }

    public static double NextDouble(this Random r, double min, double max)
        => r.NextDouble() * (max - min) + min;

    public static double NextDouble(this Random r, double max)
        => r.NextDouble(0f, max);

    public static float NextFloat(this Random r)
        => (float)r.NextDouble();

    public static float Next1m1Float(this Random r, float num)
        => r.NextFloat(-num, num);

    public static float NextFloat(this Random r, float min, float max)
        => r.NextFloat() * (max - min) + min;

    public static float NextFloat(this Random r, float max)
        => r.NextFloat(0f, max);
}