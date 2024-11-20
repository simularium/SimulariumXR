using UnityEngine;

namespace Simularium
{
    public class Player : MonoBehaviour 
    {
        public Dataset dataset;

        static readonly int
            colorAId = Shader.PropertyToID("_ColorA"),
            colorBId = Shader.PropertyToID("_ColorB"),
            transformsId = Shader.PropertyToID("_Transforms");

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

        void OnEnable () 
        {
            transformsBuffer = new ComputeBuffer( Dataset.MAX_AGENTS * 12, 4 );
            UpdateBuffers( 0 );

            propertyBlock ??= new MaterialPropertyBlock();
        }

        void OnDisable () 
        {
            transformsBuffer.Release();
            transformsBuffer = null;
        }

        void UpdateBuffers (int ixTime)
        {
            transformsBuffer.SetData( dataset.frames[ixTime].transforms );
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
            }
            
            UpdateBuffers( currentStep );

			propertyBlock.SetColor(colorAId, Color.white);
			propertyBlock.SetColor(colorBId, Color.black);
			propertyBlock.SetBuffer( transformsId, transformsBuffer );
            var bounds = new Bounds( Vector3.zero, 6f * Vector3.one );
            Graphics.DrawMeshInstancedProcedural(
				mesh, 0, material, bounds, dataset.nAgents[currentStep], propertyBlock
			);
        }
    }
}