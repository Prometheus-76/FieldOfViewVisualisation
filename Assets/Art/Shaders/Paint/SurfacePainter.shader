Shader "Custom/SurfacePainter"
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
            float4 _MainTex_ST;

            float2 _BrushPosition;
            float _BrushInnerRadius;
            float _BrushOuterRadius;
            sampler2D _BrushTexture;
            float _BrushTextureStrength;
            float _BrushTextureScale;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            float inverselerp(float from, float to, float t)
            {
                return (t - from) / (to - from);
            }

            float evaluateBrush(float2 position)
            {
                float distanceFromBrushCenter = distance(_BrushPosition, position);
                float baseBrush = 1.0f - clamp(inverselerp(_BrushInnerRadius, _BrushOuterRadius, distanceFromBrushCenter), 0.0f, 1.0f);
                
                // Mask brush texture
                float2 textureUV = frac(position * _BrushTextureScale);
                float texturedBrush = baseBrush * tex2D(_BrushTexture, textureUV) * _BrushTextureStrength;
    
                return clamp(baseBrush + texturedBrush, 0.0f, 1.0f);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); // Where is the vertex in world-space?
                o.uv = v.uv;
    
                // Remap (and flip depending on graphics API) the UVs as the rasterizer expects -1 to 1, not 0 to 1
                float2 remappedUV = (v.uv * 2 - 1) * float2(1, _ProjectionParams.x);
                o.vertex = float4(remappedUV, 0, 1);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 existingColour = tex2D(_MainTex, i.uv);
    
                // Determine the brightness of this pixel in accordance with the brush position and settings
                float baseBrush = evaluateBrush(i.worldPos.xy);
    
                // Blend brush with the existing mask
                return max(existingColour, baseBrush);
            }

            ENDCG
        }
    }
}
