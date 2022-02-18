using System;
using System.IO;
using System.Reflection;
using Avalonia;

namespace GiganticEmu.Launcher;

public class AvaloniaAssetLoader : IAssetLoader
{
    private Avalonia.Platform.IAssetLoader _inner;
    private Assembly _assembly;

    public AvaloniaAssetLoader()
    {
        _inner = AvaloniaLocator.Current.GetService<Avalonia.Platform.IAssetLoader>()!;
        _assembly = typeof(AvaloniaAssetLoader).Assembly;
    }

    public bool Exists(string path, Assembly? assembly = null)
    {
        return _inner.Exists(new Uri($"avares://{(assembly ?? _assembly).GetName().Name}{path}"));
    }

    public Stream Open(string path, Assembly? assembly = null)
    {
        return _inner.Open(new Uri($"avares://{(assembly ?? _assembly).GetName().Name}{path}"));
    }

    public Stream? TryOpen(string path, Assembly? assembly = null)
    {
        return Exists(path) ? Open(path) : null;
    }
}