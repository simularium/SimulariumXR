using UnityEngine;

namespace Simularium
{
    public class Player : MonoBehaviour {

        public Dataset dataset;

        static readonly int
            positionsId = Shader.PropertyToID("_Positions"),
            scaleId = Shader.PropertyToID("_Scale");

        [SerializeField]
        Material material;

        [SerializeField]
        Mesh mesh;

        float stepTime;
        int currentStep;
        [SerializeField]
        int targetFPS = 1;

        ComputeBuffer positionsBuffer;

        void OnEnable () 
        {
            positionsBuffer = new ComputeBuffer( Dataset.MAX_AGENTS * 3, 4 );
            UpdatePositionsBuffer( 0 );
        }

        void OnDisable () 
        {
            positionsBuffer.Release();
            positionsBuffer = null;
        }

        void UpdatePositionsBuffer (int ixTime)
        {
            positionsBuffer.SetData( dataset.frames[ixTime].positions );
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
            
            UpdatePositionsBuffer( currentStep );
            material.SetBuffer( positionsId, positionsBuffer );
            material.SetFloat( scaleId, dataset.agentScale );
            var bounds = new Bounds( Vector3.zero, Vector3.one * (2f + dataset.agentScale) );
            Graphics.DrawMeshInstancedProcedural(
                mesh, 0, material, bounds, dataset.nAgents[currentStep]
            );
        }
    }
}