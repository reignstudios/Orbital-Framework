struct VSInput
{
    float3 position : POSITION0;
};

struct PSInput
{
    float4 position : SV_POSITION;
};

PSInput VSMain(VSInput input)
{
    PSInput result;
    result.position = float4(input.position, 1);
    return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    return float4(1, 1, 1, 1);
}
