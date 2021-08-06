Shader "Custom/Shadows"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.5
        _ShadowBias ("Shadow Bias", Range(-0.01, 0.01)) = 0.0
        _ShadowSmoothness ("Shadow Smoothness", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                SHADOW_COORDS(3)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ShadowStrength;
            float _ShadowBias;
            float _ShadowSmoothness;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // 计算光照方向
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                
                // 计算法线
                float3 normal = normalize(i.normal);
                
                // 计算漫反射
                float diff = max(dot(normal, lightDir), 0.0);
                fixed3 diffuse = _LightColor0.rgb * diff;
                
                // 计算环境光
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;
                
                // 计算阴影
                float shadow = SHADOW_ATTENUATION(i);
                
                // 应用阴影平滑
                shadow = smoothstep(0.0, _ShadowSmoothness, shadow);
                
                // 计算最终颜色
                fixed3 finalColor = (ambient + diffuse * shadow * (1.0 - _ShadowStrength)) * col.rgb;
                
                return fixed4(finalColor, col.a);
            }
            ENDCG
        }
        
        // 阴影投射器
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
    FallBack "Diffuse"
}
