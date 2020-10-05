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
       Tags { "RenderType"="Opaque" }

        Pass
        {
           Stencil {
             Ref 2
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _Weight;
            float4 _VectorOffset;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                float4 scaledVertex = ((v.vertex - _VectorOffset) * _Weight) + _VectorOffset;
                o.vertex = UnityObjectToClipPos(scaledVertex);
               // o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //fixed4 col = tex2D(_MainTex, i.uv);
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

       Pass
       {
           Stencil {
            Ref 2
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
           };

           struct v2f
           {
               float2 uv : TEXCOORD0;
               float4 vertex : SV_POSITION;
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
    }
}
