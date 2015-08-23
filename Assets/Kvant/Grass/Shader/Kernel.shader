//
// GPGPU kernels for Grass
//
// Position kernel outputs:
// .xyz = position
// .w   = random value (0-1)
//
// Rotation kernel outputs:
// .xyzw = rotation (quaternion)
//
// Scale kernel outputs:
// .xyz = scale factor
// .w   = random value (0-1)
//
Shader "Hidden/Kvant/Grass/Kernel"
{
    Properties
    {
        _MainTex("-", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"
    #include "ClassicNoise3D.cginc"

    sampler2D _MainTex;
    float2 _Extent;
    float3 _BaseScale;
    float2 _RandomScale;    // min, max
    float _RandomPitch;
    float3 _RotationNoise;  // freq, amp, time
    float2 _ScaleNoise;     // freq, amp

    // PRNG function.
    float nrand(float2 uv, float salt)
    {
        uv += float2(salt, 0);
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    // Quaternion multiplication.
    // http://mathworld.wolfram.com/Quaternion.html
    float4 qmul(float4 q1, float4 q2)
    {
        return float4(
            q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
            q1.w * q2.w - dot(q1.xyz, q2.xyz)
        );
    }

    float4 random_yaw(float2 uv)
    {
        float a = (nrand(uv, 3) - 0.5) * UNITY_PI * 2;
        float sn, cs;
        sincos(a * 0.5, sn, cs);
        return float4(0, sn, 0, cs);
    }

    float4 random_pitch(float2 uv)
    {
        float a1 = (nrand(uv, 4) - 0.5) * UNITY_PI * 2;
        float a2 = (nrand(uv, 5) - 0.5) * _RandomPitch;
        float sn1, cs1, sn2, cs2;
        sincos(a1 * 0.5, sn1, cs1);
        sincos(a2 * 0.5, sn2, cs2);
        return float4(float3(cs1, 0, sn1) * sn2, cs2);
    }

    float3 get_position(float2 uv)
    {
        float x = (nrand(uv, 0) - 0.5) * _Extent.x;
        float z = (frac(nrand(uv, 1) + _Time.x * 2.8) - 0.5) * _Extent.y;
        return float3(x, 0, z);
    }

    // Pass 0: Position kernel
    float4 frag_position(v2f_img i) : SV_Target
    {
        float2 uv = i.uv;
        return float4(get_position(uv), nrand(uv, 2));
    }

    // Pass 1: Rotation kernel
    float4 frag_rotation(v2f_img i) : SV_Target
    {
        float2 uv = i.uv;

        float4 r1 = random_yaw(uv);
        float4 r2 = random_pitch(uv);

        float x = (nrand(uv, 0) - 0.5) * _Extent.x;
        float z = (nrand(uv, 1) - 0.5) * _Extent.y;
        float a3 = cnoise(float3(x * 2.3, z * 2.3, 100 + _Time.x * 10)) * 1.2;

        float sn, cs;
        sincos(a3 * 0.5, sn, cs);
        float4 r3 = float4(float3(1, 0, 0) * sn, cs);

        return qmul(r3, qmul(r2, r1));
    }

    // Pass 2: Scale kernel
    float4 frag_scale(v2f_img i) : SV_Target
    {
        float2 uv = i.uv;

        // Random scale factor
        float vari = lerp(_RandomScale.x, _RandomScale.y, nrand(uv, 6));

        float x = (nrand(uv, 0) - 0.5) * _Extent.x;
        float z = (nrand(uv, 1) - 0.5) * _Extent.y;
        vari += cnoise(float3(x, z, 0) * 0.5) * 1.4;

        return float4(_BaseScale * vari, nrand(uv, 7));
    }

    ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_position
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_rotation
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag_scale
            ENDCG
        }
    }
}
