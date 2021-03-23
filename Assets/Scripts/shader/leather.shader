Shader "Custom/Leather"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Metallic ("Metallic", Range(0, 1)) = 0.2
        _Smoothness ("Smoothness", Range(0, 1)) = 0.3
        _Glossiness ("Glossiness", Range(0, 1)) = 0.5
        _SpecularTint ("Specular Tint", Color) = (0.2, 0.2, 0.2, 1)
        _LeatherGrain ("Leather Grain", 2D) = "white" {}
        _GrainIntensity ("Grain Intensity", Range(0, 1)) = 0.3
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
            #pragma multi_compile_fwdbase
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float3 tangent : TEXCOORD3;
                float3 binormal : TEXCOORD4;
                SHADOW_COORDS(5)
            };

            sampler2D _MainTex;
            sampler2D _NormalMap;
            sampler2D _LeatherGrain;
            float4 _MainTex_ST;
            float4 _NormalMap_ST;
            float4 _LeatherGrain_ST;
            float _Metallic;
            float _Smoothness;
            float _Glossiness;
            float4 _SpecularTint;
            float _GrainIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.tangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.binormal = cross(o.normal, o.tangent) * v.tangent.w;
                TRANSFER_SHADOW(o)
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 计算切线空间矩阵
                float3x3 TBN = float3x3(i.tangent, i.binormal, i.normal);
                
                // 采样纹理
                fixed4 albedo = tex2D(_MainTex, i.uv);
                fixed3 normal = UnpackNormal(tex2D(_NormalMap, i.uv * _NormalMap_ST.xy + _NormalMap_ST.zw));
                normal = normalize(mul(normal, TBN));
                
                // 皮革纹理效果
                fixed4 grain = tex2D(_LeatherGrain, i.uv * _LeatherGrain_ST.xy + _LeatherGrain_ST.zw);
                albedo.rgb = lerp(albedo.rgb, albedo.rgb * grain.rgb, _GrainIntensity);
                
                // 计算光照方向
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                float3 halfDir = normalize(lightDir + viewDir);
                
                // 计算漫反射
                float diff = max(dot(normal, lightDir), 0.0);
                fixed3 diffuse = _LightColor0.rgb * diff;
                
                // 计算高光
                float spec = pow(max(dot(normal, halfDir), 0.0), 32.0 * _Smoothness);
                fixed3 specular = _SpecularTint.rgb * spec * _Glossiness;
                
                // 计算环境光
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;
                
                // 计算阴影
                float shadow = SHADOW_ATTENUATION(i);
                
                // 计算最终颜色
                fixed3 finalColor = (ambient + (diffuse + specular) * shadow) * albedo.rgb;
                
                return fixed4(finalColor, albedo.a);
            }
            ENDCG
        }
        
        // 阴影投射器
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
    }
    FallBack "Diffuse"
}
