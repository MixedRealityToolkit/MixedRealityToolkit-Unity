// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#pragma warning disable CS0618

#if UNITY_WSA && !UNITY_2020_1_OR_NEWER
#define WLT_ENABLE_LEGACY_WSA
#endif

using UnityEngine;
#if WLT_ENABLE_LEGACY_WSA
using UnityEngine.XR;
using UnityEngine.XR.WSA;
#endif // WLT_ENABLE_LEGACY_WSA
using UnityEngine.Rendering;
using UnityEngine.Assertions;
using System;
using System.Collections.Generic;

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// Class to reinterpret spatial mapping data from "spongy" space into "frozen" space.
    /// This is unnecessary when using MRTK's spatial mapping, which provides this and
    /// other enhancements over the native spatial mapping.
    /// </summary>
    public class FrozenSpatialMapping : MonoBehaviour
    {
        /// <summary>
        /// The 3 states of baking a surface can be in.
        /// </summary>
        private enum BakedState
        {
            NeverBaked = 0,
            Baked,
            UpdatePostBake
        }

        /// <summary>
        /// This class holds data that is kept by the system to prioritize Surface baking. 
        /// </summary>
        private class SurfaceEntry
        {
            public GameObject surfaceObject = null; // the GameObject this surface hangs off of.
            public GameObject worldAnchorChild = null; // the GameObject child of surfaceObject generated to hold the WorldAnchor
            public int handle = 0; // this surface's identifier
            public DateTime updateTime = DateTime.MinValue; // update time as reported by the system
            public BakedState currentState = BakedState.NeverBaked;
        }

#if WLT_ENABLE_LEGACY_WSA
        /// <summary>
        /// Interface to spatial mapping
        /// </summary>
        private SurfaceObserver observer;
#endif // WLT_ENABLE_LEGACY_WSA

        /// <summary>
        /// Store known surfaces by handle.
        /// </summary>
        private readonly Dictionary<int, SurfaceEntry> surfaces = new Dictionary<int, SurfaceEntry>();

        /// <summary>
        /// Frozen World Manager for conversion to Frozen space.
        /// </summary>
        private WorldLockingManager manager { get { return WorldLockingManager.GetInstance(); } }

        [SerializeField]
        [Tooltip("Whether the Mapping is active. If inactive, all resources disposed and only remade when active again.")]
        private bool active = true;
        /// <summary>
        /// Whether the Mapping is active. If inactive, all resources disposed and only remade when active again.
        /// </summary>
        public bool Active { get { return active; } set { active = value; } }

        [SerializeField]
        [Tooltip("Material to draw surfaces with. May be null if no display wanted.")]
        private Material drawMaterial = null;
        /// <summary>
        /// Material to draw surfaces with. May be null if no display wanted. 
        /// </summary>
        public Material DrawMaterial => drawMaterial;

        [SerializeField]
        [Tooltip("Whether to render the active surfaces with the given material")]
        private bool display = true;
        /// <summary>
        /// Whether to render the active surfaces with the given material.
        /// </summary>
        public bool Display {
            get { return display && (DrawMaterial != null); }
            set
            {
                // Note that changing the value of display might not change the value of Display (if displayMat == null).
                bool oldDisplay = Display;
                display = value;
                if (Display != oldDisplay)
                {
#if WLT_ENABLE_LEGACY_WSA
                    ChangeDisplayState();
#endif // WLT_ENABLE_LEGACY_WSA 
                }
            }
        }

        [SerializeField]
        [Tooltip("Whether to perform collisions and raycasts against these surfaces")]
        private bool collide = true;
        /// <summary>
        /// Whether to perform collisions and raycasts against these surfaces.
        /// </summary>
        public bool Collide => collide;

        [SerializeField]
        [Tooltip("Object to attach surface objects to. May be null to add surface objects to scene root.")]
        private Transform hangerObject = null;
        /// <summary>
        /// Object to attach surface objects to. May be null to add surface objects to scene root.
        /// </summary>
        public Transform HangerObject => hangerObject;

        [SerializeField]
        [Tooltip("Object around which spatial mappings are centered. Set to null to center around the camera.")]
        private Transform centerObject = null;
        /// <summary>
        /// Object around which spatial mappings are centered. Set to null to center around the camera.
        /// </summary>
        public Transform CenterObject => centerObject;

        [SerializeField]
        [Tooltip("Period in seconds at which to update surfaces.")]
        private float updatePeriod = 2.5f;
        /// <summary>
        /// Period in seconds at which to update surfaces.
        /// </summary>
        public float UpdatePeriod => updatePeriod;

        [SerializeField]
        [Tooltip("Radius around the camera to map.")]
        private float radius = 7.0f;
        /// <summary>
        /// Radius around the camera to map.
        /// </summary>
        public float Radius => radius;

        /// <summary>
        /// Supported tessellation quality levels.
        /// </summary>
        public enum QualityType
        {
            Low = 0,
            Medium = 1,
            High = 2
        };

        [SerializeField]
        [Tooltip("Quality at which to tessellate.")]
        private QualityType quality = QualityType.Medium;
        /// <summary>
        /// Quality at which to tessellate.
        /// </summary>
        public QualityType Quality => quality;

        /// <summary>
        /// Convert an abstract quality to a numeric value suitable for input to spatial mapping system.
        /// </summary>
        private float TrianglesPerCubicMeter
        {
            get
            {
                switch(quality)
                {
                    case QualityType.Low:
                        return 100.0f;
                    case QualityType.Medium:
                        return 300.0f;
                    case QualityType.High:
                        return 600.0f;
                }
                Debug.Assert(false, $"Quality set to invalid value {quality}.");
                return 300.0f;
            }
        }

        /// <summary>
        /// Flag to gate requests to the SurfaceObserver. Only one baking request (via RequestMeshAsync)
        /// is ever in flight at one time.
        /// </summary>
        private bool waitingForBake = false;

        /// <summary>
        /// Countdown to next time to update the surface observer.
        /// </summary>
        private float updateCountdown = 0.0f;

        /// <summary>
        /// Cached spatial mapping layer.
        /// </summary>
        private int spatialMappingLayer = -1;


        private void Start()
        {
            spatialMappingLayer = LayerMask.NameToLayer("SpatialMapping");
            // dummy use of variables to silence unused variable warning in non-WSA build.
            if (updateCountdown > 0 && waitingForBake)
            {
                updateCountdown = 0;
            }
        }

        private void Setup()
        {
#if WLT_ENABLE_LEGACY_WSA
            Debug.Assert(observer == null, "Setting up an already setup FrozenSpatialMapping");
            observer = new SurfaceObserver();
            observer.SetVolumeAsSphere(Vector3.zero, Radius);
#endif // WLT_ENABLE_LEGACY_WSA
        }

        private void Teardown()
        {
#if WLT_ENABLE_LEGACY_WSA
            Debug.Assert(observer != null, "Tearing down FrozenSpatialMapping that isn't set up.");
            foreach (var surface in surfaces.Values)
            {
                Destroy(surface.surfaceObject);
            }
            surfaces.Clear();
            observer.Dispose();
            observer = null;
#endif // WLT_ENABLE_LEGACY_WSA
        }

        private void Update()
        {
#if WLT_ENABLE_LEGACY_WSA
            if (CheckState())
            {
                UpdateObserver();
                UpdateSurfaces();
            }
#endif // WLT_ENABLE_LEGACY_WSA
        }

#if WLT_ENABLE_LEGACY_WSA
        private bool CheckState()
        {
            if (Active)
            {
                if (observer == null)
                {
                    Setup();
                }
            }
            else
            {
                if (observer != null)
                {
                    Teardown();
                }
            }
            return Active;
        }

        private void UpdateSurfaces()
        {
            if (waitingForBake)
            {
                return;
            }
            SurfaceEntry bestSurface = FindBestSurfaceToBake();
            if (bestSurface != null)
            {
                DispatchBakeRequest(bestSurface);
            }
        }


        private void UpdateObserver()
        {
            updateCountdown -= Time.unscaledDeltaTime;
            // Avoid calling Update on a SurfaceObserver too frequently.
            if (updateCountdown <= 0.0f)
            {
                Vector3 frozenCenterPosition = CenterObject != null
                    ? CenterObject.position 
                    : Camera.main.transform.position;
                Vector3 spongyCenterPosition = manager.SpongyFromFrozen.Multiply(frozenCenterPosition);
                // The observer operates in Spongy space.
                observer.SetVolumeAsSphere(spongyCenterPosition, Radius);

                try
                {
                    observer.Update(SurfaceChangedHandler);
                }
                catch
                {
                    // Update can throw an exception if the specified callback is bad.
                    Debug.Log("Observer update failed unexpectedly!");
                }

                updateCountdown = UpdatePeriod;
            }
        }

        private SurfaceEntry FindBestSurfaceToBake()
        {
            // Prioritize older adds over other adds over updates.
            SurfaceEntry bestSurface = null;
            foreach (var surface in surfaces.Values)
            {
                if (surface.currentState != BakedState.Baked)
                {
                    if (bestSurface == null)
                    {
                        bestSurface = surface;
                    }
                    else
                    {
                        if (surface.currentState < bestSurface.currentState)
                        {
                            bestSurface = surface;
                        }
                        else if (surface.updateTime < bestSurface.updateTime)
                        {
                            bestSurface = surface;
                        }
                    }
                }
            }
            return bestSurface;
        }

        /// <summary>
        /// Fill out and dispatch a surface baking request.
        /// </summary>
        /// <param name="bestSurface">Info for the surface to bake.</param>
        private void DispatchBakeRequest(SurfaceEntry bestSurface)
        {
            SurfaceData sd;
            sd.id.handle = bestSurface.handle;
            sd.outputMesh = bestSurface.surfaceObject.GetComponent<MeshFilter>();
            // The WorldAnchor has been put on a generated child of the surface object.
            sd.outputAnchor = bestSurface.worldAnchorChild.GetComponent<WorldAnchor>();
            sd.outputCollider = bestSurface.surfaceObject.GetComponent<MeshCollider>();
            sd.trianglesPerCubicMeter = TrianglesPerCubicMeter;
            sd.bakeCollider = Collide;
            try
            {
                if (observer.RequestMeshAsync(sd, SurfaceDataReadyHandler))
                {
                    waitingForBake = true;
                }
                else
                {
                    // A return value of false when requesting meshes 
                    // typically indicates that the specified Surface handle is invalid.
                    Debug.Log($"Bake request for {bestSurface.handle} failed, invalid parameters suspected");
                }
            }
            catch
            {
                Debug.Log($"Bake for surface {bestSurface.handle} failed unexpectedly!");
            }
        }

        /// <summary>
        /// Go through all existing meshes and change their state to match the current display state.
        /// This should only be called when the display state changes, not to verify match.
        /// </summary>
        private void ChangeDisplayState()
        {
            foreach(var entry in surfaces.Values)
            {
                var mr = entry.surfaceObject.GetComponent<MeshRenderer>();
                mr.enabled = this.Display;
            }
        }

        /// <summary>
        /// This handler receives events when surfaces change, and propagates those events
        /// using the SurfaceObserver’s Update method
        /// </summary>
        /// <param name="id">Handle identifying the surface</param>
        /// <param name="changeType">Reason for update</param>
        /// <param name="bounds">New bounds of th esurface</param>
        /// <param name="updateTime">Time stamp of modification.</param>
        private void SurfaceChangedHandler(SurfaceId id, SurfaceChange changeType, Bounds bounds, DateTime updateTime)
        {
            SurfaceEntry entry;
            switch (changeType)
            {
                case SurfaceChange.Added:
                case SurfaceChange.Updated:
                    if (surfaces.TryGetValue(id.handle, out entry))
                    {
                        // If the system has already baked this Surface, lower its priority.
                        if (entry.currentState == BakedState.Baked)
                        {
                            entry.currentState = BakedState.UpdatePostBake;
                            entry.updateTime = updateTime;
                        }
                    }
                    else
                    {
                        // This is a brand new Surface so create an entry for it.
                        entry = new SurfaceEntry();
                        entry.currentState = BakedState.NeverBaked;
                        entry.updateTime = updateTime;
                        entry.handle = id.handle;
                        entry.surfaceObject = new GameObject(System.String.Format("Surface-{0}", id.handle));
                        entry.surfaceObject.layer = spatialMappingLayer;
                        if (HangerObject != null)
                        {
                            entry.surfaceObject.transform.SetParent(HangerObject, false);
                        }
                        entry.surfaceObject.AddComponent<MeshFilter>();
                        if (Collide)
                        {
                            entry.surfaceObject.AddComponent<MeshCollider>();
                        }
                        MeshRenderer mr = entry.surfaceObject.AddComponent<MeshRenderer>();
                        mr.shadowCastingMode = ShadowCastingMode.Off;
                        mr.receiveShadows = false;
                        mr.sharedMaterial = DrawMaterial;
                        mr.enabled = this.Display;
                        entry.worldAnchorChild = new GameObject(entry.surfaceObject.name + "WorldAnchor");
                        entry.worldAnchorChild.transform.SetParent(entry.surfaceObject.transform, false);
                        entry.worldAnchorChild.AddComponent<WorldAnchor>();
                        // Add an adapter component to keep the surface object where the WorldAnchor means it to be.
                        var adapter = entry.surfaceObject.AddComponent<WorldAnchorAdapter>();
                        adapter.TargetObject = entry.surfaceObject.transform;
                        adapter.WorldAnchorObject = entry.worldAnchorChild;
                        surfaces[id.handle] = entry;
                    }
                    break;

                case SurfaceChange.Removed:
                    if (surfaces.TryGetValue(id.handle, out entry))
                    {
                        surfaces.Remove(id.handle);
                        Destroy(entry.surfaceObject);
                        // Note entry.worldAnchorChild is child of surfaceObject, so will get destroyed
                        // along with components.
                    }
                    break;
            }
        }

        private void SurfaceDataReadyHandler(SurfaceData sd, bool outputWritten, float elapsedBakeTimeSeconds)
        {
            waitingForBake = false;
            SurfaceEntry entry;
            if (surfaces.TryGetValue(sd.id.handle, out entry))
            {
                // Check SurfaceData consistency with the request data.
                Assert.IsTrue(sd.outputMesh == entry.surfaceObject.GetComponent<MeshFilter>());
                Assert.IsTrue(sd.outputAnchor == entry.worldAnchorChild.GetComponent<WorldAnchor>());
                entry.currentState = BakedState.Baked;
            }
            else
            {
                Debug.Log(System.String.Format("Paranoia:  Couldn't find surface {0} after a bake!", sd.id.handle));
                Assert.IsTrue(false);
            }
        }
#endif // WLT_ENABLE_LEGACY_WSA
    }
}