# MultiplayerVoxelGame

A very simple Minecraft-like voxel game with basic multiplayer support.

## Features

* Players can place and break blocks within a chunk.
* Game state is replicated between clients and the server.
* Clients can see each other’s player models/positions in real time.

## Tech Stack

* **OpenTK** for graphics a .NET binding library for OpenGL.
* **TCP** for networking between clients and the server.

## Setup & Installation

1. **Download the latest release** to try the project.
2. **Edit the configuration file** to choose whether the program runs as a *host*, *client*, or *dedicated server*.
3. **Set the port and IP address** in the configuration depending on whether you are connecting to a server or hosting one.
4. If you want to **build the project yourself**, make sure to copy the `assets` folder from the release into your build output directory.


<img width="1919" height="1079" alt="image" src="https://github.com/user-attachments/assets/afeefbe7-7bb7-45c9-a721-32374372fc72" />
