# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.10] = 2022-02-11
### Changed
- Launcher setup create a subfolder when selecting a non empty folder

## [2.0.9] = 2022-02-11
### Fixed
- ArcSDK.dll compilation

## [2.0.8] - 2022-02-11
### Added 
- Launcher setup mode
- Launcher settings
- Launcher offline mode

### Changed
- Updated dependencies

## [2.0.7] - 2022-01-31
### Changed
- increase server timeout to 2h

## [2.0.6] - 2021-12-23
### Added 
- Ability to change the game language

## [2.0.5] - 2021-12-05
### Added
- Error page with Exception details to Gigantic.Agent

### Changed
- Made launching instances more resilient to errors


## [2.0.4] - 2021-12-04
### Fixed 
- Detection of default ArcSDK.dll
### Changed
- improved logging for background tasks

### Fixed 
- GiganticEmu.Web && GiganticEmu.Mice docker container .net version

## [2.0.3] - 2021-11-29
### Fixed
- Legacy/Fang api

## [2.0.2] - 2021-11-29
### Added
- Taskbar icon
- More background images (randomized for now)

### Changed
- Updated dependencies, fixed deprecations/warnings, updated codestyle

### Fixed
- Creature loadout not working on linux

## [2.0.1] - 2021-11-28
### Added
- Linux support for GiganticEmu.Launcher (requires wine)
- Embed GiganticEmu.Agent into GiganticEmu.Launcher

### Changed
- Removed dependency on MSVCR .dlls
- Removed dependency on .NET runtime

### Fixed
- GiganticEmu.Agent won't create an empy serverstart file everytime a instance is started anymore
- GiganticEmu.Agent will now always start the lowest avaiable instance instead of cycling through them

## [2.0.0] - 2021-11-22
### Added
- Initial release of GiganticEmu


