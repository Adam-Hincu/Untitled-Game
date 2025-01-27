Shader "Custom/GradientShader"
{
    Properties
    {
        _ColorA ("Color A", Color) = (1,1,1,1)
        _ColorB ("Color B", Color) = (0,0,0,1)
        [Space(10)]
        [Header(Gradient Controls)]
        _GradientDirection ("Direction (Degrees)", Range(0, 360)) = 0
        [Space(5)]
        _ScaleX ("Scale X", Range(0.1, 10)) = 1.0
        _ScaleY ("Scale Y", Range(0.1, 10)) = 1.0
        [Space(5)]
        _OffsetX ("Offset X", Range(-1, 1)) = 0
        _OffsetY ("Offset Y", Range(-1, 1)) = 0
        [Space(5)]
        _GradientSmoothness ("Smoothness", Range(0.01, 10)) = 1.0
        [Space(10)]
        [Header(Material Properties)]
        _Smoothness ("Surface Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog // Make fog work
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float fogFactor : TEXCOORD1;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _ColorA;
                float4 _ColorB;
                float _GradientDirection;
                float _ScaleX;
                float _ScaleY;
                float _OffsetX;
                float _OffsetY;
                float _GradientSmoothness;
                float _Smoothness;
                float _Metallic;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                
                // Calculate fog factor
                OUT.fogFactor = ComputeFogFactor(OUT.positionCS.z);
                
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // Apply scale and offset to UV
                float2 position = IN.uv - float2(0.5 + _OffsetX, 0.5 + _OffsetY);
                position.x *= _ScaleX;
                position.y *= _ScaleY;
                
                // Convert gradient direction from degrees to radians
                float radian = radians(_GradientDirection);
                float2 dir = float2(cos(radian), sin(radian));
                
                // Calculate gradient value
                float t = dot(position, dir) + 0.5;
                
                // Improved smoothness calculation
                float smoothness = 1.0 / max(_GradientSmoothness, 0.001);
                t = saturate((t - 0.5) * smoothness + 0.5);
                
                // Interpolate between colors
                float4 finalColor = lerp(_ColorA, _ColorB, t);
                
                // Mix fog with final color
                float3 foggedColor = MixFog(finalColor.rgb, IN.fogFactor);
                return float4(foggedColor, finalColor.a);
            }
            ENDHLSL
        }
    }
}
