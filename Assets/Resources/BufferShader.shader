﻿
Shader "Custom/BufferShader"
{
    SubShader
    {
       Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
       Pass
       {
	        Cull Off
            ZWrite On
            ZTest LEqual

            CGPROGRAM
            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            uniform StructuredBuffer<float3> vertexBuffer;
            uniform StructuredBuffer<float3> normalBuffer;
            uniform StructuredBuffer<int> indexBuffer;

            uniform matrix model_matrix;
            uniform matrix inv_model_matrix;
 
            struct v2f
            {
                float4  pos : SV_POSITION;
                float4  diff : COLOR0;
            };
 
            v2f vert(uint id : SV_VertexID)
            {
                float4 pos = float4(vertexBuffer[indexBuffer[id]], 1);
		        float3 normal = normalize(mul(normalBuffer[indexBuffer[id]], inv_model_matrix));
 
                v2f OUT;
                OUT.pos = UnityObjectToClipPos(mul(model_matrix, pos));
		        OUT.diff = max(0, dot(normal, _WorldSpaceLightPos0.xyz));
                return OUT;
            }
 
            fixed4 frag(v2f IN) : SV_Target
            {
                return fixed4(0,0,1,1)*IN.diff;
            }
 
            ENDCG
        }
    }
}