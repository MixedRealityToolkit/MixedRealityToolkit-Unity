// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591
#if HAS_ASSET_STORE_VALIDATION

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MixedReality.Toolkit.Core.Tests.EditMode
{
    /// <summary>
    /// A class that will load the results of the ASValidationSuite and
    /// provide the number of failed, succeeded, and not run tests.
    /// </summary>
    public class PackageValidatorResults
    {
        /// <summary>
        /// The type of message.
        /// </summary>
        [Flags]
        public enum MessageType
        {
            /// <summary>
            /// A failed message.
            /// </summary>
            Failed = 1,

            /// <summary>
            /// A succeeded message.
            /// </summary>
            Succeeded = 2,

            /// <summary>
            /// A message for a test that was not run.
            /// </summary>
            NotRun = 4
        }

        private string fileName;
        private string directoryPath = "Library\\ASValidationSuiteResults";

        /// <summary>
        /// The number of failed tests.
        /// </summary>
        public int FailedCount { get; private set; }

        /// <summary>
        /// The number of succeeded tests.
        /// </summary>
        public int SucceededCount { get; private set; }

        /// <summary>
        /// The number of tests that were not run.
        /// </summary>
        /// <value></value>
        public int NotRunCount { get; private set; }

        /// <summary>
        /// The list of failed messages.
        /// </summary>
        public List<string> FailedMessages { get; private set; } = new List<string>();

        /// <summary>
        /// The list of succeeded messages.
        /// </summary>
        public List<string> SucceededMessages { get; private set; } = new List<string>();

        /// <summary>
        /// The list of not run messages.
        /// </summary>
        public List<string> NotRunMessages { get; private set; } = new List<string>();

        /// <summary>
        /// Get or set whether to ignore account related errors.
        /// </summary>
        public bool IgnoreAccountErrors { get; set; } = true;


        /// <summary>
        /// Create a new instance of the PackageValidatorResults class.
        /// </summary>
        /// <param name="packageId">
        /// The package ID who's validation results will be loaded.
        /// </param>
        public PackageValidatorResults(string packageId)
        {
            fileName = packageId + ".txt";
        }

        /// <summary>
        /// Load the results from the package validation log.
        /// </summary>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the package's log file is not found.
        /// </exception>
        public void LoadResults()
        {
            string filePath = Path.Combine(Application.dataPath, "..", directoryPath, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            string fileContent = File.ReadAllText(filePath);
            var startIndex = fileContent.IndexOf("VALIDATION RESULTS:");
            if (startIndex == -1) throw new Exception("File does not contain 'VALIDATION RESULTS:'");

            var resultsContent = fileContent.Substring(startIndex + "VALIDATION RESULTS:".Length);

            var failedPrefix = "\nFailed - ";
            var succeededPrefix = "\nSucceeded - ";
            var notRunPrefix = "\nNotRun - ";

            int currentIndex = 0;
            int failedIndex = resultsContent.IndexOf(failedPrefix);
            int succeededIndex = resultsContent.IndexOf(succeededPrefix);
            int notRunIndex = resultsContent.IndexOf(notRunPrefix);

            int nextIndex = Math.Min(failedIndex != -1 ? failedIndex : int.MaxValue,
                Math.Min(succeededIndex != -1 ? succeededIndex : int.MaxValue,
                    notRunIndex != -1 ? notRunIndex : int.MaxValue));

            while (currentIndex < resultsContent.Length && nextIndex != int.MaxValue)
            {
                // Find the next prefix to get the end of the current message
                int nextFailedIndex = resultsContent.IndexOf(failedPrefix, currentIndex + 1);
                int nextSucceededIndex = resultsContent.IndexOf(succeededPrefix, currentIndex + 1);
                int nextNotRunIndex = resultsContent.IndexOf(notRunPrefix, currentIndex + 1);

                nextIndex = Math.Min(nextFailedIndex != -1 ? nextFailedIndex : int.MaxValue,
                    Math.Min(nextSucceededIndex != -1 ? nextSucceededIndex : int.MaxValue,
                        nextNotRunIndex != -1 ? nextNotRunIndex : int.MaxValue));

                // If no more prefixes are found, set nextIndex to the end of the string
                if (nextIndex == int.MaxValue)
                {
                    nextIndex = resultsContent.Length;
                }

                string result = resultsContent.Substring(currentIndex, nextIndex - currentIndex).Trim();

                if (currentIndex == failedIndex)
                {
                    if (!IgnoreError(result))
                    {
                        FailedMessages.Add(result);
                    }
                    else
                    {
                        NotRunMessages.Add(result);
                    }
                }
                else if (currentIndex == succeededIndex)
                {
                    SucceededMessages.Add(result);
                }
                else if (currentIndex == notRunIndex)
                {
                    NotRunMessages.Add(result);
                }

                // Update indices for the next iteration
                currentIndex = nextIndex;
                failedIndex = nextFailedIndex;
                succeededIndex = nextSucceededIndex;
                notRunIndex = nextNotRunIndex;
            }

            FailedCount = FailedMessages.Count;
            SucceededCount = SucceededMessages.Count;
            NotRunCount = NotRunMessages.Count;
        }

        /// <summary>
        /// Get the messages for the specified message types.
        /// </summary>
        public string ToString(MessageType messageType)
        {
            List<string> messages = new List<string>();

            if (messageType.HasFlag(MessageType.Failed))
            {
                messages.AddRange(FailedMessages);
            }

            if (messageType.HasFlag(MessageType.Succeeded))
            {
                messages.AddRange(SucceededMessages);
            }

            if (messageType.HasFlag(MessageType.NotRun))
            {
                messages.AddRange(NotRunMessages);
            }

            return string.Join(Environment.NewLine, messages);
        }

        /// <summary>
        /// Get if the specified message should be ignored.
        /// </summary>
        private bool IgnoreError(string message)
        {
            return IgnoreAccountErrors &&
                (message.Contains("\"Asset Store Terms Accepted Publish\"") ||
                message.Contains("\"User logged in\"") ||
                message.Contains("\"Publisher Account Exists\""));
        }
    }
}
#endif // HAS_ASSET_STORE_VALIDATION
#pragma warning restore CS1591
