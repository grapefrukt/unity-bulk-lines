# unity-bulk-lines
draws a lot of lines

Usage: 
Plonk a BulkLines component on your camera. 
Set it to use the BulkLines material. 

Draw lines "immediate-mode"-style: 

    BulkLines.DrawLine(Random.insideUnitCircle, Random.insideUnitCircle, Random.value, Random.ColorHSV());
