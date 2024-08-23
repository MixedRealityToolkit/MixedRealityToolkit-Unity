# Opening and Assessing Issues

All Contributors can open issues using the templates available for a [bug](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/issues/new?assignees=&labels=Bug&template=bug-report.md&title=), [document issue](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/issues/new?assignees=&labels=Documentation&template=documentation-issue.md&title=), [feature request](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/issues/new?assignees=&labels=Documentation&template=documentation-issue.md&title=), and [security vulnerability](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/security/advisories/new). Consider the following when opening a new Project issue:

* First check that there isn't already an issue tracking the problem or feature.
* Create a title that is concise, but provides enough details to quickly give a basic understanding of the issue.
* Write descriptions that provide as much as detail as possible to help Contributors diagnosis the problem. Images or videos are a big plus for visual or UI bugs.
* Make sure to provide steps that can consistently reproduce the problem or bug.
* Make sure to provide platform and version information. Such as device type, operating system, and Unity version.
* Consider providing an example Unity project that has a isolated reproduction of the bug. This is extremely helpful to Contributors fixing the issue.

## The role

Contributors with the **triage** role can assist with issue assessment. 

## Assessing issues

During triage, Contributors should look for issues with the "Needs: Triage" label. This label will be applied to any new issue created in the project.

### Initial assessment

While looking at new issues, you should quickly determine the validity of the issue. Consider the following:

* If the issue requests help using the Project packages within a particular application scenario, convert the issue to a discussion topic. Then answer the question if possible. If it is not possible, keep the "Needs: Triage" label or add a "Needs: Organization Attention" label if the organization responsible is known.
* If the issue doesn’t make appropriate use of an issue template, apply the "Needs: Template Info" label and request that the author provide the missing information.
* If the issue doesn’t mention the Project package version that was used, apply the "Needs: Version Info" label and request that the author provide the version number.
* If the issue doesn’t mention the platforms that were used, apply the "Needs: Platform Info" label and request that the author provide the platform information.
* If the issue doesn’t clearly describe steps to reproduce the issue, apply the "Needs: Repro Steps" label and request that the author provide the missing steps.

### Invalid issues

In some cases, an issue may not be a valid discussion topic, bug, or feature request. In such cases, add the "Status: Invalid" label, add a comment politely explaining your reasoning, and close the issue as not planned.

### Project appeals

Project appeals are discussed in section 2.2 of the [GOVERNANCE.md](../GOVERNANCE.md) file. These issues can only be triaged by the Project Maintainers, and should be labeled with "Type: Appeal".

## Continue assessment

After verifying that basic information is present, start parsing the content. Verify that the issue includes a clear description of the problem. If it does not, politely ask the author to provide additional feedback and add the "Needs: Author Feedback" label.

Once it has been verified that an issue has all the information and a clear description, take a moment to consider if the issue can still be improved in some meaningful way. For example, can the formatting be altered to improve readability and understanding? The reviewer is encouraged to make light edits to the issue to improve its readability.

It's also useful to apply labels to issues during triage. Consider adding the following labels where appropriate:

| Label  | Description |
|--------|------------ |
| Type: Appeal | A Project appeal that needs the attention of the Project Maintainers |
| Type: Bug | A problem with an existing feature that can be fixed with the next patched release. |
| Type: Feature Request |  A request for a new feature that can be included with the next minor version release. |
| Type: Breaking Change | A change that contains incompatible API or behavior changes from an older package version. The fix for this issue must be included with the next major version release. |
| Type: Release Blocker | A bug that should be fixed sometime before the next release. |
| Type: Documentation | Issue for new or existing documentation. |
| Type: Good First Issue | Issue that is good for newcomers. |
| Needed By: [Org Name] | The specified affiliated organization depends on this issue. |
| Needs: Author Feedback | Needs additional information from the issue author. |
| Needs: Triage | Needs to be triaged. |
| Needs: Repro | Missing clear or consistent reproduction steps. |
| Needs: Issue Template | Missing some required information in the issue template. |
| Needs: Version Info | Missing the version number in which the bug was reproduced. |
| Needs: Platform Info | Missing the platforms which this bug was reproduced on. |
| Needs: Verify on Latest | Needs to be verified on the latest version of the Project package |
| Needs: Design Feedback | A Project designer needs to provide feedback. |
| Needs: [Org Name] Attention | The specified affiliated organization needs to review the issue. |
| Package: [Package Name] | The Project package that is impacted by this issue. |
| Area: [Area Name] | The Project area of the that is impacted by this issue. |
| Status: Duplicate | The issue is a duplicate of another issue. The duplicate issue must be referenced in the comments. |
| Status: In Review | A fix for the issue has been submitted for review. |
| Status: Fixed | A fix for the issue has been merged. |
| Status: Answered | A discussion topic has been answered. |
| Status: Won’t Fix | A valid problem that will not be fixed. |
| Status: Invalid | Not a valid discussion topic, bug, or feature request. |
| Status: By Design | Described behavior that is expected. |
| Status: Help Wanted | Extra attention is needed to resolve the issue |
| Status: Blocked | Development is blocked by another task or issue. |
| Merge: Blocked | The pull request merge is blocked until Project Maintainers approve. |

### Determining impact

Part of issue assessment is determining the issue's severity. When doing this, think about the following:

#### Is this a potential release blocker?

Release blocker issues should be addressed in a timely manner, before the next release. These issues could be a regression of previously released behavior, severely broken functionality, or cause some other critical failure in an application. An issue that has a clear workaround should not be labeled with "Type: Release Blocker."

Just because the issue causes an exception, or application crash, does not mean the issue should be a release blocker if there is a clear workaround.

An issue should not be labeled with "Type: Release Blocker" if the issue was knowingly shipped in a previous release.

#### Is this a bug or feature?

Project issues that clearly describe functionality behaving differently than intended should be labeled as "Type: Bug." However, if the Project issue is describing functionality that was never designed for, consider adding the "Type: Feature Request" label.

#### Is there a known workaround?

Clearly explain any known workarounds for the described Project issue. If a workaround is of low impact and results in no loss of functionality, the issue's priority should be considered low.

## Assigning issues

If you know who can fix the Project issue, then assign it to a Contributor or Contributors. However only assign an issue to Contributors who are able to act in a timely manner. The assigned Contributors are responsible for resolving the issue in a reasonable amount of time.

If it will take weeks for an individual to start work on the Project issue, consider leaving the assignees field blank. An unassigned issue will signal that it is available for other organizations or Contributors to work on.

If a particular organization must resolve the issue and it’s unclear what Contributor will do the work, use the "Needs: [Org Name] Attention" label and provide comment explaining your reasoning.

## Milestone assignment

GitHub [milestones](https://docs.github.com/en/issues/using-labels-and-milestones-to-track-work/about-milestones) should be used sparingly. Only assign a milestone if the milestone has a clear dependency on the issue.

Before a new milestone can be created in the repository, the new milestone must be approved by the project Maintainers, following the decision making rules in the [GOVERNANCE.md](../GOVERNANCE.md) file.

## GitHub project assignments

In place of assigning a milestone, consider assigning issues to a GitHub [project](https://docs.github.com/en/issues/planning-and-tracking-with-projects/learning-about-projects/about-projects). An issue can have multiple project assignments, and each Project Maintainer can manage their own GitHub projects and the priorities within those projects.

## Critical priority issues

Such a priority must be used sparingly and only applies to released package issues that severely impact a large number of users. For an issue to be deemed critical it must be severe enough to require the immediate attention of the Project Maintainers. Security and data loss bugs, while unlikely in MRTK, are examples of issues that may warrant critical status.

High priority issues that aren't deemed critical should be addressed during scheduled Project Maintainer meetings. These meetings typically occur every two weeks.

When a critical bug is discovered, the Project Maintainers must be contacted immediately, and the issue must be tagged with "Priority: Critical". All Project Maintainers must then meet as soon as possible to decide if the issue warrants a patch release. The Project Maintainers can use an agreed upon communication method, such as a group email or messaging platform, to arrange a meeting.
