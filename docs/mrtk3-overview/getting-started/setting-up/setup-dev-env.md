---
title: Setting up the development environment
parent: Getting started with MRTK3
nav_order: 1
---

# Setting up your development environment

Before setting up a Unity Project with MRTK3, make sure you have the following software tools installed.

| Software | Version | Notes |
| --- | --- | --- |
| Unity | 2022.3 LTS or newer | Recommend using an LTS release <br> See [Additional tools](#additional-tools) below for recommended modules |
| [Mixed Reality Feature Tool for Unity](https://aka.ms/mrfeaturetool) | | Used to acquire MRTK3 packages |

## Additional tools

### Android

If your target platform is an [AOSP-based](https://source.android.com/) device, like Android XR or Quest, your Unity installation needs to include the Android Build Support module and its submodules.

![Android module installation](../../images/setting-up/MRTK-Development-Setup-AndroidModule.png)

### HoloLens 2

| Software | Version | Notes |
| --- | --- | --- |
| [Microsoft Visual Studio](https://visualstudio.microsoft.com/) | 2022 Community or newer | Add the required workloads as noted in the [Installation Checklist](https://learn.microsoft.com/windows/mixed-reality/develop/install-the-tools) |
| Windows 10 SDK | 10.0.18362.0 or later | |

If your target platform is a HoloLens device, your Unity installation needs to include the Universal Windows Platform Support module.

![UWP module installation](../../images/setting-up/MRTK-Development-Setup-UWPModule.png)

## Next steps

After setting up the development environment, there are few options for creating a Unity Project with MRTK3.

- [Starting from the template project](./setup-template.md): This guide walks you through cloning a template project, which is pre-configured to consume all MRTK3 packages. This template project is set up with Unity project settings for running your application on a device.
- [Starting from a new project](./setup-new-project.md): This guide walks you through adding vital MRTK3 packages to a new Unity project. The guide also helps you set up the Unity project settings for running your application on a device.
