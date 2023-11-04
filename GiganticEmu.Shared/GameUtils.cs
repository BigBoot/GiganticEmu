using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GiganticEmu.Shared;

public static class GameUtils
{
    public const int BUILD_THROWBACK_EVENT = 510547;

    public static string GetBaseDir()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().Location switch
        {
            var dll when dll.EndsWith(".dll") => Directory.GetCurrentDirectory(),
            var exe => exe,
        };
    }

    public static async Task<int> GetGameBuild(string? gameDir = null)
    {
        var buildFile = Path.Join(gameDir ?? GetBaseDir(), "Binaries", "build.properties");

        if (!File.Exists(buildFile)) return 0;

        return (await File.ReadAllLinesAsync(buildFile))
            .Where(x => x.StartsWith("ChangelistBuiltFrom"))
            .Select(x => x[(x.IndexOf("=") + 1)..])
            .Select(x => int.Parse(x.Trim()))
            .SingleOrDefault();
    }
}