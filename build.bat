REM MyGet Build Commands

@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

set version=-Version 1.0.0
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

REM Restore packages
tools\nuget.exe restore Neo4jClient.sln
if not "%errorlevel%"=="0" goto failure
@echo Packages restored - on to build...

REM Build
"%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" Neo4jClient.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
if not "%errorlevel%"=="0" goto failure
@echo Built and onto tests....


REM XUnit tests
tools\nuget.exe install xunit.runner.console -Version 2.2.0 -OutputDirectory packages
if not "%errorlevel%"=="0" goto failure
@echo XUnit installed...

packages\xunit.runner.console.2.2.0\tools\xunit.console.x86.exe Neo4jClient.Tests\bin\%config%\Neo4jClient.Tests.dll
if not "%errorlevel%"=="0" goto failure
@echo Neo4jClient tests... SUCCESS.

packages\xunit.runner.console.2.2.0\tools\xunit.console.x86.exe Neo4jClient.Vb.Tests\bin\%config%\Neo4jClient.Vb.Tests.dll
if not "%errorlevel%"=="0" goto failure
@echo Neo4jClient VB tests... SUCCESS.

REM Package
mkdir Artifacts
tools\nuget.exe pack "Neo4jClient.nuspec" -o Artifacts -p Configuration=%config% %version%
if not "%errorlevel%"=="0" goto failure
@echo Packed and ready to roll!

:success
exit 0

:failure
exit -1