// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.UI
{
    /// <summary>
    /// This script provides cursor context for the manipulation handler
    /// </summary>
    [AddComponentMenu("Scripts/MRTK/SDK/CursorContextManipulationHandler")]
    [RequireComponent(typeof(ManipulationHandler))]
    public class CursorContextManipulationHandler : MonoBehaviour
    {
        private ManipulationHandler manipulationHandler;
        private CursorContextInfo contextInfo;
        private int hoverCount = 0;

        private void Start()
        {
            manipulationHandler = GetComponent<ManipulationHandler>();

            contextInfo = gameObject.EnsureComponent<CursorContextInfo>();
            contextInfo.ObjectCenter = transform;

            manipulationHandler.OnHoverEntered.AddListener(ManipulatorHoverEntered);
            manipulationHandler.OnHoverExited.AddListener(ManipulatorHoverExited);
            manipulationHandler.OnManipulationStarted.AddListener(ManipulationStarted);
            manipulationHandler.OnManipulationEnded.AddListener(ManipulationEnded);
        }

        private void ManipulatorHoverEntered(ManipulationEventData manipEvent)
        {
            hoverCount++;
            if (hoverCount >= 2 &&
                manipulationHandler.ManipulationType != ManipulationHandler.HandMovementType.OneHandedOnly)
            {
                contextInfo.CurrentCursorAction = CursorContextInfo.CursorAction.Move;
            }
        }

        private void ManipulatorHoverExited(ManipulationEventData manipEvent)
        {
            hoverCount--;
            if (hoverCount < 2)
            {
                contextInfo.CurrentCursorAction = CursorContextInfo.CursorAction.None;
            }
        }

        private void ManipulationStarted(ManipulationEventData manipEvent)
        {
            contextInfo.CurrentCursorAction = CursorContextInfo.CursorAction.Move;
        }

        private void ManipulationEnded(ManipulationEventData manipEvent)
        {
            contextInfo.CurrentCursorAction = hoverCount < 2 ?
                CursorContextInfo.CursorAction.None :
                CursorContextInfo.CursorAction.Move;
        }
    }
}
