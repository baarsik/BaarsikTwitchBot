# Changelog
All notable changes to this project will be documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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