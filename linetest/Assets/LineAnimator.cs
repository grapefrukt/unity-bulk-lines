using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LineAnimator : MonoBehaviour {

	public BulkLines lines;
	Vector4[] velocities;

	void Awake() {
		velocities = new Vector4[BulkLines.MaxLineCount];

		for (var i = 0; i < BulkLines.MaxLineCount; i++) {
			var va = Random.insideUnitCircle;
			var vb = Random.insideUnitCircle;
			velocities[i] = new Vector4(va.x, va.y, vb.x, vb.y);

			var from = Random.insideUnitCircle * 3;
			var to = Random.insideUnitCircle * 3;
			
			var color = Random.ColorHSV(0, 1, 1, 1, .5f, 1);
			var width = .005f + Random.value * .1f;

			lines.SetLine(i, from, to, color, width);
		}
	}

	void Update() {
		for (var i = 0; i < BulkLines.MaxLineCount; i++) {
			Vector2 from;
			Vector2 to;
			Color color;
			float width;

			lines.GetLine(i, out from, out to, out color, out width);

			from.x += velocities[i].x * Time.deltaTime;
			from.y += velocities[i].y * Time.deltaTime;
			to.x   += velocities[i].z * Time.deltaTime;
			to.y   += velocities[i].w * Time.deltaTime;

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

			lines.SetLine(i, from, to, color, width);
		}
	}
}
