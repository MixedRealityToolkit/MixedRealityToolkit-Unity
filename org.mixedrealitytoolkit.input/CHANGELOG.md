# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [3.2.0-development] - 2024-03-20

### Added

* Added an alternative Line of Sight (LOS), with angular offset, hand ray pose source. (#625)
* Added IsProximityHovered property of type TimedFlag to detect when a button starts being hovered or on interactor proximity and when it stops being hovered or on proximity of any interactor. (#611)

### Fixed

* Fixed support for UPM package publishing in the Unity Asset Store. (#519)
* Fix the fallback controllers being backwards (#636)
* Fix empty SpeechRecognitionKeyword breaking all speech keyword system (#612) (#614)
