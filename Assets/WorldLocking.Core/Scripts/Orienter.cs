using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Microsoft.MixedReality.WorldLocking.Core
{
    /// <summary>
    /// The Orienter class implements IOrienter.
    /// </summary>
    /// <remarks>
    /// It derives from MonoBehaviour only to facilitate assigning it
    /// in the Inspector. 
    /// Alternatively, it could be implemented as a singleton service. 
    /// There are pros and cons in either direction. The MonoBehaviour assigned in inspector
    /// was chosen to make explicit the dependency, rather than a dependency hidden by a
    /// static get internally.
    /// </remarks>
    public class Orienter : MonoBehaviour, IOrienter
    {
        /// <summary>
        /// An object whose rotation needs to be computed, and the weight of its rotation.
        /// </summary>
        private struct WeightedRotation
        {
            public IOrientable orientable;
            public float weight;
            public Quaternion rotation;

            public FragmentId FragmentId => orientable.FragmentId;
        }

        /// <summary>
        /// Registered orienables.
        /// </summary>
        private readonly List<IOrientable> orientables = new List<IOrientable>();

        /// <summary>
        /// Orientables in the currently processing fragment.
        /// </summary>
        private readonly List<WeightedRotation> actives = new List<WeightedRotation>();

        /// <inheritdocs />
        public void Register(IOrientable orientable)
        {
            int idx = orientables.FindIndex(o => o == orientable);
            if (idx < 0)
            {
                orientables.Add(orientable);
            }
        }

        /// <inheritdocs />
        public void Unregister(IOrientable orientable)
        {
            orientables.Remove(orientable);
        }

        /// <inheritdocs />
        public void Reorient(FragmentId fragmentId, IAlignmentManager mgr)
        {
            if (!InitRotations(fragmentId))
            {
                return;
            }
            if (!ComputeRotations())
            {
                return;
            }
            if (!SetRotations(mgr))
            {
                return;
            }
        }


        /// <summary>
        /// Collect all orientables in the current fragment for processing.
        /// </summary>
        /// <param name="fragmentId"></param>
        /// <returns></returns>
        private bool InitRotations(FragmentId fragmentId)
        {
            actives.Clear();
            for (int i = 0; i < orientables.Count; ++i)
            {
                if (orientables[i].FragmentId == fragmentId)
                {
                    actives.Add(
                        new WeightedRotation()
                        {
                            orientable = orientables[i],
                            weight = 0.0f,
                            rotation = Quaternion.identity
                        }
                    );
                }
            }
            return actives.Count > 0;
        }

        /// <summary>
        /// Compute rotations by pairs, weighting by distance and averaging for each orientable.
        /// </summary>
        /// <returns></returns>
        private bool ComputeRotations()
        {
            for (int i = 0; i < actives.Count; ++i)
            {
                for (int j = i + 1; j < actives.Count; ++j)
                {
                    WeightedRotation wrotNew = ComputeRotation(actives[i].orientable, actives[j].orientable);
                    WeightedRotation wrot = actives[i];
                    wrot = AverageRotation(wrot, wrotNew);
                    actives[i] = wrot;
                    wrot = actives[j];
                    wrot = AverageRotation(wrot, wrotNew);
                    actives[j] = wrot;
                }
            }
            return true;
        }

        /// <summary>
        /// Compute the rotation that aligns a and b correctly in pinned space.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private WeightedRotation ComputeRotation(IOrientable a, IOrientable b)
        {
            Vector3 lockedAtoB = b.LockedPosition - a.LockedPosition;
            lockedAtoB.y = 0.0f;
            lockedAtoB.Normalize();

            Vector3 virtualAtoB = b.ModelPosition - a.ModelPosition;
            virtualAtoB.y = 0.0f;
            virtualAtoB.Normalize();

            Quaternion rotVirtualFromLocked = Quaternion.FromToRotation(virtualAtoB, lockedAtoB);
            rotVirtualFromLocked.Normalize();

            float weight = (a.ModelPosition - b.ModelPosition).sqrMagnitude;
            float minDistSq = 0.0f;
            weight = weight > minDistSq ? 1.0f / weight : 1.0f;

            return new WeightedRotation()
            {
                orientable = null,
                rotation = rotVirtualFromLocked,
                weight = weight
            };
        }

        /// <summary>
        /// Compute a new weighted rotation representing the two input weighted rotations.
        /// </summary>
        /// <param name="accum">The accumulator rotation.</param>
        /// <param name="add">The rotation to add in.</param>
        /// <returns>A new aggregate weighted rotation.</returns>
        private WeightedRotation AverageRotation(WeightedRotation accum, WeightedRotation add)
        {
            float interp = add.weight / (accum.weight + add.weight);

            Quaternion combinedRot = Quaternion.Slerp(accum.rotation, add.rotation, interp);
            combinedRot.Normalize();

            float combinedWeight = accum.weight + add.weight;

            return new WeightedRotation()
            {
                orientable = accum.orientable,
                rotation = combinedRot,
                weight = combinedWeight
            };
        }

        /// <summary>
        /// Apply the computed rotations to the orientables.
        /// </summary>
        /// <param name="mgr">The alignment manager.</param>
        /// <returns>True on success.</returns>
        private bool SetRotations(IAlignmentManager mgr)
        {
            for (int i = 0; i < actives.Count; ++i)
            {
                actives[i].orientable.PushRotation(mgr, actives[i].rotation);
            }
            return true;
        }

    }
}