using System.Threading.Tasks;

public static class GeneralCommands
{
    [MiceCommand(".echo")]
    public static async Task<object> GetInfo(dynamic payload) => payload.data;
}