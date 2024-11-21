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

        float stepTime;
        int currentStep;
        [SerializeField]
        int targetFPS = 1;

        ComputeBuffer transformsBuffer;
        ComputeBuffer colorsBuffer;

        void OnEnable () 
        {
            transformsBuffer = new ComputeBuffer( Dataset.MAX_AGENTS * 12, 4 );
            colorsBuffer = new ComputeBuffer( Dataset.MAX_AGENTS * 3, 4 );
            UpdateBuffers( 0 );

            propertyBlock ??= new MaterialPropertyBlock();
        }

        void OnDisable () 
        {
            transformsBuffer.Release();
            transformsBuffer = null;
            colorsBuffer.Release();
            colorsBuffer = null;
        }

        void UpdateBuffers (int ixTime)
        {
            transformsBuffer.SetData( dataset.frames[ixTime].transforms );
            colorsBuffer.SetData( dataset.frames[ixTime].colors );
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

                UpdateBuffers( currentStep );
                propertyBlock.SetBuffer( transformsId, transformsBuffer );
                propertyBlock.SetBuffer( colorsId, colorsBuffer );
            }

            Bounds bounds = new Bounds( Vector3.zero, 6f * Vector3.one );
            Graphics.DrawMeshInstancedProcedural(
                mesh, 0, material, bounds, dataset.nAgents[currentStep], propertyBlock
            );
        }
    }
}