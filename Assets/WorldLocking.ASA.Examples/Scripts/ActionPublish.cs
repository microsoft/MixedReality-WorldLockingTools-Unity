// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Tools;
using Microsoft.MixedReality.WorldLocking.ASA;

using TMPro;

namespace Microsoft.MixedReality.WorldLocking.ASA.Examples
{
    /// <summary>
    /// Script for implementing button actions. Simple passthrough to perform publisher actions on interaction events.
    /// </summary>
    public class ActionPublish : ActionCube
    {
        public SpacePinBinder spacePinBinder;

        public SpacePinBinderFile spacePinBinderFile;

        public TextMeshPro statusLine;

        private IBinder binder;

        private IBindingOracle bindingOracle;

        [Tooltip("Number seconds to change color at finish.")]
        public float finishSeconds = 1.0f;

        [Tooltip("Color when waiting for binder to be ready.")]
        public Color notReadyColor = new Color(0.2f, 0.2f, 0.2f);

        [Tooltip("Color when waiting on previous command")]
        public Color workingColor = new Color(0.7f, 0.7f, 0.4f);

        private bool goodSetup = false;

        private void Start()
        {
            SetColors(Color.grey);

            goodSetup = CheckSetup();
        }

        private bool CheckSetup()
        {
            binder = spacePinBinder;
            bindingOracle = spacePinBinderFile;
            bool good = true;
            if (binder == null)
            {
                SimpleConsole.AddLine(11, $"Missing Space Pin Binder on {name}");
                good = false;
            }
            if (bindingOracle == null)
            {
                SimpleConsole.AddLine(11, $"Missing binding oracle (Space Pin Binder File) on {name}");
                good = false;
            }
            return good;
        }

        private void Update()
        {
            DisplayStatus();
            if (!goodSetup)
            {
                return;
            }
            if (working)
            {
                SetColors(workingColor);
            }
            else if (binder.IsReady)
            {
                RestoreColors();
            }
            else
            {
                SetColors(notReadyColor);
            }
        }

        private bool working = false;

        private void DisplayStatus()
        {
            if (statusLine != null)
            {
                var status = new ReadinessStatus();
                if (binder != null)
                {
                    status = binder.PublisherStatus;
                }
                statusLine.faceColor = status.readiness == PublisherReadiness.Ready ? Color.white : Color.red;
                string statusText = $"Status: {status.readiness}";
                if (status.readiness == PublisherReadiness.NotReadyToCreate)
                {
                    statusText += $" Create={status.recommendedForCreate.ToString("0.00")}, {status.readyForCreate.ToString("0.00")}";
                }
                string wltStatus = WorldLocking.Core.WorldLockingManager.GetInstance().ErrorStatus;
                if (string.IsNullOrEmpty(wltStatus))
                {
                    wltStatus = "Tracking";
                }
                statusText += $" {wltStatus}";
                statusLine.text = statusText;
            }
        }

        public async void DoTogglePin()
        {
            working = true;
            SetColors(Color.black);
            SimpleConsole.AddLine(8, $"Toggle pins, binder is {(binder == null ? "null" : binder.Name)}");
            var spacePinBinder = binder as SpacePinBinder;
            if (spacePinBinder != null)
            {
                var spacePins = spacePinBinder.SpacePins;
                foreach (var spacePin in spacePins)
                {
                    spacePin.gameObject.SetActive(!spacePin.gameObject.activeSelf);
                    SimpleConsole.AddLine(8, $"Setting spacePin={spacePin.SpacePinId} active={spacePin.gameObject.activeSelf}");
                }
            }
            SimpleConsole.AddLine(8, $"Finished.");

            await ChangeColorForSeconds(finishSeconds, Color.green);
            working = false;
        }

        public async void DoPublish()
        {
            working = true;
            SetColors(Color.black);

            SimpleConsole.AddLine(8, $"Publish cube, binder is {(binder == null ? "null" : binder.Name)}");
            if (binder != null)
            {
                await binder.Publish();

                if (bindingOracle != null)
                {
                    SimpleConsole.AddLine(8, $"Putting to {bindingOracle.Name}");
                    bindingOracle.Put(binder);
                }
            }
            SimpleConsole.AddLine(8, $"Finished.");

            await ChangeColorForSeconds(finishSeconds, Color.green);
            working = false;
        }

        public async void DoDownload()
        {
            working = true;
            SetColors(Color.black);

            SimpleConsole.AddLine(8, $"Download cube, binder is {(binder == null ? "null" : binder.Name)}");
            if (binder != null)
            {
                if (bindingOracle != null)
                {
                    SimpleConsole.AddLine(8, $"Getting from {bindingOracle.Name}");
                    bindingOracle.Get(binder);
                }
                SimpleConsole.AddLine(8, $"Starting download from {binder.Name}");
                await binder.Download();
            }
            SimpleConsole.AddLine(8, $"Finished.");

            await ChangeColorForSeconds(finishSeconds, Color.green);
            working = false;
        }

        public async void DoSearch()
        {
            working = true;
            SetColors(Color.black);

            SimpleConsole.AddLine(8, $"Search cube, binder is {(binder == null ? "null" : binder.Name)}");
            if (binder != null)
            {
                SimpleConsole.AddLine(8, $"Starting search from {binder.Name}");
                await binder.Search();

                // Could check return result for binder.Search() to know whether we've gotten any new bindings.
                // If we have new bindings, we want to update the oracle.
                if (bindingOracle != null)
                {
                    bindingOracle.Put(binder);
                }
            }
            SimpleConsole.AddLine(8, $"Finished.");

            await ChangeColorForSeconds(finishSeconds, Color.green);
            working = false;
        }

        public async void DoPurge()
        {
            working = true;
            SetColors(Color.black);

            SimpleConsole.AddLine(8, $"Purge cube, binder is {(binder == null ? "null" : binder.Name)}");
            if (binder != null)
            {
                SimpleConsole.AddLine(8, $"Starting clear from {binder.Name}");
                await binder.Clear();
                SimpleConsole.AddLine(8, $"Starting purge from {binder.Name}");
                await binder.Purge();

                if (bindingOracle != null)
                {
                    bindingOracle.Put(binder);
                }
            }
            SimpleConsole.AddLine(8, $"Finished.");

            await ChangeColorForSeconds(finishSeconds, Color.green);
            working = false;
        }

        public async void DoClear()
        {
            working = true;
            SetColors(Color.black);

            SimpleConsole.AddLine(8, $"Clear cube, binder is {(binder == null ? "null" : binder.Name)}");

            if (binder != null)
            {
                if (bindingOracle != null)
                {
                    SimpleConsole.AddLine(8, $"Getting from {bindingOracle.Name}");
                    bindingOracle.Get(binder);
                }
                await binder.Clear();
                if (bindingOracle != null)
                {
                    SimpleConsole.AddLine(8, $"Putting empty binder to {bindingOracle.Name}");
                    bindingOracle.Put(binder);
                }
            }
            SimpleConsole.AddLine(8, $"Finished.");

            await ChangeColorForSeconds(finishSeconds, Color.green);
            working = true;
        }

        public async void DoReset()
        {
            working = true;
            SetColors(Color.black);
            SimpleConsole.AddLine(8, $"Reset pins, binder is {(binder == null ? "null" : binder.Name)}");
            var spacePinBinder = binder as SpacePinBinder;
            if (spacePinBinder != null)
            {
                var spacePins = spacePinBinder.SpacePins;
                foreach (var spacePin in spacePins)
                {
                    spacePin.Reset();
                    SimpleConsole.AddLine(8, $"Reseting spacePin={spacePin.SpacePinId}");
                }
            }
            SimpleConsole.AddLine(8, $"Finished.");

            await ChangeColorForSeconds(finishSeconds, Color.green);
            working = false;

        }
    }
}

