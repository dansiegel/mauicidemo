using JetBrains.Annotations;
using Nuke.Common;

[PublicAPI]
public interface IHazBuildConfiguration : INukeBuild
{
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    Configuration Configuration => IsLocalBuild ? Configuration.Debug : Configuration.Release;
}
