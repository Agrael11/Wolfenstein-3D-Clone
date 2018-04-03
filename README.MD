#Wolfenstein 3D Clone

This is my Wolfenstein 3D engine clone made only with Windows Forms.
Everything is rendered into Bitmap using Graphics class.

This project is made just for fun and is not intended to be full game in future.



## Currently in game

*  Walls rendering
*  Door entity rendering -  Really buggy - needs to be fixed



## Possible (if project will be continued)

*  Collisions
*  Block entities (for hidden rooms)
*  Sprite entities (for enemies and random objects)
*  Main Menu
*  Some kind of AI
*  Remaking of all 6 episodes
*  Adding some customized episodes (and maybe level editor)

## Controls

W, S - Forward & Backward movement
A, D - Camera rotation
Q, E - Strafing
ALT + Enter - Fullscreen mode

## Images

(/images/1.png)
(/images/2.png)
(/images/3.png)
(/images/4.png)
(/images/anim.png)

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
of texture is rendered on x spot on screen.

For controls I'm currently using Windows Forms keyDown event, but that may change in future.