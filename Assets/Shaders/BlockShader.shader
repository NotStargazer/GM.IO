Shader "Unlit/BlockShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_OutlineColor("Block Outline Color", Color) = (1,1,1,1)
		_OutlineThickness("Outline Thickness", Range(0, 1)) = 0.2
		_TestValueC("Outline Test", Vector) = (1,1,1,1)
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float4 color : COLOR;
					float2 uv : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
					float2 uv : TEXCOORD0;
					float2 oluv : OUTLINE;
					float4 oll : OUTLINELINE;
					float4 olc : OUTLINECORNER;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _Colors[1023];
				float4 _OutlinesL[1023];
				float4 _OutlinesC[1023];
				float4 _OutlineColor;
				float4 _TestValueC;
				float _OutlineThickness;

				v2f vert(appdata v, uint instanceID: SV_InstanceID)
				{
					UNITY_SETUP_INSTANCE_ID(v);

					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.oluv = v.uv;
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);

					o.color = _Colors[instanceID];
					o.oll = _OutlinesL[instanceID];
					o.olc = _OutlinesC[instanceID];

					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					const float2 oluv = float2(i.oluv.x * 2 - 1, i.oluv.y * 2 - 1);
					const float2 oluvs = sign(oluv);

					float xl = 0;
					float xr = 0;
					float yd = 0;
					float yu = 0;

					if (abs(oluv.x) > 1 - _OutlineThickness)
					{
						xl = abs((oluvs.x - 1) / 2);
						xr = (oluvs.x + 1) / 2;
					}
					if (abs(oluv.y) > 1 - _OutlineThickness)
					{
						yd = abs((oluvs.y - 1) / 2);
						yu = (oluvs.y + 1) / 2;
					}

					const float bl = i.olc.x * xl * yd;
					const float br = i.olc.y * xr * yd;
					const float tl = i.olc.z * xl * yu;
					const float tr = i.olc.w * xr * yu;

					xl *= i.oll.x;
					xr *= i.oll.y;
					yd *= i.oll.z;
					yu *= i.oll.w;

					float ol = clamp(0, 1, xl + xr + yd + yu);
					float c = clamp(0, 1, bl + br + tl + tr);
					ol = max(ol, c);

					fixed4 neo = tex2D(_MainTex, i.uv);
					neo *= i.color;

					neo = fixed4(lerp(neo.rgb, _OutlineColor.rgb, ol * _OutlineColor.a), neo.a);

					return neo;
				}
				ENDCG
			}
		}
}
