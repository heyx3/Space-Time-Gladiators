Shader "Custom/TeamDraw" {

	Properties {
		_MainTex ("Sprite Sheet", Rect) =  "white" {}
		_TeamCol ("Team Color",   Color) = (1, 1, 1, 10)
		_ACut	 ("Alpha Cutoff", float) = 0.9
	}
	
	
	SubShader {
	
		//Tags { "RenderType" = "Opaque" }
		
		AlphaTest GEqual [_ACut]
		Lighting Off
			
		CGPROGRAM
		#pragma surface surf Lambert
	
		sampler2D _MainTex;
		half _ACut;
		float4 _TeamCol;
	 
		struct Input {
			float2 uv_MainTex;
		};
	
		void surf (Input IN, inout SurfaceOutput o) {

			half4 c = tex2D (_MainTex, IN.uv_MainTex);

			//o.Alpha = 0;
			//o.Albedo = (0, 0, 0);
			//o.Alpha = 0;
			//if (c.a >= _ACut)
			//{
			//	o.Alpha = 1.0;
			//}

			if (c.a >= _ACut)
			{
				float ler = (c.a - _ACut) / (1.0 - _ACut);
				
				o.Alpha = 1;
				o.Albedo = lerp(_TeamCol.rgb, c.rgb, ler);//ler);
			}
			else
			{
				o.Alpha = 0;
				o.Albedo = (0, 0, 0);
			}
			
			o.Emission = o.Albedo;
			//o.Gloss = 1;
		}
		ENDCG
	}
	
	//SubShader {
	
	//	AlphaTest Greater [_ACut]
	//	Lighting off
		
	//	Pass {
		
	//		SetTexture [_MainTex] {
			
	//			ConstantColor [_TeamCol]
				
	//			combine texture lerp (texture) constant,
	//					texture * constant
	//		}
		
	//	}
	//}
	
	//FallBack "Transparent/VertexLit"
}
					
		