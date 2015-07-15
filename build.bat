@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

set version=
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

set nunit="tools\nunit\nunit-console.exe"

REM Build
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Neo4jClient.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
if not "%errorlevel%"=="0" goto failure

REM Unit tests
%nunit% Test\bin\%config%\Neo4jClient.Test.dll
if not "%errorlevel%"=="0" goto failure

REM Package
mkdir Build
call %nuget% pack "Neo4jClient\Neo4jClient.csproj" -symbols -o Build -p Configuration=%config% %version%
if not "%errorlevel%"=="0" goto failure

:success
exit 0

:failure
exit -1