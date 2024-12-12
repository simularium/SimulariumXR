using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

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
        bool playing = true;

        ComputeBuffer transformsBuffer;
        ComputeBuffer colorsBuffer;

        public UnityEvent<int> timeUpdated = new UnityEvent<int>();

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

        public void Play ()
        {
            stepTime = 0;
            playing = true;
        }

        public void Pause ()
        {
            playing = false;
        }

        public void IncrementStep (int direction)
        {
            stepTime = 0;
            currentStep += direction;
            if (currentStep >= dataset.totalSteps)
            {
                currentStep = 0;
            }
            if (currentStep < 0)
            {
                currentStep = dataset.totalSteps - 1;
            }
            timeUpdated.Invoke( currentStep );

            VisualizeCurrentStep();
        }

        void LoadLineRenderers ()
        {
            if (dataset == null || !dataset.hasLines)
            {
                if (lineRenderer != null)
                {
                    lineRenderer.mesh = null;
                }
                return;
            }

            // initialize renderer
            if (lineRenderer == null)
            {
                lineRenderer = (Instantiate( Resources.Load( "LineMesh", typeof(GameObject) ), transform ) as GameObject).GetComponent<MeshFilter>();
                lineRenderer.transform.localPosition = Vector3.zero;
                lineRenderer.transform.localRotation = Quaternion.identity;
                lineRenderer.transform.localScale = Vector3.one;
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

            if (playing)
            {
                stepTime += Time.deltaTime;
                if (stepTime >= 1 / (float)dataset.targetFPS) 
                {
                    IncrementStep( 1 );
                }
            }

            Bounds bounds = new Bounds( Vector3.zero, 6f * Vector3.one );
            Graphics.DrawMeshInstancedProcedural(
                mesh, 0, material, bounds, dataset.nMeshAgents[currentStep], propertyBlock
            );
        }
    }
}