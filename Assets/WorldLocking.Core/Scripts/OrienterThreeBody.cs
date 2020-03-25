using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;


namespace Microsoft.MixedReality.WorldLocking.Core
{
    public class OrienterThreeBody : Orienter
    {

        protected override bool ComputeRotations()
        {
            if (actives.Count <= 2)
            {
                return base.ComputeRotations();
            }

            for (int i = 0; i < actives.Count; ++i)
            {
                for (int j = i + 1; j < actives.Count; ++j)
                {
                    for (int k = j + 1; k < actives.Count; ++k)
                    {
                        WeightedRotation wrotNew = ComputeRotation(actives[i].orientable, actives[j].orientable, actives[k].orientable);
                        WeightedRotation wrot = actives[i];
                        wrot = AverageRotation(wrot, wrotNew);
                        actives[i] = wrot;
                        wrot = actives[j];
                        wrot = AverageRotation(wrot, wrotNew);
                        actives[j] = wrot;
                        wrot = actives[k];
                        wrot = AverageRotation(wrot, wrotNew);
                        actives[k] = wrot;
                    }
                }
            }
            return true;
        }

        protected override WeightedRotation ComputeRotation(IOrientable a, IOrientable b)
        {
            Vector3 lockedAtoB = b.LockedPosition - a.LockedPosition;
            lockedAtoB.Normalize();

            Vector3 virtualAtoB = b.ModelPosition - a.ModelPosition;
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

        private WeightedRotation ComputeRotation(IOrientable a, IOrientable b, IOrientable c)
        {
            Vector3 lockedA = a.LockedPosition;
            Vector3 lockedB = b.LockedPosition;
            Vector3 lockedC = c.LockedPosition;

            Vector3 lockedBtoA = lockedA - lockedB;
            Vector3 lockedBtoC = lockedC - lockedB;

            float weight = ComputeWeight(lockedBtoA, lockedBtoC);

            Quaternion rotVirtualFromLocked = Quaternion.identity;

            if (weight > 0)
            {
                Vector3 virtualBtoA = a.ModelPosition - b.ModelPosition;
                Vector3 virtualBtoC = c.ModelPosition - b.ModelPosition;

                Quaternion rotationFirst = Quaternion.FromToRotation(virtualBtoA, lockedBtoA);

                virtualBtoC = rotationFirst * virtualBtoC;

                Vector3 dir = lockedBtoA;
                dir.Normalize();
                Vector3 up = Vector3.Cross(lockedBtoC, dir);
                up.Normalize();
                Vector3 right = Vector3.Cross(dir, up);

                float sinRads = Vector3.Dot(virtualBtoC, up);
                float cosRads = Vector3.Dot(virtualBtoC, right);

                float rotRads = Mathf.Atan2(sinRads, cosRads);

                Quaternion rotationSecond = Quaternion.AngleAxis(Mathf.Rad2Deg * rotRads, dir);

                rotVirtualFromLocked = rotationSecond * rotationFirst;

                rotVirtualFromLocked.Normalize();
            }

            return new WeightedRotation()
            {
                orientable = null,
                rotation = rotVirtualFromLocked,
                weight = weight
            };

        }
        private float ComputeWeight(Vector3 lockedBtoA, Vector3 lockedBtoC)
        {
            float weight = 1.0f;

            float minDist = 0.01f; // a centimeter, really should be much further apart to be provide satisfactory results (like 10s of meters).
            if (lockedBtoA.magnitude < minDist || lockedBtoC.magnitude < minDist)
            {
                weight = 0.0f;
            }
            if (weight > 0)
            {
                float dist = Mathf.Max(lockedBtoA.magnitude, lockedBtoC.magnitude);
                weight *= 1.0f / dist;
            }
            if (weight > 0)
            {
                // Check absolute value of normalized dot product. If too aligned (near 1) then computed transforms will be unstable.
                // Note degenerate cases of zero length difference vectors has been filtered out above.
                float maxAbsDot = 0.985f; // about 10 degrees
                float absDot = Math.Abs(Vector3.Dot(lockedBtoA.normalized, lockedBtoC.normalized));
                if (absDot > maxAbsDot)
                {
                    weight = 0.0f;
                }
                else
                {
                    weight *= 1.0f - absDot;
                }

            }
            return weight;
        }
    }
}