# Boids
This project implements a **boids simulation** in **Unity** using the **Entity Component System (ECS)**.<br>
Its goal is to explore high-performance by applying **Data-Oriented Design (DOD)** principles.<br>
I implemented the system in both Object-Oriented (OOP) and ECS styles to compare their performance and efficiency.<br>

A boid is an autonomous agent following simple rules of alignment, separation and cohesion to simulate flocking behavior [see more](https://www.red3d.com/cwr/boids/)

<p align="center">
    <img src="./ReadmeAssets/boid.gif" alt="boid"/>
</p>

## Table of Content
- [Implementation](#implementation)
    - [ECS Implementation](#ecs-implementation)
    - [Object Oriented Design Implementation](#object-oriented-design-implementation)
    - [Improvement](#improvement)
- [Technology](#technology)
- [Credit](#credit)

## Implementation
### ECS Implementation
- **Architecture:** Pure DOTS using **Entities 1.0**, **Burst Compiler**, and the **Job System**.
- **Flocking Logic:** Currently uses a naive **$O(N^2)$** approach (brute-force neighbor check) inside a parallel `IJobEntity`.
- **Movement:** 2D logic on the **X/Z plane** with Y-axis rotation.
- **Obstacle Avoidance:** Uses **Unity Physics** with a "Whisker" raycast system to scan angles and steer around static geometry.

### Object Oriented Design Implementation
* **Architecture:** Standard `MonoBehaviour` scripts attached to GameObjects.
* **Logic:** Single-threaded `Update()` loops.
* **Purpose:** Serves as a performance baseline to demonstrate the limitations of traditional Unity architecture when handling hundreds of autonomous agents.

### Improvement
**Current Issue:** The simulation checks every boid against every other boid ($O(N^2)$), causing performance to drop quadratically as population grows. <br>
**Solution:** Implement a **Spatial Hash Grid**:
- Boids will be bucketed into grid cells based on position.
- Queries will only check the specific cell and immediate neighbors.
- **Goal:** Reduce complexity to **$O(N)$** to support thousands of entities.

## Technology
- [Unity 6](https://unity.com/releases/editor/whats-new/6000.0.58f2#installs) *(version: 6000.0.58f2)*

## Credit
Made by: [Vincent DEVINE](https://github.com/Vincent-Devine)

Character Asset: [Starter Assets - ThirdPerson by Unity](https://assetstore.unity.com/packages/essentials/starter-assets-thirdperson-updates-in-new-charactercontroller-pa-196526?srsltid=AfmBOopdG8VHqp3Fo8kroE2Cl3727aKIMhZLZameR2AFm6Vc3FSr50da) <br>
Building Asset: [City Kit by kenney](https://kenney.nl/assets/city-kit-suburban)

Implementation of the OOP architecture is based on [Sebastien Lague](https://github.com/SebLague)'s work  on [boids](https://www.youtube.com/watch?v=bqtqltqcQhw)<br>
Implementation of the ECS architecture is based on [Unity](https://github.com/Unity-Technologies)'s work  on the [ECS Samples](https://github.com/Unity-Technologies/EntityComponentSystemSamples/tree/master)
