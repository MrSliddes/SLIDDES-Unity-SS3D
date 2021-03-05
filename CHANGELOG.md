# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

## [0.0.3] - 2021-03-05
### Added
- Mouse now displays icon of current tool being used
- Asset buttons now display the asset name as tooltip
- Asset view display index now gets saved
- New Asset display, text only button
- Setting to keep GUI button visable for toggeling inUse

### Changed
- 2D toggle now has an icon stead of "2D View"

### Fixed
- Editor now correctly toggles inUse to false when going in play mode and toggeling inUse to true if inUse was true before play mode after exiting play mode
- Assets displayed in grid now allign correctly to the left instead of being spaced out evenly
- Z index layer now stays on selected layer if changed while in play mode and then exiting

## [0.0.2] - 2021-03-04
### Added
- Editor Window now works out of the box
- Buttons for increasing/decreasing z layer by one

### Changed
- Objects created now instantiate prefabs instead of gameobject copies
- SS3D is now a single editor script and has no runtime scripts anymore

### Removed
- "SS3D Temporary GameObject" now no longer appears in Hierarchy
- OnFocus function

## [0.0.1] - 2021-03-03
### Added
- First public version