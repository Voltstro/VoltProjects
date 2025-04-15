# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.3.0] - 2025-04-15

NOTE: This release requires Postgres 17! Upgrade to Postgres 17 before updating apps.

### Added

- Admin UI
- Job Based Project Building
  - Projects used to be built on a fixed schedule. Now are able to customize how often/when a project and its versions are built.
- Breadcrumbs
- Project "Publish" flag

### Changed

- Upgrade to .NET 9
- Change Theme Select to a Button
- Minor theme improvements
- Improve ProjectMenu and ProjectToc
  - These tables used to store what they need as JSON. Lets not do this anymore, and use actual tables
- Sitemap Generation Improvements
- Use compiled EF queries
- Replace Upserts with Merge

### Fixed

- Fix Mobile No-Aside With No-TOC Pages

### Removed

- Remove deprecated VDocFx Builder

## [2.2.5] - 2025-04-04

### Changed

- Replace `reglcass` datatype with `oid` in `language` as well

## [2.2.4] - 2025-04-02

### Changed

- Replace `regclass` datatype with `oid` (for Postgres upgrade)
- Bump copyright year

## [2.2.3] - 2024-12-27

### Added

- Add S3 storage provider

### Changed

- Update deps

## [2.2.2] - 2024-10-04

### Changed

- Update deps

### Deprecated

- VDocFX builder was made obsolete, it will be removed in the next version
- Obsolete and ignore `ProjectPage.ParentPageId` and `ProjectPage.ParentPage`

## [2.2.1] - 2024-09-17

### Added

- Add activity to Sitemap Background Service
- Add activity to DB migrations

### Changed

- Update deps
- Update tracking initialization

### Fixed

- Fix OpenTelemetry not recording exceptions
- Fix error page ProjectNav partial having incorrect path

## [2.2.0] - 2024-08-10

### Added

- MKDocs Ingestion Support
- Site-wide search
- Contrast NavBar in light mode
- Page storage items are now more "generic", allowing other assets like JS to be uploaded and served by an object storage provider
- Added more SEO tags
- Added Google Cloud Storage as an object storage provider
- Added caching controls to uploaded storage objects
- Added display name to projects

### Changed

- Changed "Twitter" to "X"
- Changed FK relations to restrict on delete
- Update deps

### Fixed

- Make project name case-insensitive, to fix issue where if someone provided a project name in lower case, it ending in a 404
- Added constraint on `project_page_storage_item`, fixing duplicated data

## [2.1.4] - 2024-04-17

### Changed

- Update deps

## [2.1.3] - 2024-03-07

### Changed

- Updated deps

## [2.1.2] - 2024-02-18

### Changed

- Updated deps

### Fixed

- Fix DocFx alerts
- Fix incorrect ID links in DocFx

## [2.1.1] - 2024-01-23

### Changed

- Update deps
- Change Npgsql default log level to warning
- Delete built docs directory if it exists

### Fixed

- Fix builder not uploading images correctly

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
