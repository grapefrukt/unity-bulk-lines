using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Utilities {
	public class PerformanceBars : MonoBehaviour {
		const int Size = 5;

		Stopwatch swUpdate;
		Stopwatch swRender;

		float[] ubuff;
		float[] rbuff;
		float usum;
		float rsum;
		int uindex = 0;
		int rindex = 0;

		Rect urect = new Rect();
		Rect rrect = new Rect();
		Rect mrect = new Rect();

		public float uavg;
		public float ravg;

		public Color colorUpdate;
		public Color colorRender;
		public Color colorMark;
	
		// Use this for initialization
		void OnEnable () {
			swUpdate = new Stopwatch();
			swRender = new Stopwatch();

			ubuff = new float[Size];
			rbuff = new float[Size];

			usum = 0;
			rsum = 0;

			StartCoroutine(FrameEnd());
		}

		IEnumerator FrameEnd() {
			while (true) {
				yield return new WaitForEndOfFrame();
				swRender.Stop();

				rsum -= rbuff[rindex];
				rbuff[rindex] = swRender.ElapsedTicks;
				rsum += rbuff[rindex];
				ravg = rsum / Size / Stopwatch.Frequency * 1000f;
				rindex = (rindex + 1) % Size;
			}
		}

		void Update () {
			swUpdate.Reset();
			swUpdate.Start();
		}

		void LateUpdate() {
			swUpdate.Stop();

			usum -= ubuff[uindex];
			ubuff[uindex] = swUpdate.ElapsedTicks;
			usum += ubuff[uindex];
			uavg = usum / Size / Stopwatch.Frequency * 1000f;
			uindex = (uindex + 1) % Size;

			swRender.Reset();
			swRender.Start();
		}

		Material materialRender;
		Material materialUpdate;
		Material materialMark;

		// Will be called from camera after regular rendering is done.
		public void OnPostRender() {
			if (!materialRender) materialRender = MakeMaterial(colorRender);
			if (!materialUpdate) materialUpdate = MakeMaterial(colorUpdate);
			if (!materialMark)   materialMark =   MakeMaterial(colorMark);

			GL.PushMatrix();
			GL.LoadOrtho();

			// the screen is 1 unit wide, divide it up so that the full width is equal to a 25fps frametime (40ms)
			const float sizeX = 1000 / 25f;
			const float sizeY = .0025f;
			const float spacing = 0.003f;

			mrect.x = 1000f / 60f / sizeX;
			mrect.width = 0.003f;
			mrect.height = sizeY * 2;
			materialMark.SetPass(0);
			DrawQuad(mrect);
			mrect.x = 1000f / 30f / sizeX;
			DrawQuad(mrect);

			urect.width = uavg / sizeX;
			urect.height = sizeY;
			materialUpdate.SetPass(0);
			DrawQuad(urect);
			
			rrect.x = urect.width + spacing;
			rrect.width = ravg / sizeX - spacing;
			rrect.height = sizeY;
			materialRender.SetPass(0);
			DrawQuad(rrect);

			rrect.x = Time.unscaledDeltaTime * 1000f / sizeX;
			rrect.height = sizeY * 3;
			rrect.width = 0.003f;
			DrawQuad(rrect);

			GL.PopMatrix();
		}

		static void DrawQuad(Rect r) {
			// draw a quad over whole screen
			GL.Begin(GL.QUADS);
			GL.Vertex3(r.xMin, r.yMin, 0);
			GL.Vertex3(r.xMax, r.yMin, 0);
			GL.Vertex3(r.xMax, r.yMax, 0);
			GL.Vertex3(r.xMin, r.yMax, 0);
			GL.End();
		}

		static Material MakeMaterial(Color color) {
			// Unity has a built-in shader that is useful for drawing
			// simple colored things. In this case, we just want to use
			// a blend mode that inverts destination colors.
			var shader = Shader.Find("Hidden/Internal-Colored");
			var mat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
			// Turn off backface culling, depth writes, depth test.
			mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			mat.SetInt("_ZWrite", 0);
			mat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
			mat.SetColor("_Color", color);

			return mat;
		}


	}
}
