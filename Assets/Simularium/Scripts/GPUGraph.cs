using UnityEngine;

public class GPUGraph : MonoBehaviour {

	const int maxResolution = 1000;

	static readonly int
		positionsId = Shader.PropertyToID("_Positions"),
		stepId = Shader.PropertyToID("_Step");

	[SerializeField]
	Material material;

	[SerializeField]
	Mesh mesh;

	[SerializeField, Range(10, maxResolution)]
	int resolution = 10;

	ComputeBuffer positionsBuffer;
    float currentTime;
    float playbackDuration = 10f;

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

    float[,] LoadTestData (float time)
    {
        float[,] data = new float[resolution * resolution, 3];
        for (int i = 0; i < resolution * resolution; i++)
        {
            data[i, 0] = Mathf.Floor(i / resolution);
            data[i, 1] = time;
            data[i, 2] = i % resolution;
        }
        return data;
    }

    void UpdatePositionsBuffer (float time)
    {
        positionsBuffer.SetData( LoadTestData( time ) );
    }

	void Update () 
    {
		currentTime += Time.deltaTime;
		if (currentTime >= playbackDuration) 
        {
			currentTime = 0;
		}
        UpdatePositionsBuffer( currentTime );
		UpdateGPU();
	}

	void UpdateGPU () 
    {
		float step = 2f / resolution;
		material.SetBuffer(positionsId, positionsBuffer);
		material.SetFloat(stepId, step);
		var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
		Graphics.DrawMeshInstancedProcedural(
			mesh, 0, material, bounds, resolution * resolution
		);
	}
}