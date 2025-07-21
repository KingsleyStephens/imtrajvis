Shader "Kingsley/AdditiveTrail"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _FogFactor("Fog Factor", RANGE(0, 1)) = 0.01
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector"="True"}
        LOD 100
        Blend One One
        ZWrite Off  // Disable depth writing

        Pass
        {
            HLSLPROGRAM
            #pragma target 4.5

            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 projectionSpaceVertex : SV_POSITION;
                half  fogCoord : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            half _FogFactor;
            float4 _BaseColor = float4(0.0, 0.0, 0.0, 0.0);

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.projectionSpaceVertex = TransformObjectToHClip(v.vertex.xyz);
                float4 worldSpacePosition = float4(TransformObjectToWorld(v.vertex), 1.0);
                o.fogCoord = InitializeInputDataFog(worldSpacePosition, _FogFactor);
                return o;
            }



            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                half4 finalColor = _BaseColor;
                finalColor.rgb = MixFog(finalColor.rgb, i.fogCoord);
                return finalColor;
            }
            ENDHLSL
        }
    }
}
