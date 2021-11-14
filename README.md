# godot-jigglebones-CS
An addon that brings jigglebones to Godot Engine 3.0. Redone in C# for Godot Mono Version.


How to use it
1. Download the repository as zip and extract it.
2. Copy the addons folder into the root folder of your project, writing into it if it already exists.
3. Now re-open your project, then in the menu bar go to Project → Project Settings, then go to the Plugins tabs, and then set the JiggleboneCS addon to activated. It should look like this:

4. In the scene of your game, find the Skeleton node of your character and select it. (If you don't have a rigged skeleton yet, skip to section Rigging, then come back here.) Click the plus icon above the scene tree to add a node, then select JiggleboneCS.
5. Now your scene should look like this, with the Jigglebone being a direct child of the Skeleton:

6. With the jiggleboneCS selected, go to the inspector and set the Bone name property to the name of the bone you want to turn into a jiggleboneCS. E.g. if you have a bone named "Chin", then it would look like this:

7. That's all! Enjoy your jiggling! If you want more jiggleboneCSes, just repeat step 4-6 again and enter another bone name.
8. Optionally, for further control, you can tune the parameters, see below.
