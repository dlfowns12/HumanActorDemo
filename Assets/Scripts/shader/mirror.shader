Shader "Unlit/mirror"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MirrorU("flipx",int) = 0
        _MirrorV("flipy",int) = 0
    }
    SubShader
    {
         Tags {"RenderType" = "Opaque" "Queue" = "Geometry"}
        LOD 100

       Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float _MirrorU;
            float _MirrorV;

            struct a2v {
                float4 vertex : POSITION;
                float3 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(a2v v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                o.uv = v.texcoord;
                if (_MirrorU > 0) {
                    o.uv.x = 1 - o.uv.x;//Flip the coordinates to get the image sampling data
                }
                if (_MirrorV > 0) {
                    o.uv.y = 1 - o.uv.y;//Flip the coordinates to get the image sampling data
                }

                return o;
            }

            fixed4 frag(v2f i) : SV_Target{
                return tex2D(_MainTex,i.uv);
            }

            ENDCG
        }
    }
     FallBack off
   
}
