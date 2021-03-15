# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)

## [1.0.0] (Alpha) - 2021-03-15
### Added
- String field in settings to change defalt label name ("SS3D-Asset")
- Reset button to label name

### Changed
- Icons that are from unity are no longer loaded with Resourece.Load but instead with EditorGUIUtility.IconContent
- Assets are now loaded from the entire unity project instead of a single folder if they are marked with the label "SS3D-Asset"
- Cleaned up code SS3D
- README to be more updated and insightfull

### Removed
- Setting Asset folder, no need for it since assets are now loaded with labels
- Editor/Resources unity icons since they are now loaded with EditorGUIUtility.IconContent

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