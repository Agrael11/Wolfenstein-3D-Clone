# Wolfenstein 3D Clone

This is my Wolfenstein 3D engine clone made only with Windows Forms.
Everything is rendered into Bitmap using Graphics class.

This project is made just for fun and is not intended to be full game in future.



## Currently in game

*  Walls rendering
*  Door entity rendering
*  Collisions
*  Block entities (for hidden rooms)
*  Sprite entities (for enemies and random objects)



## Possible (if project will be continued)

*  Main Menu
*  Some kind of AI
*  Remaking of all 6 episodes
*  Adding some customized episodes 
*  Level editor

## Controls

W, S - Forward & Backward movement
A, D - Camera rotation
Q, E - Strafing
Space - Open Doors
ALT + Enter - Fullscreen mode

## Images

![CELL](https://raw.githubusercontent.com/Agrael11/Wolfenstein-3D-Clone/master/Images/1.png)
![CELLS](https://raw.githubusercontent.com/Agrael11/Wolfenstein-3D-Clone/master/Images/2.png)
![ESCAPE](https://raw.githubusercontent.com/Agrael11/Wolfenstein-3D-Clone/master/Images/3.png)
![DIRT](https://raw.githubusercontent.com/Agrael11/Wolfenstein-3D-Clone/master/Images/4.png)
![GIF](https://raw.githubusercontent.com/Agrael11/Wolfenstein-3D-Clone/master/Images/anim.gif)

## Technical Informations

This game clone uses raytraced rendering (same as original game). This means for each pixel in screen buffer on
X axis (currently set to 320 pixels) there's one ray shoot in specific angle.

Angle is calculated by following formula: _ANGLE = playerAngle - FOV/2 + FOV / gameResolution.X * x_  where x is
current x coordinate.

To make this process use less CPU, only intesections of 64x64 grid are calculated - for this I used simple
formula to calculate side of triangle, where i know two angles (90° & angle of ray) and distance to nearest intersection
on Y or X axis. Currently raycasting have 2 passes - one for vertical and one for horizontal walls in 2D map, but
that's gonna change to fix door rendering.

After ray hits target, precise collsion is calculated and send along with collider as result of raycast. From this one stripe
of texture is rendered on x spot on screen, also it's distance is saved into zBuffer.

Sprites are rendered separately. Their position is transformed into screen space and each vertical stripe is rendered, if
zBuffer is not bigger than it's distance from player.

For controls I'm currently using Windows Forms keyDown and keyUp event.
