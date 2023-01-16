Shader "Particles/AdditiveLPPV" {
	Properties{
		_MainTex("Particle Texture", 2D) = "white" {}
		_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	}

		Category{
			Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			Blend SrcAlpha One
			ColorMask RGB
			Cull Off Lighting Off ZWrite Off

			SubShader {
				Pass {

					CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag
					#pragma multi_compile_particles
					#pragma multi_compile_fog

					// Specify the target
					#pragma target 3.0

					#include "UnityCG.cginc"

					// You must include this header to have access to ShadeSHPerPixel
					#include "UnityStandardUtils.cginc"

					fixed4 _TintColor;
					sampler2D _MainTex;

					struct appdata_t {
						float4 vertex : POSITION;
						float3 normal : NORMAL;
						fixed4 color : COLOR;
						float2 texcoord : TEXCOORD0;
					};

					struct v2f {
						float4 vertex : SV_POSITION;
						fixed4 color : COLOR;
						float2 texcoord : TEXCOORD0;
						UNITY_FOG_COORDS(1)
						float3 worldPos : TEXCOORD2;
						float3 worldNormal : TEXCOORD3;
					};

					float4 _MainTex_ST;

					v2f vert(appdata_t v)
					{
						v2f o;
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.worldNormal = UnityObjectToWorldNormal(v.normal);
						o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
						o.color = v.color;
						o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
						UNITY_TRANSFER_FOG(o,o.vertex);
						return o;
					}

					fixed4 frag(v2f i) : SV_Target
					{
						half3 currentAmbient = half3(0, 0, 0);
						half3 ambient = ShadeSHPerPixel(i.worldNormal, currentAmbient, i.worldPos);
						fixed4 col = _TintColor * i.color * tex2D(_MainTex, i.texcoord);
						col.xyz += ambient;
						UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0)); // fog towards black due to our blend mode
						return col;
					}
					ENDCG
				}
			}
		}
}