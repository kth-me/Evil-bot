@echo off

for %%G in ("%~dp0\..") do set repoName=%%~nxG
set startUpProject="ConsoleUI"

:BeginProcess
cd /d "..\src\%repoName%.%startUpProject%"
dotnet run --configuration Release
goto BeginProcess