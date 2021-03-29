using System.Collections.Generic;
using UnityEngine;

// BulkLines by grapefrukt https://grapefrukt.com

[RequireComponent(typeof(Camera))]
[DefaultExecutionOrder(-500)]
public class BulkLines : MonoBehaviour {

	static BulkLines instance;

	public static void DrawLine(Vector2 origin, Vector2 end, float width, Color color, float z = 0) {
		if (instance == null) Initialize();
		instance._DrawLine(new Vector3(origin.x, origin.y, z), new Vector3(end.x, end.y, z), color, width);
	}

	public static void DrawLine3D(Vector3 origin, Vector3 end, float width, Color color) {
		if (instance == null) Initialize();
		instance._DrawLine(origin, end, color, width);
	}

	public static void DrawDot(Vector3 position, float radius, Color color, float z) {
		DrawLine(position, position + Vector3.right * .001f, radius, color, z);
	}

	public static void DrawCircle(Vector2 position, float radius, Color color, float lineWidth, int numSegments = 28,
	                              float z = 0) {
		var last = Vector2.zero;
		for (var i = 0; i <= numSegments; i++) {
			var r = Mathf.PI * 2 * ((float) i / numSegments);
			var x = position.x + Mathf.Cos(r) * radius;
			var y = position.y + Mathf.Sin(r) * radius;

			if (i > 0) DrawLine(new Vector2(x, y), last, lineWidth, color, z);
			last = new Vector2(x, y);
		}
	}

	// BatchSize NEEDS TO MATCH WHATEVER IS IN THE SHADER
	// IF YOU CHANGE THIS, YOU MAY NEED TO RESTART UNITY FOR
	// IT TO TAKE HOLD (YES, IT'S BIZARRE)
	// this determines how many lines are in the mesh
	const  int      BatchSize         = 255;
	
	// the batch count determines how may times over we can draw that mesh
	// 255 batch size * 8 BatchCount = 2040 lines / frame
	const  int      BatchCount        = 8;
	const  int      MaxIndex          = BatchCount * BatchSize;
	
	// this determines how many vertices are in the rounded ends
	const  int      NumSectorVertices = 16;
	
	public Material material;

	int         index;
	Vector4[][] vectorFrom;
	Vector4[][] vectorTo;
	Vector4[][] vectorColor;

	Mesh mesh;

	int                   layer;
	readonly int          propertyFrom  = Shader.PropertyToID("_From");
	readonly int          propertyTo    = Shader.PropertyToID("_To");
	readonly int          propertyColor = Shader.PropertyToID("_Color");
	readonly int          propertyPass  = Shader.PropertyToID("_Pass");
	readonly int          propertyCount = Shader.PropertyToID("_Count");
	MaterialPropertyBlock propertyBlock;

	static void Initialize() {
		if (instance != null) return;
		instance = FindObjectOfType<BulkLines>();
		instance.InitializeInternal();
	}

	void InitializeInternal() {
		vectorFrom   = new Vector4[BatchCount][];
		vectorTo     = new Vector4[BatchCount][];
		vectorColor = new Vector4[BatchCount][];

		for (var i = 0; i < BatchCount; i++) {
			vectorFrom[i]  = new Vector4[BatchSize];
			vectorTo[i]    = new Vector4[BatchSize];
			vectorColor[i] = new Vector4[BatchSize];
		}
		
		GenerateMesh();

		propertyBlock = new MaterialPropertyBlock();
		layer = LayerMask.NameToLayer("Default");
	}
	
	void _DrawLine(Vector3 from, Vector3 to, Color color, float width) {
		if (index >= BatchSize * BatchCount) {
			Debug.LogWarning($"{name} exceeded maximum line count! {index} (max is {BatchSize * BatchCount}, which is {BatchCount} batches of {BatchSize} lines each)");
			return;
		}

		Set(index, 
			new Vector4(from.x, from.y, from.z, width), 
			new Vector4(to.x, to.y, to.z, width), 
			color);
		
		index++;
	}

	void Set(int index, Vector4 from, Vector4 to, Color color) {
		var batch = index / BatchSize;
		var batchIndex = index - batch * BatchSize;

		if (index > MaxIndex) return;

		vectorFrom[batch][batchIndex]  = from;
		vectorTo[batch][batchIndex]    = to;
		vectorColor[batch][batchIndex] = color;
	}

	void OnEnable() {
		Camera.onPreCull -= RenderInternal;
		Camera.onPreCull += RenderInternal;
	}

	void OnDisable() {
		Camera.onPreCull -= RenderInternal;
	}

	void Update() {
		// after a render was made (may be multiples depending on camera count, the scene view counts as an extra camera)
		// we reset the index and start over from the beginning
		index = 0;
	}

	void RenderInternal(Camera cam) {
		if (!mesh) return;
		
		// do not render in prefab focus mode
		if (cam.scene.isLoaded) return;
		
		// move the bounds to always be in front of the camera, else the line mesh will get culled
		if (cam.cameraType == CameraType.Game) {
			var p = cam.transform.position + Vector3.forward * Mathf.Lerp(cam.nearClipPlane, cam.farClipPlane, .5f);		
			mesh.bounds = new Bounds(p, Vector3.one * 1000);
		}
		
		var numBatches = Mathf.CeilToInt((float) index / BatchSize);
		for (var batch = numBatches - 1; batch >= 0; batch--) {
			var count = index - BatchSize * batch;
			if (count > BatchSize) count = BatchSize;
			if (count <= 0) continue;
			
			propertyBlock.SetVectorArray(propertyFrom, vectorFrom[batch]);
			propertyBlock.SetVectorArray(propertyTo, vectorTo[batch]);
			propertyBlock.SetVectorArray(propertyColor, vectorColor[batch]);
			propertyBlock.SetInt(propertyPass, batch);
			propertyBlock.SetInt(propertyCount, count);
			Graphics.DrawMesh(mesh, Matrix4x4.identity, material, layer, cam, 0, propertyBlock);
		}

	}

	void GenerateMesh(bool force = false) {
		if (force) mesh = null;
		
		if (mesh == null) {
			mesh = new Mesh();
		} else if (mesh.vertexCount > 0) {
			Debug.Log("Using cached bulk line mesh");
			return;
		}

		var vertices = new List<Vector3>();
		var triangles = new List<int>();
		var uvs = new List<Vector2>();
		for (var i = 0; i < BatchSize; i++) CacheBulkLine(i, vertices, triangles, uvs);

		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uvs);
		mesh.bounds = new Bounds(Vector3.zero, Vector3.one);
	}

	static void CacheBulkLine(int index, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs) {
		var start = vertices.Count;
		CacheDrawSector(index + .0f, vertices, triangles, uvs, false);
		CacheDrawSector(index + .5f, vertices, triangles, uvs, true);

		triangles.Add(start);
		triangles.Add(start + 1);
		triangles.Add(start + NumSectorVertices * 2 + 1);
		
		triangles.Add(start);
		triangles.Add(start + NumSectorVertices * 2 + 1);
		triangles.Add(start + NumSectorVertices + 1);

		triangles.Add(start);
		triangles.Add(start + NumSectorVertices + 1);
		triangles.Add(start + NumSectorVertices);
		
		triangles.Add(start + NumSectorVertices);
		triangles.Add(start + NumSectorVertices + 1);
		triangles.Add(start + NumSectorVertices + 2);

	}

	static void CacheDrawSector(float zDepth, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, bool flipUVX) {
		const float radius = 1f;
		const float startAngle = -Mathf.PI;
		const float arc = Mathf.PI;

		var vertexStartIndex = vertices.Count;

		vertices.Add(new Vector3(0, 0, zDepth));
		// x in the uvs is represents closeness to the center of the line, 1 is the middle, 0 is the edge
		// y represents a left to right gradient
		uvs.Add(new Vector2(1, 0));

		for (var i = 0; i < NumSectorVertices; i++) {
			var r = startAngle + i / (NumSectorVertices - 1f) * arc;
			var x = Mathf.Cos(r) * radius;
			var y = Mathf.Sin(r) * radius;
			vertices.Add(new Vector3(x, y, zDepth));
			uvs.Add(new Vector2(0, x * (flipUVX ? -1 : 1)));
		}

		for (var i = 0; i < NumSectorVertices - 1; i++) {
			triangles.Add(vertexStartIndex);
			triangles.Add(vertexStartIndex + i + 2);
			triangles.Add(vertexStartIndex + i + 1);
		}
	}

	void OnDrawGizmos() {
		if (mesh == null) return;
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(mesh.bounds.center, mesh.bounds.size);
	}
}
