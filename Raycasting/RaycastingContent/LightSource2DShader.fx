sampler s0;
float X;
float Y;
float TexX;
float TexY;
float Intensity;

float4 PixelShaderFunction(float2 coords: TEXCOORD0) : COLOR0
{
	float4 a;

	a = tex2D(s0, float2(coords.x, coords.y));

	float dx = coords.x * TexX - X;
	float dy = coords.y * TexY - Y;
	float distance = sqrt(dx * dx + dy * dy);

	a.a = 1 - sqrt(distance) / 21;

    return a;
}

technique Technique1
{
	pass Pass0
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}
