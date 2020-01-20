cbuffer ConstantBufferObject : register(b0)
{
    float offset;
	float constrast;
};

struct VSInput
{
    float4 position : POSITION0;
    float4 color : COLOR0;
    float2 uv : TEXCOORD0;
};

struct PSInput
{
    float4 position : SV_POSITION;
    float4 color : COLOR0;
    float2 uv : TEXCOORD0;
};

PSInput VSMain(VSInput input)
{
    PSInput result;

    result.position = input.position;
    result.position.x += offset;
    result.color = input.color * constrast;
    result.uv = input.uv;

    return result;
}

Texture2D mainTexture : register(t0);
SamplerState mainSampler : register(s0);

float4 PSMain(PSInput input) : SV_TARGET
{
    return input.color * mainTexture.Sample(mainSampler, input.uv);
}
