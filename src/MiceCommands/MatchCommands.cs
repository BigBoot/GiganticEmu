using System;
using System.Text;
using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;

public static class MatchCommands
{
    [MiceCommand("match.me")]
    public static async Task<object> Me(dynamic payload)
    {
        Task match = Task.Delay(3000).ContinueWith(async _ =>
        {
            Console.WriteLine("Sending [match.ready]");
            var ck2 = Convert.ToBase64String(Encoding.UTF8.GetBytes("imagoodcipherkey"));
            var ck = Convert.ToBase64String(Encoding.UTF8.GetBytes("amotigadeveloper"));
            var bcryptHmac = Convert.ToBase64String(Encoding.UTF8.GetBytes("totsagoodsuperlonghmacsecretkeys"));
            var msg = new object[] { "match.ready", new
            {
                matchinfo = new
                {
                    server = new
                    {
                        connstr = "127.0.0.1:7777",
                        map = "LV_Canyon",
                    },
                    instanceid = "12",
                    token = ck + ck2 + bcryptHmac,
                    meta = new
                    {
                        moid = 2,
                    },
                },
            }}.ToJson();
            await MiceClient.INSTANCE.SendResponse(msg);
        });
        return new object { };
    }
}