using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace GiganticEmu.Shared
{
    public enum RequestResult
    {
        [Description("success")]
        Success,

        [Description("invalid username or password")]
        InvalidUser,

        [Description("two factor authentication required")]
        Require2FA,

        [Description("unauthorized")]
        Unauthorized,

        [Description("account locked")]
        AccountLocked,

        [Description("error during registration")]
        RegistrationError,

        [Description("email not confirmed")]
        EmailNotConfirmed,

        [Description("no instance available")]
        NoInstanceAvailable,

        [Description("unknown username")]
        UnknownUser,

        [Description("the party is already full")]
        SessionFull,

        [Description("the invite is invalid")]
        SessionInvalid,

        [Description("invalid insttance")]
        InvalidInstance,
    }

    public static class RequestResultExtensions
    {
        private static IDictionary<RequestResult, string> DESCRIPTIONS;

        static RequestResultExtensions()
        {
            DESCRIPTIONS = Enum.GetValues<RequestResult>()
                .ToDictionary(e => e, e => (e
                    .GetType()
                    .GetTypeInfo()
                    .GetMember(e.ToString())
                    .FirstOrDefault(member => member.MemberType == MemberTypes.Field)
                    ?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    ?.SingleOrDefault() as DescriptionAttribute)
                    ?.Description ?? e.ToString()
                );
        }

        public static string GetDescription(this RequestResult e)
        {
            return DESCRIPTIONS[e];
        }
    }
}