#tool "nuget:?package=NUnit.Runners&version=2.6.4"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var solution = "./WebApplication1.sln";
var major = "3";
var minor = "0";
var patch = "0";
var buildNumber = EnvironmentVariable("BUILD_NUMBER") ?? "0";
var version = string.Format("{0}.{1}.{2}.{3}", major, minor, patch, buildNumber );

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
        settings.SetVerbosity(Verbosity.Minimal).
			WithProperty("RunOctoPack", "true").
			WithProperty("OctoPackUseFileVersion", "true");
      });
});


Task("CreateAssemblyInfoOnly" )
    .Does(() => {

    CreateAssemblyInfo("./GlobalAssemblyInfo.cs", new AssemblyInfoSettings {
    Version = version,
    InformationalVersion = version,
    FileVersion = version,
    Company = "Configit",
    ComVisible = false,
    CLSCompliant  = false,
    Copyright = string.Format("Copyright Configit A/S {0}", DateTime.Now.Year)
    });
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
    .IsDependentOn("CreateAssemblyInfoOnly")
    .IsDependentOn("BuildOnly");
    
Task("Tests")
    .IsDependentOn("Build")
    .IsDependentOn("TestsOnly");
    

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
