#!/usr/build/env pwsh

param (
    [string]$Destination = "dist"
)

if (!(Test-Path -path $Destination)) { New-Item $Destination -ItemType "Directory" }

$ArcDistDir = "$(Resolve-Path $Destination)"
$ArcBuildDir = "build/win-x64/ArcSDK/"
$ArcSrcDir = Resolve-Path "ArcSDK"
    
if (!(Test-Path -path $ArcBuildDir)) { New-Item $ArcBuildDir -ItemType "Directory" }
if (!(Test-Path -path $ArcDistDir)) { New-Item $ArcDistDir -ItemType "Directory" }

if ($IsWindows)
{
    Push-Location $ArcBuildDir 
    cmake -G "Visual Studio 16 2019" -DCMAKE_BUILD_TYPE=Release $ArcSrcDir
    cmake --build . --config=Release 
    Pop-Location
    Copy-Item -Path $ArcBuildDir/Release/ArcSDK.dll -Destination $ArcDistDir
}



dotnet publish -r win8-x64 -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:DebugType=None -p:DebugSymbols=false --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -c Release -o build/win-x64 GiganticEmu.Launcher/GiganticEmu.Launcher.csproj
strip build/win-x64/GiganticEmu.Launcher.exe
Copy-Item -Path build/win-x64/GiganticEmu.Launcher.exe -Destination $Destination/MistforgeLauncher.exe

dotnet publish -r linux-x64 -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:DebugType=None -p:DebugSymbols=false --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -c Release -o build/linux-x64 GiganticEmu.Launcher/GiganticEmu.Launcher.csproj
strip build/linux-x64/GiganticEmu.Launcher
Copy-Item -Path build/linux-x64/GiganticEmu.Launcher -Destination $Destination/MistforgeLauncher

dotnet publish -r win-x64 -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:DebugType=None -p:DebugSymbols=false --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -c Release -o build/win-x64 GiganticEmu.Agent/GiganticEmu.Agent.csproj
strip build/win-x64/GiganticEmu.Agent.exe
Copy-Item -Path build/win-x64/GiganticEmu.Agent.exe -Destination $Destination/GiganticEmu.Agent.exe

dotnet publish -r linux-x64 -p:PublishSingleFile=true -p:PublishReadyToRun=false -p:DebugType=None -p:DebugSymbols=false --self-contained true -p:IncludeNativeLibrariesForSelfExtract=true -c Release -o build/linux-x64 GiganticEmu.Agent/GiganticEmu.Agent.csproj
strip build/linux-x64/GiganticEmu.Agent
Copy-Item -Path build/linux-x64/GiganticEmu.Agent -Destination $Destination/GiganticEmu.Agent
