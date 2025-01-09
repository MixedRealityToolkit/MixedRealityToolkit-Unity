// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MixedReality.Toolkit.Editor
{
    /// <summary>
    /// Provides helper methods for accessing MRTK-defined files and folders.
    /// </summary>
    public class MRTKFiles
    {
        private const string GeneratedName = "MRTK.Generated";
        private const string GeneratedSentinelFileName = GeneratedName + ".sentinel";

        private static readonly string DefaultGeneratedFolderPath = Path.Combine("Assets", GeneratedName);
        private static readonly string DefaultSentinelFilePath = Path.Combine(DefaultGeneratedFolderPath, GeneratedSentinelFileName);
        private static string generatedFolderPath = string.Empty;

        /// <summary>
        /// Finds the current MRTK.Generated folder based on the sentinel file. If a sentinel file is not found,
        /// a new MRTK.Generated folder and sentinel are created and this new path is returned.
        /// </summary>
        /// <returns>The AssetDatabase-compatible path to the MRTK.Generated folder.</returns>
        public static string GetOrCreateGeneratedFolderPath()
        {
            if (string.IsNullOrWhiteSpace(generatedFolderPath))
            {
                foreach (string guid in AssetDatabase.FindAssets(GeneratedName))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains(GeneratedSentinelFileName))
                    {
                        generatedFolderPath = Path.GetDirectoryName(path);
                        return generatedFolderPath;
                    }
                }

                if (!Directory.Exists(DefaultGeneratedFolderPath))
                {
                    Directory.CreateDirectory(DefaultGeneratedFolderPath);
                }

                if (!File.Exists(DefaultSentinelFilePath))
                {
                    // Make sure we create and dispose/close the filestream just created
                    using FileStream f = File.Create(DefaultSentinelFilePath);
                }
                generatedFolderPath = DefaultGeneratedFolderPath;
            }
            return generatedFolderPath;
        }

        /// <summary>
        /// Checks for an existing MRTK.Generated sentinel file on asset import. Allows the path to be pre-cached before use.
        /// </summary>
        private class AssetPostprocessor : UnityEditor.AssetPostprocessor
        {
            public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (string asset in deletedAssets.Concat(movedFromAssetPaths))
                {
                    if (Path.GetFileName(asset) == GeneratedSentinelFileName && Path.GetDirectoryName(asset) == generatedFolderPath)
                    {
                        generatedFolderPath = string.Empty;
                        break;
                    }
                }

                foreach (string asset in importedAssets.Concat(movedAssets))
                {
                    if (Path.GetFileName(asset) == GeneratedSentinelFileName)
                    {
                        string newPath = Path.GetDirectoryName(asset);
                        if (generatedFolderPath != newPath)
                        {
                            if (generatedFolderPath != string.Empty)
                            {
                                Debug.LogWarning($"Previous MRTK.Generated folder was not unregistered properly: {generatedFolderPath}.\nReplacing with {newPath}");
                            }
                            Debug.Log($"Found MRTK.Generated sentinel at {newPath}.");
                            generatedFolderPath = newPath;
                        }
                        break;
                    }
                }
            }
        }
    }
}
