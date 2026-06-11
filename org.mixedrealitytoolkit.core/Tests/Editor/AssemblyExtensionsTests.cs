// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MixedReality.Toolkit.Core.Tests.EditMode
{
    /// <summary>
    /// Unit tests for AssemblyExtensions.
    /// These run outside of PlayMode and do not require Unity engine initialization.
    /// </summary>
    public class AssemblyExtensionsTests
    {
        [Test]
        public void GetLoadableTypes_ReturnsTypes_ForValidAssembly()
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            IEnumerable<Type> types = currentAssembly.GetLoadableTypes();

            Assert.IsNotNull(types, "GetLoadableTypes should never return null.");
            Assert.Greater(types.Count(), 0, "GetLoadableTypes should return the types within the executing assembly.");
            Assert.IsTrue(types.Contains(typeof(AssemblyExtensionsTests)), "GetLoadableTypes failed to return known loadable types.");
        }

        [Test]
        public void GetLoadableTypes_HandlesNullAssembly_Safely()
        {
            Assembly nullAssembly = null;
            Assert.Throws<ArgumentNullException>(() => nullAssembly.GetLoadableTypes());
        }
    }
}
