# Approving Pull Requests

All merges into the `main` branch must be done through a pull request process to ensure changes are reviewed and approved by code owners.

## The role

Contributors with the **write** role can approve pull requests.

## Basic validation

Contributors must ensure that the following criteria are met before a pull request (PR) can be merged:

* Has a clear and concise title, and a detailed description explaining the changes.
* All conversations are resolved.
* All status checks, including unit tests, pass successfully.
* Code changes must have XML documentation text (i.e.`<summary>`) for all public visible types and members. See [Compiler Warning (level 4) CS1591](https://learn.microsoft.com/dotnet/csharp/language-reference/compiler-messages/cs1591) for more details.
* The author confirmed code changes were tested within the Unity Editor and on at least one XR device.
* Must have at least one approval from an assigned [code owner](code-owners.md). Some circumstances may require additional approvals.
* Pull request approvals are dismissed when a new commit is pushed.

If any of these criteria are not met, the pull request can not be merged.

## Additional considerations

Pull requests should be kept as small as possible. If a change requires a large amount of code, it should be split into smaller requests that are easier to review. When in doubt, please contact the [maintainers group](../MAINTAINERS.md) *before* opening a large pull request. They can often assist in refactoring the change into smaller pieces that facilite effective code reviews. 

The pull request author should also consider adding the following before a PR is merged:

* References to Project issues that is being fixed or implemented.
* New or updated unit tests to validate the code changes.
* Visual updates should have screen shots or videos demonstrating the changes.

## Build breaks and automation failures

If a merged pull request breaks subsequent builds or automated tests, the author must fix the failure as soon as possible, or risk having the change reverted.

## Blocking pull requests

There may be times when a pull request contains changes that need further consideration before approval. Such occurrences include, but not limited to, design and breaking changes.  In such cases, you should block the pull request by adding the "Merge: Blocked" label, and kindly explain your reasoning for blocking. Then notify the Project's Maintainers.

### Design changes

If a pull request contains visual changes to UI controls that can't be easily undone or turned off by an application developer, these pull requests must be blocked until the Project Maintainers review the change. Mark these changes with the "Type: Design Change" label.

### Breaking changes

A change is considered breaking if it contains incompatible API or behavior changes, such that it will break applications built on a previous released versions of a Project package. If a pull request introduces a breaking change, these pull requests must be blocked. Mark these changes with the "Type: Breaking Change" label.

### Unblocking a pull request

Only project Maintainers can unblock a pull request, and remove the "Merge: Blocked" label. To unblock a pull request, Maintainers follow the decision making rules in the [GOVERNANCE.md](../GOVERNANCE.md) file.
