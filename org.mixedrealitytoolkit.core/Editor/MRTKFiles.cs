// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MixedReality.Toolkit.Editor
{
    public class MRTKFiles
    {
        private const string GeneratedName = "MRTK.Generated";
        private const string GeneratedSentinelFileName = GeneratedName + ".sentinel";

        private static readonly string DefaultGeneratedFolderPath = Path.Combine("Assets", GeneratedName);
        private static readonly string DefaultSentinelFilePath = Path.Combine(DefaultGeneratedFolderPath, GeneratedSentinelFileName);
        private static string generatedFolderPath = string.Empty;

        public static string GeneratedFolderPath
        {
            get
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
        }

        private class AssetPostprocessor : UnityEditor.AssetPostprocessor
        {
            public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                foreach (string asset in deletedAssets.Concat(movedFromAssetPaths))
                {
                    if (Path.GetFileName(asset) == GeneratedSentinelFileName && Path.GetDirectoryName(asset) == generatedFolderPath)
                    {
                        generatedFolderPath = string.Empty;
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
                            Debug.Log($"Found MRTK.Generated at {newPath}.");
                            generatedFolderPath = newPath;
                        }
                    }
                }
            }
        }
    }
}
