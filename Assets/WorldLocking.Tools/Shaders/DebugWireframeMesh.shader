Shader "Debug/WireframeMesh"
{
    Properties
    {
        _MainColor("Main Color",Color) = (0,0,0,1)
        _OutlineColor("Outline Color",Color) = (0,0,0,1)
        _OutlineWidth("Outline Width",Range(0.01,2)) = 0.1
    }
        SubShader
    {
       Tags { "RenderType" = "Transparent" "Queue" = "Geometry" }
       Cull Off
       ZWrite Off

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma target 4.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geo

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float _OutlineWidth;
            fixed4 _MainColor;
            fixed4 _OutlineColor;
            float4 _MainTex_ST;

            struct InterpolatorsGeometry {
                v2f data;
                float3 bary : TEXCOORD9;
            };

            [maxvertexcount(3)]
            void geo(triangle v2f i[3], inout TriangleStream<InterpolatorsGeometry> stream) {

                InterpolatorsGeometry g1, g2, g3;
                g1.data = i[0];
                g2.data = i[1];
                g3.data = i[2];

                g1.bary = float3(1, 0, 0);
                g2.bary = float3(0, 1, 0);
                g3.bary = float3(0, 0, 1);

                stream.Append(g1);
                stream.Append(g2);
                stream.Append(g3);
            }

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(InterpolatorsGeometry i) : SV_Target
            {
                float minBary = min(i.bary.r,i.bary.g);
                minBary = min(minBary, i.bary.b);

                fixed4 mainCol = _MainColor;

                float vignette = 1 - dot(normalize(i.bary), normalize(float3(.33, .33, .33)));
                mainCol.rgb -= vignette;

                return lerp(mainCol, _OutlineColor, smoothstep(0.02 * _OutlineWidth, 0.02 * _OutlineWidth - 0.001, minBary));
            }
            ENDCG
        }
    }
}
