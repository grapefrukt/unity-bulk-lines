using UnityEngine;
using Utilities;

public class BulkLines : MonoBehaviour {

	// THIS NUMBER NEEDS TO MATCH WHATEVER'S IN THE SHADER
	// IF YOU CHANGE THIS, YOU MAY NEED TO RESTART UNITY FOR
	// IT TO TAKE HOLD (YES, IT'S BIZARRE)
	public const int MaxLineCount = 123;
	
	const float VertexDensity = .05f;

	public Material material;
	Vector4[] segments;
	Vector4[] colors;

	bool dirty = true;

	void Awake () {
		segments = new Vector4[MaxLineCount];
		colors = new Vector4[MaxLineCount];

		for (var i = 0; i < MaxLineCount; i++) {
			GeometryDraw.DrawBulkLine(gameObject, i, VertexDensity);
		}

		GeometryDraw.Finalize(gameObject);

		material = new Material(material) { hideFlags = HideFlags.DontSave };
		GetComponent<MeshRenderer>().material = material;
	}
	
	public void SetLine(int index, Vector2 from, Vector2 to, Color color, float width) {
		segments[index] = new Vector4(from.x, from.y, to.x, to.y);
		colors[index] = new Vector4(color.r, color.g, color.b, width);
	}

	public void GetLine(int index, out Vector2 from, out Vector2 to, out Color color, out float width) {
		from = new Vector2(segments[index].x, segments[index].y);
		to   = new Vector2(segments[index].z, segments[index].w);
		color = new Color(colors[index].x, colors[index].y, colors[index].z);
		width = colors[index].w;
		dirty = true;
	}

	void LateUpdate() {
		if (!dirty) return;

		material.SetVectorArray("_Points", segments);
		material.SetVectorArray("_Colors", colors);
		dirty = false;
	}
}
