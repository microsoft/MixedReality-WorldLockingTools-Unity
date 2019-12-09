// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.WorldLocking.Core;

namespace Microsoft.MixedReality.WorldLocking.Tools
{

    /// <summary>
    /// Component to handle frozen world adjustments for dynamic (moving) objects.
    /// </summary>
    /// <remarks>
    /// For stationary objects, use <see cref="AdjusterFixed"/>
    /// </remarks>
    public class AdjusterMoving : AdjusterFixed
    {
        private void Update()
        {
            if (AttachmentPoint != null)
            {
                Manager.MoveAttachmentPoint(AttachmentPoint, gameObject.transform.position);
            }
        }
    }
}