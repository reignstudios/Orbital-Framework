RWTexture2D<float4> outputTexture : register(u0);

[numthreads(1, 1, 1)]
void CSMain(uint3 dtID : SV_DispatchThreadID)
{
    outputTexture[dtID.xy] = float4(1, 0, 0, 1);
}