using UnityEngine;
using Random = UnityEngine.Random;

public class LineTest : MonoBehaviour {

    [Range(0, 1000)] public int numLines = 64;
    
    [Range(0, 1)] public float minWidth = .1f;
    [Range(0, 1)] public float maxWidth = 1;
    
    [Range(1, 64)] public float areaRadius = 1;
    
    void Update() {
        // let's draw a bunch of lines!
        
        for (var i = 0; i < numLines; i++) {
            BulkLines.DrawLine(
                Random.insideUnitCircle * areaRadius, 
                Random.insideUnitCircle * areaRadius, 
                Random.Range(minWidth, maxWidth), 
                Random.ColorHSV(0, 1, 1, 1, 1, 1, 1, 1)
            );
        }
    }

    void OnValidate() {
        // make sure max/min values stay separate
        minWidth = Mathf.Min(minWidth, maxWidth);
        maxWidth = Mathf.Max(minWidth, maxWidth);
    }
}
