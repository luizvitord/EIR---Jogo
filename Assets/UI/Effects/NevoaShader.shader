Shader "Custom/NevoaShader_URP"
{
    Properties
    {
        _MainTex ("Noise Texture", 2D) = "white" {}
        [HDR] _Color ("Cor da Nevoa", Color) = (1, 1, 1, 1)
        _Density ("Densidade Total", Range(0.0, 2.0)) = 0.25
        _NoiseScale ("Escala do Ruido", Range(0.1, 5.0)) = 1.0
        _Contrast ("Contraste da Fumaca", Range(0.1, 10.0)) = 2.0
        _SpeedX ("Velocidade X", Float) = 0.02
        _SpeedY ("Velocidade Y", Float) = 0.01
    }
    SubShader
    {
        // Tag importante para o URP identificar como transparente
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _Color;
            float _Density, _NoiseScale, _Contrast, _SpeedX, _SpeedY;

            Varyings vert (Attributes input) {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv * _NoiseScale;
                return output;
            }

            half4 frag (Varyings input) : SV_Target {
                float2 animatedUV = input.uv + float2(_SpeedX, _SpeedY) * _Time.y;
                float noise = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, animatedUV).r;
                float fog = saturate(pow(noise, _Contrast));
                half4 finalColor = _Color;
                finalColor.a *= fog * _Density;
                return finalColor;
            }
            ENDHLSL
        }
    }
}