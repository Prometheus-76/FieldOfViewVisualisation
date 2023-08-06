Shader "Custom/SurfacePainter"
{
    Properties
    {
        
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Off

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float2 _BrushPosition;
            float _BrushRadius;
            float _BrushHardness;
            float _BrushStrength;

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

            float evaluateBrush(float2 position, float2 center, float radius, float hardness)
            {
                float distanceFromBrushCenter = distance(center, position);
                return 1.0f - smoothstep(radius * hardness, radius, distanceFromBrushCenter);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex); // Where is the vertex in world-space?
                o.uv = v.uv; // UV value of this vertex, passed through rasterizer and interpolated for fragment shader
    
                // Remap (and potentially flip depending on the renderer used) the UVs as the rasterizer expects -1 to 1, not 0 to 1
                float2 remappedUV = (v.uv * 2 - 1) * float2(1, _ProjectionParams.x);
                o.vertex = float4(remappedUV, 0, 1);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 existingColour = tex2D(_MainTex, i.uv);
    
                // Determine pixel brightness based on proximity to brush, and brush settings
                float baseBrush = evaluateBrush(i.worldPos.xy, _BrushPosition, _BrushRadius, _BrushHardness);
                float edge = baseBrush * _BrushStrength;
    
                // Blend brush with the existing mask
                return lerp(existingColour, float4(1, 0, 0, 0), edge);
            }

            ENDCG
        }
    }
}
