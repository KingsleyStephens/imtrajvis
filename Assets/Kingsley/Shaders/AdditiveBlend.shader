Shader "Kingsley/AdditiveBlend"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        _Alpha("Alpha", RANGE(0, 1)) = 1.0
        _FogFactor("Fog Factor", RANGE(0, 1)) = 0.01
		_WireThickness ("Wire Thickness", RANGE(0, 800)) = 100
		_WireSmoothness ("Wire Smoothness", RANGE(0, 20)) = 3
		_WireColor ("Wire Color", Color) = (0.0, 1.0, 0.0, 1.0)
		_MaxTriSize ("Max Tri Size", RANGE(0, 200)) = 25
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector"="True"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
         
        Pass
        {
            HLSLPROGRAM
            #pragma target 4.5
            #pragma require geometry

            // https://github.com/Chaser324/unity-wireframe/blob/master/Assets/Wireframe/Shaders/Wireframe.cginc

            #pragma vertex vert
            #pragma geometry geom
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

            struct v2g
            {
                float4 projectionSpaceVertex : SV_POSITION;
                float4 worldSpacePosition : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            struct g2f
            {
                float4 projectionSpaceVertex : SV_POSITION;
                float4 worldSpacePosition : TEXCOORD0;
                float4 dist : TEXCOORD1;
                float4 area : TEXCOORD2;
                half  fogCoord : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2g vert (appdata v)
            {
                v2g o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.projectionSpaceVertex = TransformObjectToHClip(v.vertex.xyz);
                o.worldSpacePosition = float4(TransformObjectToWorld(v.vertex), 1.0);
                return o;
            }

            CBUFFER_START(UnityPerMaterial)
                half4 _WireBaseColor;
            CBUFFER_END
            half _FogFactor;


            float _WireThickness = 100;
            float _WireSmoothness = 3;
            float4 _WireColor = float4(0.0, 1.0, 0.0, 1.0);
            float4 _BaseColor = float4(0.0, 0.0, 0.0, 0.0);
            float _MaxTriSize = 25.0;
            float _Alpha = 1.0;

            [maxvertexcount(3)]
            void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream)
            {
                float2 p0 = i[0].projectionSpaceVertex.xy / i[0].projectionSpaceVertex.w;
                float2 p1 = i[1].projectionSpaceVertex.xy / i[1].projectionSpaceVertex.w;
                float2 p2 = i[2].projectionSpaceVertex.xy / i[2].projectionSpaceVertex.w;

                float2 edge0 = p2 - p1;
                float2 edge1 = p2 - p0;
                float2 edge2 = p1 - p0;

                float4 worldEdge0 = i[0].worldSpacePosition - i[1].worldSpacePosition;
                float4 worldEdge1 = i[1].worldSpacePosition - i[2].worldSpacePosition;
                float4 worldEdge2 = i[0].worldSpacePosition - i[2].worldSpacePosition;

                // To find the distance to the opposite edge, we take the
                // formula for finding the area of a triangle Area = Base/2 * Height, 
                // and solve for the Height = (Area * 2)/Base.
                // We can get the area of a triangle by taking its cross product
                // divided by 2.  However we can avoid dividing our area/base by 2
                // since our cross product will already be double our area.
                float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);
                float wireThickness = 800 - _WireThickness;

                g2f o;

                o.area = float4(0, 0, 0, 0);
                o.area.x = max(length(worldEdge0), max(length(worldEdge1), length(worldEdge2)));

                o.worldSpacePosition = i[0].worldSpacePosition;
                o.projectionSpaceVertex = i[0].projectionSpaceVertex;
                o.dist.xyz = float3( (area / length(edge0)), 0.0, 0.0) * o.projectionSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                o.fogCoord = InitializeInputDataFog(float4(o.worldSpacePosition.xyz, 1.0), _FogFactor);
                UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[0], o);
                triangleStream.Append(o);

                o.worldSpacePosition = i[1].worldSpacePosition;
                o.projectionSpaceVertex = i[1].projectionSpaceVertex;
                o.dist.xyz = float3(0.0, (area / length(edge1)), 0.0) * o.projectionSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                o.fogCoord = InitializeInputDataFog(float4(o.worldSpacePosition.xyz, 1.0), _FogFactor);
                UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[1], o);
                triangleStream.Append(o);

                o.worldSpacePosition = i[2].worldSpacePosition;
                o.projectionSpaceVertex = i[2].projectionSpaceVertex;
                o.dist.xyz = float3(0.0, 0.0, (area / length(edge2))) * o.projectionSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                o.fogCoord = InitializeInputDataFog(float4(o.worldSpacePosition.xyz, 1.0), _FogFactor);
                UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[2], o);
                triangleStream.Append(o);
            }

            half4 frag (g2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float minDistanceToEdge = min(i.dist[0], min(i.dist[1], i.dist[2])) * i.dist[3];

                // Early out if we know we are not on a line segment.
                if(minDistanceToEdge > 0.9 || i.area.x > _MaxTriSize)
                {
                    half4 col = _WireColor;
                    // apply fog
                    col.rgb = MixFog(col.rgb, i.fogCoord);
                    col.a = _Alpha;
                    return col;
                }

                // Smooth our line out
                float t = exp2(_WireSmoothness * -1.0 * minDistanceToEdge * minDistanceToEdge);
                half4 finalColor = lerp(_BaseColor, _WireColor, t);
                finalColor.rgb = MixFog(finalColor.rgb, i.fogCoord);
                finalColor.a = _Alpha;
                return finalColor;
            }
            ENDHLSL
        }
    }
}
