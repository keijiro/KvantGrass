//
// GPGPU kernels for Grass
//
// Position kernel outputs:
// .xyz = position
// .w   = 0
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
    CGINCLUDE

    #include "UnityCG.cginc"
    #include "ClassicNoise3D.cginc"

    float2 _Extent;
    float2 _Scroll;

    float _RandomPitch;
    float3 _RotationNoise;  // freq, amp, time
    float3 _RotationAxis;

    float3 _BaseScale;
    float2 _RandomScale;    // min, max
    float2 _ScaleNoise;     // freq, amp

    // PRNG function
    float nrand(float2 uv, float salt)
    {
        uv += float2(salt, 0);
        return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
    }

    // Quaternion multiplication
    // http://mathworld.wolfram.com/Quaternion.html
    float4 qmul(float4 q1, float4 q2)
    {
        return float4(
            q2.xyz * q1.w + q1.xyz * q2.w + cross(q1.xyz, q2.xyz),
            q1.w * q2.w - dot(q1.xyz, q2.xyz)
        );
    }

    // Get the point bound to the UV
    float2 get_point(float2 uv)
    {
        float2 p = float2(nrand(uv, 0), nrand(uv, 1));
        return (p - 0.5) * _Extent;
    }

    float2 get_point_offs(float2 uv, float2 offs)
    {
        float2 p = float2(nrand(uv, 0), nrand(uv, 1));
        return (frac(p + offs) - 0.5) * _Extent;
    }

    // Random rotation around the Y axis
    float4 random_yaw(float2 uv)
    {
        float a = (nrand(uv, 2) - 0.5) * UNITY_PI * 2;
        float sn, cs;
        sincos(a * 0.5, sn, cs);
        return float4(0, sn, 0, cs);
    }

    // Random pitch rotation
    float4 random_pitch(float2 uv)
    {
        float a1 = (nrand(uv, 3) - 0.5) * UNITY_PI * 2;
        float a2 = (nrand(uv, 4) - 0.5) * _RandomPitch * 2;
        float sn1, cs1, sn2, cs2;
        sincos(a1 * 0.5, sn1, cs1);
        sincos(a2 * 0.5, sn2, cs2);
        return float4(float3(cs1, 0, sn1) * sn2, cs2);
    }

    // Pass 0: Position kernel
    float4 frag_position(v2f_img i) : SV_Target
    {
        float2 p = get_point_offs(i.uv, _Scroll);
        return float4(p.x, 0, p.y, 0);
    }

    // Pass 1: Rotation kernel
    float4 frag_rotation(v2f_img i) : SV_Target
    {
        float4 r1 = random_yaw(i.uv);
        float4 r2 = random_pitch(i.uv);

        // Noise to rotation
        float2 np = get_point(i.uv) * _RotationNoise.x;
        float3 nc = float3(np.xy, _RotationNoise.z);
        float na = cnoise(nc) * _RotationNoise.y;

        // Getting a quaternion of it
        float sn, cs;
        sincos(na * 0.5, sn, cs);
        float4 r3 = float4(_RotationAxis * sn, cs);

        return qmul(r3, qmul(r2, r1));
    }

    // Pass 2: Scale kernel
    float4 frag_scale(v2f_img i) : SV_Target
    {
        // Random scale factor
        float s1 = lerp(_RandomScale.x, _RandomScale.y, nrand(i.uv, 5));

        // Noise to scale factor
        float2 np = get_point(i.uv) * _ScaleNoise.x;
        float3 nc = float3(np.xy, 0.92 /* magic num */);
        float s2 = cnoise(nc) * _ScaleNoise.y;

        return float4(_BaseScale * (s1 + s2), nrand(i.uv, 6));
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
