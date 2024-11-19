#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<float> _Positions;
	StructuredBuffer<float> _Scales;
#endif

void ConfigureProcedural () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		unity_ObjectToWorld = 0.0;
		unity_ObjectToWorld._m03_m13_m23_m33 = float4(
            _Positions[3 * unity_InstanceID], 
            _Positions[3 * unity_InstanceID + 1], 
            _Positions[3 * unity_InstanceID + 2], 
            1.0
        );
		unity_ObjectToWorld._m00_m11_m22 = _Scales[unity_InstanceID];
	#endif
}

void ShaderGraphFunction_float (float3 In, out float3 Out) {
	Out = In;
}

void ShaderGraphFunction_half (half3 In, out half3 Out) {
	Out = In;
}