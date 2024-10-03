# SpaceTurret-DefensePoint
Space Turret is about player defense in space against hordes of enemies and bosses.<br>
This game has 1st and 2nd campaign, few survival modes and few multiplayer modes.<br>
![A few of my hundreds of scripts](https://user-images.githubusercontent.com/101559700/167312887-2c20c8e4-4bb4-4a02-938e-999a880732a0.png)<br><br>
![game modes](https://user-images.githubusercontent.com/101559700/167312887-2c20c8e4-4bb4-4a02-938e-999a880732a0.png)
<br>
## 1st campaign<br>
Player can control up to 3 turrets to fight against enemies. There are 5 stages with 20 rounds. At the end of each stage there is a Boss. When the player defeats the Boss and proceeds to the next stage he unlocks new turret skins and unique turrets.<br>
![gameplay](https://user-images.githubusercontent.com/101559700/167313113-1ecb7fc4-9b63-42b4-9907-4578ca380c3d.png)
<br>Each stage has unique skybox and station
![image](https://user-images.githubusercontent.com/101559700/167314213-78561c35-2689-4428-b29b-900f3209cbd4.png)
![image](https://user-images.githubusercontent.com/101559700/167314278-5b3905a5-ff86-4e99-9c42-45eab346fb86.png)
<br>The shop has 21 turrets. There is different ammo types: bullets, rockets and lasers.<br>
![the shop](https://user-images.githubusercontent.com/101559700/167313240-77c4e603-ac04-4a64-879c-b21d0dc8083a.png)
<br>Each turret has many upgrades<br>
![turret upgrades](https://user-images.githubusercontent.com/101559700/167313377-2ffafe5a-6ef7-44e6-bb1d-fb9abc11e3d1.png)
<br>And turret skins
![image](https://user-images.githubusercontent.com/101559700/167709506-9f83ee08-c216-41a7-9eb6-c27dffb5357a.png)
<br>
The 1st campaign has a few types of enemies: Fighters, Frigates, Cruisers, Carriers and Suiciders.<br> 
Each enemy type has its own weapons, logic and behaviour. Same with b osses.<br>
![Zorg boss](https://user-images.githubusercontent.com/101559700/167636112-cd5e17be-786c-4da5-b18f-f2dbce3d3d5b.png)

## 2st campaign<br>
This mode has slightly different gameplay than the 1st campaign. <br>
The Player defends the space ship. Ship consists of different modules, each module has its own turret spots. Player can purchase, sell or swap ship modules.<br>
![image](https://user-images.githubusercontent.com/101559700/167655957-650f3d70-35aa-4108-a02c-146320484e95.png)
<br>
Turret spots can be controlled by AI or by the player. The player can set target priorities for AI on each turret.<br>
![image](https://user-images.githubusercontent.com/101559700/167656665-91fb7906-1d64-406f-b05b-1ce84dfb0d9a.png)
<br>
Also the player controls the ship power system. The ship's power system supplies three subsystems: damage system, repair system and cooling system.<br>
![image](https://user-images.githubusercontent.com/101559700/167657344-ed9e600f-a108-4be1-a162-7d962f7e12cd.png)
<br>
the player starts the game in the safe zone - in hyperdrive. There the player can purchase or upgrade turrets, manage power of the ship system or purchase, sell and manage ship modules. There are three types of modules: front, middle and back modules.
![image](https://user-images.githubusercontent.com/101559700/167658379-156eb29d-381e-45a2-a0bf-e8ec63845e79.png)
<br>
Heavily damaged modules have visual damages: scratches and plating damages. If a module is destroyed, all turrets on that module will stop working.
![image](https://user-images.githubusercontent.com/101559700/167660311-84cf2518-2dae-4b79-918f-5e44da83580a.png)
![image](https://user-images.githubusercontent.com/101559700/167660619-dd980db8-c1bd-46ab-859b-8a7a6792b160.png)
<br>
At this moment there are few enemy types in the 2nd campaign: fighters, 2 types of frigates and 2 types of cruisers. Frigates and Cruisers are dynamically generated and their health and firepower is adjusted. Enemy spawner is using dynamic difficulty adjustment. This mode can adapt itself to the player's skills so the difficulty of the game becomes dynamic. High difficulty waves will be set if the players use good strategy and low difficulty waves will be set if the players use bad strategy.
![image](https://user-images.githubusercontent.com/101559700/167707555-9c6e6fca-026b-4762-af75-6cddf7a6e579.png)
<br>
Environment is also generated dynamically: asteroids, shipwrecks, nebula clouds.<br>
2nd campaign has infinite rounds and is not finished yet. I want to add huge station-bosses, new enemies and more cool environment elements.
![image](https://user-images.githubusercontent.com/101559700/167708640-d26dbb49-7352-4aa9-88fd-356cba810cc7.png)

## Multiplayer<br>
Multiplayer mode is based on the Photon Multiplayer plugin. There are few game modes with completely different gameplay and with bot support.
![image](https://user-images.githubusercontent.com/101559700/167709991-b4617f3e-d48c-437b-9b09-5ce716baed70.png)

### 1. Lost Aurora<br>
This mode is for 1-4 players, bots are allowed. Game starts in hyperspace, player ship is flying on a mission to rescue Aurora ship somewhere in a cloud.
![image](https://user-images.githubusercontent.com/101559700/167712395-27101d8e-9b63-49f6-8219-606b69ab1a25.png)
<br>
Player leaves hyperspace in front of huge nebula cloud
![image](https://user-images.githubusercontent.com/101559700/167712547-9db9a8b6-63ec-49a8-bc64-f195977311ce.png)
<br>
There are 13 different turrets on the ship - from light turrets to heavy turrets. Different types of turrets are best used against certain groups of enemies.
![image](https://user-images.githubusercontent.com/101559700/167712930-7617ab1d-e182-422e-8b68-ccd03725a06f.png)
<br>
The player needs to make his way through the waves of enemies to the place of the aurora signal. Enemy spawner is using dynamic difficulty adjustment for best player experience.
![image](https://user-images.githubusercontent.com/101559700/167713042-a931cb98-801d-4b31-a7cc-d6e44aff41c2.png)
<br>
When the player reaches the aurora, a rescue mission will begin. Several shuttles will be sent to transfer the crew to the player's ship.
![image](https://user-images.githubusercontent.com/101559700/167713240-5fda55e7-2bc9-466e-8897-177c0eb98035.png)
<br>
During that rescue operation the player ship will be attacked by a new enemy type - scifi worms.
![image](https://user-images.githubusercontent.com/101559700/167713574-eb681b9d-fd22-452b-ab75-0acf8730aac9.png)
<br>
After completing the rescue operation the player ship will set course to home. When the player leaves the cloud he will get into a trap where he will have to fight against a huge boss. If the player will defeat him, he will unlock a unique turret from the last killed boss module.
![image](https://user-images.githubusercontent.com/101559700/167713851-3095ad6d-f6e3-4de5-93b3-3a76f58192d8.png)
<br>

### 2. Warp Gates<br>
This mode supports 1-3 players, bots are allowed.<br>
Players have to defend the station together against hordes of enemies. Star Gates will provide the enemy with limitless reinforcements. Players need to destroy these gates to interrupt enemy supply routes and clear off the remaining forces. New enemies become stronger with every destroyed gate.
![image](https://user-images.githubusercontent.com/101559700/167715220-8bade8f1-3517-4aab-b0d7-1189ea5873c2.png)

## Purchases<br>
This game supports InApp purchases. Players can purchase Crion Points and get unique badges. These badges will be shown with the player nickname. With badges the player removes ads, unlock new skins and new colors for skins.<br>
Players can check what each badge will unlock.
![image](https://user-images.githubusercontent.com/101559700/167783429-7ca19cfe-84f3-4f9e-ab28-071b0f0975aa.png)
![image](https://user-images.githubusercontent.com/101559700/167783491-06524fb6-79dd-4ad2-9ae2-acbce2720ca9.png)

## My roles on this project<br>
1. C# programmer (UI, Game Logic, AI, Multiplayer and many more)
2. Game designer (prepare game content, meshes, sprites, create levels, balance enemies)
3. Shader programmer (optimizations and graphic improvements)
4. Team leader (Initiator of different changes, discussions, quality check, team collaboration)
5. Tester (test new features, game balance and many more)
6. Google play publisher
7. Support & development
