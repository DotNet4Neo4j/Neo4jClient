param (
	[Parameter(Mandatory=$true)]
	[ValidatePattern("\d\.\d\.\d\.\d")]
	[string]
	$ReleaseVersionNumber
)

$ErrorActionPreference = "Stop"

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName

$SolutionRoot = Split-Path -Path $PSScriptFilePath -Parent

$Is64BitSystem = (Get-WmiObject -Class Win32_OperatingSystem).OsArchitecture -eq "64-bit"
$Is64BitProcess = [IntPtr]::Size -eq 8

$RegistryArchitecturePath = ""
if ($Is64BitProcess) { $RegistryArchitecturePath = "\Wow6432Node" }

$FrameworkArchitecturePath = ""
if ($Is64BitSystem) { $FrameworkArchitecturePath = "64" }

$ClrVersion = (Get-ItemProperty -path "HKLM:\SOFTWARE$RegistryArchitecturePath\Microsoft\VisualStudio\10.0")."CLR Version"

$MSBuild = "$Env:SYSTEMROOT\Microsoft.NET\Framework$FrameworkArchitecturePath\$ClrVersion\MSBuild.exe"

# Set the version number in AssemblyInfo.cs
$AssemblyInfoPath = Join-Path -Path $SolutionRoot -ChildPath "Neo4jClient\Properties\AssemblyInfo.cs"
(gc -Path $AssemblyInfoPath) `
	-replace "(?<=Version\(`")[.\d]*(?=`"\))", $ReleaseVersionNumber |
	sc -Path $AssemblyInfoPath -Encoding UTF8

# Build the solution in release mode
$SolutionPath = Join-Path -Path $SolutionRoot -ChildPath "Neo4jClient.sln"
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount /t:Clean
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}
& $MSBuild "$SolutionPath" /p:Configuration=Release /maxcpucount
if (-not $?)
{
	throw "The MSBuild process returned an error code."
}

# Build the NuGet package
$NuSpecPath = Join-Path -Path $SolutionRoot -ChildPath "Neo4jClient\Neo4jClient.Edge.nuspec"
& nuget pack $NuSpecPath -OutputDirectory $SolutionRoot -Version $ReleaseVersionNumber
if (-not $?)
{
	throw "The NuGet process returned an error code."
}

# Upload the NuGet package
$NuPkgPath = Join-Path -Path $SolutionRoot -ChildPath "Neo4jClient.Edge.$ReleaseVersionNumber.nupkg"
& nuget push $NuPkgPath
if (-not $?)
{
	throw "The NuGet process returned an error code."
}