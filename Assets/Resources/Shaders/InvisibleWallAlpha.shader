 Shader "Calamity/InvisibleWallAlpha" {
     Properties
     {
         _Color ("Main Color", Color) = (1,1,1,1)
         _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
     }
      
     Category
     {
         SubShader
         {
                Tags { "Queue"="Overlay+1"
                 "RenderType"="Transparent"}
      
             Pass
             {
                 Blend SrcAlpha OneMinusSrcAlpha
                 ZWrite Off
                 ZTest Greater
                 Lighting Off
                 SetTexture [_MainTex] {combine texture}
             }
      
                 Pass
             {
                 Blend SrcAlpha OneMinusSrcAlpha
                 ZTest Less
                 SetTexture [_MainTex] {combine texture}
             }
     
         }
     }
      
     FallBack "Diffuse"
 }