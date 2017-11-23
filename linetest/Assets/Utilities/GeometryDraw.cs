using System.Collections.Generic;
using UnityEngine;

namespace Utilities {
	public static class GeometryDraw {

		static GameObject lastTarget;
		static Mesh mesh;
		static int vertStartIndex;
		static List<Vector3> vertices;
		static List<Vector2> uv;
		static List<int> triangles;
		static List<Color32> colors;

		public static void DrawCircle(GameObject target, float centerX, float centerY, float radius, Color32 color, float zDepth = 0f, float vertexDensity = 4f) {
			const int baseCount = 8;
			const float baseDensity = 40.0f;

			var numVerts = baseCount + Mathf.FloorToInt(radius * 2 * Mathf.PI * vertexDensity * baseDensity);
			var numTriangles = numVerts - 1;
				
			Expand(target, numVerts, numTriangles);
		
			vertices.Add(new Vector3(centerX, centerY, zDepth));
			uv.Add(Vector2.zero);

			for (var i = 1; i < numVerts; i++) {
				var p = i / (numVerts - 1f) * Mathf.PI * 2;
				vertices.Add(new Vector3(centerX + Mathf.Cos(p) * radius, centerY + Mathf.Sin(p) * radius, zDepth));
				uv.Add(Vector2.zero);
			}
		
			for (var i = 0; i < numVerts - 2; i++) {
				triangles.Add(vertStartIndex);
				triangles.Add(vertStartIndex + i + 2);
				triangles.Add(vertStartIndex + i + 1);
			}

			triangles.Add(vertStartIndex);
			triangles.Add(vertStartIndex + 1);
			triangles.Add(vertStartIndex + numVerts - 1);

			for (var i = 0; i < numVerts; i++) colors.Add(color);

		}

		public static void DrawSector(GameObject target, float centerX, float centerY, float radius, float startAngle, float arc, Color32 color, float zDepth = 0f, float vertexDensity = 4f) {
			const int baseCount = 8;
			const float baseDensity = .18f;

			var numVerts = baseCount + Mathf.FloorToInt(radius * arc * vertexDensity * baseDensity);
			var numTriangles = numVerts - 1;

			Expand(target, numVerts, numTriangles);

			vertices.Add(new Vector3(centerX, centerY, zDepth));
			//uv.Add(vertices[vertices.Count - 1]);

			for (var i = 0; i < numVerts - 1; i++) {
				var r = startAngle + i / (numVerts - 2f) * arc;
				vertices.Add(new Vector3(centerX + Mathf.Cos(r) * radius, centerY + Mathf.Sin(r) * radius, zDepth));
				//uv.Add(vertices[vertices.Count - 1]);
			}

			for (var i = 0; i < numVerts - 2; i++) {
				triangles.Add(vertStartIndex);
				triangles.Add(vertStartIndex + i + 2);
				triangles.Add(vertStartIndex + i + 1);
			}
			
			//for (var i = 0; i < numVerts; i++) colors.Add(color);
		}

		public static void DrawBulkLine(GameObject target, int index, float vertexDensity) {
			// call expand to make sure we have all the mesh data
			Expand(target, 0, 0);
			var preSectorAVertCount = vertices.Count;
			DrawSector(target, 0, 0, 1, -Mathf.PI, Mathf.PI, Color.white, index + .0f, vertexDensity);
			var preSectorBVertCount = vertices.Count;
			DrawSector(target, 0, 0, 1, -Mathf.PI, Mathf.PI, Color.white, index + .5f, vertexDensity);
			var diff = preSectorBVertCount - preSectorAVertCount;

			Expand(target, 0, 2);
			triangles.Add(preSectorAVertCount + 1);
			triangles.Add(vertices.Count - diff + 1);
			triangles.Add(preSectorAVertCount + diff - 1);

			triangles.Add(preSectorAVertCount + 1);
			triangles.Add(vertices.Count - 1);
			triangles.Add(vertices.Count - diff + 1);
		}

		public static void DrawArc(GameObject target, float centerX, float centerY, float innerRadius, float outerRadius, float startAngle, float arc, Color32 color, float zDepth = 0f, float vertexDensity = 1f) {
			const int baseCount = 8;
			const float baseDensity = .18f;

			var numVerts = Mathf.FloorToInt(baseCount + (outerRadius * arc) * vertexDensity * baseDensity) * 2;
			var numTriangles = numVerts;

			Expand(target, numVerts, numTriangles);

			for (var i = 0; i < numVerts; i += 2) {
				var p = i / (numVerts - 2f);
				var r = startAngle + p * arc;
				vertices.Add(new Vector3(centerX + Mathf.Cos(r) * outerRadius, centerY + Mathf.Sin(r) * outerRadius, zDepth));
				vertices.Add(new Vector3(centerX + Mathf.Cos(r) * innerRadius, centerY + Mathf.Sin(r) * innerRadius, zDepth));
				uv.Add(vertices[vertices.Count - 2]);
				uv.Add(vertices[vertices.Count - 1]);
			}

			for (var i = 0; i < numVerts - 2; i++) {
				triangles.Add(vertStartIndex + i);
				// flip every other triangle to get the facing direction right
				if (i % 2 != 0) triangles.Add(vertStartIndex + i + 2);
				triangles.Add(vertStartIndex + i + 1);
				if (i % 2 == 0) triangles.Add(vertStartIndex + i + 2);
			}

			for (var i = 0; i < numVerts; i++) colors.Add(color);

		}

		public static void DrawDonut(GameObject target, float centerX, float centerY, float innerRadius, float outerRadius, Color32 color, float zDepth = 0f, float vertexDensity = 1f) {
			const int baseCount = 16;
			const float baseDensity = 80f;

			var numVerts = Mathf.FloorToInt(baseCount + ((outerRadius * 2 * Mathf.PI)) * vertexDensity * baseDensity) * 2;

			var numTriangles = numVerts;

			Expand(target, numVerts, numTriangles);

			for (var i = 0; i < numVerts; i += 2) {
				var po = (float) i / (numVerts) * Mathf.PI * 2;
				var pi = (float)(i + 1) / (numVerts) * Mathf.PI * 2;
				vertices.Add(new Vector3(centerX + Mathf.Cos(po) * outerRadius, centerY + Mathf.Sin(po) * outerRadius, zDepth));
				vertices.Add(new Vector3(centerX + Mathf.Cos(pi) * innerRadius, centerY + Mathf.Sin(pi) * innerRadius, zDepth));
				uv.Add(new Vector2(1, po));
				uv.Add(new Vector2(0, pi));
			}

			for (var i = 0; i < numVerts; i++) {
				triangles.Add(vertStartIndex + (i + 0) % numVerts);
				// flip every other triangle to get the facing direction right
				if (i % 2 != 0) triangles.Add(vertStartIndex + (i + 2) % numVerts);
				triangles.Add(vertStartIndex + (i + 1) % numVerts);
				if (i % 2 == 0) triangles.Add(vertStartIndex + (i + 2) % numVerts);
			}
		
			for (var i = 0; i < numVerts; i++) colors.Add(color);

		}

		public static void DrawRectCentered(GameObject target, float x, float y, float width, float height, Color32 color, float zDepth = 0f) {
			DrawRect(target, x - width / 2, y - height / 2, width, height, color, zDepth);
		}

		public static void DrawRect(GameObject target, float x, float y, float width, float height, Color32 color, float zDepth = 0f) {
			Expand(target, 4, 2);

			vertices.Add(new Vector3(x, y, zDepth));
			vertices.Add(new Vector3(x + width, y, zDepth));
			vertices.Add(new Vector3(x, y + height, zDepth));
			vertices.Add(new Vector3(x + width, y + height, zDepth));

			uv.Add(vertices[vertices.Count - 4]);
			uv.Add(vertices[vertices.Count - 3]);
			uv.Add(vertices[vertices.Count - 2]);
			uv.Add(vertices[vertices.Count - 1]);
			
			triangles.Add(vertStartIndex + 0);
			triangles.Add(vertStartIndex + 3);
			triangles.Add(vertStartIndex + 1);

			triangles.Add(vertStartIndex + 0);
			triangles.Add(vertStartIndex + 2);
			triangles.Add(vertStartIndex + 3);

			for (var i = 0; i < 4; i++) colors.Add(color);

		}

		public static void DrawLine(GameObject target, Vector2[] points, float width, Color color, float zDepth = 0f, float sideOffset = 0) {
			DrawLine(target, points, 0, points.Length, width, color, zDepth, sideOffset);
		}

		public static void DrawLine(GameObject target, Vector2[] points, int from, int to, float width, Color color, float zDepth = 0f, float sideOffset = 0) {
			Expand(target, points.Length * 2, (points.Length - 1) * 2);

			for (var i = from; i < points.Length && i < to; i++) {
				var diff = i == from ? (points[i] - points[i+1]).normalized : (points[i - 1] - points[i]).normalized;
				var perp = R90(diff);
				var p0 = points[i] + perp * width * .5f + perp * sideOffset;
				var p1 = points[i] - perp * width * .5f + perp * sideOffset;

				vertices.Add(new Vector3(p0.x, p0.y, zDepth));
				vertices.Add(new Vector3(p1.x, p1.y, zDepth));

				colors.Add(color);
				colors.Add(color);

				uv.Add(new Vector2(0, from + i));
				uv.Add(new Vector2(1, from + i));

				if (i == from) continue;

				triangles.Add(vertStartIndex + (i - from) * 2 - 2);
				triangles.Add(vertStartIndex + (i - from) * 2 + 1);
				triangles.Add(vertStartIndex + (i - from) * 2 - 1);
				triangles.Add(vertStartIndex + (i - from) * 2 - 2);
				triangles.Add(vertStartIndex + (i - from) * 2 + 0);
				triangles.Add(vertStartIndex + (i - from) * 2 + 1);
			}

		}

		public static void SetAllVertexColors(GameObject target, Color color) {
			GetMesh(target);
			var clr = new List<Color32>(mesh.vertexCount);
			for (var i = 0; i < mesh.vertexCount; i++) clr.Add(color);
			mesh.SetColors(clr);
		}

		public static void SetAllUV2(GameObject target, Vector2 pos) {
			GetMesh(target);
			var uv2 = new List<Vector2>(mesh.vertexCount);
			for (var i = 0; i < mesh.vertexCount; i++) uv2.Add(pos);
			mesh.SetUVs(1, uv2);
		}

		static Vector2 R90(Vector2 v) {
			var tmp = v.y;
			v.y = -v.x;
			v.x = tmp;
			return v;
		}

		static void Expand(GameObject target, int numVertices, int numTriangles) {
			if (target == lastTarget) {
				
			} else {
				GetMesh(target);
				if (mesh.vertexCount > 0) {
					vertices = new List<Vector3>(mesh.vertices);
					triangles = new List<int>(mesh.triangles);
					//colors = new List<Color32>(mesh.colors32);
					//uv = new List<Vector2>(mesh.uv);
					Debug.Log("readback of " + vertices.Count + " vertices");
				} else {
					vertices = new List<Vector3>();
					triangles = new List<int>();
					//colors = new List<Color32>();
					//uv = new List<Vector2>();
				}
			}

			vertStartIndex = vertices.Count;
			vertices.Capacity = vertices.Count + numVertices;
			triangles.Capacity = triangles.Count + numTriangles * 3;
			//colors.Capacity = vertices.Capacity;
			//uv.Capacity = vertices.Capacity;
		}

		static void GetMesh(GameObject target) {
			mesh = target.GetComponent<MeshFilter>().sharedMesh;
			if (mesh == null) mesh = target.GetComponent<MeshFilter>().sharedMesh = new Mesh();

			lastTarget = target;
		}

		static void Apply(GameObject target) {
			// if lastTarget is still null, no drawing operations took place since clearing, there's nothing to apply
			if (lastTarget == null) return;

			mesh.SetVertices(vertices);
			mesh.SetTriangles(triangles, 0);
			mesh.SetColors(colors);
			mesh.SetUVs(0, uv);
		}

		public static void Finalize(GameObject target) {
			Apply(target);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		}

		public static void Clear(GameObject target) {
			GetMesh(target);
			mesh.Clear();
			lastTarget = null;
		}

	}
}