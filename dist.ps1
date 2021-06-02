#!/usr/bin/env pwsh

param (
    [string]$Destination = "dist"
)

$BuildDir = "bin/ArcSDK"
$SrcDir = Resolve-Path "ArcSDK"

if (!(Test-Path -path $BuildDir)) { New-Item $BuildDir -ItemType "Directory" }
if (!(Test-Path -path $Destination)) { New-Item $Destination -ItemType "Directory" }

dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained false -c Release
Copy-Item -Path ./bin/Release/net5.0/win-x64/publish/GiganticEmu.exe -Destination $(Resolve-Path $Destination)

Push-Location $BuildDir 
cmake -G "Visual Studio 16 2019" -DCMAKE_BUILD_TYPE=Release $SrcDir
cmake --build . --config=Release 
Pop-Location
Copy-Item -Path $BuildDir/Release/ArcSDK.dll -Destination $(Resolve-Path $Destination)

if (!(Test-Path -path "$Destination/username.txt")) { New-Item "$Destination/username.txt" }
Set-Content "$Destination/username.txt" "TheLegend27"


$Files = Get-ChildItem $Destination -Exclude "GiganticEmu.zip"
Compress-Archive -Path $Files -Update -CompressionLevel "Optimal" -DestinationPath "$Destination/GiganticEmu.zip"