using UnityEngine;

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

        Mesh[] lineMeshes;
        MeshFilter lineRenderer;

        float stepTime;
        int currentStep;
        [SerializeField]
        int targetFPS = 1;

        ComputeBuffer transformsBuffer;
        ComputeBuffer colorsBuffer;

        void OnEnable () 
        {
            if (dataset.hasLines) 
            {
                LoadLineRenderers();
            }

            transformsBuffer = new ComputeBuffer( Dataset.MAX_AGENTS * 12, 4 );
            colorsBuffer = new ComputeBuffer( Dataset.MAX_AGENTS * 3, 4 );
            propertyBlock ??= new MaterialPropertyBlock();

            SetCurrentFrame( 0 );
        }

        void OnDisable () 
        {
            transformsBuffer.Release();
            transformsBuffer = null;
            colorsBuffer.Release();
            colorsBuffer = null;
        }

        void LoadLineRenderers ()
        {
            lineRenderer = (Instantiate( Resources.Load( "LineMesh", typeof(GameObject) ) ) as GameObject).GetComponent<MeshFilter>();
            lineMeshes = new Mesh[dataset.totalSteps];
            for (int t = 0; t < dataset.totalSteps; t++) 
            {
                lineMeshes[t] = Resources.Load<Mesh>( dataset.name + "_Mesh_" + t );
            }
        }

        void SetCurrentFrame (int t)
        {
            transformsBuffer.SetData( dataset.frames[t].meshTransforms );
            colorsBuffer.SetData( dataset.frames[t].meshColors );
            propertyBlock.SetBuffer( transformsId, transformsBuffer );
            propertyBlock.SetBuffer( colorsId, colorsBuffer );

            if (dataset.hasLines) 
            {
                lineRenderer.mesh = lineMeshes[t];
            }
        }

        void Update () 
        {
            stepTime += Time.deltaTime;
            if (stepTime >= 1 / (float)targetFPS) 
            {
                stepTime = 0;
                currentStep++;
                if (currentStep >= dataset.totalSteps)
                {
                    currentStep = 0;
                }

                SetCurrentFrame( currentStep );
            }

            Bounds bounds = new Bounds( Vector3.zero, 6f * Vector3.one );
            Graphics.DrawMeshInstancedProcedural(
                mesh, 0, material, bounds, dataset.nMeshAgents[currentStep], propertyBlock
            );
        }
    }
}