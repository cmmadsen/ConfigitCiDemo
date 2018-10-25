#tool "nuget:?package=NUnit.Runners&version=2.6.4"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var solution = "./WebApplication1.sln";
var branch = EnvironmentVariable("APPVEYOR_REPO_BRANCH") ?? "master";
var major = "2";
var minor = "0";
var patch = "1";
var buildNumber = EnvironmentVariable("APPVEYOR_BUILD_NUMBER") ?? "0";
var commitId = EnvironmentVariable("APPVEYOR_REPO_COMMIT") ?? "0";
var version = $"{major}.{minor}.{patch}.{buildNumber}";
var appveyor = EnvironmentVariable("APPVEYOR");
var appveyorJobId = EnvironmentVariable("APPVEYOR_JOB_ID");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var buildDir = Directory("./src/bin") + Directory(configuration);

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("CleanOnly")
    .Does(() =>
{
    Information("branch:" + branch);
    CleanDirectory(buildDir);
});


Task("RestoreNuGetPackagesOnly")
    .Does(() =>
{
    NuGetRestore( solution );
});

Task("BuildOnly")
    .Does(() =>
{
      // Use MSBuild
      MSBuild(solution, settings => {
        settings.SetConfiguration(configuration);
        settings.SetMaxCpuCount(0);
        settings.SetVerbosity(Verbosity.Minimal);
      });
});


Task("CreateAssemblyInfoOnly" )
    .Does(() => {

    if (appveyor != null) {
    CreateAssemblyInfo("build/GlobalAssemblyInfo.cs", new AssemblyInfoSettings {
    Version = version,
    InformationalVersion = $"{version}-{branch}-{commitId}",
    FileVersion = version,
    Company = "Configit",
    ComVisible = false,
    CLSCompliant  = false,
    Copyright = string.Format("Copyright Configit A/S {0}", DateTime.Now.Year)
    });
    }
});

Task("TestsOnly")
    .Does(() =>
{
    var testAssemblies = GetFiles("**/bin/Release/*.Tests.dll");
    NUnit(testAssemblies );
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Tests");

Task("Clean")
    .IsDependentOn("CleanOnly");

Task("RestoreNuGetPackages")
    .IsDependentOn("CleanOnly")
    .IsDependentOn("RestoreNuGetPackagesOnly");
    
Task("Build")
    .IsDependentOn("RestoreNuGetPackages")
    .IsDependentOn("BuildOnly");
    
Task("Tests")
    .IsDependentOn("Build")
    .IsDependentOn("TestsOnly");
    

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
