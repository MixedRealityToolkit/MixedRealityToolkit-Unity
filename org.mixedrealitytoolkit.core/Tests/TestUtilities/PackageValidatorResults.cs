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

        private const string directoryPath = "Library\\ASValidationSuiteResults";
        private const string txtExtension = ".txt";
        private const string directoryLevelUp = "..";
        private const string validationResults = "VALIDATION RESULTS:";

        /// <summary>
        /// The "failed" prefix in the Asset Store Validation log.
        /// </summary>
        /// <remarks>
        /// The log uses Unix newlines, `\n`, so we can't use `Environment.NewLine` here.
        /// </remarks>
        private const string failedPrefix = "\nFailed - ";

        /// <summary>
        /// The "succeeded" prefix in the Asset Store Validation log.
        /// </summary>
        /// <remarks>
        /// The log uses Unix newlines, `\n`, so we can't use `Environment.NewLine` here.
        /// </remarks>
        private const string succeededPrefix = "\nSucceeded - ";

        /// <summary>
        /// The "not run" prefix in the Asset Store Validation log.
        /// </summary>
        /// <remarks>
        /// The log uses Unix newlines, `\n`, so we can't use `Environment.NewLine` here.
        /// </remarks>
        private const string notRunPrefix = "\nNotRun - ";

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
            fileName = packageId + txtExtension;
        }

        /// <summary>
        /// Load the results from the package validation log.
        /// </summary>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the package's log file is not found.
        /// </exception>
        public void LoadResults()
        {
            string filePath = Path.Combine(Application.dataPath, directoryLevelUp, directoryPath, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            string resultsContent = File.ReadAllText(filePath);
            var startIndex = resultsContent.IndexOf(validationResults);
            if (startIndex == -1)
            {
                throw new Exception($"File does not contain '{validationResults}'");
            }

            // Skip the validation results line
            startIndex += validationResults.Length;

            // Find the first status message.
            int failedIndex = resultsContent.IndexOf(failedPrefix, startIndex);
            int succeededIndex = resultsContent.IndexOf(succeededPrefix, startIndex);
            int notRunIndex = resultsContent.IndexOf(notRunPrefix, startIndex);
            int currentIndex = MinIndex(failedIndex, succeededIndex, notRunIndex);

            while (currentIndex < resultsContent.Length)
            {
                // Find the next prefix to get the end of the current message
                int nextFailedIndex = resultsContent.IndexOf(failedPrefix, currentIndex + 1);
                int nextSucceededIndex = resultsContent.IndexOf(succeededPrefix, currentIndex + 1);
                int nextNotRunIndex = resultsContent.IndexOf(notRunPrefix, currentIndex + 1);
                int nextIndex = MinIndex(nextFailedIndex, nextSucceededIndex, nextNotRunIndex);

                // If no more prefixes are found, set nextIndex to the end of the string.
                if (nextIndex == int.MaxValue)
                {
                    nextIndex = resultsContent.Length;
                }

                string currentMessage = resultsContent.Substring(currentIndex, nextIndex - currentIndex).Trim();

                if (currentIndex == failedIndex)
                {
                    if (!IgnoreError(currentMessage))
                    {
                        FailedMessages.Add(currentMessage);
                    }
                    else
                    {
                        NotRunMessages.Add(currentMessage);
                    }
                }
                else if (currentIndex == succeededIndex)
                {
                    SucceededMessages.Add(currentMessage);
                }
                else if (currentIndex == notRunIndex)
                {
                    NotRunMessages.Add(currentMessage);
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
        /// Return the minimum index of the specified indices from a validation log.
        /// </summary>
        /// <returns>
        /// Returns the minimum index in a validation log, or int.MaxValue if there was no validate indice.
        /// </returns>
        private static int MinIndex(int failedIndex, int succeededIndex, int notRunIndex)
        {
            return Math.Min(failedIndex != -1 ? failedIndex : int.MaxValue,
                Math.Min(succeededIndex != -1 ? succeededIndex : int.MaxValue,
                    notRunIndex != -1 ? notRunIndex : int.MaxValue));
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
