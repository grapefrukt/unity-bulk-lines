![usfxr](github~/header.png?raw=true "usfxr")

# Bulk lines for Unity

Ever need to draw a bunch of nice thick lines in a hurry?
You've come to the right place!

### Quickstart:
Clone this repository and open it in your editor. It's all set up and ready to go. 

### Usage:
 * Grab the [BulkLines folder](https://github.com/grapefrukt/unity-bulk-lines/tree/master/Assets/BulkLines) and drop it in your project.
 * Plonk a BulkLines component on your camera. 
 * Set it to use the BulkLines material. 
 * Draw lines "immediate-mode"-style from anywhere in your code: 

    ```BulkLines.DrawLine(Random.insideUnitCircle, Random.insideUnitCircle, Random.value, Random.ColorHSV());```

If you've used Debug.DrawLine(), this is very similar. There is no persistence, if you want a line to stay across multiple frames, you will need to draw it again. 
    
You can draw lines in 3D space, but the assumption is that the camera is facing along the Z axis (as per default in Unity 2D).  
The lines will not turn to face the camera. 

A variation of code has shipped for Android, iOS, Switch and PC in my game [holedown](https://holedown.com/).

## How does it work?

When the first line is drawn a big mesh is generated, this is then uploaded to the GPU where it stays, no more mesh data is transferred. 
This mesh is then wrangled into shape using a vertex shader, positioning the lines where you need them. Any lines not drawn will be hidden off screen. 

A mesh can only be so big, and more importantly, arrays passed to a shader have a maximum length. So, once you want to draw *extra* many lines (more than 255 by default), an additional batch is required. This all happens automatically and you don't need to worry about it. I set the maximum batch count to 8, but you could go higher if you need to. 

Each line is represented by 3 float4:

| from |   |
|---|---|
| xyz | from position |
| w | line width*  |

| to |   |
|---|---|
| xyz | to position |
| w | line width*  |

| color |   |
|---|---|
| rgb | color |
| a | alpha**  |

_*(current implementation will not allow setting differing start/end widths, but this could be added)_  
_** (not used in example implementation, but should be passed along)_
