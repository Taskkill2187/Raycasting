sampler s0;

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float4 a;

	a = tex2D(s0, float2(coords.x, coords.y));

	a += tex2D(s0, float2(coords.x - 0.000625, coords.y - 0.000625)) * 0.15f;
	a += tex2D(s0, float2(coords.x + 0.000625, coords.y + 0.000625)) * 0.15f;
	a += tex2D(s0, float2(coords.x + 0.000625, coords.y - 0.000625)) * 0.15f;
	a += tex2D(s0, float2(coords.x - 0.000625, coords.y + 0.000625)) * 0.15f;

	a += tex2D(s0, float2(coords.x - 0.000625, coords.y - 0.000625)) * 0.07f;
	a += tex2D(s0, float2(coords.x + 0.000625, coords.y + 0.000625)) * 0.07f;
	a += tex2D(s0, float2(coords.x + 0.000625, coords.y - 0.000625)) * 0.07f;
	a += tex2D(s0, float2(coords.x - 0.000625, coords.y + 0.000625)) * 0.07f;

    return a;
}

technique Technique1
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
