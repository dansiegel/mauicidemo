using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Nuke.Components;

[PublicAPI]
public interface IHazMauiWorkload : INukeBuild
{
    Target InstallWorkload => _ => _
        .TryBefore<IRestore>()
        .Executes(() => DotNetTasks.DotNet("workload install maui"));
}
