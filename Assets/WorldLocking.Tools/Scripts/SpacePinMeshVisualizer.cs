using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.WorldLocking.Core;
using UnityEngine;

using Microsoft.MixedReality.WorldLocking.Core.Triangulator;

namespace Microsoft.MixedReality.WorldLocking.Tools
{

    public class SpacePinMeshVisualizer : MonoBehaviour
    {

        private static SpacePinMeshVisualizer instance = null;
        public static SpacePinMeshVisualizer Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<SpacePinMeshVisualizer>();
                return instance;
            }
        }

        [Range(0.1f, 2.0f)]
        public float weightCubeMaxSize = 1.0f;

        public float downwardsOffsetFromUser = 1.65f;

        public Material meshMaterial = null;
        public Material extrapolatedMeshMaterial = null;
        public Material weightsMaterial = null;

        private AlignmentManager alignmentManager = null;
        private SimpleTriangulator triangulator = null;
        private Interpolant currentInterpolant = null;

        private MeshRenderer meshRenderer = null;
        private MeshFilter meshFilter = null;

        private Mesh currentTriangleMesh = null;
        private int currentBoundaryVertexIDx = -1;
        private Vector3 firstPinPosition, secondPinPosition, thirdPinPosition;
        private Vector3 firstCubePosition, secondCubePosition, thirdCubePosition;
        private Mesh[] triangleWeightMeshes = new Mesh[3];

        private int[] lastGeneratedTriangleIDs = new int[3] { -1, -1, -1 };

        [SerializeField]
        private bool isVisible = true;

        private const string WeightVectorOffsetMaterialProperty = "_VectorOffset";
        private const string WeightMaterialProperty = "_Weight";

        #region Public APIs
        public bool GetVisibility()
        {
            return isVisible;
        }

        public void SetVisibility(bool visible)
        {
            isVisible = visible;
            RefreshVisibility();
        }

        #endregion

        private void RefreshVisibility()
        {
            meshRenderer.enabled = isVisible;
        }

        /// <summary>
        /// Injecting the reference to the triangulation that was newly built.
        /// </summary>
        /// <param name="triangulator">Reference to the data on the triangle that was built.</param>
        public void Initialize(ITriangulator triangulator)
        {
            this.triangulator = (SimpleTriangulator)triangulator;

            if (meshRenderer == null && meshFilter == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

                meshFilter = gameObject.AddComponent<MeshFilter>();

                Material[] materials = new Material[4];

                materials[0] = new Material(meshMaterial);
                materials[1] = new Material(weightsMaterial);
                materials[2] = new Material(weightsMaterial);
                materials[3] = new Material(weightsMaterial);

                meshRenderer.materials = materials;
            }

            transform.position = new Vector3(transform.position.x, GetLockedHeadPosition().y - downwardsOffsetFromUser, transform.position.z);
        }

        /// <summary>
        /// Generates and combines a triangle and cubes representing the three SpacePins and the area between them as sub meshes.
        /// </summary>
        private void GenerateMeshes()
        {
            currentTriangleMesh = new Mesh();

            if (currentInterpolant == null)
                return;

            bool hasBoundaryVertex = false;

            int[] vertIDxs = currentInterpolant.idx;

            for (int i = 0; i < vertIDxs.Length; i++)
            {
                if (currentInterpolant.weights[i] <= 0)
                {
                    hasBoundaryVertex = true;
                    currentBoundaryVertexIDx = i;
                }
            }

            currentBoundaryVertexIDx = hasBoundaryVertex ? currentBoundaryVertexIDx : -1;

            CalculatePinPositionsFromCurrentInterpolant();

            Vector3[] vertices = new Vector3[3]
            {
                firstPinPosition,
                secondPinPosition,
                thirdPinPosition
            };
            currentTriangleMesh.vertices = vertices;

            int[] tris = new int[3]
            {
                2, 1, 0
            };

            currentTriangleMesh.triangles = tris;

            Vector3[] normals = new Vector3[vertices.Length];

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.up;
            }

            currentTriangleMesh.normals = normals;

            Vector2[] uv = new Vector2[vertices.Length];

            for (int i = 0; i < uv.Length; i++)
            {
                uv[i] = new Vector2(Mathf.InverseLerp(-1000f, 1000f, vertices[i].x), Mathf.InverseLerp(-1000f, 1000f, vertices[i].z));
            }

            currentTriangleMesh.uv = uv;

            currentTriangleMesh.RecalculateBounds();

            if (meshRenderer.materials[0] != null)
                Destroy(meshRenderer.materials[0]);

            Material[] materials = meshRenderer.materials;
            materials[0] = currentBoundaryVertexIDx != -1 ? new Material(extrapolatedMeshMaterial) : new Material(meshMaterial);
            meshRenderer.materials = materials;

            CombineInstance[] combine = new CombineInstance[4];

            combine[0].mesh = currentTriangleMesh;
            combine[0].transform = Matrix4x4.zero;
            meshRenderer.materials[0].SetVector(WeightVectorOffsetMaterialProperty, (firstPinPosition + secondPinPosition + thirdPinPosition) / 3);

            firstCubePosition = firstPinPosition;
            combine[1].mesh = triangleWeightMeshes[0] = CreateCube(firstPinPosition);
            combine[1].transform = Matrix4x4.zero;
            meshRenderer.materials[1].SetVector(WeightVectorOffsetMaterialProperty, firstPinPosition);

            secondCubePosition = secondPinPosition;
            combine[2].mesh = triangleWeightMeshes[1] = CreateCube(secondPinPosition);
            combine[2].transform = Matrix4x4.zero;
            meshRenderer.materials[2].SetVector(WeightVectorOffsetMaterialProperty, secondPinPosition);

            thirdCubePosition = thirdPinPosition;
            combine[3].mesh = triangleWeightMeshes[2] = CreateCube(thirdPinPosition);
            combine[3].transform = Matrix4x4.zero;
            meshRenderer.materials[3].SetVector(WeightVectorOffsetMaterialProperty, thirdPinPosition);

            meshFilter.mesh = new Mesh();
            meshFilter.mesh.CombineMeshes(combine, false, false);
        }

        private Mesh CreateCube(Vector3 offset)
        {
            Mesh cube = new Mesh();

            float s = weightCubeMaxSize;

            Vector3[] vertices = {
                new Vector3 (0, 0, 0),
                new Vector3 (s, 0, 0),
                new Vector3 (s, s, 0),
                new Vector3 (0, s, 0),
                new Vector3 (0, s, s),
                new Vector3 (s, s, s),
                new Vector3 (s, 0, s),
                new Vector3 (0, 0, s),
            };

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] += offset - new Vector3(s / 2, s / 2, s / 2);
            }

            cube.vertices = vertices;

            int[] triangles = {
                0, 2, 1, //face front
                0, 3, 2,
                2, 3, 4, //face top
                2, 4, 5,
                1, 2, 5, //face right
                1, 5, 6,
                0, 7, 4, //face left
                0, 4, 3,
                5, 4, 7, //face back
                5, 7, 6,
                0, 6, 7, //face bottom
                0, 1, 6
            };

            cube.triangles = triangles;

            cube.RecalculateNormals();
            cube.RecalculateBounds();

            return cube;
        }

        private void UpdateCubeWeights()
        {
            meshRenderer.materials[1].SetFloat(WeightMaterialProperty, currentInterpolant.weights[0]);
            meshRenderer.materials[2].SetFloat(WeightMaterialProperty, currentInterpolant.weights[1]);
            meshRenderer.materials[3].SetFloat(WeightMaterialProperty, currentInterpolant.weights[2]);
        }

        private void CalculatePinPositionsFromCurrentInterpolant()
        {
            Vector3 lockedHeadPosition = GetLockedHeadPosition();
            lockedHeadPosition.y = 0.0f;

            firstPinPosition = currentBoundaryVertexIDx == 0 ? lockedHeadPosition : triangulator.Vertices[currentInterpolant.idx[0] + 4];
            secondPinPosition = currentBoundaryVertexIDx == 1 ? lockedHeadPosition : triangulator.Vertices[currentInterpolant.idx[1] + 4];
            thirdPinPosition = currentBoundaryVertexIDx == 2 ? lockedHeadPosition : triangulator.Vertices[currentInterpolant.idx[2] + 4];

            //    DEBUG TRIANGLE    //
            //firstPinPosition = new Vector3(5.0f, 0.0f, 0.0f);
            //secondPinPosition = new Vector3(1.0f, 0.0f, 1.0f);
            //thirdPinPosition = new Vector3(-1.0f,0.0f,-1.0f);

            firstPinPosition.y = secondPinPosition.y = thirdPinPosition.y = 0.0f;
        }

        /// <summary>
        /// Updates the three vertex's position that make the currently interpolated triangle
        /// </summary>
        private void UpdateVertexPositions()
        {
            CalculatePinPositionsFromCurrentInterpolant();

            List<Vector3> vertices = new List<Vector3>();
            meshFilter.mesh.GetVertices(vertices);

            bool anyPositionChanged = false;

            if (currentBoundaryVertexIDx != 0 && vertices[0] != firstPinPosition)
            {
                vertices[0] = firstPinPosition;
                anyPositionChanged = true;
            }
            if (currentBoundaryVertexIDx != 1 && vertices[1] != secondPinPosition)
            {
                vertices[1] = secondPinPosition;
                anyPositionChanged = true;
            }
            if (currentBoundaryVertexIDx != 2 && vertices[2] != thirdPinPosition)
            {
                vertices[2] = thirdPinPosition;
                anyPositionChanged = true;
            }

            Vector3[] pinPositions = new Vector3[3] { firstPinPosition, secondPinPosition, thirdPinPosition };
            if (currentBoundaryVertexIDx != -1 && vertices[currentBoundaryVertexIDx] != pinPositions[currentBoundaryVertexIDx])
            {
                vertices[currentBoundaryVertexIDx] = pinPositions[currentBoundaryVertexIDx];
                anyPositionChanged = true;
            }

            if (anyPositionChanged)
            {
                meshFilter.mesh.SetVertices(vertices);
                UpdateCubeVertexPositions();
            }

            meshRenderer.materials[0].SetVector(WeightVectorOffsetMaterialProperty, (vertices[0] + vertices[1] + vertices[2]) / 3);
        }


        /// <summary>
        /// Snap the 3 cube meshes to the pin positions by changing local vertex data.
        /// </summary>
        private void UpdateCubeVertexPositions()
        {
            List<Vector3> vertices = new List<Vector3>();
            meshFilter.mesh.GetVertices(vertices);

            Vector3[] oldPositions = new Vector3[3] { firstCubePosition,secondCubePosition,thirdCubePosition };
            Vector3[] newPositions = new Vector3[3] { firstPinPosition,secondPinPosition,thirdPinPosition };

            int index = 0;
            for (int i = 3; i < vertices.Count; i += 8)
            {
                for (int j = i; j < i+8; j++)
                {
                    vertices[j] -= oldPositions[index];
                    vertices[j] += newPositions[index];
                }

                meshRenderer.materials[index+1].SetVector(WeightVectorOffsetMaterialProperty, newPositions[index]);

                index++;
            }

            firstCubePosition = firstPinPosition;
            secondCubePosition = secondPinPosition;
            thirdCubePosition = thirdPinPosition;

            meshFilter.mesh.SetVertices(vertices);
        }

        private Vector3 GetLockedHeadPosition()
        {
            WorldLockingManager wltMgr = WorldLockingManager.GetInstance();
            Pose lockedHeadPose = wltMgr.LockedFromPlayspace.Multiply(wltMgr.PlayspaceFromSpongy.Multiply(wltMgr.SpongyFromCamera));
            return lockedHeadPose.position;
        }

        private void Awake()
        {
            AlignSubtree subTree = FindObjectOfType<AlignSubtree>();

            if (subTree != null)
            {
                subTree.OnAlignManagerCreated += (sender,manager) =>
                {
                    this.alignmentManager = manager;
                    alignmentManager.OnTriangulationBuilt += OnNewTriangulationWasBuilt;
                };
            }
        }

        private void OnNewTriangulationWasBuilt(object sender, ITriangulator triangulation)
        {
            Initialize(triangulation);
        }

        private void OnDestroy()
        {
            if (alignmentManager != null)
                alignmentManager.OnTriangulationBuilt -= OnNewTriangulationWasBuilt;
        }

        private void Update()
        {
            if (triangulator != null && isVisible)
            {
                // Find the three closest SpacePins this frame
                Interpolant interpolantThisFrame = triangulator.Find(GetLockedHeadPosition());

                if (interpolantThisFrame != null)
                {
                    currentInterpolant = interpolantThisFrame;

                    // Only generate new mesh if SpacePins are different from the currently generated ones
                    if (!Enumerable.SequenceEqual(interpolantThisFrame.idx, lastGeneratedTriangleIDs))
                    {
                        GenerateMeshes();
                        lastGeneratedTriangleIDs = interpolantThisFrame.idx;
                    }
                    // if SpacePins are same update the vertices in case the current SpacePins moved somehow,
                    // or there is a boundary vertex that needs to be snapped to the headset position every frame.
                    else
                    {
                        UpdateVertexPositions();
                    }

                    UpdateCubeWeights();
                }
            }
        }
    }
}
