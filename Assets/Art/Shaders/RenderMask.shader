Shader "Custom/RenderMask"
{
    Properties { }
    
    // This object exists only to write to the stencil buffer
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
        }

        Pass
        {
            // Prevents the object from writing to the screen's pixels, and to the depth buffer
            Blend Zero One
            ZWrite Off
        }
    }
}
