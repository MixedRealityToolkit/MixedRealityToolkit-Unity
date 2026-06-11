// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using NUnit.Framework;
using System;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.Core.Tests.EditMode
{
    /// <summary>
    /// Unit tests for SystemType.
    /// These run outside of PlayMode and do not require Unity engine initialization.
    /// </summary>
    public class SystemTypeTests
    {
        [Test]
        public void GetReference_FromType_ReturnsValidString()
        {
            string reference = SystemType.GetReference(typeof(Vector3));

            Assert.IsFalse(string.IsNullOrEmpty(reference));
            Assert.IsTrue(reference.Contains("UnityEngine.Vector3"));
            Assert.IsTrue(reference.Contains("UnityEngine.CoreModule"));
        }

        [Test]
        public void GetReference_NullOrEmpty_ReturnsEmptyString()
        {
            Assert.AreEqual(string.Empty, SystemType.GetReference((Type)null));
            Assert.AreEqual(string.Empty, SystemType.GetReference((string)null));
            Assert.AreEqual(string.Empty, SystemType.GetReference(string.Empty));
        }

        [Test]
        public void Constructors_SetPropertiesCorrectly()
        {
            Type targetType = typeof(int);

            // Initialize from Type
            SystemType fromType = new SystemType(targetType);
            Assert.AreEqual(targetType, fromType.Type);
            Assert.AreEqual(SystemType.GetReference(targetType), (string)fromType);

            // Initialize from AssemblyQualifiedName
            SystemType fromString = new SystemType(targetType.AssemblyQualifiedName);
            Assert.AreEqual(targetType, fromString.Type);
        }

        [Test]
        public void Constructor_AbstractType_SetsTypeToNull()
        {
            // SystemType is intentionally designed to nullify abstract types when initialized via string
            SystemType fromString = new SystemType(typeof(Array).AssemblyQualifiedName);
            Assert.IsNull(fromString.Type, "SystemType should not allow abstract types when initialized from an assembly string.");
        }

        [Test]
        public void InvalidTypeAssignment_LogsError_ButSetsType()
        {
            SystemType sysType = new SystemType(typeof(int));

            // Enums violate the ValidConstraint. SystemType logs an error but still completes the assignment.
            LogAssert.Expect(LogType.Error, $"'{typeof(DayOfWeek).FullName}' is not a valid class or struct type.");

            sysType.Type = typeof(DayOfWeek);

            Assert.AreEqual(typeof(DayOfWeek), sysType.Type);
        }

        [Test]
        public void ImplicitConversions_WorkCorrectly()
        {
            Type originalType = typeof(string);

            // Type -> SystemType
            SystemType sysType = originalType;
            Assert.IsNotNull(sysType);

            // SystemType -> Type
            Type convertedType = sysType;
            Assert.AreEqual(originalType, convertedType);

            // SystemType -> string
            string reference = sysType;
            Assert.AreEqual(SystemType.GetReference(originalType), reference);
        }

        [Test]
        public void Equality_MatchesSameTypes()
        {
            SystemType type1 = new SystemType(typeof(float));
            SystemType type2 = new SystemType(typeof(float));
            SystemType type3 = new SystemType(typeof(double));

            Assert.IsTrue(type1.Equals(type2));
            Assert.IsFalse(type1.Equals(type3));
            Assert.IsFalse(type1.Equals(null));

            // HashCodes should match for identical references
            Assert.AreEqual(type1.GetHashCode(), type2.GetHashCode());
        }

        [Test]
        public void Serialization_RestoresType()
        {
            var sysType = new SystemType(typeof(int));
            ISerializationCallbackReceiver receiver = sysType;

            // SystemType uses OnAfterDeserialize to re-establish the `type` mapping from the string `reference`
            receiver.OnAfterDeserialize();

            Assert.AreEqual(typeof(int), sysType.Type);
        }
    }
}
