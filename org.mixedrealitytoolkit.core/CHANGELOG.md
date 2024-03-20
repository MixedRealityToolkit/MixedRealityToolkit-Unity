# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [3.2.0-development] - 2024-03-20

### Added

* StabilizedRay constructor with explicit position and direction half life values. (#625)
* Added IsProximityHovered property of type TimedFlag to detect when a button starts being hovered or on interactor proximity and when it stops being hovered or on proximity of any interactor. (#611)
* Adding ProximityHover events (Entered & Exited) to PressableButton class. (#611)


### Fixed

* Fixed support for UPM package publishing in the Unity Asset Store. (#519)
* Fix warning and event triggered on disabled StatefulInteractable after changing speech settings (#591) (#608)