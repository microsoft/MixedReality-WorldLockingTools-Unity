Shader "Debug/DebugTriangle"
{
    Properties
    {
        _MainColor("Main Color",Color) = (0,0,0,1)
        _OutlineColor("Outline Color",Color) = (0,0,0,1)
        _OutlineWidth("Outline Width",Range(0.01,1)) = 0.1
        _VectorOffset("Offset",Vector) = (0,0,0,0)
    }
    SubShader
    {
       Tags { "RenderType"="Opaque" }

       Cull Off

        Pass
        {
           Stencil {
             Ref 3
             Comp Always
             Pass Replace
           }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
            float4 _VectorOffset;
            float _OutlineWidth;
            fixed4 _MainColor;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4 scaledVertex = ((v.vertex - _VectorOffset) * (1 - _OutlineWidth)) + _VectorOffset;
                o.vertex = UnityObjectToClipPos(scaledVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _MainColor;
            }
            ENDCG
        }

       Pass
       {
           Stencil {
            Ref 3
            Comp NotEqual
           }

           CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag

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
           float _Weight;
           fixed4 _OutlineColor;
           float _OutlineWidth;
           float4 _VectorOffset;
           float4 _MainTex_ST;

           v2f vert(appdata v)
           {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4 scaledVertex = ((v.vertex - _VectorOffset) * (1)) + _VectorOffset;
                o.vertex = UnityObjectToClipPos(scaledVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

           fixed4 frag(v2f i) : SV_Target
           {
              return _OutlineColor;
           }
           ENDCG
        }
    }
}
