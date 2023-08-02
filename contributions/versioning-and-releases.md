# Versioning and Releases

## Semantic versioning

Project packages are individually versioned, following the [Semantic Versioning 2.0.0 specification](https://semver.org/spec/v2.0.0.html).

Individual versioning will enable faster servicing while providing improved developer understanding of the magnitude of changes and reducing the number of packages needing to be updated to acquire the desired fix(es).

For example, if a non-breaking new feature is added to the UX core package that contains the logic for user interface behavior, the minor version number will increase (from 3.0.x to 3.1.0). Since the change is non-breaking, the UX components package, which depends upon UX core, is not required to be updated.

As a result of this change, there isn't a unified Project version.

To help identify specific packages and their versions, the Project provides an "about" dialog that lists the relevant packages included in the project. To access this dialog, in Unity on the menu bar, select `Mixed Reality` > `MRTK3` > `About MRTK`.

## Process of updating versions

Package versioning is managed by the Project Maintainers. During pull request reviews, Maintainers should decide if package versions need to be changed. Maintainers should consider the following questions, and then if needed, request that the pull request author update package versions.

> During this time Maintainers should be aware of the released packages published to the Project's [releases](https://github.com/MixedRealityToolkit/MixedRealityToolkit-ProjectDocs/releases) page.

### Is this a breaking change?

Follow the rules outlined under the [Breaking changes](merging-pull-requests.md#breaking-changes) section. If this is breaking change, the package's major version should be incremented from the latest released major version of the package. For example, if the package's latest release was v3.4.1, the new version should be v4.0.0.

### Is this new functionality?

If the change contains new functionality that works in a backward compatible manner, the package's minor version should be incremented from the latest released minor version. For example, if the package's latest minor release was v3.4.1, the new version should be v3.5.0.

### Is this just backward compatible bug fix?

If the change only contains backward compatible bug fixes, the package's patch version should be incremented from the latest released patch version. For example, if the package's latest patch release was v3.4.1, the new version should be v3.4.2.

## Package release process

A release can be made to the Project's [releases](https://github.com/MixedRealityToolkit/MixedRealityToolkit-ProjectDocs/releases) page by using the following process:

1. Create release notes using [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).
2. Create UPM package.
3. Verify that the UPM package is in stable condition and has no release blocking issues.
4. Create ZIP file containing source code.
5. Present changelog and release candidate files to Project Maintainers.
6. Project Maintainers then must agree to the release.
7. Once approved, add a `git` tag to the commit hash being released, following the format [package-postfix]-vMAJOR.MINOR.PATCH. For example, core-v3.0.1 or input-v3.2.0.
8. Finally, the release notes, UPM package, and source code are posted under project's [releases](https://github.com/MixedRealityToolkit/MixedRealityToolkit-ProjectDocs/releases) page.

This process only applies when releasing to the Project's [releases](https://github.com/MixedRealityToolkit/MixedRealityToolkit-ProjectDocs/releases) page. This means Contributors are permitted to release versions of Project packages via other mechanisms, outside of this Project.
