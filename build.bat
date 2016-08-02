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

REM Build
"%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" Neo4jClient.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
if not "%errorlevel%"=="0" goto failure

REM Unit tests
tools\nuget.exe install NUnit.Runners -Version 2.6.4 -OutputDirectory packages
if not "%errorlevel%"=="0" goto failure

packages\NUnit.Runners.2.6.4\tools\nunit-console.exe /config:%config% /framework:net-4.5 Neo4jClient.Tests\bin\%config%\Neo4jClient.Tests.dll
if not "%errorlevel%"=="0" goto failure

packages\NUnit.Runners.2.6.4\tools\nunit-console.exe /config:%config% /framework:net-4.5 Neo4jClient.Vb.Tests\bin\%config%\Neo4jClient.Vb.Tests.dll
if not "%errorlevel%"=="0" goto failure

REM Package
mkdir Artifacts
tools\nuget.exe pack "Neo4jClient.nuspec" -o Artifacts -p Configuration=%config% %version%
if not "%errorlevel%"=="0" goto failure

:success
exit 0

:failure
exit -1