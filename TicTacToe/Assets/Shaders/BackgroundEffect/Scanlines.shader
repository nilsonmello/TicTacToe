Shader "Custom/Scanlines"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _PaletteTex ("Palette Texture", 2D) = "white" {}
        _Speed ("Speed", float) = 1.0
        _LineDensity ("Line Density", float) = 200.0
        _LineWidth ("Line Width", float) = 0.2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _PaletteTex;
            float _Speed;
            float _LineDensity;
            float _LineWidth;
            float4 _MainTex_ST;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                float lineDensity = _LineDensity;
                float lineWidth = _LineWidth;
                float time = _Time.y * _Speed;

                float jitter = sin(time * 20.0 + uv.y * 100.0) * 0.003;

                float animatedY = uv.y + frac(time);
                float lineValue = fmod(animatedY * lineDensity + jitter, 1.0);

                float flicker = 0.8 + 0.2 * sin(time * 60.0 + uv.y * 500.0);

                float strongFlicker = step(0.98, frac(time * 0.5)) * 0.5;

                float mask = 0.0;
                for (int k = -2; k <= 2; ++k) {
                    float offset = float(k) * 0.0025; 
                    float neighborLine = fmod((animatedY + offset) * lineDensity + jitter, 1.0);
                    mask += step(1.0 - lineWidth, neighborLine);
                }
                mask /= 5.0;
                mask *= (flicker + strongFlicker);

                fixed4 scanlineColor = fixed4(0, 0, 0, mask);
                return scanlineColor;
            }
            ENDCG
        }
    }
}
