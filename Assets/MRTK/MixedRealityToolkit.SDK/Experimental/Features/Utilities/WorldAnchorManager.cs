// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

#if UNITY_WSA
using System;
using System.Collections.Generic;
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
#endif

namespace Microsoft.MixedReality.Toolkit.Experimental.Utilities
{
    /// <summary>
    /// Wrapper around Unity's WorldAnchorStore to simplify usage of persistence operations.
    /// </summary>
    /// <remarks>
    /// This class only functions when built for the WSA platform. It uses APIs that are only present
    /// on that platform.
    /// </remarks>
    public class WorldAnchorManager : MonoBehaviour
    {
        /// <summary>
        /// If non-null, verbose logging messages will be displayed on this TextMesh.
        /// </summary>
        [Tooltip("If non-null, verbose logging messages will be displayed on this TextMesh.")]
        [SerializeField]
        private TextMesh anchorDebugText = null;

        /// <summary>
        /// If non-null, verbose logging messages will be displayed on this TextMesh.
        /// </summary>
        /// <remarks>
        /// Note that ShowDetailedLogs and AnchorDebugText will cause the same set of information
        /// to be displayed.
        /// </remarks>
        public TextMesh AnchorDebugText => anchorDebugText;

        /// <summary>
        /// If true, more verbose logging messages will be written to the console window.
        /// </summary>
        [Tooltip("If true, more verbose logging messages will be written to the console window.")]
        [SerializeField]
        private bool showDetailedLogs = false;

        /// <summary>
        /// If true, more verbose logging messages will be written to the console window.
        /// </summary>
        /// <remarks>
        /// Note that ShowDetailedLogs and AnchorDebugText will cause the same set of information
        /// to be displayed.
        /// </remarks>
        public bool ShowDetailedLogs => showDetailedLogs;

        /// <summary>
        /// Enables anchors to be stored from subsequent game sessions.
        /// </summary>
        [Tooltip("Enables anchors to be stored from subsequent game sessions.")]
        [SerializeField]
        private bool persistentAnchors = false;

        /// <summary>
        /// Enables anchors to be stored from subsequent game sessions.
        /// </summary>
        public bool PersistentAnchors => persistentAnchors;

#if UNITY_WSA
        /// <summary>
        /// The WorldAnchorStore for the current application.
        /// Can be null when the application starts.
        /// </summary>
        public WorldAnchorStore AnchorStore { get; protected set; }

        /// <summary>
        /// To prevent initializing too many anchors at once
        /// and to allow for the WorldAnchorStore to load asynchronously
        /// without callers handling the case where the store isn't loaded yet
        /// we'll setup a queue of anchor attachment operations.
        /// The AnchorAttachmentInfo struct has the data needed to do this.
        /// </summary>
        private struct AnchorAttachmentInfo
        {
            public GameObject AnchoredGameObject { get; set; }
            public string AnchorName { get; set; }
            public AnchorOperation Operation { get; set; }
        }

        /// <summary>
        /// Enumeration defining the types of anchor operations.
        /// </summary>
        private enum AnchorOperation
        {
            /// <summary>
            /// Save anchor to anchor store.  Creates anchor if none exists.
            /// </summary>
            Save,
            /// <summary>
            /// Deletes anchor from anchor store.
            /// </summary>
            Delete
        }

        /// <summary>
        /// The queue for local device anchor operations.
        /// </summary>
        private Queue<AnchorAttachmentInfo> LocalAnchorOperations = new Queue<AnchorAttachmentInfo>();

        /// <summary>
        /// Internal list of anchors and their GameObject references.
        /// </summary>
        private Dictionary<string, GameObject> AnchorGameObjectReferenceList = new Dictionary<string, GameObject>(0);

        #region Unity Methods

        private void Awake()
        {
            AnchorStore = null;
        }

        private void Start()
        {
            WorldAnchorStore.GetAsync(AnchorStoreReady);
        }

        private void Update()
        {
            if (AnchorStore == null)
            {
                return;
            }

            if (LocalAnchorOperations.Count > 0)
            {
                DoAnchorOperation(LocalAnchorOperations.Dequeue());
            }
        }

        #endregion // Unity Methods

        #region Event Callbacks

        /// <summary>
        /// Callback function that contains the WorldAnchorStore object.
        /// </summary>
        /// <param name="anchorStore">The WorldAnchorStore to cache.</param>
        private void AnchorStoreReady(WorldAnchorStore anchorStore)
        {
            AnchorStore = anchorStore;

            if (!persistentAnchors)
            {
                AnchorStore.Clear();
            }
        }

        /// <summary>
        /// Called when tracking changes for a 'cached' anchor.
        /// When an anchor isn't located immediately we subscribe to this event so
        /// we can save the anchor when it is finally located or downloaded.
        /// </summary>
        /// <param name="anchor">The anchor that is reporting a tracking changed event.</param>
        /// <param name="located">Indicates if the anchor is located or not located.</param>
        private void Anchor_OnTrackingChanged(WorldAnchor anchor, bool located)
        {
            if (located && SaveAnchor(anchor))
            {
                if (showDetailedLogs)
                {
                    Debug.LogFormat("[WorldAnchorManager] Successfully updated cached anchor \"{0}\".", anchor.name);
                }

                if (anchorDebugText != null)
                {
                    anchorDebugText.text += string.Format("\nSuccessfully updated cached anchor \"{0}\".", anchor.name);
                }
            }
            else
            {
                if (showDetailedLogs)
                {
                    Debug.LogFormat("[WorldAnchorManager] Failed to locate cached anchor \"{0}\", attempting to acquire anchor again.", anchor.name);
                }

                if (anchorDebugText != null)
                {
                    anchorDebugText.text += string.Format("\nFailed to locate cached anchor \"{0}\", attempting to acquire anchor again.", anchor.name);
                }

                GameObject anchoredObject;
                AnchorGameObjectReferenceList.TryGetValue(anchor.name, out anchoredObject);
                AnchorGameObjectReferenceList.Remove(anchor.name);
                AttachAnchor(anchoredObject, anchor.name);
            }

            anchor.OnTrackingChanged -= Anchor_OnTrackingChanged;
        }

        #endregion // Event Callbacks
#endif

        /// <summary>
        /// Attaches an anchor to the GameObject.
        /// If the anchor store has an anchor with the specified name it will load the anchor,
        /// otherwise a new anchor will be saved under the specified name.
        /// If no anchor name is provided, the name of the anchor will be the same as the GameObject.
        /// </summary>
        /// <param name="gameObjectToAnchor">The GameObject to attach the anchor to.</param>
        /// <param name="anchorName">Name of the anchor. If none provided, the name of the GameObject will be used.</param>
        /// <returns>The name of the newly attached anchor.</returns>
        public string AttachAnchor(GameObject gameObjectToAnchor, string anchorName = null)
        {
#if !UNITY_WSA || UNITY_EDITOR
            Debug.LogWarning("World Anchor Manager does not work for this build. AttachAnchor will not be called.");
            return null;
#else
            if (gameObjectToAnchor == null)
            {
                Debug.LogError("[WorldAnchorManager] Must pass in a valid gameObject");
                return null;
            }

            // This case is unexpected, but just in case.
            if (AnchorStore == null)
            {
                Debug.LogWarning("[WorldAnchorManager] AttachAnchor called before anchor store is ready.");
            }

            anchorName = GenerateAnchorName(gameObjectToAnchor, anchorName);

            LocalAnchorOperations.Enqueue(
                new AnchorAttachmentInfo
                {
                    AnchoredGameObject = gameObjectToAnchor,
                    AnchorName = anchorName,
                    Operation = AnchorOperation.Save
                }
            );

            return anchorName;
#endif
        }

        /// <summary>
        /// Removes the anchor component from the GameObject and deletes the anchor from the anchor store.
        /// </summary>
        /// <param name="gameObjectToUnanchor">The GameObject reference with valid anchor to remove from the anchor store.</param>
        public void RemoveAnchor(GameObject gameObjectToUnanchor)
        {
            if (gameObjectToUnanchor == null)
            {
                Debug.LogError("[WorldAnchorManager] Invalid GameObject! Try removing anchor by name.");
                if (anchorDebugText != null)
                {
                    anchorDebugText.text += "\nInvalid GameObject! Try removing anchor by name.";
                }
                return;
            }

            RemoveAnchor(string.Empty, gameObjectToUnanchor);
        }

        /// <summary>
        /// Removes the anchor from the anchor store, without a GameObject reference.
        /// If a GameObject reference can be found, the anchor component will be removed.
        /// </summary>
        /// <param name="anchorName">The name of the anchor to remove from the anchor store.</param>
        public void RemoveAnchor(string anchorName)
        {
            if (string.IsNullOrEmpty(anchorName))
            {
                Debug.LogErrorFormat("[WorldAnchorManager] Invalid anchor \"{0}\"! Try removing anchor by GameObject.", anchorName);
                if (anchorDebugText != null)
                {
                    anchorDebugText.text += string.Format("\nInvalid anchor \"{0}\"! Try removing anchor by GameObject.", anchorName);
                }
                return;
            }

            RemoveAnchor(anchorName, null);
        }

        /// <summary>
        /// Removes the anchor from the game object and deletes the anchor
        /// from the anchor store.
        /// </summary>
        /// <param name="anchorName">Name of the anchor to remove from the anchor store.</param>
        /// <param name="gameObjectToUnanchor">GameObject to remove the anchor from.</param>
        private void RemoveAnchor(string anchorName, GameObject gameObjectToUnanchor)
        {
            if (string.IsNullOrEmpty(anchorName) && gameObjectToUnanchor == null)
            {
                Debug.LogWarning("Invalid Remove Anchor Request!");
                return;
            }

#if !UNITY_WSA || UNITY_EDITOR
            Debug.LogWarning("World Anchor Manager does not work for this build. RemoveAnchor will not be called.");
#else
            // This case is unexpected, but just in case.
            if (AnchorStore == null)
            {
                Debug.LogWarning("[WorldAnchorManager] RemoveAnchor called before anchor store is ready.");
            }

            LocalAnchorOperations.Enqueue(
                new AnchorAttachmentInfo
                {
                    AnchoredGameObject = gameObjectToUnanchor,
                    AnchorName = anchorName,
                    Operation = AnchorOperation.Delete
                });
#endif
        }

        /// <summary>
        /// Removes all anchors from the scene and deletes them from the anchor store.
        /// </summary>
        public void RemoveAllAnchors()
        {
#if !UNITY_WSA || UNITY_EDITOR
            Debug.LogWarning("World Anchor Manager does not work for this build. RemoveAnchor will not be called.");
#else
            // This case is unexpected, but just in case.
            if (AnchorStore == null)
            {
                Debug.LogWarning("[WorldAnchorManager] RemoveAllAnchors called before anchor store is ready.");
            }

            var anchors = FindObjectsOfType<WorldAnchor>();

            if (anchors == null)
            {
                return;
            }

            for (int i = 0; i < anchors.Length; i++)
            {
                // Let's check to see if there are anchors we weren't accounting for.
                // Maybe they were created without using the WorldAnchorManager.
                if (!AnchorGameObjectReferenceList.ContainsKey(anchors[i].name))
                {
                    Debug.LogWarning("[WorldAnchorManager] Removing an anchor that was created outside of the WorldAnchorManager.  Please use the WorldAnchorManager to create or delete anchors.");
                    if (anchorDebugText != null)
                    {
                        anchorDebugText.text += string.Format("\nRemoving an anchor that was created outside of the WorldAnchorManager.  Please use the WorldAnchorManager to create or delete anchors.");
                    }
                }

                LocalAnchorOperations.Enqueue(new AnchorAttachmentInfo
                {
                    AnchorName = anchors[i].name,
                    AnchoredGameObject = anchors[i].gameObject,
                    Operation = AnchorOperation.Delete
                });
            }
#endif
        }

#if UNITY_WSA
        /// <summary>
        /// Called before creating anchor.  Used to check if import required.
        /// </summary>
        /// <remarks>
        /// Return true from this function if import is required.
        /// </remarks>
        /// <param name="anchorId">Name of the anchor to import.</param>
        /// <param name="objectToAnchor">GameObject to anchor.</param>
        protected virtual bool ImportAnchor(string anchorId, GameObject objectToAnchor)
        {
            return true;
        }

        /// <summary>
        /// Called after creating a new anchor.
        /// </summary>
        /// <param name="anchor">The anchor to export.</param>
        protected virtual void ExportAnchor(WorldAnchor anchor) { }

        /// <summary>
        /// Executes the anchor operations from the localAnchorOperations queue.
        /// </summary>
        /// <param name="anchorAttachmentInfo">Parameters for attaching the anchor.</param>
        private void DoAnchorOperation(AnchorAttachmentInfo anchorAttachmentInfo)
        {
            if (AnchorStore == null)
            {
                Debug.LogError("[WorldAnchorManager] Remove anchor called before anchor store is ready.");
                return;
            }

            string anchorId = anchorAttachmentInfo.AnchorName;
            GameObject anchoredGameObject = anchorAttachmentInfo.AnchoredGameObject;
            switch (anchorAttachmentInfo.Operation)
            {
                case AnchorOperation.Save:
                    DoSaveAnchorOperation(anchorId, anchoredGameObject);
                    break;
                case AnchorOperation.Delete:
                    DoDeleteAnchorOperation(anchorId, anchoredGameObject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Executes an AnchorOperation.Save operation.
        /// </summary>
        private void DoSaveAnchorOperation(string anchorId, GameObject anchoredGameObject)
        {
            if (anchoredGameObject == null)
            {
                Debug.LogError("[WorldAnchorManager] The GameObject referenced must have been destroyed before we got a chance to anchor it.");
                if (anchorDebugText != null)
                {
                    anchorDebugText.text += "\nThe GameObject referenced must have been destroyed before we got a chance to anchor it.";
                }
                return;
            }

            if (string.IsNullOrEmpty(anchorId))
            {
                anchorId = anchoredGameObject.name;
            }

            // Try to load a previously saved world anchor.
            WorldAnchor savedAnchor = AnchorStore.Load(anchorId, anchoredGameObject);

            if (savedAnchor == null)
            {
                // Check if we need to import the anchor.
                if (ImportAnchor(anchorId, anchoredGameObject))
                {
                    if (showDetailedLogs)
                    {
                        Debug.LogFormat("[WorldAnchorManager] Anchor could not be loaded for \"{0}\". Creating a new anchor.", anchoredGameObject.name);
                    }

                    if (anchorDebugText != null)
                    {
                        anchorDebugText.text += string.Format("\nAnchor could not be loaded for \"{0}\". Creating a new anchor.", anchoredGameObject.name);
                    }

                    // Create anchor since one does not exist.
                    CreateAnchor(anchoredGameObject, anchorId);
                }
            }
            else
            {
                savedAnchor.name = anchorId;
                if (showDetailedLogs)
                {
                    Debug.LogFormat("[WorldAnchorManager] Anchor loaded from anchor store and updated for \"{0}\".", anchoredGameObject.name);
                }

                if (anchorDebugText != null)
                {
                    anchorDebugText.text += string.Format("\nAnchor loaded from anchor store and updated for \"{0}\".", anchoredGameObject.name);
                }
            }

            AnchorGameObjectReferenceList.Add(anchorId, anchoredGameObject);
        }

        /// <summary>
        /// Executes an AnchorOperation.Delete operation.
        /// </summary>
        private void DoDeleteAnchorOperation(string anchorId, GameObject anchoredGameObject)
        {
            // If we don't have a GameObject reference, let's try to get the GameObject reference from our dictionary.
            if (!string.IsNullOrEmpty(anchorId) && anchoredGameObject == null)
            {
                AnchorGameObjectReferenceList.TryGetValue(anchorId, out anchoredGameObject);
            }

            if (anchoredGameObject != null)
            {
                var anchor = anchoredGameObject.GetComponent<WorldAnchor>();

                if (anchor != null)
                {
                    anchorId = anchor.name;
                    DestroyImmediate(anchor);
                }
                else
                {
                    Debug.LogErrorFormat("[WorldAnchorManager] Unable remove WorldAnchor from {0}!", anchoredGameObject.name);
                    if (anchorDebugText != null)
                    {
                        anchorDebugText.text += string.Format("\nUnable remove WorldAnchor from {0}!", anchoredGameObject.name);
                    }
                }
            }
            else
            {
                Debug.LogError("[WorldAnchorManager] Unable find a GameObject to remove an anchor from!");
                if (anchorDebugText != null)
                {
                    anchorDebugText.text += "\nUnable find a GameObject to remove an anchor from!";
                }
            }

            if (!string.IsNullOrEmpty(anchorId))
            {
                AnchorGameObjectReferenceList.Remove(anchorId);
                DeleteAnchor(anchorId);
            }
            else
            {
                Debug.LogError("[WorldAnchorManager] Unable find an anchor to delete!");
                if (anchorDebugText != null)
                {
                    anchorDebugText.text += "\nUnable find an anchor to delete!";
                }
            }
        }

        /// <summary>
        /// Creates an anchor, attaches it to the gameObjectToAnchor, and saves the anchor to the anchor store.
        /// </summary>
        /// <param name="gameObjectToAnchor">The GameObject to attach the anchor to.</param>
        /// <param name="anchorName">The name to give to the anchor.</param>
        private void CreateAnchor(GameObject gameObjectToAnchor, string anchorName)
        {
            var anchor = gameObjectToAnchor.EnsureComponent<WorldAnchor>();
            anchor.name = anchorName;

            // Sometimes the anchor is located immediately. In that case it can be saved immediately.
            if (anchor.isLocated)
            {
                SaveAnchor(anchor);
            }
            else
            {
                // Other times we must wait for the tracking system to locate the world.
                anchor.OnTrackingChanged += Anchor_OnTrackingChanged;
            }
        }

        /// <summary>
        /// Saves the anchor to the anchor store.
        /// </summary>
        /// <param name="anchor">Anchor.</param>
        private bool SaveAnchor(WorldAnchor anchor)
        {
            // Save the anchor to persist holograms across sessions.
            if (AnchorStore.Save(anchor.name, anchor))
            {
                if (showDetailedLogs)
                {
                    Debug.LogFormat("[WorldAnchorManager] Successfully saved anchor \"{0}\".", anchor.name);
                }

                if (anchorDebugText != null)
                {
                    anchorDebugText.text += string.Format("\nSuccessfully saved anchor \"{0}\".", anchor.name);
                }

                ExportAnchor(anchor);

                return true;
            }

            Debug.LogErrorFormat("[WorldAnchorManager] Failed to save anchor \"{0}\"!", anchor.name);

            if (anchorDebugText != null)
            {
                anchorDebugText.text += string.Format("\nFailed to save anchor \"{0}\"!", anchor.name);
            }
            return false;
        }

        /// <summary>
        /// Deletes the anchor from the Anchor Store.
        /// </summary>
        /// <param name="anchorId">The anchor id.</param>
        private void DeleteAnchor(string anchorId)
        {
            if (AnchorStore.Delete(anchorId))
            {
                Debug.LogFormat("[WorldAnchorManager] Anchor {0} deleted successfully.", anchorId);
                if (anchorDebugText != null)
                {
                    anchorDebugText.text += string.Format("\nAnchor {0} deleted successfully.", anchorId);
                }
            }
            else
            {
                if (string.IsNullOrEmpty(anchorId))
                {
                    anchorId = "NULL";
                }

                Debug.LogErrorFormat("[WorldAnchorManager] Failed to delete \"{0}\".", anchorId);
                if (anchorDebugText != null)
                {
                    anchorDebugText.text += string.Format("\nFailed to delete \"{0}\".", anchorId);
                }
            }
        }

        /// <summary>
        /// Generates the name for the anchor.
        /// If no anchor name was specified, the name of the anchor will be the same as the GameObject's name.
        /// </summary>
        /// <param name="gameObjectToAnchor">The GameObject to attach the anchor to.</param>
        /// <param name="proposedAnchorName">Name of the anchor. If none provided, the name of the GameObject will be used.</param>
        /// <returns>The name of the newly attached anchor.</returns>
        private static string GenerateAnchorName(GameObject gameObjectToAnchor, string proposedAnchorName = null)
        {
            return string.IsNullOrEmpty(proposedAnchorName) ? gameObjectToAnchor.name : proposedAnchorName;
        }
#endif
    }
}
