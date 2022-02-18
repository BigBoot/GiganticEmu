using System.IO;
using System.Reflection;

namespace GiganticEmu.Launcher;

public interface IAssetLoader
{
    bool Exists(string path, Assembly? assembly = null);
    Stream Open(string path, Assembly? assembly = null);
    Stream? TryOpen(string path, Assembly? assembly = null);
}