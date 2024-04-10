// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Tools
{
    /// <summary>
    /// A simple runtime console to help in debugging on device.
    /// </summary>
    /// <remarks>
    /// The system accepts a verbosity level along with each line of text.
    /// If the system is enabled, 
    ///     If the verbosity level is >= the logVerbosity
    ///         the line is written to the Unity log file via Debug.Log().
    ///     If the verbosity level is >= the screenVerbosity
    ///         the line will be displayed on screen.
    /// If the system is NOT enabled
    ///     If the verbosity level is >= 5
    ///         the line is written to the Unity log file.
    /// To enable the system:
    ///   1) Add a SimpleConsole component to any game object in the scene.
    ///   2) Fill in the SimpleConsole's TextMesh console field with a valid TextMesh.
    /// Ideally, place the console TextMesh where it can be seen.
    /// If enabled, then every frame the last lineCount lines added are displayed on the console TextMesh,
    /// where lineCount is an inspector accessible property below.
    /// Use is thread safe, and from threads besides the main thread.
    /// </remarks>
    public class SimpleConsole : MonoBehaviour
    {
        [Tooltip("The (optional) TextMesh on which to display the output log.")]
        public TextMesh console;

        [Tooltip("The maximum number of lines to display.")]
        [SerializeField]
        private int lineCount = 10;

        /// <summary>
        /// The maximum number of lines to display.
        /// </summary>
        public int LineCount
        {
            get { return lineCount; }
            set { lineCount = value; }
        }

        [Tooltip("The minimum verbosity level message to display on screen.")]
        [SerializeField]
        public int screenVerbosity = 11;

        [Tooltip("The minimum verbosity level message to write to Unity log.")]
        [SerializeField]
        public int logVerbosity = 5;

        [Tooltip("Optional file to write log entries into.")]
        [SerializeField]
        private string logFile = "";

        public string LogFile
        {
            get { return logFile; }
            set
            {
                logFile = value;
                OpenLogWriter();
            }
        }

        /// <summary>
        /// The most recent lineCount messages.
        /// </summary>
        private readonly Queue<string> lines = new Queue<string>();

        /// <summary>
        /// The global instance. May be null.
        /// </summary>
        private static SimpleConsole _consoleInstance = null;

        /// <summary>
        /// The time stamp to prepend to messages.
        /// </summary>
        private string timeStamp = "[000]: ";

        /// <summary>
        /// The current message to display (up to lineCount lines of text).
        /// </summary>
        private string currentStatus = "";

        private StreamWriter logWriter = null;

        /// <summary>
        /// Whether the onscreen component is active.
        /// </summary>
        public static bool Active { get { return _consoleInstance != null && _consoleInstance.gameObject.activeInHierarchy; } }

        /// <summary>
        /// Static helper for adding a line of text for output.
        /// </summary>
        /// <param name="level">The verbosity level of this line.</param>
        /// <param name="line">The text to display and/or write.</param>
        /// <returns>The number of lines added.</returns>
        public static int AddLine(int level, string line)
        {
            if (_consoleInstance != null)
            {
                return _consoleInstance.Add(level, line);
            }
            if (level >= 5)
            {
                Debug.Log(line);
            }
            return 0;
        }

        /// <summary>
        /// Cache this as the instance and open log writer.
        /// </summary>
        private void Awake()
        {
            SetupInstance(this);
        }

        /// <summary>
        /// Cache this as the instance and open log writer.
        /// </summary>
        private void Start()
        {
            SetupInstance(this);
        }

        /// <summary>
        /// Cache the provided instance and open log writer.
        /// </summary>
        /// <param name="simpleConsole"></param>
        private static void SetupInstance(SimpleConsole simpleConsole)
        {
            if (_consoleInstance != simpleConsole)
            {
                if (_consoleInstance != null)
                {
                    Debug.LogWarning($"More than one SimpleConsole in the scene? {simpleConsole.name} overriding {_consoleInstance.name}");
                    _consoleInstance.CloseLogWriter();
                }
                _consoleInstance = simpleConsole;
                if (_consoleInstance != null)
                {
                    _consoleInstance.OpenLogWriter();
                }
            }
        }

        private void CloseLogWriter()
        {
            if (logWriter != null)
            {
                logWriter.Dispose();
                logWriter = null;
            }
        }

        private void OpenLogWriter()
        {
            CloseLogWriter();
            if (!string.IsNullOrEmpty(LogFile))
            {
                string path = Application.persistentDataPath;
                path = Path.Combine(path, LogFile);

                logWriter = new StreamWriter(new FileStream(path, FileMode.Create));
            }
        }

        private void WriteToLogFile(string line)
        {
            if (logWriter != null)
            {
                logWriter.WriteLine(line);
                logWriter.Flush();
            }
        }

        /// <summary>
        /// Update the message if it has changed.
        /// </summary>
        private void Update()
        {
            timeStamp = $"F[{Time.frameCount}]: ";

            if (console != null && currentStatus != console.text)
            {
                console.text = currentStatus;
            }
        }

        /// <summary>
        /// Attempt to add a line of text, subject to verbosity settings.
        /// </summary>
        /// <param name="level">Verbosity level of the message.</param>
        /// <param name="line">The message.</param>
        /// <returns>Number of lines of text active after adding this one.</returns>
        private int Add(int level, string line)
        {
            line = AddTimeStamp(line);
            if (level >= logVerbosity)
            {
                Debug.Log(line);
                WriteToLogFile(line);
            }

            if (level < screenVerbosity)
            {
                return 0;
            }

            if (console == null)
            {
                return 0;
            }

            int numLines = 0;
            lock (lines)
            {
                EnsureSpace(1);

                lines.Enqueue(line);

                UpdateConsole();

                numLines = lines.Count;
            }

            return numLines;
        }

        /// <summary>
        /// Remove enough messages from the queue to fit the number of new lines coming in.
        /// </summary>
        /// <param name="numNewLines">Number of new lines coming in.</param>
        private void EnsureSpace(int numNewLines)
        {
            if (numNewLines > LineCount)
            {
                numNewLines = LineCount;
            }
            while (lines.Count + numNewLines > LineCount)
            {
                lines.Dequeue();
            }
        }

        /// <summary>
        /// Prepend a timestamp onto the message.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private string AddTimeStamp(string line)
        {
            line = timeStamp + line;
            return line;
        }

        /// <summary>
        /// Create a single string including line breaks from the array of lines.
        /// </summary>
        private void UpdateConsole()
        {
            Debug.Assert(console != null);
            bool isFirst = true;
            string text = "";
            foreach (string line in lines)
            {
                if (!isFirst)
                {
                    text += "\n";
                }
                isFirst = false;
                text += line;
            }
            currentStatus = text;
        }
    }
}
