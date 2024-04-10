// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

//#define WLT_NAN_EXTRA_DEBUGGING
//#define WLT_LOG_SAVE_LOAD

#if WLT_DISABLE_LOGGING
#undef WLT_NAN_EXTRA_DEBUGGING
#undef WLT_LOG_SAVE_LOAD
#endif // WLT_DISABLE_LOGGING

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Unity level implementation of aligning Unity's coordinate system  
    /// with a discrete finite set of markers in the real world.
    /// </summary>
    /// <remarks>
    /// In addition to anchoring the otherwise arbitrary WorldLocked coordinate space to
    /// this set of correspondences, this addresses the tracker-scale issue, whereby due to
    /// tracker error, traversing a known distance in the real world traverses a different distance
    /// in Unity space. This means that, given a large object of length L meters in Unity space,
    /// starting at one end and walking L meters will not end up at the other end of the object,
    /// but only within +- 10% of L.
    /// Use of this service gives fairly exact correspondence at alignment points, and by interpolation
    /// gives fairly accurate correspondence within the convex set of alignment points.
    /// Note that no extrapolation is done, so outside the convex set of alignment points results,
    /// particularly with respect to scale compensation, will be less accurate.
    /// </remarks>
    public class AlignmentManager : IAlignmentManager
    {
#region Lifetime management

        /// <summary>
        /// When a level is unloaded, resend the reference poses. The unloaded scene
        /// will have removed any of its alignment pairs from reference poses.
        /// </summary>
        /// <param name="scene">The unloaded scene, ignored.</param>
        private void OnSceneUnloaded(Scene scene)
        {
            SendAlignmentAnchors();
        }

        /// <summary>
        /// Constructor, binds to a specific WorldLockingManager. Also registers for scene loading events.
        /// </summary>
        /// <param name="manager">WorldLocking manager which owns this sub-manager.</param>
        public AlignmentManager(WorldLockingManager manager)
        {
            this.manager = manager;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            manager.FragmentManager.RegisterForRefitNotifications(OnRefit);
        }

        /// <summary>
        /// Dispose of internals on shutdown.
        /// </summary>
        ~AlignmentManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose of internals on shutdown.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose of internals on shutdown.
        /// </summary>
        private void Dispose(bool disposing)
        {
            manager.FragmentManager.UnregisterForRefitNotifications(OnRefit);
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        /// <summary>
        /// Actually perform the send of the pending new list of alignment anchors into the active state.
        /// </summary>
        /// <remarks>
        /// This is deferred after request until update, to be sure all the pieces have been 
        /// wired up during Start()/OnEnable().
        /// </remarks>
        private void PerformSendAlignmentAnchors()
        {
            sentPoses.Clear();
            for (int i = 0; i < referencePoses.Count; ++i)
            {
                sentPoses.Add(referencePoses[i]);
            }
            ActivateCurrentFragment();
        }

#endregion Lifetime management

#region Public events

        /// <inheritdocs />
        public event EventHandler<Triangulator.ITriangulator> OnTriangulationBuilt;

#endregion

#region Public methods

        /// <summary>
        /// The pose to insert into the camera's hierarchy above the WorldLocking Adjustment transform (if any).
        /// </summary>
        public Pose PinnedFromLocked { get; private set; }

        /// <summary>
        /// Do the weighted average of all active reference poses to get an alignment pose.
        /// </summary>
        /// <param name="worldHeadPose"></param>
        public void ComputePinnedPose(Pose lockedHeadPose)
        {
            CheckSend();
            CheckFragment();
            CheckSave();
            if (activePoses.Count < 1)
            {
                PinnedFromLocked = Pose.identity;
            }
            else
            {
                List<WeightedPose> poses = ComputePoseWeights(lockedHeadPose.position);
                PinnedFromLocked = WeightedAverage(poses);
            }
        }

        /// <summary>
        /// File to save to and load from.
        /// </summary>
        /// <remarks>
        /// May optionally contain subpath. For optimal portability, use forward slashes, e.g.
        /// "myPath/myFile.myExt".
        /// May NOT be an absolute path (e.g. "/myPath.txt" or "c:/myPath.txt" are NOT allowed and will be ignored.)
        /// Application can check validity of path using static AlignmentManager.IsValidSavePath(string).
        /// Defaults to "Persistence/Alignment.fwb".
        /// </remarks>
        public string SaveFileName
        {
            get { return poseDB.SaveFileName; }
            set { poseDB.SaveFileName = value; }
        }

        /// <summary>
        /// Check validity of a save/load path. Any path not passing this test will be ignored without error.
        /// </summary>
        /// <param name="filePath">The path to test.</param>
        /// <returns>True if a valid path.</returns>
        public static bool IsValidSavePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;
            if (Path.IsPathRooted(filePath))
                return false;
            return true;
        }

        /// <summary>
        /// Load the database and issue notification if loaded.
        /// </summary>
        /// <returns>True if loaded.</returns>
        public bool Load()
        {
            bool loaded = poseDB.Load();
            if (loaded)
            {
                afterLoadNotifications?.Invoke();
                SendAlignmentAnchors();
                needSave = false;
            }
            return loaded;
        }

        /// <inheritdocs />
        public bool NeedSave { get { return needSave; } }

        /// <summary>
        /// Explicitly save the database.
        /// </summary>
        /// <returns>True if successfully saved.</returns>
        public bool Save()
        {
            bool saved = poseDB.Save();
            if (saved)
            {
                needSave = false;
            }
            return saved;
        }

        /// <summary>
        /// Register for notification after any successful loads.
        /// </summary>
        /// <param name="del">Delegate to call after successful load.</param>
        /// <remarks>
        /// Registration holds until a corresponding call to <see cref="UnregisterForLoad(PostAlignmentLoadedDelegate)"/>.
        /// </remarks>
        public void RegisterForLoad(PostAlignmentLoadedDelegate del)
        {
            afterLoadNotifications += del;
            if (poseDB.IsLoaded)
            {
                del?.Invoke();
                needSend = true;
            }
        }

        /// <summary>
        /// Un-register for post load notifications, after registration via <see cref="RegisterForLoad(PostAlignmentLoadedDelegate)"/>.
        /// </summary>
        /// <param name="del">The delegate to unregister.</param>
        public void UnregisterForLoad(PostAlignmentLoadedDelegate del)
        {
            afterLoadNotifications -= del;
        }

        /// <inheritdocs />
        public AnchorId AddAlignmentAnchor(string uniqueName, Pose virtualPose, Pose lockedPose)
        {
            FragmentId fragmentId = CurrentFragmentId;
            AnchorId anchorId = ClaimAnchorId();

            if (IsGlobal)
            {
                /// Bake in current snapshot of any application imposed transform (teleport).
                virtualPose = manager.PinnedFromFrozen.Multiply(virtualPose);
            }
            else
            {
                /// For subtree, applied adjustment transform is LockedFromPinned. Remove existing
                /// adjustment here by premultiplying PinnedFromLocked.
                virtualPose = PinnedFromLocked.Multiply(virtualPose);
            }
#if WLT_EXTRA_LOGGING
            string label = "AddAlign1";
            Debug.Log($"F{Time.frameCount} {label} {uniqueName} vp={virtualPose.ToString("F3")} lp={lockedPose.ToString("F3")} sp={manager.SpongyFromLocked.Multiply(lockedPose).ToString("F3")}");
#endif // WLT_EXTRA_LOGGING

            ReferencePose refPose = new ReferencePose()
            {
                name = uniqueName,
                fragmentId = fragmentId,
                anchorId = anchorId,
                virtualPose = virtualPose
            };
            refPose.LockedPose = lockedPose;
            referencePoses.Add(refPose);
            QueueForSave(refPose);

            return anchorId;
        }

        /// <inheritdocs />
        public bool GetAlignmentPose(AnchorId anchorId, out Pose lockedPose)
        {
            /// mafinc - if any perf issue shows up, this could be a dictionary.
            if (anchorId.IsKnown())
            {
                for (int i = 0; i < referencePoses.Count; ++i)
                {
                    if (referencePoses[i].anchorId == anchorId)
                    {
                        lockedPose = referencePoses[i].LockedPose;
                        return true;
                    }
                }
            }
            lockedPose = Pose.identity;
            return false;
        }

        /// <inheritdocs />
        public bool RemoveAlignmentAnchor(AnchorId anchorId)
        {
            bool found = false;
            if (anchorId.IsKnown())
            {
                for (int i = referencePoses.Count - 1; i >= 0; --i)
                {
                    if (referencePoses[i].anchorId == anchorId)
                    {
                        poseDB.Forget(referencePoses[i].name);
                        referencePoses.RemoveAt(i);
                        found = true;
                    }
                }
                for (int i = referencePosesToSave.Count - 1; i >= 0; --i)
                {
                    if (referencePosesToSave[i].anchorId == anchorId)
                    {
                        referencePosesToSave.RemoveAt(i);
                    }
                }
            }
            return found;
        }

        /// <inheritdocs />
        public void ClearAlignmentAnchors()
        {
            poseDB.Clear();
            referencePoses.Clear();
            referencePosesToSave.Clear();
        }

        /// <inheritdocs />
        public void SendAlignmentAnchors()
        {
            needSend = true;
        }

        /// <inheritdocs />
        public AnchorId RestoreAlignmentAnchor(string uniqueName, Pose virtualPose)
        {
            /// mafinc - this API needs settling.
            /// virtualPose unused, it needs to be checked against the virtualPose in the found refPose (if any).
            ReferencePose refPose = poseDB.Get(uniqueName);
            if (refPose == null)
            {
                return AnchorId.Invalid;
            }
            var index = referencePoses.FindIndex(x => x.name == uniqueName);
            if (index >= 0)
            {
                /// The reference pose already exists. Update it by replacing it
                /// with the new refpose using same anchor id.
                refPose.anchorId = referencePoses[index].anchorId;
                referencePoses[index] = refPose;
            }
            else
            {
                referencePoses.Add(refPose);
            }
            /// If the referencePose has an invalid fragment id, it's only because there isn't a valid
            /// fragment right now. Flag the condition and set the proper fragment id when there is
            /// a valid one.
            if (!refPose.fragmentId.IsKnown())
            {
                needFragment = true;
            }
            return refPose.anchorId;
        }
#endregion Public methods

#region Internal data structure definitions

        /// <summary>
        /// Persistent database for reference poses.
        /// </summary>
        private class ReferencePoseDB
        {
#region Public API

            /// <summary>
            /// Set name of file to save to and load from. See notes in <see cref="AlignmentManager.SaveFileName"/>.
            /// </summary>
            public string SaveFileName
            {
                get { return saveFileName; }
                set
                {
                    if (IsValidSavePath(value))
                    {
                        saveFileName = value;
                    }
                }
            }

            /// <summary>
            /// True if the database has been successfully loaded from disk.
            /// </summary>
            public bool IsLoaded { get; private set; } = false;

            /// <summary>
            /// Create a stream and save the database to it. Existing data is overwritten.
            /// </summary>
            /// <returns>True if successfully written.</returns>
            public bool Save()
            {
                bool saved = false;
                try
                {
                    string path = GetPersistentPath();

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    string fileName = GetPersistentFileName();

                    using (Stream stream = File.Open(fileName, FileMode.Create, FileAccess.Write))
                    {
                        saved = Save(stream);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failure opening {GetPersistentFileName()} for save: {e}");
                }
                return saved;
            }

            /// <summary>
            /// Open a stream and load the database from it.
            /// </summary>
            /// <returns>True if the database is successfully loaded.</returns>
            /// <remarks>
            /// Reference poses are assigned to the fragment that is current at the time of load.
            /// If there is not a valid current fragment at the time of their load, they will be assigned
            /// the first valid fragment.
            /// </remarks>
            public bool Load()
            {
                bool loaded = false;
                try
                {
                    string fileName = GetPersistentFileName();
                    if (File.Exists(fileName))
                    {
                        using (Stream stream = File.Open(fileName, FileMode.Open, FileAccess.Read))
                        {
                            loaded = Load(stream);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failure opening {GetPersistentFileName()} for load: {e}");
                }
                IsLoaded = true;
                return loaded;
            }

            /// <summary>
            /// If the given name is represented in the database, create a corresponding reference point.
            /// </summary>
            /// <param name="uniqueName">Unique name for the reference point.</param>
            /// <returns>A valid reference point if found, else null.</returns>
            public ReferencePose Get(string uniqueName)
            {
                Element src;
                if (!data.TryGetValue(uniqueName, out src))
                {
                    return null;
                }
                ReferencePose refPose = new ReferencePose();
                refPose.name = uniqueName;
                refPose.fragmentId = CurrentFragmentId;
                refPose.anchorId = ClaimAnchorId();
                refPose.virtualPose = src.virtualPose;
                refPose.LockedPose = src.lockedPose;

                return refPose;
            }

            /// <summary>
            /// Add or update a reference pose to the database.
            /// </summary>
            /// <param name="refPose">The reference pose to add/update to the database.</param>
            /// <returns>True on success.</returns>
            public bool Set(ReferencePose refPose)
            {
                Element elem = new Element() { virtualPose = refPose.virtualPose, lockedPose = refPose.LockedPose };
                data[refPose.name] = elem;
                return true;
            }

            /// <summary>
            /// Delete an element from the database.
            /// </summary>
            /// <param name="uniqueName">The name of the element to delete.</param>
            /// <returns>True if the element was in the database prior to deletion.</returns>
            public bool Forget(string uniqueName)
            {
                return data.Remove(uniqueName);
            }

            /// <summary>
            /// Clear the database.
            /// </summary>
            public void Clear()
            {
                data.Clear();
            }

#endregion Public API

#region Serialization element

            /// <summary>
            /// A data element containing minimal information to reconstruct its corresponding reference point.
            /// </summary>
            private struct Element
            {
                /// <summary>
                /// The virtual (modeling space) pose.
                /// </summary>
                public Pose virtualPose;

                /// <summary>
                /// The world locked space pose.
                /// </summary>
                public Pose lockedPose;

                /// <summary>
                /// Write the element with the given writer. Data is appended.
                /// </summary>
                /// <param name="writer">Binary writer to write the data with.</param>
                public void Write(BinaryWriter writer)
                {
                    WritePose(writer, virtualPose);
                    WritePose(writer, lockedPose);
                }

                /// <summary>
                /// Write a pose.
                /// </summary>
                /// <param name="writer">The writer.</param>
                /// <param name="pose">The pose</param>
                private static void WritePose(BinaryWriter writer, Pose pose)
                {
                    WriteVector3(writer, pose.position);
                    WriteQuaternion(writer, pose.rotation);
                }

                /// <summary>
                /// Write a vector.
                /// </summary>
                /// <param name="writer">The writer.</param>
                /// <param name="vector">The vector.</param>
                private static void WriteVector3(BinaryWriter writer, Vector3 vector)
                {
                    writer.Write((double)vector.x);
                    writer.Write((double)vector.y);
                    writer.Write((double)vector.z);
                }

                /// <summary>
                /// Write a quaternion.
                /// </summary>
                /// <param name="writer">The writer.</param>
                /// <param name="rotation">The quaternion.</param>
                private static void WriteQuaternion(BinaryWriter writer, Quaternion rotation)
                {
                    writer.Write((double)rotation.x);
                    writer.Write((double)rotation.y);
                    writer.Write((double)rotation.z);
                    writer.Write((double)rotation.w);
                }

                /// <summary>
                /// Read an element from the current cursor position in the reader.
                /// </summary>
                /// <param name="reader">Source reader.</param>
                /// <returns>The element read.</returns>
                public static Element Read(BinaryReader reader)
                {
                    Element elem;
                    elem.virtualPose = ReadPose(reader);
                    elem.lockedPose = ReadPose(reader);
                    return elem;
                }

                /// <summary>
                /// Read a pose.
                /// </summary>
                /// <param name="reader">The reader.</param>
                /// <returns>The pose.</returns>
                private static Pose ReadPose(BinaryReader reader)
                {
                    Pose pose;
                    pose.position = ReadVector3(reader);
                    pose.rotation = ReadQuaternion(reader);
                    return pose;
                }

                /// <summary>
                /// Read a vector.
                /// </summary>
                /// <param name="reader">The reader.</param>
                /// <returns>The vector.</returns>
                private static Vector3 ReadVector3(BinaryReader reader)
                {
                    float x = (float)reader.ReadDouble();
                    float y = (float)reader.ReadDouble();
                    float z = (float)reader.ReadDouble();
                    return new Vector3(x, y, z);
                }

                /// <summary>
                /// Read a quaternion.
                /// </summary>
                /// <param name="reader">The reader.</param>
                /// <returns>The quaternion.</returns>
                private static Quaternion ReadQuaternion(BinaryReader reader)
                {
                    float x = (float)reader.ReadDouble();
                    float y = (float)reader.ReadDouble();
                    float z = (float)reader.ReadDouble();
                    float w = (float)reader.ReadDouble();
                    return new Quaternion(x, y, z, w);
                }
            }

#endregion Serialization element

#region Internal Members

            /// <summary>
            /// The current database version. 
            /// </summary>
            private readonly uint version = 1;

            /// <summary>
            /// The database. Elements store only enough information to reconstruct reference poses.
            /// </summary>
            private readonly Dictionary<string, Element> data = new Dictionary<string, Element>();

            /// <summary>
            /// The cached file to which to save and from which to load. Defaults to Persistence/Alignment.fwb.
            /// </summary>
            private string saveFileName = Path.Combine("Persistence", "Alignment.fwb");

#endregion Internal Members

#region Internal Implementation 

            /// <summary>
            /// Path where to store data. 
            /// </summary>
            /// <returns>The path as a usable path string.</returns>
            /// <remarks>
            /// TODO: make path configurable by application.
            /// </remarks>
            private string GetPersistentPath()
            {
                string fullPath = GetPersistentFileName();

                return Path.GetDirectoryName(fullPath);
            }

            /// <summary>
            /// Path and filename where to store data.
            /// </summary>
            /// <returns>Filename and path as usable string.</returns>
            private string GetPersistentFileName()
            {
                string path = Application.persistentDataPath;

                string fullPath = Path.Combine(path, saveFileName);

                return fullPath;
            }

            /// <summary>
            /// Write all data to given stream.
            /// </summary>
            /// <param name="stream">Destination stream.</param>
            /// <returns>True if successfully saved.</returns>
            private bool Save(Stream stream)
            {
                DebugLogSaveLoad($"Enter save {SaveFileName}");
                bool saved = false;
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    DebugLogSaveLoad($"Saving {SaveFileName} v={version} c={data.Count}");
                    writer.Write((uint)version);
                    writer.Write((int)data.Count);
                    foreach (var keyVal in data)
                    {
                        writer.Write(keyVal.Key);
                        keyVal.Value.Write(writer);
                    }
                    saved = true;
                }
                return saved;
            }

            /// <summary>
            /// Clear the database and load from given stream.
            /// </summary>
            /// <param name="stream">The source stream.</param>
            /// <returns>True if successfully loaded.</returns>
            /// <remarks>
            /// Possible reasons for failure include no data in stream or
            /// incompatible version.
            /// </remarks>
            private bool Load(Stream stream)
            {
                DebugLogSaveLoad($"Enter load {SaveFileName}");
                data.Clear();
                bool loaded = false;
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    var v = reader.ReadUInt32();
                    if (v == version)
                    {
                        int count = reader.ReadInt32();
                        DebugLogSaveLoad($"Loading {SaveFileName} v={version} c={count}");
                        for (int i = 0; i < count; ++i)
                        {
                            string name = reader.ReadString();
                            Element elem = Element.Read(reader);
                            data.Add(name, elem);
                        }
                        loaded = true;
                    }
                }
                return loaded;
            }
#endregion Internal Implementation 
        }

        /// <summary>
        /// A pose (possibly) contributing to the global camera alignment pose.
        /// </summary>
        /// <remarks>
        /// A pose will only contribute if its fragmentId is the current fragmentId,
        /// and its distance weight based on its playSpacePosition is non-zero.
        /// If there are any ReferencePose's in the current fragment, at least one is guaranteed to have non-zero
        /// contribution, but it is possible that none are in the current fragment.
        /// </remarks>
        private class ReferencePose
        {
#region Public members
            /// <summary>
            /// Unique identifier.
            /// </summary>
            public string name;

            /// <summary>
            /// Fragment this is associated with.
            /// </summary>
            public FragmentId fragmentId;

            /// <summary>
            /// Anchor identifier allocated for this reference pose.
            /// </summary>
            public AnchorId anchorId;

            /// <summary>
            /// Modelling space pose associated with this reference pose.
            /// </summary>
            public Pose virtualPose;

            /// <summary>
            /// Whether this reference pose should contribute now.
            /// </summary>
            public bool IsActive
            {
                get { return fragmentId == CurrentFragmentId; }
            }

            /// <summary>
            ///  The world locked space pose, protected for refit operations.
            /// </summary>
            public Pose LockedPose
            {
                get
                {
                    return lockedPose;
                }
                set
                {
                    lockedPose = value;
                    CheckAttachmentPoint();
                    AfterAdjustmentPoseChanged();
                }
            }

#endregion Public members

#region Private members

            private readonly WorldLockingManager manager = WorldLockingManager.GetInstance();

            /// <summary>
            /// World locked space pose corresponding to the virtual pose.
            /// </summary>
            private Pose lockedPose;

            /// <summary>
            /// Attachment point for adjustment to refit events.
            /// </summary>
            private IAttachmentPoint attachmentPoint;

#endregion Private members

#region Public APIs
            /// <summary>
            /// Release any resources bound to this reference point.
            /// </summary>
            public void Release()
            {
                if (attachmentPoint != null)
                {
                    manager.AttachmentPointManager.ReleaseAttachmentPoint(attachmentPoint);
                    attachmentPoint = null;
                }
            }

#endregion Public APIs

#region Internal implmentations

            /// <summary>
            /// When the reference point position is initially set, create an attachment point if there isn't one,
            /// or if there is, updated its position.
            /// </summary>
            private void CheckAttachmentPoint()
            {
                IAttachmentPointManager mgr = manager.AttachmentPointManager;
                if (attachmentPoint == null)
                {
                    attachmentPoint = mgr.CreateAttachmentPoint(LockedPose.position, null, OnLocationUpdate, null);
                }
                else
                {
                    mgr.TeleportAttachmentPoint(attachmentPoint, LockedPose.position, null);
                }
            }

            /// <summary>
            /// Update the pose for refit operations.
            /// </summary>
            /// <param name="adjustement">The adjustment to apply.</param>
            private void OnLocationUpdate(Pose adjustement)
            {
                fragmentId = CurrentFragmentId;
                lockedPose = adjustement.Multiply(lockedPose);
                AfterAdjustmentPoseChanged();
            }

            private void AfterAdjustmentPoseChanged()
            {
                /// Do any adjustment pose dependent caching here.
            }
#endregion Internal implmentations
        }

        /// <summary>
        /// The final pose with a single weight.
        /// </summary>
        /// <remarks>
        /// Note that this weight is not normalized in the sense of the weights summing to one,
        /// as this would not be helpful where they are actually used.
        /// </remarks>
        private struct WeightedPose
        {
            public Pose pose;
            public float weight;
        }


#endregion Internal data structure definitions

#region Internal data declarations

        /// <summary>
        /// The manager that owns this sub-manager.
        /// </summary>
        private readonly WorldLockingManager manager;

        /// <summary>
        /// The persistent database of reference poses.
        /// </summary>
        private readonly ReferencePoseDB poseDB = new ReferencePoseDB();

        /// <summary>
        /// All added poses, which will become active poses after then next <see cref="SendAlignmentAnchors"/>.
        /// </summary>
        private readonly List<ReferencePose> referencePoses = new List<ReferencePose>();

        /// <summary>
        /// Poses that have been activated by <see cref="SendAlignmentAnchors"/>.
        /// </summary>
        private readonly List<ReferencePose> sentPoses = new List<ReferencePose>();

        /// <summary>
        /// All reference poses in the current fragment that have been submitted by <see cref="SendAlignmentAnchors"/>.
        /// </summary>
        private readonly List<ReferencePose> activePoses = new List<ReferencePose>();

        /// <summary>
        /// The fragment all the active poses belong in. Stored as ulong for serialization.
        /// </summary>
        private ulong activeFragmentId = (ulong)FragmentId.Unknown;

        /// <summary>
        /// Converter of fragment all active poses are in into FragmentId.
        /// </summary>
        private FragmentId ActiveFragmentId { get { return (FragmentId)activeFragmentId; } set { activeFragmentId = (ulong)value; } }

        /// <summary>
        /// WeightedPoses with a final normalized weight each computed"/>
        /// </summary>
        private readonly List<WeightedPose> weightedPoses = new List<WeightedPose>();

        /// <summary>
        /// Modified reference poses saved to a list through the frame, saved at most once per frame.
        /// </summary>
        private List<ReferencePose> referencePosesToSave = new List<ReferencePose>();

        /// <summary>
        /// Notification that a new saved reference point database has been loaded.
        /// </summary>
        private PostAlignmentLoadedDelegate afterLoadNotifications;

        /// <summary>
        /// Flag that the current state has not been saved to persistent storage.
        /// </summary>
        private bool needSave = false;

        /// <summary>
        /// Flag that a the reference pose list needs to be updated to the active list.
        /// </summary>
        private bool needSend = false;

        /// <summary>
        /// Flag that the system is waiting for a valid fragment to assign to loaded reference poses.
        /// </summary>
        private bool needFragment = false;

        /// <summary>
        /// Next available anchor id. Anchor id's here are independent of any other anchor ids,
        /// and only need to be unique within this object.
        /// </summary>
        private static uint nextAnchorId = (uint)AnchorId.FirstValid;

#endregion Internal data declarations

#region Internal utilities
        /// <summary>
        /// Claim a unique anchor id.
        /// </summary>
        /// <returns>The exclusive anchor id.</returns>
        private static AnchorId ClaimAnchorId()
        {
            return (AnchorId)nextAnchorId++;
        }

        /// <summary>
        /// Return whether this is the global alignment manager owned and managed by the WorldLockingManager.
        /// </summary>
        /// <remarks>
        /// If not the global, this is an independently owned, managed, and applied alignment manager.
        /// </remarks>
        private bool IsGlobal
        {
            get
            {
                return manager.AlignmentManager == this;
            }
        }

        /// <summary>
        /// Return the current WorldLocking fragment id.
        /// </summary>
        private static FragmentId CurrentFragmentId
        {
            get
            {
                if (WorldLockingManager.GetInstance() != null)
                {
                    return WorldLockingManager.GetInstance().FragmentManager.CurrentFragmentId;
                }
                return FragmentId.Unknown;
            }
        }

        private void ActivateCurrentFragment()
        {
            DebugLogSaveLoad($"Active fragment from {ActiveFragmentId.FormatStr()} to {CurrentFragmentId.FormatStr()}");
            activePoses.Clear();
            for (int i = 0; i < sentPoses.Count; ++i)
            {
                if (sentPoses[i].IsActive)
                {
                    activePoses.Add(sentPoses[i]);
                }
            }
            ActiveFragmentId = CurrentFragmentId;
            BuildTriangulation();
        }

        /// <summary>
        /// Search input list for reference pose with given id.
        /// </summary>
        /// <param name="poseList">The list to search.</param>
        /// <param name="id">The id to search for.</param>
        /// <returns>The index in the list if found, else -1.</returns>
        private static int FindReferencePoseById(List<ReferencePose> poseList, AnchorId id)
        {
            return poseList.FindIndex(x => x.anchorId == id);
        }

        private void OnRefit(FragmentId mainId, FragmentId[] absorbedIds)
        {
            ActiveFragmentId = FragmentId.Unknown;
        }

#endregion Internal utilities

#region Persistence synchronizations

        [System.Diagnostics.Conditional("WLT_LOG_SAVE_LOAD")]
        private static void DebugLogSaveLoad(string message)
        {
            Debug.Log($"F={Time.frameCount}: {message}");
        }

        /// <summary>
        /// Add to queue for being saved to database next chance.
        /// </summary>
        /// <param name="refPose"></param>
        private void QueueForSave(ReferencePose refPose)
        {
            DebugLogSaveLoad($"QueueForSave {SaveFileName}");
            int idx = FindReferencePoseById(referencePosesToSave, refPose.anchorId);
            if (idx < 0)
            {
                referencePosesToSave.Add(refPose);
            }
        }

        /// <summary>
        /// Complete any queued saves.
        /// </summary>
        private void CheckSave()
        {
            if (referencePosesToSave.Count > 0)
            {
                DebugLogSaveLoad($"{SaveFileName} has {referencePosesToSave.Count} to save");
                for (int i = referencePosesToSave.Count - 1; i >= 0; --i)
                {
                    poseDB.Set(referencePosesToSave[i]);
                }
                referencePosesToSave.Clear();
                needSave = true;
            }
        }

        /// <summary>
        /// If any reference poses are eligible, promote them to active.
        /// </summary>
        private void CheckSend()
        {
            if (needSend)
            {
                PerformSendAlignmentAnchors();
                needSend = false;
            }
        }

        /// <summary>
        /// If still waiting for a valid current fragment since last load,
        /// and there is a current valid fragment, set it to reference poses.
        /// </summary>
        private void CheckFragment()
        {
            bool changed = ActiveFragmentId != CurrentFragmentId;
            if (needFragment && CurrentFragmentId.IsKnown())
            {
                FragmentId fragmentId = CurrentFragmentId;
                for (int i = 0; i < referencePoses.Count; ++i)
                {
                    if (!referencePoses[i].fragmentId.IsKnown())
                    {
                        DebugLogSaveLoad($"Transfer {referencePoses[i].anchorId.FormatStr()} from frag={referencePoses[i].fragmentId.FormatStr()} to {fragmentId.FormatStr()}");
                        referencePoses[i].fragmentId = fragmentId;
                        changed = true;
                    }
                }
                needFragment = false;
            }
            if (changed)
            {
                ActivateCurrentFragment();
            }
        }

#endregion Persistence synchronizations

#region Pose transformation math

        /// <summary>
        /// Collapse a list of weighted poses into a single equivalent pose.
        /// </summary>
        /// <param name="poses">The poses to average.</param>
        /// <returns>The weighted average.</returns>
        /// <remarks>
        /// If the list is empty, returns an identity pose.
        /// </remarks>
        private static Pose WeightedAverage(List<WeightedPose> poses)
        {
            if (poses.Count < 1)
            {
                return Pose.identity;
            }

            // While there are multiple poses to be averaged, average them.
            // Since there was at least one to start with (see early out above),
            // there will be exactly one single pose at the end.
            while (poses.Count > 1)
            {
                // Collapse the last two into an equivalent average one.
                poses[poses.Count - 2] = WeightedAverage(poses[poses.Count - 2], poses[poses.Count - 1]);
                poses.RemoveAt(poses.Count - 1);
            }

            return poses[0].pose;
        }

        /// <summary>
        /// Combine two weighted poses via interpolation into a single equivalent weighted pose.
        /// </summary>
        /// <param name="lhs">Left hand pose</param>
        /// <param name="rhs">Right hand pose</param>
        /// <returns>The equivalent pose.</returns>
        private static WeightedPose WeightedAverage(WeightedPose lhs, WeightedPose rhs)
        {
            float minCombinedWeight = 0.0f;
            if (lhs.weight + rhs.weight <= minCombinedWeight)
            {
                return new WeightedPose()
                {
                    pose = Pose.identity,
                    weight = 0.0f
                };
            }
            float interp = rhs.weight / (lhs.weight + rhs.weight);
#if WLT_NAN_EXTRA_DEBUGGING
            if (float.IsNaN(interp))
            {
                Debug.LogError("Interp NAN");
            }
#endif // WLT_NAN_EXTRA_DEBUGGING

            WeightedPose ret;
            ret.pose.position = lhs.pose.position + interp * (rhs.pose.position - lhs.pose.position);
            ret.pose.rotation = Quaternion.Slerp(lhs.pose.rotation, rhs.pose.rotation, interp);
            ret.pose.rotation = Quaternion.Normalize(ret.pose.rotation);
            ret.weight = lhs.weight + rhs.weight;
#if WLT_NAN_EXTRA_DEBUGGING
            if (float.IsNaN(ret.pose.position.x))
            {
                Debug.LogError("Position NAN");
            }
            if (float.IsNaN(ret.weight))
            {
                Debug.LogError("Weight NAN");
            }
#endif // WLT_NAN_EXTRA_DEBUGGING

            return ret;
        }

        /// <summary>
        /// Compute the PinnedFromLocked pose for the given reference pose.
        /// </summary>
        /// <param name="refPose">The reference pose to evaluate.</param>
        /// <returns>The computed PinnedFromLocked pose.</returns>
        private Pose ComputePinnedFromLocked(ReferencePose refPose)
        {
            Pose pinnedFromLocked = Pose.identity;
            if (IsGlobal)
            {
                /// Here we essentially solve for pose Z, where
                /// refPose.virtualPose == Z * refPose.lockedPose.
                /// More precisely, we solve for PfL in:
                /// AppFromHolder * HolderFromObject = AppFromPinned * PinnedFromLocked * LockedFromObject, or
                /// AfH * HfO = AfP * PfL * LfO
                /// PfA * AfH * HfO * OfL = PfL
                /// refPose.virtualPose == PfA * AfH * HfO, and refPose.LockedPose == LockedFromObject, so it reduces to the above simpler line.
                Pose pinnedFromObject = refPose.virtualPose;
                Pose objectFromLocked = refPose.LockedPose.Inverse();

                pinnedFromLocked = pinnedFromObject.Multiply(objectFromLocked);
            }
            else
            {
                /// The math is slightly different for an alignment manager being applied to a subgraph of the scene.
                /// Here we are essentially solving for pose Z, such that
                /// Z * refPose.virtualPose == FrozenFromLocked * refPose.LockedPose.
                Pose frozenFromVirtual = manager.FrozenFromLocked
                    .Multiply(refPose.LockedPose)
                    .Multiply(refPose.virtualPose.Inverse());

                pinnedFromLocked = frozenFromVirtual.Inverse();
            }

            // mafinc - obviously this could be cached when refPose.LockedPose is set (and changed).
            return pinnedFromLocked;
        }

#endregion Pose transformation math

#region Weight computation

        private readonly Triangulator.ITriangulator triangulator = new Triangulator.SimpleTriangulator();

        private void BuildTriangulation()
        {
            // Seed with for far-out corners
            InitTriangulator();
            if (activePoses.Count > 0)
            {
                Vector3[] positions = new Vector3[activePoses.Count];
                for (int i = 0; i < positions.Length; ++i)
                {
                    positions[i] = activePoses[i].LockedPose.position;
                }
                triangulator.Add(positions);
            }

            OnTriangulationBuilt?.Invoke(this,triangulator);
        }

        private void InitTriangulator()
        {
            triangulator.Clear();
            if (activePoses.Count > 0)
            {
                triangulator.SetBounds(new Vector3(-1000, 0, -1000), new Vector3(1000, 0, 1000));
            }
        }

        private List<WeightedPose> ComputePoseWeights(Vector3 lockedHeadPosition)
        {
            weightedPoses.Clear();

            Triangulator.Interpolant bary = triangulator.Find(lockedHeadPosition);
            if (bary != null)
            {
                for (int i = 0; i < 3; ++i)
                {
                    weightedPoses.Add(new WeightedPose()
                    {
                        pose = ComputePinnedFromLocked(activePoses[bary.idx[i]]),
                        weight = bary.weights[i]
                    });
                }
            }
            else
            {
                Debug.Assert(activePoses.Count == 0, "Failed to find an interpolant even though there are pins active.");
            }
            return weightedPoses;
        }
#endregion Weight computation
    }
}