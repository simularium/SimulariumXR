#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<float> _Transforms;
	StructuredBuffer<float> _Colors;
    float4x4 _ParentTransform;
#endif

void ConfigureProcedural () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)

        // get agent transform
        float4x4 m;
        for (int n = 0; n < 12; n++)
        {
            m[floor( n / 4 )][n % 4] = _Transforms[12 * unity_InstanceID + n];
        }
        m._m30_m31_m32_m33 = float4(0.0, 0.0, 0.0, 1.0);

        // transform agent position by parent transform
        float4 pos = float4(m[0][3], m[1][3], m[2][3], 1.0);
        float4 newpos = mul(_ParentTransform, pos);

        // transform agent rotation/scale by parent rotation/scale
        float4x4 w = mul(m, _ParentTransform);

        // set rotated position
        w[0][3] = newpos[0];
        w[1][3] = newpos[1];
        w[2][3] = newpos[2];

        // set transform
		unity_ObjectToWorld._m00_m01_m02_m03 = w._m00_m01_m02_m03;
		unity_ObjectToWorld._m10_m11_m12_m13 = w._m10_m11_m12_m13;
		unity_ObjectToWorld._m20_m21_m22_m23 = w._m20_m21_m22_m23;
		unity_ObjectToWorld._m30_m31_m32_m33 = float4(0.0, 0.0, 0.0, 1.0);

	#endif
}

float4 GetAgentColor () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		return float4(
            _Colors[3 * unity_InstanceID],
            _Colors[3 * unity_InstanceID + 1],
            _Colors[3 * unity_InstanceID + 2],
            1.0
        );
	#else
		return float4(1.0, 0.0, 1.0, 1.0);
	#endif
}

void ShaderGraphFunction_float (float3 In, out float3 Out, out float4 Color) {
	Out = In;
	Color = GetAgentColor();
}

void ShaderGraphFunction_half (half3 In, out half3 Out, out half4 Color) {
	Out = In;
	Color = GetAgentColor();
}