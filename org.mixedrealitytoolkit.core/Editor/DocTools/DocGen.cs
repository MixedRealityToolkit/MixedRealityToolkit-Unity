// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace MixedReality.Toolkit.Editor
{
    /// <summary>
    /// Generates XML documentation and compiled binaries for MRTK packages,
    /// and validates that all publicly visible APIs have XML documentation (CS1591).
    /// </summary>
    public static class DocGen
    {
        private const string DefaultOutputFolder = "artifacts/docs";

        /// <summary>
        /// Menu item to generate XML documentation and validate CS1591 across all production MRTK assemblies.
        /// </summary>
        [MenuItem("Mixed Reality/MRTK3/Utilities/Documentation/Generate and Validate Docs")]
        public static void GenerateDocsMenu()
        {
            GenerateDocsBinaries(outputFolder: DefaultOutputFolder, warnAsError: true, verbose: false);
        }

        /// <summary>
        /// Menu item to generate XML documentation without failing on missing doc warnings.
        /// </summary>
        [MenuItem("Mixed Reality/MRTK3/Utilities/Documentation/Generate Docs (No WarnAsError)")]
        public static void GenerateDocsNoWarnAsErrorMenu()
        {
            GenerateDocsBinaries(outputFolder: DefaultOutputFolder, warnAsError: false, verbose: false);
        }

        /// <summary>
        /// Entry point for batch mode execution from command line or CI.
        /// </summary>
        /// <remarks>
        /// Command line arguments supported:
        /// <list type="bullet">
        /// <item><description><c>-docOutput:&lt;path&gt;</c>: Output folder for generated XML and DLL files.</description></item>
        /// <item><description><c>-docFilter:&lt;substring&gt;</c>: Filter to compile only matching assemblies (e.g. "Input").</description></item>
        /// <item><description><c>-docWarnAsError</c>: Fail build if CS1591 doc warnings are encountered (default: true).</description></item>
        /// <item><description><c>-docIncludeEditor</c>: Include Editor assemblies in addition to runtime assemblies.</description></item>
        /// <item><description><c>-docVerbose</c>: Print verbose compiler diagnostic messages.</description></item>
        /// </list>
        /// </remarks>
        public static void GenerateDocsBinariesBatchMode()
        {
            string outputFolder = DefaultOutputFolder;
            string filter = null;
            bool warnAsError = true;
            bool includeEditor = false;
            bool verbose = false;

            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                if (arg.StartsWith("-docOutput:", StringComparison.OrdinalIgnoreCase))
                {
                    outputFolder = arg["-docOutput:".Length..];
                }
                else if (arg.StartsWith("-docFilter:", StringComparison.OrdinalIgnoreCase))
                {
                    filter = arg["-docFilter:".Length..];
                }
                else if (arg.Equals("-docWarnAsError", StringComparison.OrdinalIgnoreCase) ||
                         arg.Equals("-docWarnAsError:true", StringComparison.OrdinalIgnoreCase))
                {
                    warnAsError = true;
                }
                else if (arg.Equals("-docWarnAsError:false", StringComparison.OrdinalIgnoreCase))
                {
                    warnAsError = false;
                }
                else if (arg.Equals("-docIncludeEditor", StringComparison.OrdinalIgnoreCase))
                {
                    includeEditor = true;
                }
                else if (arg.Equals("-docVerbose", StringComparison.OrdinalIgnoreCase))
                {
                    verbose = true;
                }
            }

            bool success = GenerateDocsBinaries(outputFolder, warnAsError, verbose, filter, includeEditor);

            if (Application.isBatchMode)
            {
                // Ensure logs flush
                Thread.Sleep(500);
                EditorApplication.Exit(success ? 0 : 1);
            }
        }

        /// <summary>
        /// Compiles assemblies with the /doc flag enabled and optionally enforces CS1591 as an error.
        /// </summary>
        /// <param name="outputFolder">Folder to write compiled DLLs and XML doc files to.</param>
        /// <param name="warnAsError">Whether to treat CS1591 missing XML comment warnings as errors.</param>
        /// <param name="verbose">Whether to log all compiler warnings and verbose progress.</param>
        /// <param name="assemblyFilter">Optional substring filter for assembly names.</param>
        /// <param name="includeEditor">Whether to include editor assemblies.</param>
        /// <returns><see langword="true"/> if all target assemblies compiled successfully without documentation errors, otherwise <see langword="false"/>.</returns>
        public static bool GenerateDocsBinaries(
            string outputFolder = DefaultOutputFolder,
            bool warnAsError = true,
            bool verbose = false,
            string assemblyFilter = null,
            bool includeEditor = false)
        {
            Directory.CreateDirectory(outputFolder);

            Assembly[] allAssemblies = CompilationPipeline.GetAssemblies();
            var targetAssemblies = allAssemblies.Where(a =>
                a.name.StartsWith("MixedReality.Toolkit") &&
                !a.name.EndsWith(".Tests") &&
                !a.name.EndsWith("Tests") &&
                !a.name.Contains(".Tests.") &&
                !a.name.EndsWith("TestUtilities") &&
                (includeEditor || !a.flags.HasFlag(AssemblyFlags.EditorAssembly))
            );

            if (!string.IsNullOrEmpty(assemblyFilter))
            {
                targetAssemblies = targetAssemblies.Where(a =>
                    a.name.IndexOf(assemblyFilter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            List<Assembly> assemblyList = targetAssemblies.OrderBy(a => a.name).ToList();

            if (assemblyList.Count == 0)
            {
                Debug.LogWarning($"[DocGen] No assemblies matched criteria (Filter: '{assemblyFilter ?? "None"}', IncludeEditor: {includeEditor})");
                return true;
            }

            Debug.Log($"[DocGen] Starting documentation generation for {assemblyList.Count} assemblies into '{outputFolder}' (WarnAsError: {warnAsError})...");

            int successCount = 0;
            int failureCount = 0;
            int totalCs1591 = 0;

            foreach (Assembly assembly in assemblyList)
            {
                string dllFileName = Path.GetFileName(assembly.outputPath);
                string outputDllPath = Path.Combine(outputFolder, dllFileName);
                string xmlFileName = Path.GetFileNameWithoutExtension(dllFileName) + ".xml";
                string outputXmlPath = Path.Combine(outputFolder, xmlFileName);

                AssemblyBuilder builder = new AssemblyBuilder(outputDllPath, assembly.sourceFiles)
                {
                    additionalDefines = assembly.defines,
                    referencesOptions = ReferencesOptions.UseEngineModules
                };

                if (assembly.flags.HasFlag(AssemblyFlags.EditorAssembly))
                {
                    builder.flags = AssemblyBuilderFlags.EditorAssembly;
                }

                builder.additionalReferences = assembly.allReferences;
                builder.compilerOptions = assembly.compilerOptions;
                builder.excludeReferences = builder.defaultReferences.Except(assembly.allReferences).ToArray();

                // Add /doc compiler argument
                var compilerArgs = new List<string>(builder.compilerOptions.AdditionalCompilerArguments ?? Array.Empty<string>())
                {
                    $"/doc:{outputXmlPath}"
                };
                if (warnAsError)
                {
                    compilerArgs.Add("/warnaserror:1591");
                }
                builder.compilerOptions.AdditionalCompilerArguments = compilerArgs.ToArray();

                List<CompilerMessage> messages = new List<CompilerMessage>();
                builder.buildFinished += (path, compilerMessages) =>
                {
                    if (compilerMessages != null)
                    {
                        messages.AddRange(compilerMessages);
                    }
                };

                Debug.Log($"[DocGen] Compiling {assembly.name} -> {xmlFileName}...");

                if (!builder.Build())
                {
                    Debug.LogError($"[DocGen] Failed to initiate build for {assembly.name}!");
                    failureCount++;
                    continue;
                }

                int timeoutMs = 60000;
                int elapsed = 0;
                while (builder.status != AssemblyBuilderStatus.Finished && elapsed < timeoutMs)
                {
                    Thread.Sleep(50);
                    elapsed += 50;
                }

                if (builder.status != AssemblyBuilderStatus.Finished)
                {
                    Debug.LogError($"[DocGen] Build timed out for assembly {assembly.name} after {timeoutMs / 1000}s!");
                    failureCount++;
                    continue;
                }

                int errorCount = 0;
                int assemblyCs1591 = 0;

                foreach (CompilerMessage msg in messages)
                {
                    bool isCs1591 = msg.message.Contains("CS1591");
                    if (isCs1591)
                    {
                        assemblyCs1591++;
                        totalCs1591++;
                    }

                    if (msg.type == CompilerMessageType.Error)
                    {
                        errorCount++;
                        Debug.LogError($"[DocGen] [{assembly.name}] {msg.file}({msg.line},{msg.column}): {msg.message}");
                    }
                    else if (msg.type == CompilerMessageType.Warning)
                    {
                        if (verbose || isCs1591)
                        {
                            Debug.LogWarning($"[DocGen] [{assembly.name}] {msg.file}({msg.line},{msg.column}): {msg.message}");
                        }
                    }
                }

                if (errorCount > 0 || (warnAsError && assemblyCs1591 > 0))
                {
                    failureCount++;
                    Debug.LogError($"[DocGen] {assembly.name} FAILED: {errorCount} errors, {assemblyCs1591} missing XML doc comments (CS1591).");
                }
                else
                {
                    successCount++;
                    Debug.Log($"[DocGen] {assembly.name} PASSED. (XML: {outputXmlPath})");
                }
            }

            Debug.Log($"[DocGen] ========================================================");
            Debug.Log($"[DocGen] Documentation Build Summary:");
            Debug.Log($"[DocGen] Total assemblies: {assemblyList.Count}, Succeeded: {successCount}, Failed: {failureCount}");
            Debug.Log($"[DocGen] Total CS1591 missing doc warnings: {totalCs1591}");
            Debug.Log($"[DocGen] ========================================================");

            bool success = failureCount == 0;

            if (!Application.isBatchMode)
            {
                string title = success ? "Doc Generation Succeeded" : "Doc Generation Failed";
                string message = $"Assemblies: {assemblyList.Count}\nSucceeded: {successCount}\nFailed: {failureCount}\nCS1591 Warnings: {totalCs1591}\n\nOutput folder: {outputFolder}";
                EditorUtility.DisplayDialog(title, message, "OK");
            }

            return success;
        }
    }
}
