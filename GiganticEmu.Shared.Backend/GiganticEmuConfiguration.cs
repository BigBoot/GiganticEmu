namespace GiganticEmu.Shared.Backend
{
    public class GiganticEmuConfiguration
    {
        public const string GiganticEmu = "GiganticEmu";

        public string SalsaCK { get; set; } = default!;
        public int WebPort { get; set; } = 3000;
        public int MicePort { get; set; } = 4000;
        public string MiceHost { get; set; } = "localhost";
        public int GameVersion { get; set; } = 301530;
    }
}