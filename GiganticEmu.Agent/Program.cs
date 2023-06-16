using System.IO;
using System.Threading.Tasks;
using Fetchgoods.Text.Json.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GiganticEmu.Agent;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("                                       ;++++###");
        Console.WriteLine("                                     +++++#####'");
        Console.WriteLine("                                   ;++++++#########");
        Console.WriteLine("         `                               ```....,#@#      .+'++++");
        Console.WriteLine("       '''+#                    ``  `::::'`      ``:: +++++++##++##");
        Console.WriteLine("  '''#:'+###@;:               ````;:,,,,;'+`     ```,;+'''''########");
        Console.WriteLine("  '####;@###;;;;',         ```@@ ::,,,,,;'+ ````` ``.,:;;''+++########");
        Console.WriteLine("   @##+';;;'';;'''#    ``     ``:::,:,:;'+#`````````.:;'+'+++@#+++####");
        Console.WriteLine("     '''''';:;+''##+; .`````````,;:''##+'+````#@'```,:;'+@@###+###@###");
        Console.WriteLine("''''':;;;;:::;;###++++`````+'`````````````  `````````:::;'@@@@@@@##@@@");
        Console.WriteLine(" ####;::::::;'++++++++````+##;''+,',,.,:,.```````````.,,,:;';;@@@@@@@@");
        Console.WriteLine("     `:::::'++++++++++````;####@+@@@@'+','++#:````....,:::;''';;     @");
        Console.WriteLine("       '''++++++++++++`````+#`.;+#@+;;;;++@@++.``.:,:::;;';''''''");
        Console.WriteLine("        +++++++++++++',`````'#`##::::::,;;;:':```.,::::;''''''''''");
        Console.WriteLine("         `++++++++'''''``````.'',.. `:`;: `+````.,,::::;'''''+''''''");
        Console.WriteLine("           '''''''''''''```````  ;'''''; ``````.,:,::;;'''''++++'''''");
        Console.WriteLine("             ''''''''''':`````````````````````,,::::;''''''++++++++'''");
        Console.WriteLine("               `''''''';;'`````````````````.,,:::;;''''++++++;;;;;+++'");
        Console.WriteLine("                  :';;;;;;,`````````````,,,,::::;''''+++++++;;;;;;;;;'");
        Console.WriteLine("                   ,,,,,,,,.:``````.,,,,,:::::;;'''''+++++;;;;;;,.....");
        Console.WriteLine("                    ,,,,,,...,;:,,,,:::::;''''''''''+++''''';;.......,");
        Console.WriteLine("                     ,,,,,..```.::::;;;;;'''+++#'++'''''''''.....,,,,,");

        JsonExtensionMethods.DefaultOptions.WriteIndented = false;
        await CreateHostBuilder(new string[] { }).Build().RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                var configuration = new ConfigurationBuilder()
                    .AddAgentConfiguration(args)
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .Build();
                var agentConfiguration = new AgentConfiguration();
                configuration.Bind(agentConfiguration, o => o.BindNonPublicProperties = true);

                webBuilder.UseConfiguration(configuration);
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls($"http://{agentConfiguration.BindInterface}:{agentConfiguration.WebPort}/");
            }).UseConsoleLifetime();
    }
}
