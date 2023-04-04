using System.IO;
using System.Text;

namespace MVE;

public class SaveData
{
    public List<Sid> OwnedCards { get; set; }

    public SaveData()
    {
        OwnedCards = new();
    }

    public void SaveTo(Stream stream)
    {
        using BinaryWriter bw = new(stream, Encoding.UTF8);
        bw.Write(new Version(0, 0, 1, 0));
        bw.WriteList(OwnedCards, (bw, v) => bw.Write(v));
    }

    public void ReadFrom(Stream stream)
    {
        using BinaryReader br = new(stream, Encoding.UTF8);
        Version v = br.ReadVersion();
        if (v is { Major: 0, Minor: 0, Build: 1 })
        {
            OwnedCards = br.ReadList(r => r.ReadSid());
        }
        else
        {
            throw new Exception($"Version {v} does not support.");
        }
    }

    public void ReadFromUser(string userPath)
        => ReadFromNative(ProjectSettings.GlobalizePath(userPath));

    public void SaveToUser(string userPath)
        => SaveToNative(ProjectSettings.GlobalizePath(userPath));

    public void SaveToNative(string nativePath)
    {
        using FileStream fs = new(nativePath, FileMode.Create, FileAccess.Write);
        this.SaveTo(fs);
    }

    public void ReadFromNative(string nativePath)
    {
        using FileStream fs = new(nativePath, FileMode.Open, FileAccess.Read);
        this.ReadFrom(fs);
    }
}
