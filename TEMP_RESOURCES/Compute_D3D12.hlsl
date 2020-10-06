RWTexture2D<float4> outputTexture : register(u0);

[numthreads(8, 8, 1)]
void CSMain(uint3 id : SV_DispatchThreadID)
{
    outputTexture[id.xy] *= float4(id.xy / 256.0, 1, 1);
}