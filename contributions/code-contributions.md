# Code Contributions

This project is an open source project under the BSD 3-Clause license. Community contributions are welcome and appreciated, both for new features and bug fixes.

Contributing to this project is easy. We recommend using the `MRTKDevTemplate` Unity project as a convenient development testbed, as it already includes all of the Project packages as local on-disk dependencies. See [Exploring MRTK3 sample scenes](https://learn.microsoft.com/windows/mixed-reality/mrtk-unity/mrtk3-overview/getting-started/exploring-features/mrtk3-sample-scenes) for more information on how to use this Unity project.

## Contribution guide

1. Fork the Project repository to your GitHub account.

2. Clone your forked Project repository by following our guide on [starting from a template project](https://learn.microsoft.com/windows/mixed-reality/mrtk-unity/mrtk3-overview/getting-started/setting-up/setup-new-project) Ensure you have the required tooling, especially the correct Unity version. To ensure you are on the right branch, clone using the command:

    ``
    git clone --branch main YOUR_GIT_URL
    ``

3. Create a new branch for your changes or fixes.

    ``
    git checkout -b foobar_fix
    ``

4. Open the `MRTKDevTemplate` template project located in `UnityProjects/MRTKDevTemplate`. You can add the project to your Unity Hub for easy access.

5. Make your desired changes and create unit tests that ensure your changes work as expected. Make sure to test in-editor and deployed to device. Commit your changes to your branch. Publish your branch to your fork upstream.

6. Open a pull request on the Project repo, targeting the `main` branch. Make sure to accurately describe the changes you've made, and apply relevant labels to your pull request for better categorization and triage. If you're a new contributor to the Project, you may need to sign our contribution agreement.

7. Address any fixes requested by the community or the maintenance team, and merge your PR after approval.

## Documenting code

Anyone making code changes must make sure that documentation is updated to reflect their changes. If individuals make changes that render the Project documentation invalid, it is their responsibility to submit documentation updates and ensure that the changes are merged into the public documentation prior to the feature being completed.

When making code changes, the contributor must update the in-code XML documentation tags. At a bare minimum, types and their members must have a `<summary>` tag clearly explaining the type and their members. These tags are used when generating API reference documentation. So it is critical that any new public or protected type or member have a `<summary>` block. Also, functions should have all parameters and return values documented using the appropriate tags. For more information on in-code documentation tags, please see [Recommended XML tags for C# documentation comments](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/recommended-tags).

## Writing tests

Tests are a critical part of ensuring the Project is a reliable foundation for high-quality mixed reality applications. Any new features that are added should have unit tests to ensure their functionality remains correct as other changes are made to the codebase in the future.

To write unit tests, we recommend that you first take a look at the existing unit tests and learn how the Project test utilities and simulator are used to mock XR input. You can mock hand input, gaze, HMD position, and other basic input-related features. Here's some general advice for writing good unit tests:

- Try to write more granular tests that evaluate smaller pieces of functionality, rather than larger monolithic tests. More granular unit tests allow maintainers to see which specific feature has been broken. More general end-to-end functionality tests are also appreciated, but ensure that each smaller part of your feature is well-tested to begin with.
- Make sure your test (and your feature) doesn't make any assumptions about the orientation or location of the user. Your tests and features should work at any arbitrary offset or rotation from the world origin.
- If your tests mock user input, make sure to subclass our `BaseRuntimeInputTests`, which ensures that the proper test harness is set up and torn down.
- Use NUnit parameterization to easily increase the variety and flexibility of your test. [See the documentation for parameterized NUnit tests here.](https://docs.nunit.org/articles/nunit/technical-notes/usage/Parameterized-Tests.html)
- Some inputs or interactions may take multiple frames to register. You can use `yield return RuntimeTestUtilities.WaitForUpdates()` to add extra frames of delay to your test if your interactions aren't registering.
- Try to write unit tests that execute as quickly as possible to avoid slow CI iteration times.
- Make sure you add the relevant test dependencies to the `package.json`, and the correct references to the test folder's assembly definition file.

## Continuous integration

Every pull request is subject to automated tests before being able to be merged. Other continuous integration (CI) jobs are also run on the resulting commit on the main development branch to ensure broken packages aren't deployed to the feed.

If your tests are passing in-editor but fail in the CI run, you should run your tests locally in batch mode. Some types of tests may unexpectedly fail when running in no-graphics batch mode due to timing differences or other Unity quirks. Running your tests locally in batch mode helps identify these inconsistent tests before the CI does.

Use the `Tooling/Tests/run_playmode_tests.ps1` script to run tests locally in batch mode. You'll need to close your Unity editor to do so.

``
./Tooling/Tests/run_playmode_tests.ps1
``

The script will generate output files in the `/out` folder, including both `.log` files and the test results `.xml`. You can filter which tests are run by passing a regular expression to the script. Custom Unity versions and project folder locations can also be provided as arguments.

``
./Tooling/Tests/run_playmode_tests.ps1 -unityVersion 2021.3.5f1 -projectPath ../my/project/path
``
