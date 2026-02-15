#ifndef FLAT_KIT_PIXELATION_INCLUDED
#define FLAT_KIT_PIXELATION_INCLUDED

TEXTURE2D_X (_BlitTexture);
SAMPLER (sampler_BlitTexture);

float4 SampleCameraColor(float2 uv)
{
    // Using sampler_PointClamp instead of sampler_BlitTexture to avoid bilinear filtering.
    return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, UnityStereoTransformScreenSpaceTex(uv));
}

void Pixelation_float(float2 UV, out float4 Out)
{
    float longerScreenSizePixelSize = _PixelSize;
    float aspectRatio = _ScreenParams.x / _ScreenParams.y;
    float2 pixelSize = aspectRatio > 1
                           ? float2(longerScreenSizePixelSize, longerScreenSizePixelSize * aspectRatio)
                           : float2(longerScreenSizePixelSize / aspectRatio, longerScreenSizePixelSize);
    float2 uv = UV / pixelSize;
    uv = floor(uv);
    uv *= pixelSize;
    Out = SampleCameraColor(uv);
}
#endif  // FLAT_KIT_PIXELATION_INCLUDED
