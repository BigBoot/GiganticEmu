using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;
using Microsoft.EntityFrameworkCore;

public static class PartyCommands
{
    [MiceCommand("party.create")]
    public static async Task<object> Create(dynamic payload, MiceClient client)
    {
        var session_settings = payload.session_settings;
        var session_configuration = payload.configuration;
        var member_settings = payload.member_settings;

        var user = await client.Database.Users.SingleAsync(user => user.Id == client.UserId);

        user.SessionId = Guid.NewGuid();
        user.IsSessionHost = true;
        user.SessionVersion = 1;
        user.SessionConfiguration = ((object)session_configuration).ToJson();
        user.SessionSettings = ((object)session_settings).ToJson();
        user.MemberSettings = ((object)member_settings).ToJson();

        await client.Database.SaveChangesAsync();

        return new
        {
            session_id = user.SessionId.ToString(),
            session = new
            {
                host = user.MotigaId.ToString(),
                document_version = user.SessionVersion,
                join_state = user.JoinState,
                session_settings = session_settings,
                configuration = session_configuration,
                members = new Dictionary<int, object>()
            {
                {user.MotigaId, new {username = user.UserName, member_settings = payload.member_settings} },
            }
            }
        };
    }

    [MiceCommand("party.update")]
    public static async Task<object> Update(dynamic payload, MiceClient client)
    {
        var user = await client.Database.Users.SingleAsync(user => user.Id == client.UserId);
        var sessionHost = await client.Database.Users.SingleAsync(x => x.SessionId == user.SessionId && x.IsSessionHost);

        if (DynamicJsonConverter.GetProperty<dynamic>(() => payload.session_settings) is object session_settings)
        {
            sessionHost.SessionSettings = ((object)session_settings).ToJson();
        }
        if (DynamicJsonConverter.GetProperty<dynamic>(() => payload.configuration) is object session_configuration)
        {
            sessionHost.SessionConfiguration = ((object)session_configuration).ToJson();
        }
        if (DynamicJsonConverter.GetProperty<dynamic>(() => payload.member_settings) is object member_settings)
        {
            user.MemberSettings = ((object)member_settings).ToJson();
        }

        var sessionVersion = sessionHost.SessionVersion++;
        var members = await client.Database.Users
            .Where(x => x.SessionId == user.SessionId)
            .ToDictionaryAsync(x => x.MotigaId, x => new
            {
                username = x.UserName,
                member_settings = x.MemberSettings.FromJsonTo<dynamic>()
            });

        return new
        {
            session_id = sessionHost.SessionId.ToString(),
            session = new
            {
                host = sessionHost.Id.ToString(),
                document_version = sessionVersion,
                join_state = sessionHost.JoinState,
                session_settings = sessionHost.SessionSettings.FromJsonTo<dynamic>(),
                configuration = sessionHost.SessionConfiguration.FromJsonTo<dynamic>(),
                members = members
            }
        };
    }
}