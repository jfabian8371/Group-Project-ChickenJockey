# Group-Project Chicken Jockey
Our plan currently is to create an arcade/roguelite game with a variety of enemies and weapons the player can use to kill the enemies. The player will be in an open arena with some verticality which spawns enemies from a few rooms/spawn points each round, at the end of each round the player will be able to go into a store and buy armor, health, upgraded stats, weapons, and unique skills. Outside of the store there is a button that you push in order to move the game to the next round. As rounds progress more enemies and more different enemies spawn in, and every 5 rounds, a boss variant (or multiple) can spawn.

Rather than having a round time limit, or a certain number of enemies to kill per round, there will be a total "health pool" and killing any kind of enemy takes away from the total pool. When the HP pool reaches zero, the next round starts.

Currently planning for 5 main enemy types with different AI for each, as well as boss/elite variants with better stats, including...
* basic COD Zombies-type follower AI
* flying enemy AI
* burowing worm enemy AI
* large nest/spawner AI
* TBD

# Group Roles
* Park: Camera, main character guy
* Aidan: Weapons, enemies
* Liam: Store, next round button, calculating balanced enemy scaling
* Bryan: Weapons, enemies, shaders
* Roger: UI, hud

Tentative plan for the powerup/hud screen between rounds:
Offered powerups:
restore health, increase max health, get fixed amount of armor (percent reduction or temp health), inc move speed, inc damage, fire rate, new gun, Bullet variety, ie (exploding , slower but bigger, more damage but smaller, etc)

For the screen itself, put it on a wall or in the center of the arena
Just needs buttons for : new round, purchase item (one for each) , activated by shooting the buttons.

offered powerups/items should follow:
	guaranteed to offer restore health
	2 or 3 others chosen from a pool 
		1 for defense
		1 for offense 
		new gun or extra offensive perk if all guns owned
