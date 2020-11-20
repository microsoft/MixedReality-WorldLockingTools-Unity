﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Mixed Reality Toolkit controller definition, used to manage a specific controller type
    /// </summary>
    public interface IMixedRealityDictationSystem : IMixedRealityInputDeviceManager
    {
        /// <summary>
        /// Is the system currently listing for dictation input?
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Turns on the dictation recognizer and begins recording audio from the default microphone.
        /// </summary>
        /// <param name="listener">GameObject listening for the dictation input.</param>
        /// <param name="initialSilenceTimeout">The time length in seconds before dictation recognizer session ends due to lack of audio input in case there was no audio heard in the current session.</param>
        /// <param name="autoSilenceTimeout">The time length in seconds before dictation recognizer session ends due to lack of audio input.</param>
        /// <param name="recordingTime">Length in seconds for the manager to listen.</param>
        /// <param name="micDeviceName">Optional: The microphone device to listen to.</param>
        void StartRecording(GameObject listener, float initialSilenceTimeout = 5f, float autoSilenceTimeout = 20f, int recordingTime = 10, string micDeviceName = "");

        /// <summary>
        /// Turns on the dictation recognizer and begins recording audio from the default microphone.
        /// </summary>
        /// <param name="listener">GameObject listening for the dictation input.</param>
        /// <param name="initialSilenceTimeout">The time length in seconds before dictation recognizer session ends due to lack of audio input in case there was no audio heard in the current session.</param>
        /// <param name="autoSilenceTimeout">The time length in seconds before dictation recognizer session ends due to lack of audio input.</param>
        /// <param name="recordingTime">Length in seconds for the manager to listen.</param>
        /// <param name="micDeviceName">Optional: The microphone device to listen to.</param>
        Task StartRecordingAsync(GameObject listener, float initialSilenceTimeout = 5f, float autoSilenceTimeout = 20f, int recordingTime = 10, string micDeviceName = "");

        /// <summary>
        /// Ends the recording session.
        /// </summary>
        void StopRecording();

        /// <summary>
        /// Ends the recording session.
        /// </summary>
        /// <returns><see href="https://docs.unity3d.com/ScriptReference/AudioClip.html">AudioClip</see> of the last recording session.</returns>
        Task<AudioClip> StopRecordingAsync();

        /// <summary>
        /// Get the audio clip associated with the current session.
        /// </summary>	 	 
        AudioClip AudioClip { get; }
    }
}
