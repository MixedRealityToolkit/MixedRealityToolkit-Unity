# MRTK3 Tenets

The following are the guiding principles by which the MRTK3 project abides. This is a living document and the list may be modified over time.

1. [Break large changes into manageable chunks](#break-large-changes-into-manageable-chunks)
1. [Support all application stages](#support-all-application-stages)
1. [Centralized project authority](#centralized-project-authority)
1. [Get community feedback on large decisions](#get-community-feedback-on-large-decisions)
1. [Avoid backchannel code reviews](#avoid-backchannel-code-reviews)
1. [Community contributions are the responsibility of the project](#community-contributions-are-the-responsibility-of-the-project)
1. [Set clear project guidelines](#set-clear-project-guidelines)
1. [Be prompt with feedback and anticipated shipping updates](#be-prompt-with-feedback-and-anticipated-shipping-updates)
1. [Be kind and professional](#be-kind-and-professional)

## Break large changes into manageable chunks

Every contributor is expected to limit the size of their pull requests to something that can be considered reasonable to review. Excessively large changes are difficult to review and often hide bugs and less than ideal architectures.

For more change management details, please review the [merging pull requests](merging-pull-requests.md) document.

## Support all application stages

MRTK3 is designed to be used by developers of new and existing Unity projects. Features should not be added that limit the abilities of developers to create their applications, regardless of the current stage of development.

## Centralized project authority

MRTK3 is governed by a [steering committee](https://github.com/MixedRealityToolkit/MixedRealityToolkit-MVG/blob/main/org-docs/CHARTER.md). This committee is comprised of members from multiple companies with a vested interest in the success of MRTK. Decisions are made per the documented [governance](../GOVERNANCE.md).

Being an open source project does not mean that everyone has the ability to approve or reject changes. The [maintainers group](../MAINTAINERS.md) has the responsibility for determining if a change is appropriate and meets the project guidelines and standards. This is not to imply that the MRTK3 project does not wish to receive code review feedback from non-maintainers, we openly encourage feedback and will take it under advisement when making approval decisions. 

## Get community feedback on large decisions

The community are our partners and they will have the opportunity to voice their opinions on large decisions. This feedback will be taken into account by the [steering committee](https://github.com/MixedRealityToolkit/MixedRealityToolkit-MVG/blob/main/org-docs/CHARTER.md) and weighed against the "needs of the many".

The primary location for requesting community feedback will be the [Discussions](https://github.com/orgs/MixedRealityToolkit/discussions) section of this repository. Other channels may be considered, based on where the MRTK3 community gravitates.

Examples of decisions in which the community feedback has been solicited include; minimum Unity version for MRTK3 GA, selection of Unity XR Interaction Toolkit as the underlying abstraction layer, etc.

## Avoid backchannel code reviews

Code review feedback benefits everybody. All feedback should be documented in the pull request so that everyone can learn from the discussions and understand the criteria by which the change was approved. In person conversations are not discouraged, provided that the feedback is documented in the appropriate pull request.

Please review the process for [merging pull requests](merging-pull-requests.md).

## Community contributions are the responsibility of the project

When a code change is submitted and approved, it becomes the responsibility of the [maintainers group](../MAINTAINERS.md) and other contributors to fix bugs and evolve the code as needed.

## Set clear project guidelines

The MRTK3 [steering committee](https://github.com/MixedRealityToolkit/MixedRealityToolkit-MVG/blob/main/org-docs/CHARTER.md) and [maintainers](../MAINTAINERS.md) strive to set clear guidelines for the project. This includes project scope, supported platforms and anticipated roadmaps.

## Be prompt with feedback and anticipated shipping updates

The MRTK3 [maintainers](../MAINTAINERS.md) strive to be prompt with pull request feedback and the package release in which the change is anticipated to be published. We also work to ensure that new issues are acknowledged and receive an appropriate level of discussion.

If you file an issue or open a pull request, please respond to feedback and questions so that fixes can be released as quickly as possible.

## Be kind and professional

MRTK3 exists to make developing XR applications enjoyable for everyone. Please maintain a friendly and professional disposition when working in the project. There are users and contributors at all skill and experience levels and we wish for them to all feel welcome.

Please review the [code of conduct](../CODE_OF_CONDUCT.md). We take this code seriously and will make enforcement decisions as required.
