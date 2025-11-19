# Deploy an MRTK3 project to HoloLens 2

This page describes how to deploy your Unity Project with MRTK3 onto a HoloLens 2.

> [!NOTE]
> We strongly recommend using [Holographic remoting](streaming.md) for rapid iteration and testing on HoloLens 2, which allows for instant testing on the device without the need for compile + deploy.

## Deployment Pre-requisites

- Add MRTK to your project and ensure your [project settings](../getting-started/setting-up/setup-new-project.md#5-configure-openxr-related-settings) are configured correctly to use the OpenXR pipeline and MRTK's feature set. **These features are required to deploy your project onto your HoloLens**.

> [!NOTE]
> If starting from our [template project](../getting-started/setting-up/setup-template.md), these project settings should already be configured for you.

## Deploying to Device

1. After you have the project configured, proceed to [Build the Unity Project](/windows/mixed-reality/develop/unity/build-and-deploy-to-hololens#build-the-unity-project).

1. Once built, you'll need to deploy the project through [Visual Studio](/windows/mixed-reality/develop/advanced-concepts/using-visual-studio?tabs=hl2).
