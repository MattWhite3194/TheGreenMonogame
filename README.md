# Vestige
![alt text](Vestige/VestigeProgressPhotos/Cover.png?raw=true)

## About
Vestige is a 2D sandbox game where players can build, fight, and explore a fully destructable, procedurally/randomly generated tile world.  

## Technical
Vestige was built using the open-source Monogame framework, which is a reimplementation of Microsoft's XNA game framework.   

### Modules built while developing this project
Comprehensive, modular UI system.  
Random and procedural generation algorithms.  
Collision system with floating point collisions.  
File management and saving system.  
Crafting system.  
In-Game Debugging terminal and chat.  

## Progress

### Collisions, Inventory, and the Player:  

![alt text](Vestige/VestigeProgressPhotos/CollisionsInventoryPlayer.png?raw=true)

**Notes:**  
Added basic classes to the game including an entity, player, and world.  
Working on handling tile-based collisions.  
Created an input handler that sends out input events to any objects marked as an input handler, using a tree based structure.  


### World Generation:  

![alt text](Vestige/VestigeProgressPhotos/WorldGeneration.jpg?raw=true)

**Notes:**  
The terrain is generated using a 1D simple noise algorithm.  
Caves are generated from a 2D Perlin noise map.  
The map is saved as a png image, each pixel mapping to a tile.  

### Textures, Lighting, and Items:  

![alt text](Vestige/VestigeProgressPhotos/TexturesLightingItems.png?raw=true)

**Notes:**  
The player and tiles now have textures, and tiles merge with other tiles around them.  
Tiles store a tilestate, which is a one byte state representing if it's merged with tiles around it.  
These tilestates map to a dictionary of texture atlases  
Example: A dirt block with no surrounding tiles will return tilestate 0, A dirt block with a tile above it will return tilestate 1, etc...  
The binary representation of the tiles it is touching is seen below:  
![alt text](Vestige/VestigeProgressPhotos/TileStates.png?raw=true)  

The tilestate is retrieved by summing these values, corners are only added if both touching edges are both active, otherwise, they are ignored.  
Created a flood fill lighting algorithm that draws to the screen.  
There are now item classes in the game, with each one having a unique UseItem funtion.  

### Parallax Backgrounds:    

![alt text](Vestige/VestigeProgressPhotos/ParallaxBackgrounds.png?raw=true)

**Notes:**  
Created parallax background objects, which scroll with the players movement, and fill the entire screen.  
Not the best artist, Hence the pixel art.  

### Dynamic Lighting and tile checks:  

![alt text](Vestige/VestigeProgressPhotos/DynamicLightingTileChecks.png?raw=true)

**Notes:**  
The lighting algorithm can now handle per frame lighting updates, so things like torches, or flares that move will light tiles.  
Tiles now have checks they perform when being updated, including:  
&nbsp;UpdateState: gets the tiles texture atlas based on other tiles around it in the world.  
&nbsp;VerifyTile: checks if the tiles position is valid in the world, an example is a torch has to be touching either a wall or solid tile.  
&nbsp;CanTileBeDamaged: checks if the tile is able to be destroyed. Example: A chest cannot be broken if it's inventory is not empty. A solid tile cannot be mined if there is a tree above it  
There is a database of tiles, with different tile data classes that define each type.  
The database is accessed by the tile ID, and it holds methods and properties of each tile type.  

### New Lighting Algorithm, Tile Entities, Chests, and Water:  

![alt text](Vestige/VestigeProgressPhotos/NewLighting.png?raw=true)

![alt text](Vestige/VestigeProgressPhotos/TileEntitiesChestsWater.png?raw=true)

**Notes:**  
New lighting algorithm using an iterative approach, with light blurring across a 2d grid.  
The lighting algorithm now supports colors and blending.  
Large tiles (tiles larger than the standard 16x16) were added to the game.  
Large tile can have different animations.  
Interactable tiles and inventory tiles were added. Tiles can have a custom inventory size and can store items, which are saved in a dictionary in the WorldGen class.  

### Enemies and Item collisions:  

![alt text](Vestige/VestigeProgressPhotos/EnemiesAndItemCollisions.png?raw=true)

**Notes:**  
Enemies were added to the game, with animation and behavior components for composition and easier implementation of new enemies.  
Enemies types are stored in a database by their ID, and contain a template class of the enemy that is copied to spawn one.  
When a player swings an item, it is drawn with the corresponding draw style, Swing, Point, Hold, or None.  
The item drawn has a hitbox that will always contain the item based on its rotation.  
Collisions now have layers. Layers include Player, Enemy, ItemDrop, ItemCollider, and Friendly/Hostile Projectile.  
Collisions are filtered by layers.  
Example: An entity that collides with collision layer player will recieve collision events oly from entities with a layer value of Player.  

### UI System
Implemented a Component/Container based UI system that's easily scalable.  
Added a full anchor system  
Components include: Label, Button, Textbox, Slider, as well as any kind of custom UI component via extending the UIComponent class  
Containers include: Dropdowns, Grids, Panels, ScrollContainers, SelectionContainers, and any customizable container.  

### Crafting System
![alt text](Vestige/VestigeProgressPhotos/Crafting1.png?raw=true)
![alt text](Vestige/VestigeProgressPhotos/Crafting2.png?raw=true)
![alt text](Vestige/VestigeProgressPhotos/Crafting3.png?raw=true)  
A full crafting system with dynamic recipes has been implemented.  
Shapeless recipes are on the TODO list, but will be quick to implement with the current system.

### Shell / Terminal
![alt text](Vestige/VestigeProgressPhotos/Terminal.gif?raw=true)  
A command prompt and chatbox has been added to the game, which can be accessed by pressing '~' in-game.  
Commands include:  
```/summon <enemyID> <X> <Y>``` - Summons an entity at the (x, y) position in the world.  
```/item <id> <playerName> --quantity``` - gives the specified item to the player with the given name. Quantity is optional.  
```/position <playerName>``` - prints the position of the player to the terminal.