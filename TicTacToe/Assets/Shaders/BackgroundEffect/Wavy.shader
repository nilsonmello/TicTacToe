Shader "Custom/Wavy"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _PaletteTex ("Palette Texture", 2D) = "white" {}
        _Speed ("Speed", float) = 3.0
        _Frequency ("Frequency", float) = 10.0
        _Amplitude ("Amplitude", float) = 0.05
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
            float _Frequency;
            float _Amplitude;
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

                uv.x += sin(uv.y * _Frequency + _Time.y * _Speed) * _Amplitude;

                fixed4 spriteColor = tex2D(_MainTex, uv);

                float t = frac(uv.x * 3.0 + _Time.y * 0.5);
                t = saturate(t);

                fixed3 paletteColor = tex2D(_PaletteTex, float2(t, 0.5)).rgb;

                return fixed4(paletteColor, spriteColor.a);
            }
            ENDCG
        }
    }
}
