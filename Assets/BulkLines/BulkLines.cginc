#include "UnityCG.cginc"
#define MAX_LINE_COUNT 255
#define HALF_PI 1.570796

uniform int _Pass;
uniform int _Count;
uniform float4 _From[MAX_LINE_COUNT];
uniform float4 _To[MAX_LINE_COUNT];
uniform float4 _Color[MAX_LINE_COUNT];

// this gets fed to the vertex shader
struct appdata {
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
};

// this gets fed to the fragment shader
struct v2_f {
	float4 pos : SV_POSITION;
	fixed4 color : COLOR;
};

v2_f vert(const appdata v) {
	v2_f o;

	// figure out which index in the position array this vertex maps to
	const int index = floor(v.vertex.z);

	// figure out if its a "from" point or a "to" point
	// either a=1 and b=0, or b=1 and a=0
	const half b = frac(v.vertex.z) * 2.0;
	const half a = 1 - b;

	// for convenience, stash the from and to points into variables
	const float3 from = _From[index].xyz;
	const float3 to = _To[index].xyz;

	// copy the position to a temp var
	float4 pos = v.vertex;

	// work out the angle between the two points
	float angle = atan2(from.y - to.y, from.x - to.x);
	// flip the endcaps to face either way, using the weights from before
	angle += HALF_PI * a;
	angle -= HALF_PI * b;
	// finally, do the trig to get a rotation matrix
	const float c = cos(angle);
	const float s = sin(angle);
	const float2x2 rot = float2x2(c, -s, s, c);
	// and rotate the position
	pos.xy = mul(rot, pos.xy);

	half width = _From[index].w * a + _To[index].w * b;
	pos.xy *= width;

	// and set z to zero
	pos.z = 0;
	
	// then, translate to the proper spot
	pos.xyz += from * a;
	pos.xyz += to * b;
	
	// and set z to zero
	// pos.z = _From[index]. - _Pass * .01f;
	
	if (index >= _Count) pos.z = 999999;
	
	// apply unitys transforms and matrices
	o.pos = UnityObjectToClipPos(pos);
	
	// set the color for the fragment shader
	o.color = _Color[index];

	return o;
}

fixed4 frag(v2_f i) : COLOR {
	return i.color;
}