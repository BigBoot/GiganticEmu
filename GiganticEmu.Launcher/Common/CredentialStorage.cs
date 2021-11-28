using CredentialManagement;
using System.Threading.Tasks;

namespace GiganticEmu.Launcher;

public class CredentialStorage
{
    private const string CREDENTIALS_TARGET = "giganticemu.mistforge.net";

    public async Task<string?> LoadToken()
    {
        var cm = new Credential { Target = CREDENTIALS_TARGET };
        if (!cm.Load())
        {
            return null;
        }

        return cm.Password;
    }

    public async Task SaveToken(string token)
    {
        new Credential
        {
            Target = CREDENTIALS_TARGET,
            Username = "<None>",
            Password = token,
            PersistanceType = PersistanceType.LocalComputer
        }.Save();
    }

    public async Task<bool> ClearToken()
    {
        return new Credential { Target = CREDENTIALS_TARGET }.Delete();
    }
}
