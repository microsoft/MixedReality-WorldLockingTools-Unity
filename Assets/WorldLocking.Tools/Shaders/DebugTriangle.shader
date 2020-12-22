// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

Shader "Debug/DebugTriangle"
{

	Properties
	{
		_MainColor("Main Color",Color) = (0,0,0,1)
		_OutlineColor("Outline Color",Color) = (0,0,0,1)
		_OutlineWidth("Outline Width",Range(0.01,2)) = 0.1
		_VectorOffset("Offset",Vector) = (0,0,0,0)
		[Toggle(SHADER_FEATURE_UsePositionMarker)]
		_UsePositionMarker("Should use little circle to mark headset position?", Int) = 0
		[Toggle(SHADER_FEATURE_UseOutlineAnimation)]
		_UseOutlineAnimation("Pulsing animation to the outline", Int) = 0
		[Toggle(SHADER_FEATURE_UseCircularVignette)]
		_UseCircularVignette("Pulsing circular color darkening on the mesh", Int) = 0
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
			  #pragma shader_feature SHADER_FEATURE_UsePositionMarker
			  #pragma shader_feature SHADER_FEATURE_UseOutlineAnimation
			  #pragma shader_feature SHADER_FEATURE_UseCircularVignette


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
				   float triangleSize : TEXCOORD2;

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
			   int _UseOutlineAnimation;
			   int _UseCircularVignette;

			   v2f vert(appdata v)
			   {
				   v2f o;

				   UNITY_SETUP_INSTANCE_ID(v);
				   UNITY_INITIALIZE_OUTPUT(v2f, o);
				   UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				   o.vertex = UnityObjectToClipPos(v.vertex);
				   o.vertexNoMod = v.vertex;
				   o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				   o.triangleSize += length(v.vertex.xyz - _VectorOffset.xyz);
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

				   float scale = .25f;
				   float3 d1 = float3(distance(i[0].vertexNoMod, (i[1].vertexNoMod + i[2].vertexNoMod) / 2) * scale, 0, 0);
				   float3 d2 = float3(0, distance(i[1].vertexNoMod, (i[0].vertexNoMod + i[2].vertexNoMod) / 2) * scale, 0);
				   float3 d3 = float3(0, 0, distance(i[2].vertexNoMod, (i[1].vertexNoMod + i[0].vertexNoMod) / 2) * scale);

				   g1.bary = d1;
				   g2.bary = d2;
				   g3.bary = d3;

				   stream.Append(g1);
				   stream.Append(g2);
				   stream.Append(g3);
			   }

			   float2 rotateVector(float2 vec, float angle) {
				   float2x2 rotMat = float2x2(
					   cos(angle), -sin(angle),
					   sin(angle), cos(angle)
				   );
				   return mul(rotMat, vec);
			   }

			   fixed4 frag(InterpolatorsGeometry i) : SV_Target
			   {
				   fixed4 mainCol = _MainColor;
				   fixed4 outlineCol = _OutlineColor;
				   float antiAliasing = .0001;

				   // Add circle around headset position

				   #ifdef SHADER_FEATURE_UsePositionMarker

				   float headSetDst = distance(float3(i.data.vertexNoMod.x, 0, i.data.vertexNoMod.z), float3(_HeadSetWorldPosition.x, 0, _HeadSetWorldPosition.z));

				   float circleSize = .21;
				   float animation = (sin(_Time.y * 5) + 1) / 2;
				   circleSize += animation * .04;
				   float circleOutlineWidth = .08;

				   float firstCircle = smoothstep(circleSize, circleSize - antiAliasing, saturate(headSetDst * 2));
				   float secondCircle = smoothstep(circleSize - circleOutlineWidth, circleSize - circleOutlineWidth - antiAliasing, saturate(headSetDst * 2));

				   float circle = saturate(firstCircle - secondCircle);
				   circle *= _UsePositionMarker;

				   mainCol.rgb = mainCol.rgb + circle * 2;
				   mainCol.a = saturate(mainCol.a + circle);

				   #endif

				   // Add circle around headset position

				   // For outline

				   float minBary = min(i.bary.r, i.bary.g);
				   minBary = min(minBary, i.bary.b);

				   // For outline

				   // Some outline animation, and radial vignette

				   float outlineAnimation = .0;
				   float outlineAnimationAmount = .0;

				   #if defined(SHADER_FEATURE_UseOutlineAnimation) || defined(SHADER_FEATURE_UseCircularVignette)

				   float2 rotatedVector = rotateVector(float2(0, 1), _Time.y);
				   float2 vector1 = (float2(i.data.vertexNoMod.x, i.data.vertexNoMod.z) - _VectorOffset.xz);
				   float circularVignette = dot(normalize(rotatedVector), normalize(vector1));
				   circularVignette = acos(circularVignette) / radians(180);
				   float3 c = cross(float3(rotatedVector.x, 0, rotatedVector.y), float3(vector1.x, 0, vector1.y));
				   circularVignette = lerp(circularVignette, (1 - circularVignette) + 1, step(c.y, 0)) / 2;
				   circularVignette = (sin(circularVignette * radians(360) * 6) + 1) / 2;

				   outlineAnimation = 1 + ((sin(_Time.y * 2.5) + 1) / 2);
				   outlineAnimationAmount = 0.25 * _UseOutlineAnimation * (circularVignette * 1);

				   outlineCol.rgb -= circularVignette * .45 * _UseCircularVignette;

				   #endif

				   // Some outline animation, and radial vignette

				   float t = smoothstep((.02 + outlineAnimation * .02 * outlineAnimationAmount) * _OutlineWidth, (.02 + outlineAnimation * .02 * outlineAnimationAmount) * _OutlineWidth - antiAliasing, minBary);
				   return lerp(mainCol, outlineCol, t);
			   }
			   ENDCG
		   }
		}
}
