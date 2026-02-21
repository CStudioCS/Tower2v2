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
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            fixed4 _OutlineColor;
            float _OutlineSize;
            float4 _MainTex_TexelSize; // Automatically filled by Unity (width, height)

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. Sample the original sprite color
                fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;

                // 2. If the pixel is already opaque, just return it (optimization)
                if (c.a >= 1) return c;

                // 3. Prepare to sample neighbors
                // TexelSize.xy gives us the size of one pixel in UV space
                float2 uv = IN.texcoord;
                float2 offset = _MainTex_TexelSize.xy * _OutlineSize;

                // 4. Check neighbors (Up, Down, Left, Right)
                // We accumulate their alpha values to see if we are near a solid pixel
                fixed4 pixelUp = tex2D(_MainTex, uv + float2(0, offset.y));
                fixed4 pixelDown = tex2D(_MainTex, uv - float2(0, offset.y));
                fixed4 pixelRight = tex2D(_MainTex, uv + float2(offset.x, 0));
                fixed4 pixelLeft = tex2D(_MainTex, uv - float2(offset.x, 0));

                float totalAlpha = pixelUp.a + pixelDown.a + pixelRight.a + pixelLeft.a;

                // 5. Logic: If current pixel is transparent (c.a == 0) 
                // BUT neighbors have alpha (totalAlpha > 0), this is an edge!
                
                // We clamp totalAlpha to 0-1 range
                float outlineAlpha = clamp(totalAlpha, 0, 1);
                
                // Remove the alpha of the sprite itself so we don't draw outline ON TOP of the sprite
                outlineAlpha -= c.a; 
                outlineAlpha = clamp(outlineAlpha, 0, 1);

                // 6. Return the outline color with the calculated alpha
                // Blend outline with the original color
                return lerp(c, _OutlineColor, outlineAlpha);
            }
        ENDCG
        }
    }
}