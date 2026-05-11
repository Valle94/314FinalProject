# Mixed-Reality Chess

This game was created as a final project for my class XR Game Programming. It's built on a Meta Quest 3 using a built-in room scan, spatial anchors, and fully interactable chess board and UI. The it was built as a mixed reality project utilizing full passthrough and roomscale interaction. 

## How To Run The Project

Currently, the project is built to headset "VR#01" in the school VR Lab. To access it, you need to find the Meta context menu found by pressing the Meta button on the right controller. Then, on the left side you need to open the menu and navigate to "untrusted sources." The game can be found there as "MValleFinalProject."

Instructions for creating and loading spatial anchors can be found in game. 

## Interactive Objects and Mechanics

The game currently has three types of interactions: UI raycast, grab, and poke. All interactions can be used with both controllers and hands. The chess pieces are grabable and movable around the board, the start screen and in-game UI are both usable with raycast interactors, and the timer buttons are interactable via poke interactions. 

There is currently no chess game logic built into the board. For example, pieces can be moved to any square, not just legal moves, and one player can move multiple times in a row. 

## External Assets

Project was built using Meta Quest Developer Hub and the built-in Meta Quest Unity Building Blocks. 
Chess Board/Pieces Assets: https://assetstore.unity.com/packages/3d/props/low-poly-chess-set-board-and-timer-216547
Hover Visual Asset: https://dineshpunni.notion.site/Snap-Interaction-048c3668caa34c91bce931cd4903f9e8

## Video Link

([https://www.youtube.com/embed/APOPm01BVrk](https://drive.google.com/file/d/1iYvIOGcTiUjc4xRbR4oc_L-0k9A_jIJS/view?usp=sharing))

## To-Do List/Known Limitations

Right now the only known bug is that the pieces don't spawn "snapped" into the board sockets. 

If I were to continue this project, I would add chess logic and multiplayer functionality using Meta's collocation and shared spatial anchors. 
