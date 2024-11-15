using UnityEngine;

public class GPUGraph : MonoBehaviour {

	const int maxResolution = 1000;

	static readonly int
		positionsId = Shader.PropertyToID("_Positions"),
		scaleId = Shader.PropertyToID("_Scale");

	[SerializeField]
	Material material;

	[SerializeField]
	Mesh mesh;

	[SerializeField, Range(10, maxResolution)]
	int resolution = 10;

    float currentTime;
    int currentStep;
    [SerializeField]
    int targetFPS = 1;
    [SerializeField]
    int totalSteps = 5;

    ComputeBuffer positionsBuffer;

    void OnEnable () 
    {
		positionsBuffer = new ComputeBuffer( maxResolution * maxResolution, 3 * 4 );
        UpdatePositionsBuffer( 0 );
	}

	void OnDisable () 
    {
		positionsBuffer.Release();
		positionsBuffer = null;
	}

    void UpdatePositionsBuffer (float time)
    {
        positionsBuffer.SetData( LoadTestData( time ) );
    }

    float[,] LoadTestData (float time)
    {
        float[,] data = new float[resolution * resolution, 3];
        for (int i = 0; i < resolution * resolution; i++)
        {
            data[i, 0] = Mathf.Floor( i / resolution );
            data[i, 1] = time;
            data[i, 2] = i % resolution;
        }
        return data;
    }

	void Update () 
    {
		currentTime += Time.deltaTime;
		if (currentTime >= 1 / targetFPS) 
        {
			currentTime = 0;
            currentStep++;
            if (currentStep >= totalSteps)
            {
                currentStep = 0;
            }
		}
		float agentScale = 2f / resolution;
        UpdatePositionsBuffer( 5f * currentStep / totalSteps );
		material.SetBuffer( positionsId, positionsBuffer );
		material.SetFloat( scaleId, agentScale );
		var bounds = new Bounds( Vector3.zero, Vector3.one * (2f + agentScale) );
		Graphics.DrawMeshInstancedProcedural(
			mesh, 0, material, bounds, resolution * resolution
		);
	}
}