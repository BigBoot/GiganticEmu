using System.Collections.Generic;
using System.Threading.Tasks;

public static class PartyCommands
{
    [MiceCommand("party.create")]
    public static async Task<object> Create(dynamic payload) => new
    {
        session_id = "12",  // not using atm
        session = new
        {
            host = 1,
            document_version = 0,
            join_state = "open",
            session_settings = payload.session_settings,
            configuration = payload.configuration,
            members = new Dictionary<string, object>()
            {
                {"1", new {username = "", member_settings = payload.member_settings } },
            }
        }
    };
}