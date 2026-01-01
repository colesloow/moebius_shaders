// Screen-space outline utilities.
// DepthBasedOutlines and NormalBasedOutlines adapted from this tutorial:
// https://www.youtube.com/watch?v=nc3a3THBFrg&t=1286s
//
// Extended with color-based edge detection (luminance contrast)
// to properly support outlines on transparent objects.

struct ScharrOperators
{
    float3x3 x;
    float3x3 y;
};

// ==============================================
//        Scharr X        ||   Scharr Y
//       -3,   0,   3     ||  -3, -10,  -3
//      -10,   0,  10     ||   0,   0,   0
//       -3,   0,   3     ||   3,  10,   3
// ==============================================

ScharrOperators GetEdgeDetectionKernels()
{
    ScharrOperators kernels;

    kernels.x = float3x3(
        -3, -10, -3,
         0, 0, 0,
         3, 10, 3
    );
    
    kernels.y = float3x3(
        -3, 0, 3,
       -10, 0, 10,
        -3, 0, 3
    );

    return kernels;
}

void DepthBasedOutlines_float(float2 screenUV, float2 px, out float outlines)
{
    // Always initialize to avoid compiler warnings
    outlines = 0.0;

    ScharrOperators k = GetEdgeDetectionKernels();

    float gx = 0.0;
    float gy = 0.0;

    // Sample depth in a 3x3 neighborhood
    for (int i = -1; i <= 1; i++)
    {
        for (int j = -1; j <= 1; j++)
        {
            float2 offset = float2(i, j) * px;

            float d = SampleSceneDepth(screenUV + offset);
            d = LinearEyeDepth(d, _ZBufferParams);

            // (row, col) -> (j, i)
            gx += d * k.x[j + 1][i + 1];
            gy += d * k.y[j + 1][i + 1];
        }
    }

    float g = sqrt(gx * gx + gy * gy);

    // Threshold controls outline sensitivity
    outlines = step(0.05, g);
}

void NormalBasedOutlines_float(float2 screenUV, float2 px, out float outlines)
{
    // Always initialize
    outlines = 0.0;

    ScharrOperators k = GetEdgeDetectionKernels();

    float gx = 0.0;
    float gy = 0.0;

    float3 centerNormal = SampleSceneNormals(screenUV);

    // Sample normal differences in a 3x3 neighborhood
    for (int i = -1; i <= 1; i++)
    {
        for (int j = -1; j <= 1; j++)
        {
            if (i == 0 && j == 0) continue;

            float2 offset = float2(i, j) * px;
            float3 n = SampleSceneNormals(screenUV + offset);

            // Dot product measures angular difference
            float dp = dot(centerNormal, n);

            // (row, col) -> (j, i)
            gx += dp * k.x[j + 1][i + 1];
            gy += dp * k.y[j + 1][i + 1];
        }
    }

    float g = sqrt(gx * gx + gy * gy);

    // Lower thresholds work better since dp is usually close to 1
    outlines = step(0.2, g);
}

float SG_Luma(float3 c)
{
    // Standard perceptual luminance
    return dot(c, float3(0.2126, 0.7152, 0.0722));
}

void ColorBasedOutlinesFromSamples_float(
    float4 cL,
    float4 cR,
    float4 cU,
    float4 cD,
    float threshold,
    out float outlines)
{
    // Compute luminance for each neighbor sample
    float lL = SG_Luma(cL.rgb);
    float lR = SG_Luma(cR.rgb);
    float lU = SG_Luma(cU.rgb);
    float lD = SG_Luma(cD.rgb);

    // Edge strength based on local luminance contrast
    float lMin = min(min(lL, lR), min(lU, lD));
    float lMax = max(max(lL, lR), max(lU, lD));
    float e = lMax - lMin;

    // Contrast amplification factor
    e *= 5.0;

    // Smooth thresholding for softer edges
    outlines = smoothstep(threshold, threshold * 2.0, e);
}