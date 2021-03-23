Shader "Custom/Colors"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Saturation ("Saturation", Range(0, 2)) = 1.0
        _Brightness ("Brightness", Range(0, 2)) = 1.0
        _Contrast ("Contrast", Range(0, 2)) = 1.0
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Saturation;
            float _Brightness;
            float _Contrast;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // 调整饱和度
                float grey = dot(col.rgb, fixed3(0.299, 0.587, 0.114));
                fixed3 saturatedCol = lerp(fixed3(grey, grey, grey), col.rgb, _Saturation);
                
                // 调整亮度
                fixed3 brightenedCol = saturatedCol * _Brightness;
                
                // 调整对比度
                fixed3 contrastedCol = (brightenedCol - 0.5) * _Contrast + 0.5;
                
                return fixed4(contrastedCol, col.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
