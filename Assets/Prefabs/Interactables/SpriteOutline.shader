Shader "Custom/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
        
        // OUTLINE PROPERTIES
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineSize ("Outline Thickness", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            fixed4 _OutlineColor;
            float _OutlineSize;
            float4 _MainTex_TexelSize;

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. Sample the original sprite color
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;

                // 2. Early exit: no outline when thickness is zero
                if (_OutlineSize <= 0)
                {
                    c.rgb *= c.a;
                    return c;
                }

                // 3. Define alpha threshold to reduce noise
                const float alphaThreshold = 0.1;

                // 4. If current pixel is visible, return it as-is
                if (c.a > alphaThreshold)
                {
                    c.rgb *= c.a;
                    return c;
                }

                // 5. Calculate screen-space pixel size in UV coordinates
                // This ensures consistent outline thickness across all sprite sizes
                float2 uv = IN.texcoord;
                float2 screenPixelSize = fwidth(uv);

                // Scale the outline by screen pixels, not texture pixels
                float thickness = _OutlineSize;
                float2 offset = screenPixelSize * thickness;

                // 6. Multi-step sampling for smooth, filled outline
                float neighborAlpha = 0;
                const int STEPS = 12;

                for (int i = 1; i <= STEPS; i++)
                {
                    float t = (float)i / (float)STEPS;
                    float2 sampleOffset = offset * t;

                    // 8 directions for smooth outline
                    neighborAlpha = max(neighborAlpha, tex2D(_MainTex, uv + float2(0, sampleOffset.y)).a);
                    neighborAlpha = max(neighborAlpha, tex2D(_MainTex, uv - float2(0, sampleOffset.y)).a);
                    neighborAlpha = max(neighborAlpha, tex2D(_MainTex, uv + float2(sampleOffset.x, 0)).a);
                    neighborAlpha = max(neighborAlpha, tex2D(_MainTex, uv - float2(sampleOffset.x, 0)).a);
                    neighborAlpha = max(neighborAlpha, tex2D(_MainTex, uv + sampleOffset).a);
                    neighborAlpha = max(neighborAlpha, tex2D(_MainTex, uv + float2(-sampleOffset.x, sampleOffset.y)).a);
                    neighborAlpha = max(neighborAlpha, tex2D(_MainTex, uv + float2(sampleOffset.x, -sampleOffset.y)).a);
                    neighborAlpha = max(neighborAlpha, tex2D(_MainTex, uv - sampleOffset).a);

                    if (neighborAlpha > alphaThreshold) break;
                }

                // 7. Output outline color
                float outlineAlpha = step(alphaThreshold, neighborAlpha);
                fixed4 outline = _OutlineColor;
                outline.a *= outlineAlpha;
                outline.rgb *= outline.a;

                return outline;
            }
        ENDCG
        }
    }
}