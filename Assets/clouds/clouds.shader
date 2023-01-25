Shader "Unlit/clouds"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 ro : TEXCOORD1;
                float3 hitPos : TEXCOORD2;
            };
            

            Texture3D<float4> worley;
            sampler2D _MainTex;
            float4 _MainTex_ST;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.ro = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
                o.hitPos = v.vertex;
                return o;
            }

 /*           float getDist(float3 p) {
                float distance = length(p) - 0.5;
                return distance;
            }


#define MAX_STEPS 100
#define MAX_DIST 100
#define SURF_DIST 0.001
            float raymarch(float3 ro, float3 rd) {
                float distance = 0;
                float distance_surface;
                for (int i = 0; i < MAX_STEPS; i++) {
                    float3 p = ro + rd * distance;
                    distance_surface = getDist(p);
                    distance += distance_surface;
                    if (distance_surface < SURF_DIST || distance > MAX_DIST) {
                        break;
                    }
                }
                return distance;
            }*/

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture


                float3 col = tex2D(_MainTex, i.uv);

                //col.rgb = rd;
                return float4(col,1);
            }
            ENDCG
        }
    }
}
