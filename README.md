# SteelSoldierVR
This is a short VR demo created for Introduction to Virtual Reality at Rowan University.

*Steel Soldier* allows the player to step into the shoes (or suit?) of the very blatant *Iron Man*-knockoff, Steel Soldier.
The player is given the ability to fly with repulsors located on the hands, shoot lasers, and launch rockets to survive waves of enemy aircraft.

The target platform for this game is the Oculus Rift.

## Controls
### Hands
The player is given control over than hands of Steel Soldier, thus the real-world position of the Oculus Touch controllers determines the position and orientation of the hands in-game. Pretty simple. Unfortunately, there is no individual finger tracking (such as for the hands in Oculus Home) for this game.  
  
### Thrusters
Like *Iron Man*, Steel Soldier can fly using thrusters located on his palms. **The palm thrusters are activated by the _triggers_ located near either index finger.** The triggers output an *analog* signal, so pulling only a little bit will activate the thrusters only a little bit, and pulling all the way will activate the thrusters with full force. In game this is accompanied by a semi-randomized, looping thruster sound effect which takes its volume from the thruster intensity.

<p align="center">
  <img src="https://github.com/Llcoolsouder/SteelSoldierVR/blob/master/Demos/Thrusters.gif"
       alt="Thruster demo"
       width="75%">
</p>

### Lasers
Steel Soldier is also able to shoot lasers **using the _grips_ located near either middle finger.** The lasers shoot directly from the middle finger tips and deal a small amomunt of damage over time.

<p align="center">
  <img src="https://github.com/Llcoolsouder/SteelSoldierVR/blob/master/Demos/Lasers.gif"
       alt="Laser demo"
       width="75%">
</p>

### Missiles
Lastly, Steel Soldier can fire missiles that deal massive damage to enemy aircraft but travel much slower than the lasers. **The missiles are launched by pressing the _A or X_ buttons.** They will be launched from the hands, traveling in the direction in which your fingers are pointing.

<p align="center">
  <img src="https://github.com/Llcoolsouder/SteelSoldierVR/blob/master/Demos/Missiles.gif"
       alt="Missile demo"
       width="75%">
</p>

## Objective
_Steel Soldier_ is a survival game. At the start of the game, the user is dropped (no really, _literally dropped_) in a barren, desert-like landscape. Airplanes will spawn above and around the player. The player is tasked with shooting down as many enemy airplanes as possible before being shot down himself (i.e. running out of health).

## Aircraft AI
The aircraft AI are the only other "character" in the game other than the player. The planes are controlled by an invisible entity, the _Air Traffic Controller_ (ATC). The ATC dictates when planes are spawned, how many planes are spawned, which planes are _seeking_ the target, and which planes are in a holding pattern awaiting commands. Essentially, each plane is given a distance to maintain from the target. The planes will loosely adhere to this constraint due to a hardcoded PID controller for the steering mechanism. They will also break this constraint in order to avoid obstacles such as the terrain. The ATC will randomly choose different planes, one at a time, to be the _seeker_. The seeker breaks out of the holding pattern and makes a direct path toward the target's current position. At this time the plane will slightly increase its speed and begin firing upon the player. Just before reaching the target destination, the plane will cease fire, pull away, and do a barrel roll to avoid colliding with the player. (This is a pretty exciting event in VR.)

<p align="center">
  <img src="https://github.com/Llcoolsouder/SteelSoldierVR/blob/master/Demos/PlaneAI.gif"
       alt="Player's view of the planes"
       width="75%">
</p>

<p align="center">
  <img src="https://github.com/Llcoolsouder/SteelSoldierVR/blob/master/Demos/PlaneSwarming.gif"
       alt="Bird's-eye view of the plane swarming tactics"
       width="75%">
</p>

When planes are shot down, they will spiral out of control, leaving a trail of smoke until finally crashing into the ground, at which point they are removed from the game.

<p align="center">
  <img src="https://github.com/Llcoolsouder/SteelSoldierVR/blob/master/Demos/PlaneShotDown.gif"
       alt="Plane getting shot down"
       width="75%">
</p>

## Contributors
<ul>
  <li>
    Lonnie Souder II
    <ul>
      <li>Aircraft AI</li>
      <li>Player Controls (Oculus Rift)</li>
      <li>3D Modeling (Maya)</li>
      <li>Texturing (Substance Painter)</li>
      <li>Sound FX (FMOD)</li>
      <li>Particle FX</li>
    </ul>
  </li>
  <li>
    <a href="https://github.com/sglass520">Stephen Glass</a>
    <ul>
      <li>Player Controls (HTC Vive <em>originally</em>)</li>
      <li>Game Logic (waves, player health, etc.)</li>
      <li>HUD</li>
    </ul>
  </li>
  <li>
    <a href="https://github.com/nugentb7">Brendan Nugent</a>
    <ul>
      <li>Post-Processing FX to indicate player damage</li>
      <li>Terrain</li>
      <li>Debugging</li>
    </ul>
  </li>
</ul>
