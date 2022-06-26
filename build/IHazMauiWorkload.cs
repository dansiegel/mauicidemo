using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;

[PublicAPI]
public interface IHazMauiWorkload : INukeBuild
{
    Target InstallWorkload => _ => _
        .Executes(() => DotNetTasks.DotNet("workload install maui"));
}
