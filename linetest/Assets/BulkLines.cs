using UnityEngine;
using Utilities;

public class BulkLines : MonoBehaviour {

	const int maxLineCount = 123;
	const float vertexDensity = .05f;

	public Material material;
	Vector4[] segments;
	Vector4[] velocities;
	Vector4[] colors;

	void Awake () {
		segments = new Vector4[maxLineCount];
		colors = new Vector4[maxLineCount];

		velocities = new Vector4[maxLineCount];

		for (var i = 0; i < maxLineCount; i++) {
			var from = Random.insideUnitCircle * 3;
			var to =   Random.insideUnitCircle * 3;
			segments[i] = new Vector4(from.x, from.y, to.x, to.y);

			var c = Random.ColorHSV(0, 1, 1, 1, .5f, 1);
			var w = .005f + Random.value * .1f;
			colors[i] = new Vector4(c.r, c.g, c.b, w);

			var va = Random.insideUnitCircle;
			var vb = Random.insideUnitCircle;
			velocities[i] = new Vector4(va.x, va.y, vb.x, vb.y);
			
			GeometryDraw.DrawBulkLine(gameObject, i, vertexDensity);
		}

		GeometryDraw.Finalize(gameObject);

		material = new Material(material);
		material.hideFlags = HideFlags.DontSave;
		GetComponent<MeshRenderer>().material = material;
	}

	void Update() {
		for (var i = 0; i < maxLineCount; i++) {
			segments[i].x += velocities[i].x * Time.deltaTime;
			segments[i].y += velocities[i].y * Time.deltaTime;
			segments[i].z += velocities[i].z * Time.deltaTime;
			segments[i].w += velocities[i].w * Time.deltaTime;

			var from = new Vector2(segments[i].x, segments[i].y);
			var to = new Vector2(segments[i].z, segments[i].w);

			if (from.magnitude > 3) {
				from = from.normalized * 3;
				velocities[i].x *= -1;
				velocities[i].y *= -1;
			}

			if (to.magnitude > 3) {
				to = to.normalized * 3;
				velocities[i].z *= -1;
				velocities[i].w *= -1;
			}

			segments[i].Set(from.x, from.y, to.x, to.y);
		}

		material.SetVectorArray("_Points", segments);
		material.SetVectorArray("_Colors", colors);
	}
}
