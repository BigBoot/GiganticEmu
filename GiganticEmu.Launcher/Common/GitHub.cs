using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Flurl.Http;
using GiganticEmu.Shared;
using System.IO;

namespace GiganticEmu.Launcher
{
    public class GitHub
    {

        public async Task DownloadFile(Version version, string filename, string targetDir)
        {
            if (File.Exists(Path.Join(targetDir, filename)))
            {
                File.Delete(Path.Join(targetDir, filename));
            }

            await $"https://github.com/BigBoot/GiganticEmu/releases/download/v{version.ToString()}/{filename}"
                .DownloadFileAsync(targetDir);
        }
    }
}