Shader "Custom/GeometryStencil"
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

            struct appdata
            {
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;

                // Remap (and potentially flip depending on the renderer used) the UVs as the rasterizer expects -1 to 1, not 0 to 1
                float2 remappedUV = (v.uv * 2 - 1) * float2(1, _ProjectionParams.x);
                o.vertex = float4(remappedUV, 0, 1);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Return static colour
                return float4(1, 0, 0, 0);
            }

            ENDCG
        }
    }
}
