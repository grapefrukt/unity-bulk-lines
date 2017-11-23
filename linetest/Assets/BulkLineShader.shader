Shader "Unlit/BulkLineShader"
{
	Properties{
	}

	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			#define maxLineCount 123
			#define HALFPI 1.570796
			
			uniform float4 _Points[maxLineCount];
			uniform float4 _Colors[maxLineCount];

			// this gets fed to the vertex shader
			struct appdata {
				float4 vertex : POSITION;
			};

			// this gets fed to the fragment shader
			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};

			v2f vert(appdata v) {
				v2f o;

				// figure out which index in the position array this vertex maps to
				const int index = floor(v.vertex.z);

				// figure out if its a "from" point or a "to" point
				// either a=1 and b=0, or b=1 and a=0
				const half b = frac(v.vertex.z) * 2.0;
				const half a = 1 - b;

				// for convenience, stash the from and to points into variables
				const float2 from = _Points[index].xy;
				const float2 to = _Points[index].zw;

				// copy the position to a temp var
				float4 pos = v.vertex;

				// work out the angle between the two points
				float angle = atan2(from.y - to.y, from.x - to.x);
				// flip the endcaps to face either way, using the weights from before
				angle += HALFPI * a;
				angle -= HALFPI * b;
				// finally, do the trig to get a rotation matrix
				const float c = cos(angle);
				const float s = sin(angle);
				const float2x2 rot = float2x2(c, -s, s, c);
				// and rotate the position
				pos.xy = mul(rot, pos.xy);

				pos.xy *= _Colors[index].w;

				// then, translate to the proper spot
				pos.xy += from * a;
				pos.xy += to * b;

				// and set z to zero
				pos.z = index * .001;

				// apply unitys transforms and matrices
				o.pos = UnityObjectToClipPos(pos);

				// set the color for the fragment shader
				o.color = fixed4(_Colors[index].rgb, 1);

				return o;
			}

			half4 frag(v2f i) : COLOR{
				fixed4 col = i.color;
				return col;
			}

			ENDCG
		}
	}
}
