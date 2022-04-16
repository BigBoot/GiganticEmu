using CredentialManagement;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System;
using System.IO;

namespace GiganticEmu.Launcher;

public class CredentialStorage
{
    private const string CREDENTIALS_TARGET = "giganticemu.mistforge.net";
    private string CREDENTIALS_DIR { get => Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GiganticEmu"); } 
    private string CREDENTIALS_FILE { get => Path.Join(CREDENTIALS_DIR, "GiganticEmu.Launcher.Token"); }

    public async Task<string?> LoadToken()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var cm = new Credential { Target = CREDENTIALS_TARGET };
            if (!cm.Load())
            {
                return null;
            }

            return cm.Password;
        }
        else
        {
            return File.Exists(CREDENTIALS_FILE) ? await File.ReadAllTextAsync(CREDENTIALS_FILE) : null;
        }
    }

    public async Task SaveToken(string token)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            new Credential
            {
                Target = CREDENTIALS_TARGET,
                Username = "<None>",
                Password = token,
                PersistanceType = PersistanceType.LocalComputer
            }.Save();
        }
        else
        {
            Directory.CreateDirectory(CREDENTIALS_DIR);
            await File.WriteAllTextAsync(CREDENTIALS_FILE, token);
        }
    }

    public async Task<bool> ClearToken()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new Credential { Target = CREDENTIALS_TARGET }.Delete();
        }
        else 
        {
            if(File.Exists(CREDENTIALS_FILE))
            {
                File.Delete(CREDENTIALS_FILE);
            }
            return true;
        }
    }
}
