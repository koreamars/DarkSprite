Shader "Junios/Merge" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SubTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		sampler2D _SubTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c1 = tex2D (_MainTex, IN.uv_MainTex);
			half4 c2 = tex2D (_SubTex, IN.uv_MainTex);
			o.Albedo = (c1.rgb * (1.0 - c2.a)) + (c2.rgb * (c2.a));

			o.Alpha = 1.0;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
