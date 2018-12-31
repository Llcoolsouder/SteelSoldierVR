Shader "Custom/DamageShader"
{
	Properties{
	 _MainTex("Base (RGB)", 2D) = "white" {}
	 _RedMultiplier("Red Amount", Range (1.0, 5.0)) = 1.0
	}
		SubShader{
			Pass
			{
				CGPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				uniform sampler2D _MainTex;
				fixed _RedMultiplier;

				fixed4 frag(v2f_img i) : COLOR
				{
					fixed4 renderTex = tex2D(_MainTex, i.uv);
					fixed4 original = tex2D(_MainTex, i.uv);

					//renderTex.r = (renderTex.r + 0.1) * _RedMultiplier;
					renderTex.r = (renderTex.r) * _RedMultiplier;

					//fixed4 finalColor = lerp(
					//renderTex.r = renderTex.r * _RedMultiplier;
					//if (renderTex.r >= original.r + 0.1f) {
					//renderTex.r -= 0.1f * unity_DeltaTime;
					//}
					/*if (renderTex.r <= original.r)
					{
						renderTex = tex2D(_MainTex, i.uv);
					}*/
					return renderTex;
				}

				ENDCG
			}
	 }
}
