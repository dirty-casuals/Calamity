// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Toothy Shader" {

	Properties{
		_MainTex("Texture For Diffuse Material Color", 2D) = "white" {}
	}

		SubShader
	{
		Tags
	{
		"Queue" = "Transparent"
	}

		// draw after all opaque geometry has been drawn
		Pass
	{
		ZWrite Off // don't write to depth buffer 
				   // in order not to occlude other objects
		Blend SrcAlpha OneMinusSrcAlpha // use alpha blending

		CGPROGRAM

#pragma vertex vert 
#pragma fragment frag

		uniform sampler2D _MainTex;

	struct vertexInput {
		float4 pos : POSITION;
		float3 normal : NORMAL;
		float4 texcoord : TEXCOORD0;
		float4 tex : TEXCOORD0;
	};

	struct vertexOutput {
		float4 pos : SV_POSITION;
		float4 tex : TEXCOORD0;
	};

	vertexOutput vert(vertexInput input) {
		vertexOutput vOut;
		vOut.pos = UnityObjectToClipPos(input.pos);
		vOut.tex = input.texcoord;
		return vOut;
	}

	float4 frag(vertexOutput output) : COLOR{
		tex2D(_MainTex, output.tex.xy);
	return float4(tex2D(_MainTex, output.tex.xy).rgb, 0.7);
	}

		ENDCG
	}
	}
}