# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Changed
- improved logging for background tasks

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


