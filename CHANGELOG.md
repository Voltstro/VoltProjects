# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.1.0] - 2024-01-20

### Added

- DocFx Builder
- Metabar
- Edit this page button (Opens markdown's file in Git)
- Healthz requests are filtered from tracing
- Added Tracing in Builder

### Changed

- Upgraded Builder & Server to .NET 8
- Updated deps
- DB is now responsible for LastUpdateTime in tables
  - Updates rows via a trigger
- DB is now responsible for CreationTime in tables
- Project Pages will not be updated if related content has not been updated
- Client no longer uses jsx-dom (Elements are created by just using normal JS methods)
- Images that are already WebP will not get re-encoded when uploading to storage
- Bump Copyright

### Fixed

- Fix TOC toggle and ITA left border always being white (regardless of theme)

## [2.0.2] - 2023-11-11

### Changed

- Improved main index project listing
- Sort main index project listing by name asec
- Improved project TOC mobile button
- Updated dependencies

## [2.0.1] - 2023-11-05

### Fixed

- Fix version resolving not working correctly
- Attempted fix for CORS

## [2.0.0] - 2023-11-05

### Added

- Project versioning
- Split app into two separate apps, one for Server (public facing), and a Builder (for building project's docs)
- Build project out into Docker images
- Non-HTML assets are stored and hosted by Azure Storage
- Client has light and dark theme
- Sentry telemetry

### Changed

- Project pages are now rendered by VoltProjects it self, instead of directly hosting static content, providing a unified UI
- Project pages are stored in the DB
- Project configuration is stored in a DB
- Improved project index listing page
- Updated about page
- Updated footer to have funny
- Client uses Roboto and FiraCode fonts
- Much better git repo management
- Project license is now regular GPL-3.0

## [1.1.0] - 2022-09-22

### Added

- Add "hidden" option
- Build actions can have environmental variables set or mapped
- Add "small names"

### Changed

- Updated dependencies

## [1.0.1] - 2022-08-17

### Changed

- Updated dependencies
- Allow dot files to be served
- Use assembly file version

## [1.0.0] - 2022-08-01

- Initial Release
