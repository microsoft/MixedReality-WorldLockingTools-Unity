// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

Shader "Debug/DebugWeight"
{
    Properties
    {
        _Weight("Weight",Range(0,1)) = 0
        _OutlineColor("Outline Color",Color) = (0,0,0,1)
        _OutlineWidth("Outline Width",Range(0.01,1)) = 0.1
        _VectorOffset("Offset",Vector) = (0,0,0,0)
    }
        SubShader
    {
       Tags { "RenderType" = "Opaque" "Queue" = "Geometry+1" }
       ZWrite Off

       Pass
       {
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
           float _OutlineWidth;
           fixed4 _OutlineColor;
           float4 _VectorOffset;
           float4 _MainTex_ST;

           v2f vert(appdata v)
           {
                v2f o;


                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4 scaledVertex = ((v.vertex - _VectorOffset) * (_Weight + _OutlineWidth)) + _VectorOffset;
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


        Pass
        {
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
            float4 _VectorOffset;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4 scaledVertex = ((v.vertex - _VectorOffset) * _Weight) + _VectorOffset;
                o.vertex = UnityObjectToClipPos(scaledVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 red = fixed4(1.0, 0, 0, 1);
                fixed4 green = fixed4(0, 1.0, 0, 1);
                fixed4 yellow = fixed4(1.0, 1.0, 0, 1);

                float redWeight = smoothstep(0.5,1,_Weight);
                float yellowWeight = 1 - (abs(0.5 - _Weight) * 2);
                float greenWeight = smoothstep(0.5,0,_Weight);

                float sum = redWeight + yellowWeight + greenWeight;

                redWeight = redWeight / sum;
                yellowWeight = yellowWeight / sum;
                greenWeight = greenWeight / sum;

                return red * redWeight + yellow * yellowWeight + green * greenWeight;
            }
            ENDCG
        }

    }
}
