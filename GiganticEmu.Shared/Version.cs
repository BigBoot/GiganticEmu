using System;
using System.Text.RegularExpressions;

namespace GiganticEmu.Shared
{
    public record Version : IComparable<Version>, IEquatable<Version>
    {
        private static Regex RE_VERSION = new Regex(@"(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<patch>0|[1-9]\d*)(?:-(?<prerelease>(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?", RegexOptions.Compiled);

        public int Major { get; init; } = default!;
        public int Minor { get; init; } = default!;
        public int Patch { get; init; } = default!;
        public string? PreRelease { get; init; } = default!;
        public string? BuildMetadata { get; init; } = default!;

        public int CompareTo(Version? other)
        {
            if (other == null) return 1;

            if (Major != other.Major) return Major - other.Major;
            if (Minor != other.Minor) return Minor - other.Minor;
            if (Patch != other.Patch) return Patch - other.Patch;

            return 0;
        }

        public override string ToString()
        {
            var version = $"{Major}.{Minor}.{Patch}";

            if (PreRelease != null)
            {
                version = version + $"-{PreRelease}";
            }

            if (BuildMetadata != null)
            {
                version = version + $"+{BuildMetadata}";
            }

            return version;
        }

        public virtual bool Equals(Version? other)
        {
            if (other == null) return false;

            return Major == other.Major && Minor == other.Minor && Patch == other.Patch && PreRelease == other.PreRelease;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Patch, PreRelease);
        }

        public static Version Parse(string version)
        {
            var match = RE_VERSION.Match(version);

            if (match.Success)
            {
                return new Version
                {
                    Major = int.Parse(match.Groups["major"].Value),
                    Minor = int.Parse(match.Groups["minor"].Value),
                    Patch = int.Parse(match.Groups["patch"].Value),
                    PreRelease = match.Groups["prerelease"].Success ? match.Groups["prerelease"].Value : null,
                    BuildMetadata = match.Groups["buildmetadata"].Success ? match.Groups["buildmetadata"].Value : null,
                };
            }

            throw new System.Exception("Todo");
        }

        public static Version ApplicationVersion = Parse(GitVersionInformation.InformationalVersion);

        public static bool operator <(Version left, Version right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Version left, Version right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Version left, Version right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(Version left, Version right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}