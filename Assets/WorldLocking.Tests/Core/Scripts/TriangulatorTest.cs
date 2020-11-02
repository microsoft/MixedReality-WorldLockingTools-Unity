// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

using Microsoft.MixedReality.WorldLocking.Core;
using Microsoft.MixedReality.WorldLocking.Core.Triangulator;

namespace Microsoft.MixedReality.WorldLocking.Tests.Core
{
    public class TriangulatorTest 
    {
        private static bool FloatCompare(float lhs, float rhs, float eps = -1.0f)
        {
            if (eps < 0)
            {
                eps = 1.0e-6f;
            }
            return lhs + eps >= rhs && lhs <= rhs + eps;
        }

        private static void CheckWeight(Interpolant interp, int idx, float weight)
        {
            int found = 0;
            for (int i = 0; i < interp.idx.Length; ++i)
            {
                if (interp.idx[i] == idx && interp.weights[i] > 0)
                {
                    Assert.IsTrue(FloatCompare(interp.weights[i], weight));
                    ++found;
                }
            }
            // If the input weight is non-zero, there should be exactly one non-zero weight occurrence of it.
            Assert.IsTrue((weight == 0 && found == 0) || (weight > 0 && found == 1));
        }

        private static void CheckWeightZero(Interpolant interp, int idx)
        {
            for (int i = 0; i < interp.idx.Length; ++i)
            {
                if (interp.idx[i] == idx)
                {
                    Assert.AreEqual(interp.weights[i], 0);
                }
            }
        }
        private static void CheckWeightsZero(Interpolant interp, int[] idx)
        {
            for (int i = 0; i < idx.Length; ++i)
            {
                CheckWeightZero(interp, idx[i]);
            }
        }

        private ITriangulator CreateDefaultTriangulator()
        {
            SimpleTriangulator triangulator = new SimpleTriangulator();

            triangulator.SetBounds(new Vector3(-1000, 0, -1000), new Vector3(1000, 0, 1000));

            return triangulator;
        }

        /// <summary>
        /// Test behavior with a single very obtuse triangle.
        /// </summary>
        [Test]
        public void TriangulatorTestObtuseTriangle()
        {
            ITriangulator triangulator = CreateDefaultTriangulator();

            // Use the Assert class to test conditions
            Vector3[] vertices = { new Vector3(0, 0, 0), new Vector3(2, 0, 0), new Vector3(1, 0, 0.04f) };
            triangulator.Add(vertices);

            Interpolant interp = triangulator.Find(new Vector3(0, 0, 0));
            CheckWeight(interp, 0, 1);
            CheckWeightsZero(interp, new int[] { 1, 2 });

            interp = triangulator.Find(new Vector3(2.0f, 0, 0));
            CheckWeight(interp, 1, 1);
            CheckWeightsZero(interp, new int[] { 0, 2 });

            /// This one is outside the triangle, but should interpolate to the obtuse vertex exactly.
            interp = triangulator.Find(new Vector3(1.0f, 0, 02f));
            CheckWeight(interp, 2, 1);
            CheckWeightsZero(interp, new int[] { 0, 1 });

            interp = triangulator.Find(new Vector3(1.0f, 0, 0.0f));
            //CheckWeight(interp, 2, 1);
            //CheckWeightsZero(interp, new int[] { 0, 1 });

        }

        private static int[] AllButOne(int count, int excluded)
        {
            List<int> indices = new List<int>(count - 1);
            for (int i = 0; i < count; ++i)
            {
                if (i != excluded)
                {
                    indices.Add(i);
                }
            }
            return indices.ToArray();
        }

        /// <summary>
        /// Check trivial interpolation at vertices and on edges connecting vertices.
        /// </summary>
        /// <param name="triangulator"></param>
        /// <param name="vertices"></param>
        private void CheckVertices(ITriangulator triangulator, Vector3[] vertices)
        {
            Interpolant interp;
            for (int i = 0; i < vertices.Length; ++i)
            {
                interp = triangulator.Find(vertices[i]);
                CheckWeight(interp, i, 1);
                CheckWeightsZero(interp, AllButOne(vertices.Length, i));

                int next = (i + 1) % vertices.Length;
                Vector3 midPoint = (vertices[i] + vertices[next]) * 0.5f;
                interp = triangulator.Find(midPoint);
                CheckWeight(interp, i, 0.5f);
                CheckWeight(interp, next, 0.5f);
            }
        }
        /// <summary>
        /// A perfect square should give perfect interpolation results.
        /// </summary>
        [Test]
        public void TriangulatorTestSquare()
        {
            ITriangulator triangulator = CreateDefaultTriangulator();

            Vector3[] vertices =
            {
                new Vector3(-1, 0, -1),
                new Vector3(1, 0, -1),
                new Vector3(1, 0, 1),
                new Vector3(-1, 0, 1)
            };
            triangulator.Add(vertices);

            CheckVertices(triangulator, vertices);

            Interpolant interp;

            Vector3 center = Vector3.zero;
            for (int i = 0; i < vertices.Length; ++i)
            {
                center += vertices[i];
            }
            center *= 1.0f / vertices.Length;
            interp = triangulator.Find(center);
            // Check that the interpolation is the center of either the diagonal formed by vert[0]-vert[2]
            // or the diagonal vert[1]-vert[3].
            float[] wgts = new float[vertices.Length];
            for (int i = 0; i < interp.idx.Length; ++i)
            {
                if (interp.weights[i] > 0)
                {
                    wgts[interp.idx[i]] = interp.weights[i];
                }
            }
            float eps = 1.0e-4f;
            bool diag02 = FloatCompare(wgts[0], 0.5f, eps) && FloatCompare(wgts[1], 0, eps) && FloatCompare(wgts[2], 0.5f, eps) && FloatCompare(wgts[3], 0, eps);
            bool diag13 = FloatCompare(wgts[0], 0, eps) && FloatCompare(wgts[1], 0.5f, eps) && FloatCompare(wgts[2], 0, eps) && FloatCompare(wgts[3], 0.5f, eps);
            UnityEngine.Debug.Log($"diag02 {diag02}, diag13 {diag13}");
            UnityEngine.Debug.Log($"w0={wgts[0]}, w1={wgts[1]}, w2={wgts[2]}, w3={wgts[3]}");
            
            Assert.IsTrue((diag02 || diag13) && !(diag02 && diag13));
        }

        [Test]
        public void TriangulatorTestLine()
        {
            ITriangulator triangulator = CreateDefaultTriangulator();

            Vector3[] vertices =
            {
                new Vector3(-1.3f, 0, 1.3f),
                new Vector3(-0.3f, 0, 1.3f),
                new Vector3(0.3f, 0, 1.3f),
                new Vector3(1.3f, 0, 1.3f),
                new Vector3(2.0f, 0, 1.3f),
                new Vector3(3.0f, 0, 1.3f),
                new Vector3(4.0f, 0, 1.3f)
            };
            triangulator.Add(vertices);

            Interpolant interp;

            for (int i = 0; i < vertices.Length; ++i)
            {
                interp = triangulator.Find(vertices[i]);
                CheckWeight(interp, i, 1.0f);
                CheckWeightsZero(interp, AllButOne(vertices.Length, i));
            }

            for (int i = 0; i < vertices.Length; ++i)
            {
                interp = triangulator.Find(vertices[i] + new Vector3(0.0f, 0.0f, -1.7f));
                CheckWeight(interp, i, 1.0f);
                CheckWeightsZero(interp, AllButOne(vertices.Length, i));
            }

            for (int i = 1; i < vertices.Length; ++i)
            {
                Vector3 midPoint = (vertices[i] + vertices[i - 1]) * 0.5f;
                interp = triangulator.Find(midPoint);
                CheckWeight(interp, i, 0.5f);
                CheckWeight(interp, i - 1, 0.5f);
            }

            for (int i = 1; i < vertices.Length; ++i)
            {
                Vector3 midPoint = (vertices[i] + vertices[i - 1]) * 0.5f + new Vector3(0.0f, 0.0f, 1.1f);
                interp = triangulator.Find(midPoint);
                CheckWeight(interp, i, 0.5f);
                CheckWeight(interp, i - 1, 0.5f);
            }
        }

        [Test]
        public void TriangulatorTestTriangulationTime()
        {
            List<Vector3> vertices = new List<Vector3>();
            float maxX = 50.0f;
            float maxZ = 50.0f;
            for(float x = 0; x <= maxX; ++x)
            {
                for (float z = 0; z <= maxZ; ++z)
                {
                    vertices.Add(new Vector3(x, 0.0f, z));
                }
            }

            int reduction = 3;
            List<long> buildTimes = new List<long>();
            List<long> findTimes = new List<long>();
            int maxMsPerVertBuild = 4;

            Stopwatch stopwatch = new Stopwatch();
            Interpolant triIter;
            int minVertices = reduction * 2;
            Vector3[] vertArray = vertices.ToArray();

            while (vertArray.Length >= minVertices)
            {
                ITriangulator triangulator = CreateDefaultTriangulator();
                vertArray = vertices.ToArray();
                stopwatch.Restart();
                triangulator.Add(vertArray);
                stopwatch.Stop();
                UnityEngine.Debug.Log($"Processed {vertices.Count} vertices: {stopwatch.ElapsedMilliseconds}ms");
                buildTimes.Add(stopwatch.ElapsedMilliseconds);
                Assert.IsTrue(stopwatch.ElapsedMilliseconds < vertices.Count * maxMsPerVertBuild);
                stopwatch.Restart();
                triIter = triangulator.Find(new Vector3(maxX * 0.33f, 0, maxZ * 0.33f));
                stopwatch.Stop();
                UnityEngine.Debug.Log($"Searched {vertices.Count} vertices: {stopwatch.ElapsedMilliseconds}ms");
                findTimes.Add(stopwatch.ElapsedMilliseconds);
                Assert.IsTrue(stopwatch.ElapsedMilliseconds <= 1);

                vertices.RemoveRange(vertices.Count / reduction, vertices.Count - vertices.Count / reduction);
            }
        }
    }
}
