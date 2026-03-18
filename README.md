# IMG421-Assignment3

>  **Epilepsy Warning:** This simulation contains flashing lights, rapid color changes, and chromatic aberration effects that may affect people with photosensitive epilepsy. Player discretion is advised.

A Unity-based flocking simulation built around Craig Reynolds' Boids algorithm, themed as a neon cyberpunk city. Boids navigate a dark cityscape of glowing buildings while avoiding obstacles, leaving neon gradient trails in their wake.
 
---
 
## Play It
 
- **WebGL Build:** [Play on WebGL](https://acc668.github.io/IMG421-Assignment3/)
- **Video Walkthrough:** [Watch on YouTube](https://youtu.be/2TET8kPOCu8)
- **GitHub Repo:** [View Source](https://github.com/acc668/IMG421-Assignment3)
 
---
 
## How to Run Locally
 
1. Clone the repository
2. Open the project in **Unity 2022.3.62f1**
3. Open the `TitleScene` scene and press **Play**
4. Press **Start** to enter the main simulation
 
---
 
## How Boids Work
 
Flocking is driven by three steering behaviors combined using **weighted blending**:
 
- **Alignment** — each boid steers to match the average direction of its neighbors
- **Cohesion** — each boid steers toward the average position of its neighbors, keeping the flock together
- **Separation** — each boid steers away from neighbors that are too close, preventing collisions
 
Each behavior produces a velocity vector. Weighted blending multiplies each vector's components by a scalar weight, then sums them into a final steering direction applied to the boid.
 
An **Attractor** object moves through the scene on a sine-wave path, pulling the flock through space.
 
---
 
## Obstacle Avoidance
 
Obstacle avoidance is implemented as an additional weighted steering behavior:
 
1. Each boid shoots **two parallel rays** forward, offset left and right by the boid's half-width
2. If either ray hits a collider on the **Obstacle layer**, the boid reflects off the surface normal and steers away
3. A **proximity push** adds additional repulsion force that scales inversely with distance — the closer a boid gets, the harder it steers away
4. Obstacle avoidance takes priority over alignment, cohesion, and attraction, but yields to boid-to-boid separation
 
> Note: A small percentage of boids may still collide with obstacles at high speeds or tight angles — this is expected behavior in any real-time flocking implementation.
 
---
 
## Extra Credit Features
 
### Cyberpunk Theme
The entire project follows a cohesive cyberpunk neon city aesthetic including dark reflective floor, neon grid lines, glowing building pillars, pulsing obstacle lighting, and neon-colored boids.
 
### Unique Trails
Each boid has a glowing neon gradient `TrailRenderer` using additive blending. Trails fade from white-hot at the tip through the boid's assigned neon color, dissolving to transparent at the tail.
 
### Music
An electronic/synth soundtrack loops throughout the simulation. The `MusicManager` fades in on scene load and persists across scene transitions using `DontDestroyOnLoad`.
 
### Background & Floor
`CyberpunkEnvironment.cs` procedurally generates a dark reflective floor, a neon cyan grid, atmospheric fog, and neon-accented building pillars at runtime.
 
### Glitch Effect
A fullscreen `GlitchEffect` overlay runs on both the title screen and main scene, producing CRT scanlines, chromatic aberration color splits, random screen jitter, and brightness flicker bursts.
 
---
 
## Project Structure
 
```
Assets/
├── Materials/          # Trail and environment materials
├── PreFabs/            # Boid prefab
├── Scenes/
│   ├── TitleScene      # Title screen with start button
│   └── MainScene       # Main boids simulation
└── Scripts/
    ├── Attractor.cs          # Moves the attractor on a sine-wave path
    ├── Boid.cs               # Core boid steering + obstacle avoidance
    ├── CyberpunkEnvironment.cs # Procedural floor, grid, and buildings
    ├── GlitchEffect.cs       # Fullscreen CRT glitch overlay
    ├── LookAtAttractor.cs    # Makes the camera track the attractor
    ├── MusicManager.cs       # Looping music with fade-in
    ├── Neighborhood.cs       # Tracks nearby boids via trigger collider
    ├── ObstacleManager.cs    # Registers obstacles + neon pulse visuals
    ├── Spawner.cs            # Spawns and manages all boids
    └── TitleScreen.cs        # Title screen UI and scene loading
```
 
---
 
## Tunable Parameters (via Spawner Inspector)
 
| Parameter | Description |
|---|---|
| `Num Boids` | How many boids to spawn |
| `Velocity` | Base movement speed |
| `Neighbor Dist` | Radius boids look for neighbors |
| `Coll Dist` | Distance at which boids separate |
| `Vel Matching` | Alignment weight |
| `Flock Centering` | Cohesion weight |
| `Coll Avoid` | Separation weight |
| `Attract Pull` | How strongly the attractor pulls |
| `Attract Push` | How strongly boids push away when too close to attractor |
| `Obstacle Avoid` | Obstacle avoidance steering weight |
| `Obstacle Detect Dist` | How far ahead boids look for obstacles |
 
---
 
## Built With
 
- **Unity 2022.3.62f1**
- **C#**
- Unity Physics (Rigidbody + SphereCollider triggers)
- Unity UI (Canvas, Legacy Text)
- Unity TrailRenderer + LineRenderer
 
---
 
## Music Attribution
 
"Rocket" Kevin MacLeod (incompetech.com)
Licensed under Creative Commons: By Attribution 4.0 License
http://creativecommons.org/licenses/by/4.0/
