![GitHub Workflow Status (branch)](https://img.shields.io/github/workflow/status/BigBoot/GiganticEmu/build/master) 
![GitHub](https://img.shields.io/github/license/BigBoot/GiganticEmu)
![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/BigBoot/GiganticEmu)
![GitHub release (latest SemVer including pre-releases)](https://img.shields.io/github/v/release/BigBoot/GiganticEmu?include_prereleases&label=pre-release)

# GiganticEmu

![Emu](https://repository-images.githubusercontent.com/373163363/488c5b80-c7b8-11eb-9a78-306e77d8ccf4)

Basically this is a hacked together Emulator for the Gigantic Backend Server.
By using this emulator you will be able to press the Start button on the Login screen.
It will also allow you to set your username and allow you to save your settings.



## Instructions
* Download and extract the [latest release](https://github.com/BigBoot/GiganticEmu/releases/latest) into your Binaries\Win64 folder. it should ask you if you want to replace the ArcSDK.dll file -> confirm.
* You can change your username by editing the username.txt using your favorite text editor.
* Start the RxGame-Win64-Test.exe 

*NOTE* On the first start the game will immediately close, this is expected. Just start the game again and everything should be working.

## Building
Requirements:
* MSVC 2019 (e.g. Visual Studio 2019 with C++ Build Tools)
* CMake
* .NET SDK 5.0

### ArcSDK.dll
```
mkdir build
cd build
cmake -G "Visual Studio 16 2019" -DCMAKE_BUILD_TYPE=Release ..
cmake --build . --config=Release 
```
The dll will be in build/Release/ArcSDK.dll

### GiganticEmu.exe
```
dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained false -c Release
```
The exe will be in bin\Release\net5.0\win-x64\publish\GiganticEmu.exe

## Troubleshooting

#### The game keeps closing immediately without any error:
Make sure your DefaultEngine.ini is not read-only

#### The game crashes with an error report:
Install the latest vc_redist: https://aka.ms/vs/16/release/vc_redist.x64.exe

#### The game starts but the login still fails:
Install the latest .net runtime: https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-5.0.6-windows-x64-installer
