namespace MVE;

// no content but just wait the feather `Using aliases for any type` in c#12
// like this: global using SidLib<T> = Dictionary<Sid, T>;
// https://github.com/dotnet/csharplang/issues/4284
public class SidLib<T> : Dictionary<Sid, T>
{
}