using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UE4Config.Parsing;

namespace GiganticEmu.Shared;

public enum UpgradePath { Left, Right }
public record Upgrade();
public enum Skill { Skill1, Skill2, Skill3, Skill4, Focus }
public record SkillUpgrade(Skill Skill, UpgradePath Path, UpgradePath? SubPath) : Upgrade;
public enum Talent { Talent1, Talent2, Talent3 }
public record TalentUpgrade(Talent Upgrade) : Upgrade;
public record RushLoadout(string Hero, int Index, IList<Upgrade> Upgrades);

public static partial class GameUtils
{
    public const int BUILD_THROWBACK_EVENT = 510547;

    [GeneratedRegex(@"up_(?<hero>\w+)_Rush(?<set>1|2)_(?<number>\d{2})_Provider RxUpgradePathProvider", RegexOptions.IgnoreCase)]
    private static partial Regex RushUpgradeRegex();

    [GeneratedRegex(@"UPT_(?:(?<skill>\w+)_U(?<path>1|2)(?:_SU(?<sub_path>1|2))?|Spec(?<spec>[0-3]))", RegexOptions.IgnoreCase)]
    private static partial Regex RushUpgradePathRegex();

    [GeneratedRegex(@"bPrototypeOnly\s*=\s*true")]
    private static partial Regex HeroPrototypeRegex();

    [GeneratedRegex(@"(?<hero>\w+)Game\.ini", RegexOptions.IgnoreCase)]
    private static partial Regex HeroIniRegex();

    public static string GetBaseDir()
    {
        return System.Reflection.Assembly.GetExecutingAssembly().Location switch
        {
            var dll when dll.EndsWith(".dll") => Directory.GetCurrentDirectory(),
            var exe => exe,
        };
    }

    public static async Task<int> GetGameBuild(string? gameDir = null)
    {
        var buildFile = Path.Join(gameDir ?? GetBaseDir(), "Binaries", "build.properties");

        if (!File.Exists(buildFile)) return 0;

        return (await File.ReadAllLinesAsync(buildFile))
            .Where(x => x.StartsWith("ChangelistBuiltFrom"))
            .Select(x => x[(x.IndexOf("=") + 1)..])
            .Select(x => int.Parse(x.Trim()))
            .SingleOrDefault();
    }

    public static async Task<IEnumerable<RushLoadout>> GetRushLoadouts(string hero, string? gameDir = null)
    {
        gameDir ??= GetBaseDir();

        var config = new ConfigIni("DefaultGame");

        using (var reader = File.OpenText(Path.Join(gameDir, "RxGame", "Config", "Heroes", $"{hero}Game.ini")))
        {
            config.Read(reader);
        }

        return config.Sections
            .Where(section => section.Name != null)
            .Select(section => new { section, match = RushUpgradeRegex().Match(section.Name) })
            .Where(entry => entry.match.Success)
            .Select(entry => new
            {
                set = int.Parse(entry.match.Groups["set"].Value),
                upgrade = entry.section.Tokens
                    .OfType<InstructionToken>()
                    .Last(token => token.Key.Trim().Equals("UpgradeType", StringComparison.InvariantCultureIgnoreCase))
                    .Value.Trim(),
                index = int.Parse(entry.section.Tokens
                    .OfType<InstructionToken>()
                    .Last(token => token.Key.Trim().Equals("GroupIndex", StringComparison.InvariantCultureIgnoreCase))
                    .Value.Trim())

            })
            .GroupBy(upgrade => upgrade.set)
            .Select(group => new RushLoadout(
                    hero,
                    group.Key,
                    group
                        .OrderBy(x => x.index)
                        .Select(x => RushUpgradePathRegex().Match(x.upgrade))
                        .Select(x => x switch
                        {
                            _ when x.Groups["spec"].Success => new TalentUpgrade(Enum.Parse<Talent>($"Talent{int.Parse(x.Groups["spec"].Value)}")),
                            _ => new SkillUpgrade(
                                Enum.Parse<Skill>(x.Groups["skill"].Value, true),
                                x.Groups["path"].Value switch
                                {
                                    "1" => UpgradePath.Left,
                                    _ => UpgradePath.Right
                                },
                                x.Groups["sub_path"].Value switch
                                {
                                    "1" => UpgradePath.Left,
                                    "2" => UpgradePath.Right,
                                    _ => null
                                }
                            ) as Upgrade
                        })
                        .ToList()
                )
            );
    }

    public static async Task<IEnumerable<RushLoadout>> GetRushLoadouts(string? gameDir = null)
    {
        gameDir ??= GetBaseDir();

        var heroes = await GetHeroes(gameDir);

        var loadouts = (await Task.WhenAll(heroes.Select(async hero =>
        {
            try
            {
                return (await GetRushLoadouts(hero, gameDir)).ToList();
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"Error trying to load Loadouts for hero {hero}: {ex}");
                return null;
            }
        })));

        return loadouts.OfType<IEnumerable<RushLoadout>>().SelectMany(x => x);
    }

    public static async Task<IEnumerable<string>> GetHeroes(string? gameDir = null)
    {
        gameDir ??= GetBaseDir();

        var file = Path.Join(gameDir, "RxGame", "Config", "DefaultGame.ini");

        return (await File.ReadAllLinesAsync(file))
            .Where(line => line.StartsWith("OverriddenBy=..\\RxGame\\Config\\Heroes\\"))
            .Select(line => HeroIniRegex().Match(line[37..]))
            .Where(match => match.Success)
            .Select(match => match.Groups["hero"].Value)
            .Where(hero => !HeroPrototypeRegex().IsMatch(File.ReadAllText(Path.Join(gameDir, "RxGame", "Config", "Heroes", $"{hero}Game.ini"))))
            .ToList();
    }

    public static async Task SaveRushLoadout(RushLoadout loadout, string? gameDir = null)
    {
        gameDir ??= GetBaseDir();

        var config = new ConfigIni("DefaultGame");

        using (var reader = File.OpenText(Path.Join(gameDir, "RxGame", "Config", "Heroes", $"{loadout.Hero}Game.ini")))
        {
            config.Read(reader);
        }

        foreach (var (upgrade, i) in loadout.Upgrades.Select((x, i) => (x, i)))
        {
            var sectionName = $"up_{loadout.Hero}_Rush{loadout.Index}_{i + 1:D2}_Provider RxUpgradePathProvider";
            var section = config.Sections
                .Single(section => sectionName.Equals(section.Name?.Trim(), StringComparison.InvariantCultureIgnoreCase));

            var token = section.Tokens
                .OfType<InstructionToken>()
                .Single(token => "UpgradeType".Equals(token.Key?.Trim(), StringComparison.InvariantCultureIgnoreCase));

            var value = new StringBuilder(" ");

            if (upgrade is SkillUpgrade su)
            {
                value.Append($"UPT_{su.Skill}_U{((int)su.Path) + 1}");

                if (su.SubPath is UpgradePath subPath)
                {
                    value.Append($"_SU{((int)subPath) + 1}");
                }
            }
            else if (upgrade is TalentUpgrade tu)
            {
                value.Append($"UPT_Spec{((int)tu.Upgrade) + 1}");
            }

            token.Value = value.ToString();
        }

        using var writer = new StreamWriter(File.OpenWrite(Path.Join(gameDir, "RxGame", "Config", "Heroes", $"{loadout.Hero}Game.ini")));
        config.Write(new ConfigIniWriter(writer));
    }
}