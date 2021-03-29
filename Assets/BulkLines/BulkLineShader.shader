Shader "Unlit/BulkLineShader"
{
	SubShader{
		Tags { 
		    "RenderType" = "Opaque" "Queue"="Geometry"
		}
		
		Cull Back 
		Lighting Off
		ZTest LEqual
		ZWrite On

		Pass {
			
			Stencil {
	            Ref 8
	            Comp Always
	            Pass Replace
			}
		    
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "BulkLines.cginc"

			ENDCG
		}
	}
}
