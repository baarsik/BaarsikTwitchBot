# Changelog
All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0 WIP] - 2021-05-17
### Added
- Unique chatters counter
### Changed
- Switched from SQL Server to SQLite

## [1.1.0] - 2021-05-02
### Added
- Song Player UI
- Extended logging for song player related exceptions
### Changed
- Method of volume changing to in-app mixer instead of global
### Fixed
- Rewards with custom text input being counted as messages in dashboard
- Song request being not parsed if a line break is added
- Crash when unable to connect to tmi.twitch.tv to retrieve new chatters
- !limitsong displaying incorrect reward title

## [1.0.0] - 2021-04-04
### Added
- Windows GUI
- Application version to logging
- Requirement for song requests to be at least one minute long
- !limitsong auto song skip if requested via 'Song Request' reward
- Minimum view count for YouTube song requests
- Validation to prevent running multiple bot instances
- Changelog:)
### Changed
- Required channel permissions to match latest Twitch requirements
### Fixed
- 'Video unplayable' crash on song requests from YouTube
- Song request being not parsed if extra text is added