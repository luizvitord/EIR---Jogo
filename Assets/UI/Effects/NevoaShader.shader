Shader "Custom/NevoaShader_Tunado"
{
    Properties
    {
        _MainTex ("Noise Texture", 2D) = "white" {}
        _Color ("Cor da Nevoa", Color) = (1, 1, 1, 1)
        _Density ("Densidade Total", Range(0.0, 2.0)) = 0.25
        _NoiseScale ("Escala do Ruido", Range(0.1, 5.0)) = 1.0 // ---> NOVO
        _Contrast ("Contraste da Fumaca", Range(0.1, 10.0)) = 2.0 // ---> NOVO
        _SpeedX ("Velocidade X", Float) = 0.02
        _SpeedY ("Velocidade Y", Float) = 0.01
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Density;
            float _NoiseScale; // ---> NOVO
            float _Contrast;   // ---> NOVO
            float _SpeedX;
            float _SpeedY;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Aplica a escala do ruído na UV
                o.uv = v.uv * _NoiseScale + _MainTex_ST.zw;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Movimento da névoa
                float2 animatedUV = i.uv + float2(_SpeedX, _SpeedY) * _Time.y;

                // Lê o ruído
                fixed4 noiseColor = tex2D(_MainTex, animatedUV);
                float noise = noiseColor.r;

                // ---> NOVA MATEMÁTICA: Usa o contraste para criar bordas
                // Usamos a função 'pow' (potência) para aumentar drasticamente o contraste
                float fog = saturate(pow(noise, _Contrast));

                fixed4 finalColor = _Color;
                // Aplica a densidade e o novo cálculo de névoa no Alpha
                finalColor.a = _Color.a * fog * _Density;

                return finalColor;
            }
            ENDCG
        }
    }
}