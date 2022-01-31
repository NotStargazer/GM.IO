Shader "Unlit/ParticleShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
	    SubShader
	    {
		    Tags { "RenderType" = "Transparent" }
		    ZWrite On
		    Blend SrcAlpha OneMinusSrcAlpha
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
	                float2 uv : TEXCOORD0;
					float4 puv : PARTICLECOORD;
	                float4 vertex : SV_POSITION;
					float4 color : COLOR;
	            };

	            sampler2D _MainTex;
	            float4 _MainTex_ST;
				float4 _TopColors[1023];
				float4 _BotColors[1023];
				float4 _Particle_ST[1023];

	            v2f vert (appdata v, uint instanceID: SV_InstanceID)
	            {
					UNITY_SETUP_INSTANCE_ID(v);

	                v2f o;
	                o.vertex = UnityObjectToClipPos(v.vertex);
	                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.puv = _Particle_ST[instanceID];
					o.color = lerp(_BotColors[instanceID], _TopColors[instanceID], v.uv.y);
	                return o;
	            }

	            fixed4 frag (v2f i) : SV_Target
	            {
	                fixed4 col = tex2D(_MainTex, i.uv * i.puv.xy + i.puv.zw);
	                return col * i.color;
	            }
	            ENDCG
	        }
    }
}
