param (
	[Parameter(Mandatory=$true)]
	[ValidatePattern("\d\.\d\.\d\.\d")]
	[string]
	$ReleaseVersionNumber
)

$ErrorActionPreference = "Stop"

$PSScriptFilePath = (Get-Item $MyInvocation.MyCommand.Path).FullName

$SolutionRoot = Split-Path -Path $PSScriptFilePath -Parent

# Set the version number in AssemblyInfo.cs
$AssemblyInfoPath = Join-Path -Path $SolutionRoot -ChildPath "Neo4jClient\Properties\AssemblyInfo.cs"
(gc -Path $AssemblyInfoPath) `
	-replace "(?<=Version\(`")[.\d]*(?=`"\))", $ReleaseVersionNumber |
	sc -Path $AssemblyInfoPath -Encoding UTF8

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