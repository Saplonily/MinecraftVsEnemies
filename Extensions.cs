global using GodotColor = Godot.Color;
global using GodotFileAccess = Godot.FileAccess;
global using Timer = Godot.Timer;
global using Vector2 = Godot.Vector2;
global using Vector3 = Godot.Vector3;
global using FileAccess = System.IO.FileAccess;
using System.IO;

namespace MVE;

public static class Extensions
{
    #region misc

    public static T? AsCast<T>(this object obj) where T : class
        => obj as T;

    public static bool Only<T>(this IEnumerable<T> ie, T instance) where T : class
    {
        using IEnumerator<T> ieo = ie.GetEnumerator();
        if (!ieo.MoveNext()) return false;
        var cur = ieo.Current;
        if (ieo.MoveNext()) return false;
        return ReferenceEquals(cur, instance);
    }

    #endregion

    #region gd

    public static Vector3 ToVec3WithZ0(this Vector2 vector2)
        => new(vector2.X, vector2.Y, 0);

    public static Vector3 ToVec3WithZ(this Vector2 vector2, float z)
        => new(vector2.X, vector2.Y, z);

    public static T? FindParent<T>(this Node self) where T : Node
    {
        var p = self.GetParent();
        return p is null ? null : p is T target ? target : FindParent<T>(p);
    }

    public static T RemoveSelf<T>(this T node) where T : Node
    {
        node.GetParent().RemoveChild(node);
        return node;
    }

    public static void Switch2DParent(this Node2D node2d, Node2D newParent)
    {
        node2d.Transform = node2d.GetGlobalTransformWithCanvas();
        Node parent = node2d.GetParent();
        parent.RemoveChild(node2d);
        newParent.AddChild(node2d);
        node2d.Transform = newParent.GetGlobalTransformWithCanvas().AffineInverse() * node2d.Transform;
    }

    public static void Switch2DParent(this Node2D node2d, CanvasLayer newParent)
    {
        node2d.Transform = node2d.GetGlobalTransformWithCanvas();
        Node parent = node2d.GetParent();
        parent.RemoveChild(node2d);
        newParent.AddChild(node2d);
        node2d.Transform = newParent.GetFinalTransform().AffineInverse() * node2d.Transform;
    }

    public static Vector2 GetPositionAndFree(this Marker2D marker2D)
    {
        Vector2 pos = marker2D.Position;
        marker2D.Free();
        return pos;
    }

    /// <summary>
    /// 将给定的<paramref name="from"/>的坐标<paramref name="position"/>转换为<paramref name="to"/>视角的坐标
    /// </summary>
    public static Vector2 SwitchTransform(Node2D from, Node2D to, Vector2 position)
    {
        Vector2 pos = from.GetGlobalTransformWithCanvas() * position;
        pos = to.GetGlobalTransformWithCanvas().AffineInverse() * pos;
        return pos;
    }

    /// <summary>
    /// 将给定的<paramref name="from"/>的坐标<paramref name="position"/>转换为<paramref name="to"/>视角的坐标
    /// </summary>
    public static Vector2 SwitchTransform(Node2D from, CanvasLayer to, Vector2 position)
    {
        Vector2 pos = from.GetGlobalTransformWithCanvas() * position;
        pos = to.GetFinalTransform().AffineInverse() * pos;
        return pos;
    }

    /// <summary>
    /// 将给定的<paramref name="from"/>的坐标<paramref name="position"/>转换为<paramref name="to"/>视角的坐标
    /// </summary>
    public static Vector2 SwitchTransform(Node2D from, CanvasLayer to)
        => SwitchTransform(from, to, Vector2.Zero);

    /// <summary>
    /// 将给定的<paramref name="from"/>的坐标转换为<paramref name="to"/>视角的坐标
    /// </summary>
    public static Vector2 SwitchTransform(Node2D from, Node2D to)
        => SwitchTransform(from, to, Vector2.Zero);

    public static Rect2 GetCameraRect(this Camera2D camera)
    {
        Vector2 size = camera.GetViewportRect().Size / 2 / camera.Zoom;
        Vector2 leftTop = camera.AnchorMode is Camera2D.AnchorModeEnum.DragCenter ?
            camera.GetTargetPosition() :
            camera.GetTargetPosition() - size / 2;
        return new Rect2(leftTop, size);
    }

    #endregion

    #region random math

    // don't ask me why it called 1m1, idk either

    public static double NextDouble(this Random r, double min, double max) => (r.NextDouble() * (max - min)) + min;
    public static double NextDouble(this Random r, double max) => r.NextDouble(0f, max);
    public static float NextSingle(this Random r) => (float)r.NextDouble();
    public static float NextSingle(this Random r, float min, float max) => (NextSingle(r) * (max - min)) + min;
    public static float NextSingle(this Random r, float max) => r.NextSingle(0f, max);
    public static double Next1m1Double(this Random r, double num) => r.NextDouble(-num, num);
    public static float Next1m1Single(this Random r, float num) => r.NextSingle(-num, num);
    public static float Next1m1Single(this Random r, float min, float max) => r.NextSingle(min, max) * r.Next1m1();
    public static int Next1m1(this Random r) => r.Next(2) * 2 - 1;

    #endregion

    #region formating

    public static void Write(this BinaryWriter binaryWriter, Sid sid)
        => binaryWriter.Write(sid.ToString());

    public static Sid ReadSid(this BinaryReader binaryReader)
        => Sid.Parse(binaryReader.ReadString());

    public static void Write(this BinaryWriter binaryWriter, Version version)
    {
        binaryWriter.Write(version.Major);
        binaryWriter.Write(version.Minor);
        binaryWriter.Write(version.Build);
        binaryWriter.Write(version.Revision);
    }

    public static Version ReadVersion(this BinaryReader binaryReader)
    {
        int major = binaryReader.ReadInt32();
        int minor = binaryReader.ReadInt32();
        int build = binaryReader.ReadInt32();
        int revision = binaryReader.ReadInt32();
        return new Version(major, minor, build, revision);
    }

    public static void WriteList<T>(this BinaryWriter binaryWriter, IList<T> list, Action<BinaryWriter, T> writeAction)
    {
        binaryWriter.Write(list.Count);
        foreach (var item in list)
        {
            writeAction(binaryWriter, item);
        }
    }

    public static List<T> ReadList<T>(this BinaryReader binaryReader, Func<BinaryReader, T> readFunc)
    {
        List<T> result = new();
        int count = binaryReader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            result.Add(readFunc(binaryReader));
        }
        return result;
    }
    #endregion
}

public static class MathM
{
    public static Vector3 ClampNoZ(this Vector3 vector, float minX, float maxX, float minY, float maxY)
        => new(Math.Clamp(vector.X, minX, maxX), Math.Clamp(vector.Y, minY, maxY), vector.Z);

    public static Vector3 ClampNoZ(this Vector3 vector, Vector2 min, Vector2 max)
        => ClampNoZ(vector, min.X, max.X, min.Y, max.Y);

    public static float Approach(float val, float target, float maxMove)
        => val <= target ? Math.Min(val + maxMove, target) : Math.Max(val - maxMove, target);

    public static Vector2 Approach(Vector2 val, Vector2 target, float maxMove)
    {
        if (val == target) return target;
        Vector2 diff = target - val;
        return diff.LengthSquared() <= maxMove * maxMove ? target : val + (diff.Normalized() * maxMove);
    }

    public static Vector3 Approach(Vector3 val, Vector3 target, float maxMove)
    {
        if (val == target) return target;
        Vector3 diff = target - val;
        return diff.LengthSquared() <= maxMove * maxMove ? target : val + (diff.Normalized() * maxMove);
    }

    public static Vector3 ApproachNoZ(Vector3 val, Vector3 target, float maxMove)
    {
        if (val == target)
            return target;
        Vector3 diff = (target - val) with { Z = 0 };
        return diff.LengthSquared() <= maxMove * maxMove ? target with { Z = val.Z } : val + (diff.Normalized() * maxMove);
    }

    public static T? ChooseByWeight<T>(Random random, IEnumerable<T> items, IEnumerable<int> weights)
    {
        var randomValue = random.NextDouble() * weights.Sum();
        var i = 0;
        foreach (var weight in weights)
        {
            if (randomValue < weight)
                return items.ElementAt(i);
            randomValue -= weight;
            i++;
        }
        return default;
    }
}
