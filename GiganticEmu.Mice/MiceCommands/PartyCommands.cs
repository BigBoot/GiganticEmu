using System.Collections.Generic;
using System.Threading.Tasks;

public static class PartyCommands
{
    private static int documentversion = 1;
    private static dynamic session_settings = new { };
    private static dynamic configuration = new { };
    private static dynamic member_settings = new { };

    [MiceCommand("party.create")]
    public static async Task<object> Create(dynamic payload, MiceClient client)
    {
        session_settings = payload.session_settings;
        configuration = payload.configuration;
        member_settings = payload.member_settings;

        return new
        {
            session_id = "12",  // not using atm
            session = new
            {
                host = "1",
                document_version = documentversion,
                join_state = "open",
                session_settings = session_settings,
                configuration = configuration,
                members = new Dictionary<int, object>()
            {
                {1, new {username = "TheLegend27", member_settings = payload.member_settings } },
            }
            }
        };
    }

    [MiceCommand("party.update")]
    public static async Task<object> Update(dynamic payload, MiceClient client)
    {
        if (DynamicJsonConverter.GetProperty<dynamic>(() => payload.session_settings) is object session_settings)
        {
            PartyCommands.session_settings = session_settings;
        }
        if (DynamicJsonConverter.GetProperty<dynamic>(() => payload.configuration) is object configuration)
        {
            PartyCommands.configuration = configuration;
        }
        if (DynamicJsonConverter.GetProperty<dynamic>(() => payload.member_settings) is object member_settings)
        {
            PartyCommands.member_settings = member_settings;
        }

        return new
        {
            session_id = "12",  // not using atm
            session = new
            {
                host = "1",
                document_version = documentversion++,
                join_state = "open",
                session_settings = PartyCommands.session_settings,
                configuration = PartyCommands.configuration,
                members = new Dictionary<int, object>()
            {
                {1, new {username = "TheLegend27", member_settings = PartyCommands.member_settings } },
                {2, new {username = "TheLegend28", member_settings = PartyCommands.member_settings } },
                {3, new {username = "TheLegend29", member_settings = PartyCommands.member_settings } },
                {4, new {username = "TheLegend30", member_settings = PartyCommands.member_settings } },
                {5, new {username = "TheLegend31", member_settings = PartyCommands.member_settings } },
            }
            }
        };
    }
}