using System;
using Semver;

namespace GiganticEmu.Shared;

public record SemVer : IComparable<SemVer>, IEquatable<SemVer>
{
    private readonly SemVersion _inner;

    public int Major { get => _inner.Major; }
    public int Minor { get => _inner.Minor; }
    public int Patch { get => _inner.Patch; }
    public string? PreRelease { get => _inner.Prerelease != "" ? _inner.Prerelease : null; }
    public bool IsPreRelease { get => PreRelease is not null; }
    public string? BuildMetadata { get => _inner.Build != "" ? _inner.Build : null; }
    public bool HasBuildMetadata { get => BuildMetadata is not null; }

    private SemVer(SemVersion inner)
    {
        _inner = inner;
    }

    public int CompareTo(SemVer? other)
    {
        if (other == null) return 1;

        if (Major != other.Major) return Major - other.Major;
        if (Minor != other.Minor) return Minor - other.Minor;
        if (Patch != other.Patch) return Patch - other.Patch;

        return 0;
    }

    public override string ToString()
    {
        return _inner.ToString();
    }

    public virtual bool Equals(SemVer? other)
    {
        return _inner.Equals(other?._inner);
    }

    public override int GetHashCode()
    {
        return _inner.GetHashCode();
    }

    public static SemVer Parse(string version)
    {
        return new SemVer(SemVersion.Parse(version));
    }

    public static SemVer? TryParse(string version)
    {
        if (SemVersion.TryParse(version, out var inner))
        {
            return new SemVer(inner);
        }

        return null;
    }

    public static SemVer ApplicationVersion = Parse(GitVersionInformation.InformationalVersion);

    public static bool operator <(SemVer left, SemVer right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(SemVer left, SemVer right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(SemVer left, SemVer right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(SemVer left, SemVer right)
    {
        return left.CompareTo(right) >= 0;
    }
}