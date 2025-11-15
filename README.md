# Boids
This project implements a **boids simulation** in **Unity** using the **Entity Component System (ECS)**.<br>
Its goal is to explore high-performance by applying **Data-Oriented Design (DOD)** principles.<br>
I implemented the system in both Object-Oriented (OOP) and ECS styles to compare their performance and efficiency.<br>

A boid is an autonomous agent following simple rules of alignment, separation and cohesion to simulate flocking behavior [see more](https://www.red3d.com/cwr/boids/)

## Table of Content
- [Implementation](#implementation)
    - [ECS Implementation](#ecs-implementation)
    - [OOP Implementation](#oop-implementation)
- [Technology](#technology)
- [Credit](#credit)

## Implementation
### ECS Implementation
todo

### OOP Implementation
Scene: `Scenes\ObjectOriented.unity`

| Script            | Description                                                                                  |
|-------------------|----------------------------------------------------------------------------------------------|
| OO_Boid.cs        | Calculates flocking forces (alignment, cohesion, separation) and avoids collisions per boid. |
| OO_BoidManager.cs | Updates all boids each frame, applying flocking forces and positions.                        |

## Technology
- [Unity 6](https://unity.com/releases/editor/whats-new/6000.0.58f2#installs) *(version: 6000.0.58f2)*

## Credit
Made by: [Vincent DEVINE](https://github.com/Vincent-Devine)

Implementation of the OOP architecture is based on [Sebastien Lague](https://github.com/SebLague)'s work  on [boids](https://www.youtube.com/watch?v=bqtqltqcQhw)<br>
Implementation of the ECS architecture is based on [Unity](https://github.com/Unity-Technologies)'s work  on the [ECS Samples](https://github.com/Unity-Technologies/EntityComponentSystemSamples/tree/master)
