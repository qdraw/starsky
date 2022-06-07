using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using helpers;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static SimpleExec.Command;
using static helpers.GetSolutionAllProjects;

[ShutdownDotNetAfterServerBuild]
[CheckBuildProjectConfigurations(TimeoutInMilliseconds = 0)]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = Configuration.Release;

    public const string GenericRuntimeName = "generic-netcore";
    

    [Parameter("Runtime arg")]
    readonly string Runtime = GenericRuntimeName;
    
    
    public List<string> GetRuntimesWithoutGeneric()
    {
	    return Runtime.Split(",", StringSplitOptions.TrimEntries).Where(p => p != GenericRuntimeName).ToList();
    }

    public bool IsRuntimeGeneric()
    {
	    return Runtime.Contains(GenericRuntimeName);
    }

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;

    public static readonly List<string> PublishProjectsList = new List<string>
    {
	    "starskyadmincli",
	    "starskygeocli",
	    "starskyimportercli",
	    "starskysynchronizecli",
	    "starskythumbnailcli",
	    "starskywebftpcli",
	    "starskywebhtmlcli",
	    "starskythumbnailmetacli",
	    "starsky"
    };
    
    public const string NpmBaseCommand = "npm";
    public const string ClientAppFolder = "starsky/clientapp";


    
    private string BuildToolsPath()
    {
	    return "../starsky-tools/build-tools/";
    }

    // AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    //
    // Target Clean => _ => _
    //     .Before(Restore)
    //     .Executes(() =>
    //     {
    //         EnsureCleanDirectory(ArtifactsDirectory);
    //     });
    
    Target Client => _ => _
	    .Executes(() =>
	    {
		    Console.WriteLine(":test");

		    // ProjectCheckNetCoreCommand();
		    // ClientRestoreCommand();
	    });

    void CopyAssetFileToCurrentRuntime(string runtime)
    {
	    // Restore Asset runtime file
	    foreach ( var path in GetSolutionAllProjects.GetSolutionAllProjectsList(Solution) )
	    {
		    var parent = Directory.GetParent(path)?.FullName;
		    var assetFile = $"{parent}/obj/project.assets.json";
		    var assetRuntimeFile = $"{parent}/obj/project.assets_{runtime}.json";

		    if ( File.Exists(assetRuntimeFile) )
		    {
			    // Restore
			    CopyFile(assetRuntimeFile,assetFile, FileExistsPolicy.Overwrite, false);
		    }
	    }
    }

    void CopyNewAssetFileByRuntimeId(string runtime)
    {
	    // Create a new one
	    foreach ( var path in GetSolutionAllProjectsList(Solution) )
	    {
		    var parent = Directory.GetParent(path)?.FullName;
		    var assetFile = $"{parent}/obj/project.assets.json";
		    var assetRuntimeFile = $"{parent}/obj/project.assets_{runtime}.json";
		    if ( File.Exists(assetFile) )
		    {
			    CopyFile(assetFile,assetRuntimeFile, FileExistsPolicy.Overwrite);
		    }
	    }
    }

    void RestoreNetCoreCommand()
    {
	    
	    if ( IsRuntimeGeneric() )
	    {
		    CopyAssetFileToCurrentRuntime(GenericRuntimeName);
		    DotNetRestore(_ => _
			    .SetProjectFile(Solution));
		    CopyNewAssetFileByRuntimeId(GenericRuntimeName);
	    }

	    foreach ( var runtime in GetRuntimesWithoutGeneric() )
	    {
		    CopyAssetFileToCurrentRuntime(runtime);
		    // OverwriteRuntimeIdentifier is done via Directory.Build.props
		    DotNetRestore(_ => _
			    .SetProjectFile(Solution)
			    .SetProcessArgumentConfigurator(args => args.Add($"/p:OverwriteRuntimeIdentifier={runtime}")));
		    CopyNewAssetFileByRuntimeId(runtime);
	    }
    }

    void BuildNetCoreGenericCommand()
    {
	    
	    if ( IsRuntimeGeneric() )
	    {
		    CopyAssetFileToCurrentRuntime(GenericRuntimeName);
		    
		    DotNetBuild(_ => _
			    .SetConfiguration(Configuration)
			    .EnableNoRestore()
			    .EnableNoLogo()
			    .SetProjectFile(Solution));

		    CopyNewAssetFileByRuntimeId(GenericRuntimeName);
	    }

	    foreach ( var runtime in GetRuntimesWithoutGeneric() )
	    {
		    CopyAssetFileToCurrentRuntime(runtime);
		    // OverwriteRuntimeIdentifier is done via Directory.Build.props
		    DotNetBuild(_ => _
			    .SetProjectFile(Solution)
			    .EnableNoRestore()
			    .EnableNoLogo()
			    .SetConfiguration(Configuration)
			    .SetProcessArgumentConfigurator(args => args.Add($"/p:OverwriteRuntimeIdentifier={runtime}")));
		    CopyNewAssetFileByRuntimeId(runtime);
	    }
    }

    void PublishNetCoreGenericCommand()
    {
	    if ( IsRuntimeGeneric() )
	    {
		    CopyAssetFileToCurrentRuntime(GenericRuntimeName);

		    foreach ( var publishProject in PublishProjectsList )
		    {
			    DotNetPublish(_ => _
				    .SetConfiguration(Configuration)
				    .EnableNoRestore()
				    .EnableNoBuild()
				    .EnableNoDependencies()
				    .SetSelfContained(true)
				    .SetOutput(GenericRuntimeName)
				    .SetProject(publishProject)
				    .EnableNoLogo());
		    }
		    CopyNewAssetFileByRuntimeId(GenericRuntimeName);
	    }
	    
	    foreach ( var runtime in GetRuntimesWithoutGeneric() )
	    {
		    CopyAssetFileToCurrentRuntime(runtime);
		    foreach ( var publishProject in PublishProjectsList )
		    {
			    DotNetPublish(_ => _
				    .SetConfiguration(Configuration)
				    .EnableNoRestore()
				    .EnableNoBuild()
				    .EnableNoDependencies()
				    .SetSelfContained(true)
				    .SetOutput(runtime)
				    .SetProject(publishProject)
				    .SetRuntime(runtime)
				    .EnableNoLogo());
		    }
		    CopyNewAssetFileByRuntimeId(runtime);
	    }
	    
    }

    static void ClientRestoreCommand()
    {
	    Run(NpmBaseCommand, "ci --legacy-peer-deps", ClientAppFolder, 
		    false, null, null, false);
    }
	
	void ProjectCheckNetCoreCommand()
	{
		// check branch names on CI
		Run(NpmBaseCommand, "run release-version-check", BuildToolsPath());
		
		/* Checks for valid Project GUIDs in csproj files */
		Run(NpmBaseCommand, "run project-guid", BuildToolsPath());
		
		/* List of nuget packages */
		Run(NpmBaseCommand, "run nuget-package-list", BuildToolsPath());
	}

    Target Restore => _ => _
        .Executes(() =>
        {
	        Console.WriteLine("restore");
	        RestoreNetCoreCommand();
        });

    Target Compile => _ => _
	    .DependsOn(Client)
	    .DependsOn(DotNetProjectBuild);
    
    Target DotNetProjectBuild => _ => _
	    .DependsOn(Client)
	    .DependsOn(Restore)
	    .Executes(() =>
	    {
		    BuildNetCoreGenericCommand();
		    PublishNetCoreGenericCommand();
	    });
}
