![GitHub Workflow Status (branch)](https://img.shields.io/github/workflow/status/BigBoot/GiganticEmu/build/master) 
![GitHub](https://img.shields.io/github/license/BigBoot/GiganticEmu)
![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/BigBoot/GiganticEmu)
![GitHub release (latest SemVer including pre-releases)](https://img.shields.io/github/v/release/BigBoot/GiganticEmu?include_prereleases&label=pre-release)

# GiganticEmu 
A backend emulator for the game Gigantic by Motiga.

![Logo](GiganticEmu.Agent/icon.svg)


## GiganticEmu.Launcher (Mistforge Launcher)
Avalonia based launcher application for connecting to the emulated backend.

![GiganticEmu.Launcher](GiganticEmu.Launcher/screenshot.png)

## GiganticEmu.Agent
Provides a WebInterface and a REST api for hosting private Servers.
![GiganticEmu.Agent](GiganticEmu.Agent/screenshot.png)

## GiganticEmu.Web
Provides a Rest API for user account creation/login.
As well as some methods required for authenticating the game clients.

## GiganticEmu.Mice
Implements the TCP based MICE protocol used by the client to talk to the backend.

## Instructions
* Download the Misfroge Launcher from the [latest release](https://github.com/BigBoot/GiganticEmu/releases/latest) into your Gigantic folder.
* Start the MistforgeLauncher.exe 

## Building
Requirements:
* MSVC 2019 (e.g. Visual Studio 2019 with C++ Build Tools)
* CMake
* .NET SDK 5.0
* Powershell 7.x

```
./dist.ps1
```
The resulting files will be in dist/

## Troubleshooting

#### The game crashes with an error report:
Install the latest vc_redist: https://aka.ms/vs/16/release/vc_redist.x64.exe
