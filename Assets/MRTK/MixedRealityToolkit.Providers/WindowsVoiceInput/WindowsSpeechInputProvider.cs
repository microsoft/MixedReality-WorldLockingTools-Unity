﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
using System;
using UnityEngine.Windows.Speech;
using UInput = UnityEngine.Input;
#endif // UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN

namespace Microsoft.MixedReality.Toolkit.Windows.Input
{
    [MixedRealityDataProvider(
        typeof(IMixedRealityInputSystem),
        SupportedPlatforms.WindowsStandalone | SupportedPlatforms.WindowsUniversal | SupportedPlatforms.WindowsEditor,
        "Windows Speech Input")]
    [HelpURL("https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Input/Speech.html")]
    public class WindowsSpeechInputProvider : BaseInputDeviceManager, IMixedRealitySpeechSystem, IMixedRealityCapabilityCheck
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="registrar">The <see cref="IMixedRealityServiceRegistrar"/> instance that loaded the service.</param>
        /// <param name="inputSystem">The <see cref="Microsoft.MixedReality.Toolkit.Input.IMixedRealityInputSystem"/> instance that receives data from this provider.</param>
        /// <param name="name">Friendly name of the service.</param>
        /// <param name="priority">Service priority. Used to determine order of instantiation.</param>
        /// <param name="profile">The service's configuration profile.</param>
        public WindowsSpeechInputProvider(
            IMixedRealityServiceRegistrar registrar,
            IMixedRealityInputSystem inputSystem,
            string name = null, 
            uint priority = DefaultPriority, 
            BaseMixedRealityProfile profile = null) : base(registrar, inputSystem, name, priority, profile) { }

        /// <summary>
        /// The keywords to be recognized and optional keyboard shortcuts.
        /// </summary>
        private SpeechCommands[] Commands => InputSystemProfile.SpeechCommandsProfile.SpeechCommands;

        /// <summary>
        /// The Input Source for Windows Speech Input.
        /// </summary>
        public IMixedRealityInputSource InputSource = null;

        /// <summary>
        /// The minimum confidence level for the recognizer to fire an event.
        /// </summary>
        public RecognitionConfidenceLevel RecognitionConfidenceLevel { get; set; }

        /// <inheritdoc />
        public bool IsRecognitionActive =>
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
            keywordRecognizer?.IsRunning ??
#endif
            false;

        #region IMixedRealityCapabilityCheck Implementation

        /// <inheritdoc />
        public bool CheckCapability(MixedRealityCapability capability)
        {
            return (capability == MixedRealityCapability.VoiceCommand);
        }

        #endregion IMixedRealityCapabilityCheck Implementation

        /// <inheritdoc />
        public void StartRecognition()
        {
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
            if (keywordRecognizer != null && !keywordRecognizer.IsRunning)
            {
                keywordRecognizer.Start();
            }
#endif
        }

        /// <inheritdoc />
        public void StopRecognition()
        {
#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
            if (keywordRecognizer != null && keywordRecognizer.IsRunning)
            {
                keywordRecognizer.Stop();
            }
#endif
        }

#if UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
        private KeywordRecognizer keywordRecognizer;

#if UNITY_EDITOR
        /// <inheritdoc />
        public override void Initialize()
        {
            if (!UnityEditor.PlayerSettings.WSA.GetCapability(UnityEditor.PlayerSettings.WSACapability.Microphone))
            {
                UnityEditor.PlayerSettings.WSA.SetCapability(UnityEditor.PlayerSettings.WSACapability.Microphone, true);
            }
        }
#endif // UNITY_EDITOR

        /// <inheritdoc />
        public override void Enable()
        {
            if (!Application.isPlaying || 
                (Commands == null) ||
                (Commands.Length == 0)) { return; }

            if (InputSystemProfile == null) { return; }

            IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;

            InputSource = inputSystem?.RequestNewGenericInputSource("Windows Speech Input Source", sourceType: InputSourceType.Voice);

            var newKeywords = new string[Commands.Length];

            for (int i = 0; i < Commands.Length; i++)
            {
                newKeywords[i] = Commands[i].LocalizedKeyword;
            }

            RecognitionConfidenceLevel = InputSystemProfile.SpeechCommandsProfile.SpeechRecognitionConfidenceLevel;

            if (keywordRecognizer == null)
            {
                try
                {
                    keywordRecognizer = new KeywordRecognizer(newKeywords, (ConfidenceLevel)RecognitionConfidenceLevel);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to start keyword recognizer. Are microphone permissions granted? Exception: {ex}");
                    keywordRecognizer = null;
                    return;
                }

                keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
            }

            if (InputSystemProfile.SpeechCommandsProfile.SpeechRecognizerStartBehavior == AutoStartBehavior.AutoStart)
            {
                StartRecognition();
            }
        }

        /// <inheritdoc />
        public override void Update()
        {
            if (keywordRecognizer != null && keywordRecognizer.IsRunning)
            {
                for (int i = 0; i < Commands.Length; i++)
                {
                    if (UInput.GetKeyDown(Commands[i].KeyCode))
                    {
                        OnPhraseRecognized((ConfidenceLevel)RecognitionConfidenceLevel, TimeSpan.Zero, DateTime.UtcNow, Commands[i].LocalizedKeyword);
                    }
                }
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            if (keywordRecognizer != null)
            {
                StopRecognition();
                keywordRecognizer.OnPhraseRecognized -= KeywordRecognizer_OnPhraseRecognized;

                keywordRecognizer.Dispose();
            }

            keywordRecognizer = null;
        }

#if UNITY_EDITOR
        /// <inheritdoc />
        public override void Destroy()
        {
            if (UnityEditor.PlayerSettings.WSA.GetCapability(UnityEditor.PlayerSettings.WSACapability.Microphone))
            {
                UnityEditor.PlayerSettings.WSA.SetCapability(UnityEditor.PlayerSettings.WSACapability.Microphone, false);
            }
        }
#endif // UNITY_EDITOR

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                keywordRecognizer?.Dispose();
            }
        }

        private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
        {
            OnPhraseRecognized(args.confidence, args.phraseDuration, args.phraseStartTime, args.text);
        }

        private void OnPhraseRecognized(ConfidenceLevel confidence, TimeSpan phraseDuration, DateTime phraseStartTime, string text)
        {
            IMixedRealityInputSystem inputSystem = Service as IMixedRealityInputSystem;

            for (int i = 0; i < Commands?.Length; i++)
            {
                if (Commands[i].LocalizedKeyword == text)
                {
                    inputSystem?.RaiseSpeechCommandRecognized(InputSource, (RecognitionConfidenceLevel)confidence, phraseDuration, phraseStartTime, Commands[i]);
                    break;
                }
            }
        }
#endif // UNITY_STANDALONE_WIN || UNITY_WSA || UNITY_EDITOR_WIN
    }
}
