using System;
using System.Globalization;
using Avalonia.Data.Converters;
using GiganticEmu.Shared;

namespace GiganticEmu.Launcher;

public class UpgradeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SkillUpgrade su)
        {
            return $"[{su.Skill switch
            {
                Skill.Skill1 => "LMB",
                Skill.Skill2 => "RMB",
                Skill.Skill3 => "Q",
                Skill.Skill4 => "E",
                Skill.Focus => "Focus",

            }}] {su.Path} {su.SubPath}";
        }

        if (value is TalentUpgrade tu)
        {
            return $"Talent {((int)tu.Upgrade) + 1}";
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
