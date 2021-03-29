using UnityEngine;

public class LineTest : MonoBehaviour {
    
    void Update() {
        BulkLines.DrawLine(Random.insideUnitCircle, Random.insideUnitCircle, Random.value, Random.ColorHSV());
    }
}
