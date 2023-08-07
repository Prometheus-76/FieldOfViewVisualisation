Shader "Custom/MaskExtension"
{
    Properties
    {
        
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Off

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.uv = v.uv; // UV value of this vertex, passed through rasterizer and interpolated for fragment shader
    
                // Remap (and potentially flip depending on the renderer used) the UVs as the rasterizer expects -1 to 1, not 0 to 1
                float2 remappedUV = (v.uv * 2 - 1) * float2(1, _ProjectionParams.x);
                o.vertex = float4(remappedUV, 0, 1);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float maxPixel = 0;

                for (int horIndex = -1; horIndex <= 1; horIndex++)
                {
                    for (int verIndex = -1; verIndex <= 1; verIndex++)
                    {
                        float2 uvOffset = float2(_MainTex_TexelSize.x * horIndex, _MainTex_TexelSize.y * verIndex);
                        maxPixel = max(maxPixel, tex2D(_MainTex, i.uv + uvOffset).r);
                    }
                }
    
                // Blend brush with the existing mask
                return float4(maxPixel, 0, 0, 0);
            }

            ENDCG
        }
    }
}
