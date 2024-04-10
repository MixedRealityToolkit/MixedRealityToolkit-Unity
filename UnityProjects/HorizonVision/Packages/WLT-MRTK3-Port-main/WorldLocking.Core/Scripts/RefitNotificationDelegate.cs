// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// Delegate type for notification of refit operations.
    /// </summary>
    /// <remarks>
    /// In the case of a merge operation, absorbedIds will contain all and only the ids of the fragments
    /// that were merged into mergedId, but not including mergedId.
    /// In the case of a refreeze operation, absorbedIds will also contain mergedId.
    /// </remarks>
    /// <param name="mergedId">The fragment id of the target merged fragment.</param>
    /// <param name="absorbedIds">Fragment ids of all affected fragments.</param>
    public delegate void RefitNotificationDelegate(FragmentId mergedId, FragmentId[] absorbedIds);

}