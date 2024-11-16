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

        float currentTime;
        int currentStep;
        [SerializeField]
        int targetFPS = 1;

        ComputeBuffer positionsBuffer;

        void OnEnable () 
        {
            Debug.Log( dataset.positions.Count );

            positionsBuffer = new ComputeBuffer( Dataset.MAX_AGENTS, 3 * 4 );
            UpdatePositionsBuffer( 0 );
        }

        void OnDisable () 
        {
            positionsBuffer.Release();
            positionsBuffer = null;
        }

        void UpdatePositionsBuffer (int time)
        {
            positionsBuffer.SetData( dataset.positions[time] );
        }

        void Update () 
        {
            currentTime += Time.deltaTime;
            if (currentTime >= 1 / targetFPS) 
            {
                currentTime = 0;
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