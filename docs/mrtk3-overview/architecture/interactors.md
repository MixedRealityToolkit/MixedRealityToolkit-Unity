---
title: Interactor architecture
parent: Architecture overview
nav_order: 1
---

# Interactor Architecture — MRTK3

MRTK builds upon the set of interactors offered by Unity's XR Interaction Toolkit. Mixed reality features like articulated hand tracking, gaze, and pinch require more elaborate interactors than the set provided with XRI by default. MRTK defines new interactor interfaces, categorized generally by the input modality, and corresponding implementations.

## Summary and Review

For developers new to XRI, we recommend that you first review Unity's [XRI architecture documentation](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.0/manual/architecture.html). MRTK interactors are subclasses of existing XRI interactors or implementations of the XRI interactor interfaces. See  Unity's documentation on their interactor architecture which also applies to MRTK.

### Good Citizens of XRI

The custom MRTK interactors are well-behaved with respect to the default XRI interactor interfaces; from the perspective of XRI systems, they're indistinguishable from "vanilla" interactors. The inverse is also true; when building advanced interactables in MRTK, the default XRI interactors will still work for basic hover and select. It's part of the MRTK effort to be fully compatible with existing XRI projects. If you have an XRI application, MRTK interactables and UI controls will work with your existing "vanilla" XRI setup.

### Abstraction of Input Modality

The input device, the interactor performing the interaction, and the interaction events they generate are all architecturally isolated in XRI. This isolation is critical to the input abstraction strategy in MRTK3, and enables us to write cross-platform and cross-device interactions that function well in all contexts.

From MRTK v2, there's a common instinct to code interactions specific to a particular input type or device. Many developers are accustomed to writing interactions that react specifically to a near grab, a far ray, or some other specific input type.

While MRTK3 still allows for the disambiguation and detection of individual input modes, hard-coding interactions to specific individual input types is artificially limiting and reduces the flexibility of your interactions. More on this can be found in the [interactable architecture documentation](interactables.md), but the key for interactors is that they generally don't have to map 1:1 with input devices.

### AttachTransform and Inversion of Control

Much of what MRTK v2 did in "move logics" as part of `ObjectManipulator`, `Slider`, and so forth, is now the responsibility of the interactor itself. The interactor now controls its attachTransform to define how a specific type of manipulation behaves. One no longer needs to write complex interaction logic on the interactable that differs between input modalities; instead, your unified manipulation logic can listen to the `attachTransform`'s pose regardless of the input modality or the device driving it.

For example, a `GrabInteractor`'s `attachTransform` is located at the grabbing point on the hand/controller. An `XRRayInteractor`'s `attachTransform` is located at the hit point at the ray's end. The `CanvasProxyInteractor`'s `attachTransform` is located wherever the mouse has clicked. For all of these different interactors, the interactable **_doesn't have to care about the type of interactor in order to respond appropriately to manipulations._**

The interactable queries the `attachTransform` and can treat every `attachTransform` the same regardless of the interactor type.

This approach is critical for compatibility with existing XRI interactors as well as future-proofing your interactions for input modalities that haven't yet been developed. If a new input method is introduced, you don't need to alter existing interactables if the new interactor generates a valid and well-behaved `attachTransform`.

Thus, philosophically, the `attachTransform` _is_ the interaction logic. For any custom interactions, always give preference to writing a new interactor with new `attachTransform` logic rather than rewriting or extending interactables to be customized for your new interaction. In this way, all existing interactables can enjoy the benefits of your new interaction instead of only the ones you've rewritten or extended.

### XRControllers and Input Binding

Most interactors don't bind directly to input actions. Most derive from `XRBaseControllerInteractor`, which requires an `XRController` above the interactor in the hierarchy. The `XRController` binds to input actions and then propagates the relevant actions (select, and so forth) down to all attached interactors.

Nonetheless, some interactors may need special input bindings or additional input that the `XRController` doesn't provide. In these cases, interactors have the option to bind directly to their own unique input actions or even use other non-Input-System sources for interaction logic. The XRI base classes prefer to listen to the `XRController`'s bindings, but these behaviors can be overridden to use external or alternative input sources.

## Interfaces

XRI defines the basic `IXRInteractor`, `IXRHoverInteractor`, `IXRSelectInteractor`, and `IXRActivateInteractor`. MRTK defines additional interfaces for interactors. Some expose additional information about MRTK-specific interactions, and others are simply for categorization and identification. These interfaces are all located within the **Core** package, while the implementations reside in other packages, including **Input**.

> [!IMPORTANT]
> While these interfaces are helpful if you need to filter for a specific type of interaction, we recommend that you do _not_ hard-code your interactions to listen for these interfaces specifically. _In every situation, always give preference to the generic XRI **isSelected** and **isHovered**, rather than any interaction-specific interface_. <br> <br>
Unless necessary, you shouldn't reference the concrete MRTK implementations of these interfaces in interactables unless it's absolutely necessary. In all cases, it's better to reference the interfaces. Explicitly referencing the concrete types will restrict your interactables to only work with the current, existing types. By referencing only the interfaces, you ensure compatibility with future implementations that may not subclass the existing implementations.

### IVariableSelectInteractor

Interactors implementing this interface can issue variable (that is, analog) selectedness to interactables. The variable select amount can be queried with the `SelectProgress` property. MRTK interactors that implement this interface include the `MRTKRayInteractor` and the `GazePinchInteractor`. Base interactables (the default XRI interactables, and `MRTKBaseInteractable`) won't be affected by the variable selection amount; `StatefulInteractable`, however, listens to this value and computes its `Selectedness` based on the `max()` of all participating variable and non-variable interactors.

### IGazeInteractor

Interactors that implement this interface represent the user's passive gaze, separate from any manipulation or intent. The MRTK implementation is `FuzzyGazeInteractor`, which inherits from the XRI `XRRayInteractor`, and adds fuzzy cone-casting logic. `XRBaseInteractable` will flag `IsGazeHovered` when an `IGazeInteractor` is hovering.

### IGrabInteractor

Interactors that implement this interface represent a physical near-field grabbing interaction. The `attachTransform` is defined as the grabbing point. The MRTK implementation is `GrabInteractor`, which subclasses XRI's `XRDirectInteractor`.

### IPokeInteractor

Interactors that implement this interface represent a poking interaction. Note that this doesn't necessarily imply a finger! Arbitrary interactors can implement this interface and offer poking interactions from non-finger sources. In one of the few instances where checking interactor interfaces is a good idea, interactables like `PressableButton` listen for `IPokeInteractor`s, specifically, to drive volumetric press. Any interactor that implements `IPokeInteractor` will induce 3D presses on buttons.

`IPokeInteractor` exposes the `PokeRadius` property, which defines the characteristics of the poking object. The poke is considered to be centered on the `attachTransform` and extends outwards from the `attachTransform` by the `PokeRadius`. Interactables like `PressableButton` offset their 3D push distance by this radius, which can be driven by the user's physical finger thickness in the case of finger-based presses.

The MRTK implementation of this interface is `PokeInteractor`. In our template project, we also provide another example of `IPokeInteractor` that's not finger-driven; `PenInteractor` provides poke interactions rooted on the tip of a virtual 3D stylus.

### IRayInteractor

Interactors that implement this interface represent a ray-based pointing interaction. The `attachTransform` represents the hit location of the ray on the surface of the targeted object during a selection.

The MRTK implementation of this interface is `MRTKRayInteractor`, inheriting directly from the XRI `XRRayInteractor`.

> [!NOTE]
> The XRI `XRRayInteractor` doesn't implement this MRTK interface.

### ISpeechInteractor

Interactors that implement this interface represent speech-driven interactions. The MRTK implementation is `SpeechInteractor`.

The MRTK `SpeechInteractor`, internally, uses  `PhraseRecognitionSubsystem` and subscribes to interactable registration events from the XRI `XRInteractionManager`. However, interactables need not be concerned about what subsystem is performing speech processing; `ISpeechInteractor`s generate the same XRI events (select, and so forth) that any other interactor does.

### IGazePinchInteractor

This interface is simply a specialization of the `IVariableSelectInteractor` interface. Interactors that implement this interface are, implicitly, variable-select interactors. `IGazePinchInteractor`s expressly represent an indirectly targeted remote manipulation. A separate gaze-based interactor drives the target of the interaction, and the manipulation is by a hand or controller. `attachTransform` behaves the same way `IRayInteractor`'s `attachTransform` does; it snaps to the hit point on the target when a select is initiated.

When multiple `IGazePinchInteractor`s participate in a single interaction, their `attachTransform`s are offset by their displacement from the median point between all participating pinch-points. Thus, interactables can interpret these `attachTransform`s in the same way they would for any other multi-handed interaction, like the `attachTransforms` from grab interactions, or ray interactions.

The MRTK implementation is the `GazePinchInteractor`.

### IHandedInteractor

Some interactors can choose to implement  `IHandedInteractor` interface to explicitly specify that they're associated with a particular hand on a user. Some interactors aren't associated with handedness and thus don't implement this. The most obvious examples would be ones like `SpeechInteractor` or `FuzzyGazeInteractor`.

The MRTK interactors that implement this interface are the `HandJointInteractor`, a generic, abstract `XRDirectInteractor` driven by an arbitrary hand joint, the `GazePinchInteractor`, and the `MRTKRayInteractor`.

Interactables currently use this interface to fire certain effects when selected that must disambiguate between a left or right hand. The most notable example of this is the pulse effect in the UX components library.
