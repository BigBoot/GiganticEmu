using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GiganticEmu.Shared;
using Microsoft.Extensions.Logging;
using UELib;
using UELib.Core;

namespace GiganticEmu.ModdingToolkit;

public class ModUtils
{
    private Dictionary<string, UnrealPackage> cache = new Dictionary<string, UnrealPackage>();

    public void ClearCache()
    {
        cache.Clear();
    }

    private async Task<UnrealPackage?> DecompressPackage(string packagename)
    {
        var file = Path.Join("RxGame", "CookedPCConsole", packagename);

        if (await IsPackageCompressed(packagename))
        {
            return UnrealLoader.LoadPackage(file, FileAccess.Read);
        }

        var decompress = "decompress" + PlatformUtils.ExecutableExtension;
        var tmpFile = Path.GetTempFileName();

        using (var stream = File.OpenWrite(tmpFile))
        {
            Assembly.GetExecutingAssembly().GetManifestResourceStream("GiganticEmu.ModdingToolkit.Resources." + decompress)!.CopyTo(stream);
        }

        if (PlatformUtils.IsLinux)
        {
            var unixFileInfo = new Mono.Unix.UnixFileInfo(tmpFile);
            unixFileInfo.FileAccessPermissions = unixFileInfo.FileAccessPermissions | Mono.Unix.FileAccessPermissions.UserExecute;
        }


        var resultFile = Path.Combine(Directory.GetCurrentDirectory(), "_tmp", Path.GetFileName(file));
        using (var process = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                CreateNoWindow = true,
                FileName = tmpFile,
                RedirectStandardOutput = true,
                WorkingDirectory = Directory.GetCurrentDirectory()
            },
        })
        {

            process.StartInfo.ArgumentList.Add("-out=_tmp");
            process.StartInfo.ArgumentList.Add("-game=gigantic");
            process.StartInfo.ArgumentList.Add(file);

            process.Start();
            await process.WaitForExitAsync();
            if (process.ExitCode != 0 || !File.Exists(resultFile))
            {
                Console.Error.WriteLine($"Could not decompress {file}:\n{process.StandardOutput.ReadToEnd()}");
                return null;
            }
        }

        File.Move(file, file.Replace(".upk", ".upk_backup"));
        File.Move(resultFile, file);

        File.Delete(tmpFile);

        return UnrealLoader.LoadPackage(file, FileAccess.Read);
    }

    private async Task<bool> IsPackageCompressed(string packagename)
    {
        var file = Path.Join("RxGame", "CookedPCConsole", packagename);

        using var reader = new BinaryReader(File.OpenRead(file));

        var tag = reader.ReadUInt32();
        if (tag != 0x9E2A83C1)
        {
            throw new Exception("Not a valid Unreal Engine package.");
        }

        var fileVersion = reader.ReadUInt16();
        var licenseeVersion = reader.ReadUInt16();

        var totalHeaderSize = reader.ReadInt32();

        {
            var length = reader.ReadInt32();
            var isUnicode = length < 0;

            if (length > 0)
            {
                var Data = reader.ReadBytes(length);
                var InnerString = Encoding.ASCII.GetString(Data, 0, Data.Length - 1);
            }
            else if (length < 0)
            {
                var Data = reader.ReadBytes(-length);
                var InnerString = Encoding.Unicode.GetString(Data, 0, Data.Length - 2);
            }
        }


        var packageFlags = reader.ReadUInt32();

        var unknown = reader.ReadInt32();

        var nameCount = reader.ReadInt32();
        var nameOffset = reader.ReadInt32();

        var exportCount = reader.ReadInt32();
        var exportOffset = reader.ReadInt32();

        var importCount = reader.ReadInt32();
        var importOffset = reader.ReadInt32();

        var dependsOffset = reader.ReadInt32();

        var unknown1 = reader.ReadInt32();
        var unknown2 = reader.ReadInt32();
        var unknown3 = reader.ReadInt32();
        var unknown4 = reader.ReadInt32();


        var guid1 = reader.ReadUInt32();
        var guid2 = reader.ReadUInt32();
        var guid3 = reader.ReadUInt32();
        var guid4 = reader.ReadUInt32();

        {
            var len = reader.ReadInt32();
            for (int i = 0; i < len; i++)
            {
                var exportCount_ = reader.ReadInt32();
                var nameCount_ = reader.ReadInt32();
                var netObjectCount_ = reader.ReadInt32();
            }
        }

        var engineVersion = reader.ReadUInt32();
        var cookerVersion = reader.ReadUInt32();

        var compressionFlags = reader.ReadUInt32();

        return compressionFlags == 0;
    }

    private async Task<UnrealPackage?> LoadPackage(string packagename)
    {
        if (cache.ContainsKey(packagename))
        {
            return cache[packagename];
        }

        var package = await DecompressPackage(packagename);

        if (package == null)
        {
            return null;
        }

        package.NTLPackage = new NativesTablePackage();
        package.NTLPackage.LoadPackage(Assembly.GetExecutingAssembly().GetManifestResourceStream("GiganticEmu.ModdingToolkit.Resources.NativesTableList_UDK-2012-05.NTL")!);

        package.InitializePackage();

        cache[packagename] = package;

        return package;
    }

    async Task ReplaceBytesInFile(string filePath, long offset, byte[] replacementBytes)
    {
        if (offset < 0 || replacementBytes == null || replacementBytes.Length == 0)
        {
            throw new ArgumentException("Invalid arguments.");
        }

        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite))
        {
            if (offset >= fs.Length)
            {
                throw new ArgumentOutOfRangeException("Offset is beyond the file length.");
            }

            fs.Position = offset;
            await fs.WriteAsync(replacementBytes, 0, replacementBytes.Length);
        }
    }

    private byte[] ParseHexString(string hexString)
    {
        hexString = hexString.Replace(" ", "");

        byte[] byteArray = new byte[hexString.Length / 2];

        for (int i = 0; i < byteArray.Length; i++)
        {
            byteArray[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
        }

        return byteArray;
    }

    private IBuffered? FindBuffer(UnrealPackage package, PatchFunctionHEX patch, Action<LogLevel, string>? outputCallback = null)
    {
        var uclass = package.Objects
                                .Where(o => string.Compare(o.Name, patch.Class, StringComparison.OrdinalIgnoreCase) == 0)
                                .OfType<UClass>()
                                .FirstOrDefault();
        if (uclass == null)
        {
            outputCallback?.Invoke(LogLevel.Warning, $"Unable to find class {patch.Class}");
            return null;
        }


        var ufunc = uclass.Functions.FirstOrDefault(x => x.Name == patch.Function);
        if (ufunc == null)
        {
            outputCallback?.Invoke(LogLevel.Warning, $"Unable to find function {patch.Class}::{patch.Function}");
            return null;
        }

        return ufunc;
    }

    private IBuffered? FindBuffer(UnrealPackage package, PatchObjectHEX patch, Action<LogLevel, string>? outputCallback = null)
    {
        var uobject = package.Objects
                               .Where(o => string.Compare(o.Name, patch.Object, StringComparison.OrdinalIgnoreCase) == 0)
                               .FirstOrDefault();
        if (uobject == null)
        {
            outputCallback?.Invoke(LogLevel.Warning, $"Unable to find object {patch.Object}");
            return null;
        }

        return uobject;
    }

    private async Task<Dictionary<string, List<(long, byte[])>>> PreparePatches(IEnumerable<Mod> mods, Action<LogLevel, string>? outputCallback = null)
    {
        var chunks = new Dictionary<string, List<(long, byte[])>>();

        foreach (var mod in mods)
        {
            outputCallback?.Invoke(LogLevel.Information, $"Preparing {mod.Description}");
            foreach (var (p, i) in mod.Patches.Select((x, i) => (x, i)))
            {
                outputCallback?.Invoke(LogLevel.Information, $"Preparing patch #{i + 1}");
                if (p is PatchHEX patch)
                {
                    var package = await LoadPackage(patch.File);
                    if (package == null)
                    {
                        outputCallback?.Invoke(LogLevel.Warning, $"Unable to load package {patch.File}");
                        continue;
                    }



                    var before = ParseHexString(patch.Before);
                    var after = ParseHexString(patch.After);

                    if (before.Length != after.Length)
                    {
                        outputCallback?.Invoke(LogLevel.Warning, $"Before and After differ in length, this is currently not supported...");
                        continue;
                    }

                    var buffer = p switch
                    {
                        PatchFunctionHEX patchFunction => FindBuffer(package, patchFunction, outputCallback),
                        PatchObjectHEX patchObject => FindBuffer(package, patchObject, outputCallback),
                        _ => null,
                    };

                    if (buffer == null) continue;

                    var file = Path.Join("RxGame", "CookedPCConsole", patch.File);

                    var matcher = new BoyerMoore(before);
                    var instances = matcher.Search(buffer.CopyBuffer())
                        .Select(x => x + buffer.GetBufferPosition())
                        .ToList();

                    if (instances.Count == 0)
                    {
                        outputCallback?.Invoke(LogLevel.Warning, $"Unable to find pattern \"{patch.Before}\".");
                        continue;
                    }

                    if (instances.Count > 1)
                    {
                        if (patch.Behavior == PatchFunctionHEX.PatchBehavior.Single)
                        {
                            outputCallback?.Invoke(LogLevel.Warning, $"Found multiple instances of pattern \"{patch.Before}\", but Behaviour is set to Single, skipping patch...");
                            continue;
                        }

                        instances = patch.Behavior switch
                        {
                            PatchFunctionHEX.PatchBehavior.First => new[] { instances.First() }.ToList(),
                            PatchFunctionHEX.PatchBehavior.Last => new[] { instances.Last() }.ToList(),
                            _ => instances,
                        };
                    }

                    if (!chunks.ContainsKey(patch.File))
                    {
                        chunks[patch.File] = new List<(long, byte[])>();
                    }

                    chunks[patch.File].AddRange(instances.Select(x => (x, after)));
                }
            }
        }

        return chunks;
    }

    public async Task<IEnumerable<string>> RestoreBackups()
    {
        return (await Task.WhenAll(new DirectoryInfo(Path.Join(GameUtils.GetBaseDir(), "RxGame", "CookedPCConsole")).GetFiles()
            .Where(x => x.Extension == ".upk_backup")
            .Select(async bak =>
            {
                try
                {
                    var orig = bak.FullName.Replace(".upk_backup", ".upk");
                    if (File.Exists(orig))
                    {
                        File.Delete(orig);
                    }
                    File.Move(bak.FullName, orig);
                    return bak.Name.Replace(".upk_backup", ".upk");
                }
                catch (Exception)
                {
                    return null;
                }
            })
        )).OfType<string>();
    }

    public async Task ApplyMods(IEnumerable<Mod> mods, Action<int, int>? progressCallback = null, Action<LogLevel, string>? outputCallback = null)
    {
        progressCallback?.Invoke(-1, -1);
        var patches = (await PreparePatches(mods, outputCallback)).ToList();

        var count = patches.SelectMany(x => x.Value).Count();
        var done = 0;

        outputCallback?.Invoke(LogLevel.Information, $"Patching {count} chunks.");
        progressCallback?.Invoke(done, count);

        foreach (var (package, chunks) in patches)
        {
            foreach (var (postition, bytes) in chunks)
            {
                try
                {
                    var file = Path.Join("RxGame", "CookedPCConsole", package);
                    await ReplaceBytesInFile(file, postition, bytes);
                }
                catch (Exception)
                {

                }
                finally
                {
                    done++;
                    progressCallback?.Invoke(done, count);
                }
            }
        }
    }

    public async Task<IEnumerable<string>> DecompressAll(Action<int, int>? progressCallback = null)
    {
        var files = new DirectoryInfo(Path.Join(GameUtils.GetBaseDir(), "RxGame", "CookedPCConsole")).GetFiles()
            .Where(x => x.Extension == ".upk")
            .ToList();

        var count = files.Count;
        var done = 0;

        progressCallback?.Invoke(done, count);

        return (await Task.WhenAll(
            files
            .Where(x => x.Extension == ".upk")
            .Select(async bak =>
            {
                try
                {
                    await DecompressPackage(bak.Name);
                    return bak.Name.Replace(".upk_backup", ".upk");
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    Interlocked.Increment(ref done);
                    progressCallback?.Invoke(done, count);
                }
            })
        ))
        .OfType<string>();
    }

    public async Task<IEnumerable<string>> DecompileAll(Action<int, int>? progressCallback = null)
    {
        var files = new DirectoryInfo(Path.Join(GameUtils.GetBaseDir(), "RxGame", "CookedPCConsole")).GetFiles()
            .Where(x => x.Extension == ".upk")
            .ToList();

        var count = files.Count;
        var done = 0;

        progressCallback?.Invoke(done, count);

        return (await Task.WhenAll(
            files.Select(async bak =>
            {
                try
                {
                    var pkg = await LoadPackage(bak.Name);

                    if (pkg == null) return null;

                    foreach (var c in pkg.Objects.OfType<UClass>())
                    {
                        var file = Path.Join(GameUtils.GetBaseDir(), "decompiled", pkg.PackageName, $"{c.Name}.uc");
                        var dir = Path.GetDirectoryName(file)!;
                        Directory.CreateDirectory(dir);
                        await File.WriteAllTextAsync(file, c.Decompile());
                    }

                    return bak.Name.Replace(".upk_backup", ".upk");
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    Interlocked.Increment(ref done);
                    progressCallback?.Invoke(done, count);
                }
            })
        ))
        .OfType<string>();
    }
}