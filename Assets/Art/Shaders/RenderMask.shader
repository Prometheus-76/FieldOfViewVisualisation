Shader "Custom/RenderMask"
{
    Properties { }
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
        }

        Pass
        {
            Blend Zero One
            ZWrite Off
        }
    }
}
