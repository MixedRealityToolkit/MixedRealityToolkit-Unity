// Copyright (c) Mixed Reality Toolkit Contributors
// Licensed under the BSD 3-Clause

// Disable "missing XML comment" warning for tests. While nice to have, this documentation is not required.
#pragma warning disable CS1591

using System.Collections;
using System.Linq;
using MixedReality.Toolkit.Input.Tests;
using MixedReality.Toolkit.UX.Experimental;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace MixedReality.Toolkit.UX.Runtime.Tests
{
    /// <summary>
    /// Tests for the VirtualizedScrollRectList.
    /// </summary>
    public class VirtualizedScrollRectListTests: BaseRuntimeInputTests
    {
        private const string virtualizedScrollRectListTestPrefab = "Packages/org.mixedrealitytoolkit.uxcore/Tests/Runtime/Prefabs/VirtualizedScrollRectListTest.prefab";
        private string[] wordSet1 = { "one", "two", "three", "zebra", "keyboard", "rabbit", "graphite", "ruby", };
        private string[] wordSet2 = { "four", "five", "six", "apple", "mouse", "tortoise", "wool", "car", };

        private VirtualizedScrollRectList virtualizedScrollRectList;

        private IEnumerator SetupVirtualizedScrollRectList()
        {
            if (virtualizedScrollRectList != null)
            {
                Object.Destroy(virtualizedScrollRectList);
                yield return null;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(virtualizedScrollRectListTestPrefab);
            GameObject obj = GameObject.Instantiate(prefab);
            virtualizedScrollRectList = obj.GetComponentInChildren<VirtualizedScrollRectList>();

            Assert.IsNotNull(virtualizedScrollRectList, "VirtualizedScrollRectList was not found in spawned prefab.");
        }

        /// <summary>
        /// Callback to set for the OnVisible callback for wordSet1
        /// </summary>
        private void OnVisibleCallbackForSet1(GameObject go, int i)
        {
            go.transform.name = wordSet1[i % wordSet1.Length];
        }

        /// <summary>
        /// Callback to set for the OnVisible callback for wordSet2
        /// </summary>
        private void OnVisibleCallbackForSet2(GameObject go, int i)
        {
            go.transform.name = wordSet2[i % wordSet2.Length];
        }

        [TearDown]
        public void Teardown()
        {
            if (virtualizedScrollRectList != null)
            {
                Object.Destroy(virtualizedScrollRectList);
            }
        }


        [UnityTest]
        public IEnumerator TestVirtualizedScrollRectList_ResetLayout()
        {
            yield return SetupVirtualizedScrollRectList();
            yield return null; // changes happens in next frame

            virtualizedScrollRectList.OnVisible = OnVisibleCallbackForSet1;
            yield return null; // changes happens in next frame

            GameObject item;
            GameObject refItem = null;

            int i, refI = 0, foundItems = 0;

            for (i = 0; i < wordSet1.Length; i++)
            {
                if(virtualizedScrollRectList.TryGetVisible(i, out item))
                {
                    refItem = item;
                    refI = i;
                    Assert.IsTrue(wordSet1.Contains(item.transform.name), $"Item seen does't belong to the items passed in (set1). Got {item.transform.name} at {i}");
                    foundItems++;
                }
            }
            Assert.IsTrue(foundItems > 0, "Non of the expected items were found in the scollable list (set1).");

            virtualizedScrollRectList.OnVisible = OnVisibleCallbackForSet1;
            yield return null; // changes happens in next frame

            // Setting the same value shouldn't reset the layout.
            // If foundItems > 0, refI and refItem should not be empty/null
            virtualizedScrollRectList.TryGetVisible(refI, out item);
            Assert.IsTrue(item == refItem, "Setting the same value should not trigger ResetLayout");

            virtualizedScrollRectList.ResetLayout();
            yield return null; // changes happens in next frame

            virtualizedScrollRectList.OnVisible = OnVisibleCallbackForSet2;
            yield return null; // changes happens in next frame

            for (i = 0; i < wordSet2.Length; i++)
            {
                if(virtualizedScrollRectList.TryGetVisible(i, out item))
                {
                    Assert.IsTrue(wordSet2.Contains(item.transform.name), $"Item seen does't belong to the items passed in (set2). Got {item.transform.name} at {i}");
                    foundItems++;
                }
            }
            Assert.IsTrue(foundItems > 0, "Non of the expected items were found in the scollable list (set2).");
        }
    }
}
#pragma warning restore CS1591
