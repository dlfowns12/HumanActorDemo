Shader "Custom/PostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NightVisionIntensity ("Night Vision Intensity", Range(0, 1)) = 0.8
        _EdgeThreshold ("Edge Threshold", Range(0, 1)) = 0.1
        _EdgeIntensity ("Edge Intensity", Range(0, 2)) = 1.0
        _GreenTint ("Green Tint", Color) = (0.5, 1.0, 0.5, 1.0)
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
                float2 uv1 : TEXCOORD1;
                float2 uv2 : TEXCOORD2;
                float2 uv3 : TEXCOORD3;
                float2 uv4 : TEXCOORD4;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _NightVisionIntensity;
            float _EdgeThreshold;
            float _EdgeIntensity;
            float4 _GreenTint;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                // 计算周围像素的UV坐标
                o.uv1 = o.uv + float2(_MainTex_TexelSize.x, 0);
                o.uv2 = o.uv - float2(_MainTex_TexelSize.x, 0);
                o.uv3 = o.uv + float2(0, _MainTex_TexelSize.y);
                o.uv4 = o.uv - float2(0, _MainTex_TexelSize.y);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 采样中心像素
                fixed4 center = tex2D(_MainTex, i.uv);
                
                // 采样周围像素
                fixed4 right = tex2D(_MainTex, i.uv1);
                fixed4 left = tex2D(_MainTex, i.uv2);
                fixed4 top = tex2D(_MainTex, i.uv3);
                fixed4 bottom = tex2D(_MainTex, i.uv4);
                
                // 计算边缘
                fixed4 horizontal = abs(right - left);
                fixed4 vertical = abs(top - bottom);
                fixed4 edge = sqrt(horizontal * horizontal + vertical * vertical);
                
                // 边缘检测阈值
                float edgeAmount = step(_EdgeThreshold, edge.r + edge.g + edge.b);
                
                // 应用夜视绿效果
                fixed4 nightVision = center;
                nightVision.rgb = lerp(nightVision.rgb, _GreenTint.rgb, _NightVisionIntensity);
                
                // 增加亮度和对比度以模拟夜视效果
                nightVision.rgb = (nightVision.rgb - 0.5) * 1.5 + 0.5;
                nightVision.rgb = pow(nightVision.rgb, 0.8);
                
                // 混合边缘高亮
                fixed4 finalColor = lerp(nightVision, fixed4(1, 1, 1, 1), edgeAmount * _EdgeIntensity);
                
                return finalColor;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
