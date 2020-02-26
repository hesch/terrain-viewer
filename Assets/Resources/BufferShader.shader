
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
            uniform StructuredBuffer<uint> indexBuffer;

            uniform matrix model_matrix;
            uniform matrix inv_model_matrix;
 
            struct v2f
            {
                float4  pos : SV_POSITION;
                float4  normal : NORMAL0;
            };
 
            v2f vert(uint id : SV_VertexID)
            {
                uint index = indexBuffer[id];
                float4 pos = float4(vertexBuffer[index], 1);
		        float3 normal = normalize(mul(normalBuffer[index], inv_model_matrix));
 
                v2f OUT;
                OUT.pos = UnityObjectToClipPos(mul(model_matrix, pos));
                OUT.normal = float4(normal, 1);
                return OUT;
            }
 
            fixed4 frag(v2f IN) : SV_Target
            {
                return fixed4(0,0,1,1)*max(0, dot(IN.normal.xyz, _WorldSpaceLightPos0.xyz));
            }
 
            ENDCG
        }
    }
}
