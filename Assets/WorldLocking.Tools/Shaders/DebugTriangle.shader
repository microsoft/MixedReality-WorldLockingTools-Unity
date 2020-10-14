Shader "Debug/DebugTriangle"
{
    Properties
    {
        _MainColor("Main Color",Color) = (0,0,0,1)
        _OutlineColor("Outline Color",Color) = (0,0,0,1)
        _OutlineWidth("Outline Width",Range(0.01,2)) = 0.1
        _VectorOffset("Offset",Vector) = (0,0,0,0)
        [Toggle]
        _UsePositionMarker("Should use little circle to mark headset position?", Int) = 0
        _HeadSetWorldPosition("HeadsetWorldPosition", Vector) = (0,0,0,0)
    }
        SubShader
    {
       Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
       Cull Off
       ZWrite Off

       Blend SrcAlpha OneMinusSrcAlpha

       Pass
       {
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
               float4 vertexNoMod : TEXCOORD1;

               UNITY_VERTEX_INPUT_INSTANCE_ID
               UNITY_VERTEX_OUTPUT_STEREO
           };

           sampler2D _MainTex;
           float4 _VectorOffset;
           float4 _HeadSetWorldPosition;
           float _OutlineWidth;
           fixed4 _OutlineColor;
           fixed4 _MainColor;
           float4 _MainTex_ST;
           int _UsePositionMarker;

           v2f vert(appdata v)
           {
               v2f o;

               UNITY_SETUP_INSTANCE_ID(v);
               UNITY_INITIALIZE_OUTPUT(v2f, o);
               UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

               o.vertex = UnityObjectToClipPos(v.vertex);
               o.vertexNoMod = v.vertex;
               o.uv = TRANSFORM_TEX(v.uv, _MainTex);
               return o;
           }

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

           fixed4 frag(InterpolatorsGeometry i) : SV_Target
           {
               fixed4 mainCol = _MainColor;

               float headSetDst = distance(float3(i.data.vertexNoMod.x,0, i.data.vertexNoMod.z), float3(_HeadSetWorldPosition.x,0, _HeadSetWorldPosition.z));

               // Darken mesh around headset position

               float exponent = 1 - saturate(headSetDst * 2);
               exponent = smoothstep(0, 0.85, exponent);
               mainCol.rgb -= exponent * .5 * _UsePositionMarker;

               // Darken mesh around headset position

               // Add circle around headset position
               
               float circleSize = .21;
               float animation = (sin(_Time.y * 5) + 1) / 2;
               circleSize += animation * .04;
               float circleOutlineWidth = .08;
               float antiAliasing = .0001;

               float firstCircle = smoothstep(circleSize, circleSize - antiAliasing, saturate(headSetDst * 2));
               float secondCircle = smoothstep(circleSize - circleOutlineWidth, circleSize - circleOutlineWidth - antiAliasing, saturate(headSetDst * 2));

               float circle = saturate(firstCircle - secondCircle);
               circle *= _UsePositionMarker;

               mainCol.rgb = mainCol.rgb + circle * 2;
               mainCol.a = saturate(mainCol.a + circle);
                
               // Add circle around headset position

               fixed4 outlineCol = _OutlineColor;

               float minBary = min(i.bary.r, i.bary.g);
               minBary = min(minBary, i.bary.b);

               float maxBary = max(i.bary.r, i.bary.g);
               maxBary = max(maxBary, i.bary.b);

               float vignette = 1 - dot(normalize(i.bary), normalize(float3(.33, .33, .33)));
               mainCol.rgb -= vignette;

               return lerp(mainCol, outlineCol, smoothstep(0.02 * _OutlineWidth, 0.02 * _OutlineWidth - 0.001, minBary));
           }
           ENDCG
       }
    }
}
