using UnityEngine;
using System.Collections.Generic;

namespace Simularium
{
    public class Player : MonoBehaviour 
    {
        public Dataset dataset;

        static readonly int
            transformsId = Shader.PropertyToID("_Transforms"),
            colorsId = Shader.PropertyToID("_Colors");

        static MaterialPropertyBlock propertyBlock;

        [SerializeField]
        Material material;

        [SerializeField]
        Mesh mesh;

        Dictionary<string, Mesh[]> lineMeshes = new Dictionary<string, Mesh[]>();
        MeshFilter lineRenderer;

        float stepTime;
        int currentStep;
        [SerializeField]
        int targetFPS = 1;

        ComputeBuffer transformsBuffer;
        ComputeBuffer colorsBuffer;

        static Player _Instance;
        public static Player Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindAnyObjectByType<Player>();
                }
                return _Instance;
            }
        }

        void OnEnable () 
        {
            transformsBuffer = new ComputeBuffer( Dataset.MAX_AGENTS * 12, 4 );
            colorsBuffer = new ComputeBuffer( Dataset.MAX_AGENTS * 3, 4 );
            propertyBlock ??= new MaterialPropertyBlock();

            currentStep = 0;
            LoadLineRenderers();
            VisualizeCurrentStep();
        }

        void OnDisable () 
        {
            transformsBuffer.Release();
            transformsBuffer = null;
            colorsBuffer.Release();
            colorsBuffer = null;
        }

        public void SetDataset (Dataset _dataset)
        {
            if (_dataset != dataset)
            {
                dataset = _dataset;

                currentStep = 0;
                LoadLineRenderers();
                VisualizeCurrentStep();
            }
        }

        void LoadLineRenderers ()
        {
            if (dataset == null || !dataset.hasLines)
            {
                return;
            }

            // initialize renderer
            if (lineRenderer == null)
            {
                lineRenderer = (Instantiate( Resources.Load( "LineMesh", typeof(GameObject) ) ) as GameObject).GetComponent<MeshFilter>();
            }

            // load line meshes if not already
            if (lineMeshes.ContainsKey( dataset.datasetName ))
            {
                return;
            }
            lineMeshes[dataset.datasetName] = new Mesh[dataset.totalSteps];
            for (int t = 0; t < dataset.totalSteps; t++) 
            {
                lineMeshes[dataset.datasetName][t] = Resources.Load<Mesh>( dataset.datasetName + "_Mesh_" + t );
            }
        }

        void VisualizeCurrentStep ()
        {
            if (dataset == null)
            {
                return;
            }

            transformsBuffer.SetData( dataset.frames[currentStep].meshTransforms );
            colorsBuffer.SetData( dataset.frames[currentStep].meshColors );
            propertyBlock.SetBuffer( transformsId, transformsBuffer );
            propertyBlock.SetBuffer( colorsId, colorsBuffer );

            if (dataset.hasLines) 
            {
                lineRenderer.mesh = lineMeshes[dataset.datasetName][currentStep];
            }
        }

        void Update () 
        {
            if (dataset == null)
            {
                return;
            }

            stepTime += Time.deltaTime;
            if (stepTime >= 1 / (float)targetFPS) 
            {
                stepTime = 0;
                currentStep++;
                if (currentStep >= dataset.totalSteps)
                {
                    currentStep = 0;
                }

                VisualizeCurrentStep();
            }

            Bounds bounds = new Bounds( Vector3.zero, 6f * Vector3.one );
            Graphics.DrawMeshInstancedProcedural(
                mesh, 0, material, bounds, dataset.nMeshAgents[currentStep], propertyBlock
            );
        }
    }
}