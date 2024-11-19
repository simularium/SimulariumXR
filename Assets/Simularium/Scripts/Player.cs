using UnityEngine;

namespace Simularium
{
    public class Player : MonoBehaviour 
    {
        public Dataset dataset;

        static readonly int
            positionsId = Shader.PropertyToID("_Positions"),
            scalesId = Shader.PropertyToID("_Scales");

        [SerializeField]
        Material material;

        [SerializeField]
        Mesh mesh;

        float stepTime;
        int currentStep;
        [SerializeField]
        int targetFPS = 1;

        ComputeBuffer positionsBuffer;
        ComputeBuffer radiiBuffer;

        void OnEnable () 
        {
            positionsBuffer = new ComputeBuffer( Dataset.MAX_AGENTS * 3, 4 );
            radiiBuffer = new ComputeBuffer( Dataset.MAX_AGENTS, 4 );
            UpdateBuffers( 0 );
        }

        void OnDisable () 
        {
            positionsBuffer.Release();
            positionsBuffer = null;
            radiiBuffer.Release();
            radiiBuffer = null;
        }

        void UpdateBuffers (int ixTime)
        {
            positionsBuffer.SetData( dataset.frames[ixTime].positions );
            radiiBuffer.SetData( dataset.frames[ixTime].radii );
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
            material.SetBuffer( positionsId, positionsBuffer );
            material.SetBuffer( scalesId, radiiBuffer );
            var bounds = new Bounds( Vector3.zero, 6f * Vector3.one );
            Graphics.DrawMeshInstancedProcedural(
                mesh, 0, material, bounds, dataset.nAgents[currentStep]
            );
        }
    }
}