# TheGreen
![alt text](TheGreen/TheGreenProgressPhotos/Cover.png?raw=true)

## About
The Green is a 2D sanbox game where players can explore a 2d randomly generated world.  
Players can build, fight, and explore a fully destructable tile world.  

## Game Showcase

![alt text](TheGreen/TheGreenProgressPhotos/GameShowcaseEnemies.gif?raw=true)

![alt text](TheGreen/TheGreenProgressPhotos/GameShowcaseTiles.gif?raw=true)

![alt text](TheGreen/TheGreenProgressPhotos/GameShowcaseChests.gif?raw=true)

![alt text](TheGreen/TheGreenProgressPhotos/GameShowcaseWater.gif?raw=true)

## Technical
The Green was built using the opensource Monogame framework, which is a reimplementation of Microsoft's XNA game framework.  
C# is the language of choice for this project, as it is Object-Oriented, and allows for a cleaner development environment.  
The project was written in Microsoft's Visual Studio IDE.  

## Progress

### Collisions, Inventory, and the Player:  

![alt text](TheGreen/TheGreenProgressPhotos/CollisionsInventoryPlayer.png?raw=true)

**Notes:**  
Added basic classes to the game including an entity, player, and world.  
Working on handling tile-based collisions.  
Created an input handler that sends out input events to any objects marked as an input handler, using a tree based structure.  


### World Generation:  

![alt text](TheGreen/TheGreenProgressPhotos/WorldGeneration.jpg?raw=true)

**Notes:**  
The terrain is generated using a 1D simple noise algorithm.  
Caves are generated from a 2D Perlin noise map.  
The map is saved as a png image, each pixel mapping to a tile.  

### Textures, Lighting, and Items:  

![alt text](TheGreen/TheGreenProgressPhotos/TexturesLightingItems.png?raw=true)

**Notes:**  
The player and tiles now have textures, and tiles merge with other tiles around them.  
Tiles store a tilestate, which is a one byte state representing if it's merged with tiles around it.  
These tilestates map to a dictionary of texture atlases  
Example: A dirt block with no surrounding tiles will return tilestate 0, A dirt block with a tile above it will return tilestate 1, etc...  
The binary representation of the tiles it is touching is seen below:  
![alt text](TheGreen/TheGreenProgressPhotos/TileStates.png?raw=true)  

The tilestate is retrieved by summing these values, corners are only added if both touching edges are both active, otherwise, they are ignored.  
Created a flood fill lighting algorithm that draws to the screen.  
There are now item classes in the game, with each one having a unique UseItem funtion.  

### Parallax Backgrounds:    

![alt text](TheGreen/TheGreenProgressPhotos/ParallaxBackgrounds.png?raw=true)

**Notes:**  
Created parallax background objects, which scroll with the players movement, and fill the entire screen.  
Not the best artist, Hence the pixel art.  

### Dynamic Lighting and tile checks:  

![alt text](TheGreen/TheGreenProgressPhotos/DynamicLightingTileChecks.png?raw=true)

**Notes:**  
The lighting algorithm can now handle per frame lighting updates, so things like torches, or flares that move will light tiles.  
Tiles now have checks they perform when being updated, including:  
&nbsp;UpdateState: gets the tiles texture atlas based on other tiles around it in the world.  
&nbsp;VerifyTile: checks if the tiles position is valid in the world, an example is a torch has to be touching either a wall or solid tile.  
&nbsp;CanTileBeDamaged: checks if the tile is able to be destroyed. Example: A chest cannot be broken if it's inventory is not empty. A solid tile cannot be mined if there is a tree above it  
There is a database of tiles, with different tile data classes that define each type.  
The database is accessed by the tile ID, and it holds methods and properties of each tile type.  

### New Lighting Algorithm, Tile Entities, Chests, and Water:  

![alt text](TheGreen/TheGreenProgressPhotos/NewLighting.png?raw=true)

![alt text](TheGreen/TheGreenProgressPhotos/TileEntitiesChestsWater.png?raw=true)

**Notes:**  
New lighting algorithm using an iterative approach, with light blurring across a 2d grid.  
The lighting algorithm now supports colors and blending.  
Large tiles (tiles larger than the standard 16x16) were added to the game.  
Large tile can have different animations.  
Interactable tiles and inventory tiles were added. Tiles can have a custom inventory size and can store items, which are saved in a dictionary in the WorldGen class.  

### Enemies and Item collisions:  

![alt text](TheGreen/TheGreenProgressPhotos/EnemiesAndItemCollisions.png?raw=true)

**Notes:**  
Enemies were added to the game, with animation and behavior components for composition and easier implementation of new enemies.  
Enemies types are stored in a database by their ID, and contain a template class of the enemy that is copied to spawn one.  
When a player swings an item, it is drawn with the corresponding draw style, Swing, Point, Hold, or None.  
The item drawn has a hitbox that will always contain the item based on its rotation.  
Collisions now have layers. Layers include Player, Enemy, ItemDrop, ItemCollider, and Friendly/Hostile Projectile.  
Collisions are filtered by layers.  
Example: An entity that collides with collision layer player will recieve collision events oly from entities with a layer value of Player.  
