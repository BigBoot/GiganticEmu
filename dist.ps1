#!/usr/bin/env pwsh

param (
    [string]$Destination = "dist"
)

if (!(Test-Path -path $Destination)) { New-Item $Destination -ItemType "Directory" }

$ArcDistDir = "$(Resolve-Path $Destination)"
$ArcBuildDir = "bin/ArcSDK/"
$ArcSrcDir = Resolve-Path "ArcSDK"
    
if (!(Test-Path -path $ArcBuildDir)) { New-Item $ArcBuildDir -ItemType "Directory" }
if (!(Test-Path -path $ArcDistDir)) { New-Item $ArcDistDir -ItemType "Directory" }

Push-Location $ArcBuildDir 
cmake -G "Visual Studio 16 2019" -DCMAKE_BUILD_TYPE=Release $ArcSrcDir
cmake --build . --config=Release 
Pop-Location
Copy-Item -Path $ArcBuildDir/Release/ArcSDK.dll -Destination $ArcDistDir

dotnet publish -r win-x64 -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:DebugType=None -p:DebugSymbols=false --self-contained false -c Release -o bin/ GiganticEmu.Launcher/GiganticEmu.Launcher.csproj
Copy-Item -Path bin/GiganticEmu.Launcher.exe -Destination $Destination/MistforgeLauncher/MistforgeLauncher.exe

dotnet publish -r win-x64 -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:DebugType=None -p:DebugSymbols=false --self-contained false -c Release -o bin/ GiganticEmu.Agent/GiganticEmu.Agent.csproj
Copy-Item -Path bin/GiganticEmu.Agent.exe -Destination $Destination/GiganticEmu.Agent.exe