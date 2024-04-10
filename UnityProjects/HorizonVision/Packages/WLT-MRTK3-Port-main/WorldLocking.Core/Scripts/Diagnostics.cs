// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Writer of diagnostics for postmortem analysis.
    /// </summary>
    [System.Serializable]
    public class Diagnostics: IDisposable
    {

        private SharedDiagnosticsSettings shared = new SharedDiagnosticsSettings();

        /// <summary>
        /// Provide access to the shared configuration. Get and set of settings
        /// is through <see cref="WorldLockingManager.DiagnosticsSettings"/>
        /// </summary>
        public SharedDiagnosticsSettings SharedSettings
        {
            get { return shared; }
            set { shared = value; }
        }

        #region Internal properties as shortcuts
        private bool Enabled => shared.settings.Enabled;
        private string StorageSubdirectory => shared.settings.StorageSubdirectory;
        private string StorageFileTemplate => shared.settings.StorageFileTemplate;
        private int MaxKilobytesPerFile => shared.settings.MaxKilobytesPerFile;
        private int MaxNumberOfFiles => shared.settings.MaxNumberOfFiles;
        #endregion Internal properties as shortcuts

        private IPlugin plugin;

        protected IPluginSerializer serializer;

        /// <summary>
        /// One record (chunk) of data.
        /// </summary>
        protected struct Record
        {
            /// <summary>
            /// If true, finish current 
            /// </summary>
            public bool StartNextFile;

            public DateTime Time;
            public List<byte[]> Data;
        }

        protected Task writeTask;
        protected BlockingCollection<Record> writeQueue = new BlockingCollection<Record>();

        protected string persistentDataPath;

        /// <summary>
        /// Get set up.
        /// </summary>
        /// <param name="plugin">The plugin providing necessary resources</param>
        public void Start(IPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Use a time slice for processing any accrued data.
        /// </summary>
        public void Update()
        {
            if (!Enabled) return;

            if (serializer == null)
            {
                serializer = plugin.CreateSerializer(Time.time);
                serializer.IncludePersistent = true;
                serializer.IncludeTransient = true;
            }

            if (writeTask == null)
            {
                // query path in main thread because Unity disallows it in background threads
                persistentDataPath = Application.persistentDataPath;
                writeTask = Task.Run((Action)processWriteQueue);
            }

            serializer.Time = Time.time;
            serializer.GatherRecord();

            var record = new Record
            {
                Time = DateTime.UtcNow,
                Data = serializer.ReadRecordData(),
                StartNextFile = (serializer.BytesSerialized > MaxKilobytesPerFile * 1024),
            };

            if (record.StartNextFile)
            {
                serializer.Restart();
                serializer.GatherRecord();

                record.Data = serializer.ReadRecordData();
            }

            writeQueue.Add(record);
        }

        /// <summary>
        /// Free all, after possible wait for finish.
        /// </summary>
        public void Dispose()
        {
            if (writeTask != null)
            { 
                writeQueue.CompleteAdding();

                if (!writeTask.Wait(5000))
                {
                    Debug.LogWarningFormat("Timeout waiting for background diagnostics writer ({0} records buffered)", writeQueue.Count);
                }
            }

            writeQueue.Dispose();
            serializer?.Dispose();
        }


        protected void processWriteQueue()
        {
            Stream writeStream = null;

            while (true)
            {
                Record record;

                try
                {
                    record = writeQueue.Take();
                }
                catch (InvalidOperationException)
                {
                    break;  // CompleteAdding() was called and queue is empty
                }

                try
                {
                    if (writeStream == null || record.StartNextFile)
                    {
                        writeStream?.Dispose();
                        writeStream = createNextFile(record.Time);

                        deleteObsoleteFiles();
                    }

                    foreach (byte[] block in record.Data)
                    {
                        writeStream.Write(block, 0, block.Length);
                    }

                    if (writeQueue.Count == 0)
                    {
                        writeStream.Flush();
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogErrorFormat("Background diagnostics writer: {0}", exception);

                    writeStream?.Dispose();
                    writeStream = null;
                }
            }

            writeStream?.Dispose();
        }


        protected Stream createNextFile(DateTime time)
        {
            string machineName = null;

            try
            {
                // System.Environment does not seem to have a field MachineName on ARM.?.
                machineName = Environment.GetEnvironmentVariable("COMPUTERNAME") ?? Environment.GetEnvironmentVariable("HOSTNAME");
                machineName = new string(machineName.Where(character => !Path.GetInvalidFileNameChars().Contains(character)).ToArray());
            }
            catch { }

            if (string.IsNullOrEmpty(machineName))
            {
                machineName = "Unknown";
            }

            string storageFileDir = Path.Combine(persistentDataPath, StorageSubdirectory);

            string storageFileName = StorageFileTemplate
                .Replace("[Machine]", machineName)
                .Replace("[Timestamp]", string.Format("{0:yyyyMMdd}-{0:HHmmss}-{0:fff}-UTC", time));

            Directory.CreateDirectory(storageFileDir);

            return File.Open(Path.Combine(storageFileDir, storageFileName), FileMode.Create, FileAccess.Write);
        }


        protected void deleteObsoleteFiles()
        {
            string storageFileDir = Path.Combine(persistentDataPath, StorageSubdirectory);

            string storageFileNamePattern = StorageFileTemplate
                .Replace("[Machine]",   "*")
                .Replace("[Timestamp]", "*");

            int maxNumStorageFiles = MaxNumberOfFiles;

            var prevOldestStorageFilePaths = new HashSet<string>();

            while (true)
            {
                // re-query each iteration and delete only one to play well with concurrent cleanup
                string[] storageFilePaths = Directory.GetFiles(storageFileDir, storageFileNamePattern).Except(prevOldestStorageFilePaths).ToArray();

                if (storageFilePaths.Length <= maxNumStorageFiles)
                {
                    break;
                }

                string oldestStorageFilePath = storageFilePaths.OrderBy(storageFileName => File.GetLastWriteTime(storageFileName)).First();

                // attempt to delete each file once at most
                prevOldestStorageFilePaths.Add(oldestStorageFilePath);

                try
                {
                    // can fail due to concurrent deletion, concurrent read lock, ...
                    File.Delete(oldestStorageFilePath);
                }
                catch { }
            }
        }
    }
}
