Shader "Unlit/clouds"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    SubShader
    {
        //Tags { "Queue"="Transparent" "RenderType"="Transaprent" }
        //LOD 100
        //Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "UnityCG.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;

                float3 rd : TEXCOORD1;
            };


            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                float3 rd = mul(unity_CameraInvProjection, float4(v.texcoord * 2 - 1, 0, -1));
                o.rd = mul(unity_CameraToWorld, float4(rd, 0));

                return o;
            }

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;
            sampler3D _Worley;
            float alpha_range;
            float3 boxMin;
            float3 boxMax;


            #define MAX_STEPS 100
            #define MAX_DIST 100
            #define SURF_DIST 0.001

            float getDist(float3 p, float3 s) {
                float3 q = abs(p) - s;
                return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);

            }


            float2 rayBoxDst(float3 boundsMin, float3 boundsMax, float3 rayOrigin, float3 invRaydir) {
                // Adapted from: http://jcgt.org/published/0007/03/04/
                float3 t0 = (boundsMin - rayOrigin) * invRaydir;
                float3 t1 = (boundsMax - rayOrigin) * invRaydir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);

                float dstA = max(max(tmin.x, tmin.y), tmin.z);
                float dstB = min(tmax.x, min(tmax.y, tmax.z));

                // CASE 1: ray intersects box from outside (0 <= dstA <= dstB)
                // dstA is dst to nearest intersection, dstB dst to far intersection

                // CASE 2: ray intersects box from inside (dstA < 0 < dstB)
                // dstA is the dst to intersection behind the ray, dstB is dst to forward intersection

                // CASE 3: ray misses box (dstA > dstB)

                float dstToBox = max(0, dstA);
                float dstInsideBox = max(0, dstB - dstToBox);
                return float2(dstToBox, dstInsideBox);
            }

            float raymarch(float3 ro, float3 rd) {
                float distance = 0;
                float distance_surface;
                for (int i = 0; i < MAX_STEPS; i++) {
                    float3 p = ro + rd * distance;
                    distance_surface = getDist(p, float3(0.5f,0.5f,0.5f));
                    distance += distance_surface;
                    if (distance_surface < SURF_DIST || distance > MAX_DIST) {
                        break;
                    }
                }
                return distance;
            }



            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                float4 color = tex2D(_MainTex, i.texcoord);

                float3 ro = _WorldSpaceCameraPos;
                float viewLength = length(i.rd);
                float3 rd = normalize(i.rd);
                //float d = raymarch(ro,rd);

                float nonlin_depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.texcoord);
                float depth = LinearEyeDepth(nonlin_depth) * viewLength;
                float2 containerInfo = rayBoxDst(boxMin, boxMax, ro,  1/rd);

                bool isHit = containerInfo.y > 0 && containerInfo.x < depth;
                float limit = min(depth - containerInfo.x, containerInfo.y);

                //if (isHit) {
                //    color = 0;
                //}



                float3 entry_point = ro + rd * containerInfo.x;

                float transmittance = 1.;
                float distance_traveled = 0.001;
                float step_size = 0.03;
                int num_steps = 100;
                float4 sample_color = float4(0.0f, 0.0f, 0.0f, 0.0f);
                if (isHit) {

                    for (int i = 0; i < num_steps; i++) {
                        if (distance_traveled < limit) {
                            float3 p = entry_point + rd * distance_traveled;
                            //color.r = tex3D(_Worley, p);
                            sample_color.rgb = distance_traveled;
                            distance_traveled += step_size;


                            //color.r = tex3D(_Worley, p);
                            //float density = tex3D(_Worley, p);
                            //transmittance = density;
                            //transmittance *= exp(-density);
                        }

                    }

                    // COLOR


                    //if (sample_color.r < alpha_range) {
                    //    sample_color.a = 0.0f;
                    //}
                }

                color *= 1 - sample_color;

                return color;
            }
            ENDCG
        }
    }
}
