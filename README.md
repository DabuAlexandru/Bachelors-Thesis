# Procedural Generation in Game Development

## Summary

This project tackles the problem of generating content for video games using procedural generation techniques. The aim is to use mathematical principles and algorithms to imitate real-world aspects, such as natural environments, using noise functions and controlled random number generators. The goal is to create relevant elements of reasonable quality in a reasonable time while providing game developers with a flexible and efficient way of creating game content.

## Description

The project was created in Unity C#, utilizing the Universal Rendering Pipeline, as a 3D game that uses shaders and mesh manipulation to enhance the game design.
The focus of the game is the procedurally generated game elements, including puzzles, terrain, and trees, using noise functions and a graph-based representation of tree ramification.

The game has two levels: 
* **Platformer**: A level focused more on the game design, in which the mesh manipulation element is the column puzzle. The puzzle can be resolved by modeling the column after a given shape such that we satisfy a similarity threshold.
* **Explore and Search**: A level in which all the elements are procedurally generated. This level consists of an island where the player must locate 5 blobs to pass. The key elements of the scene are the terrain, which is generated after a chosen noise function, and the trees, which are generated after a binary-tree configuration of the ramification.

## Final Results

### Sculpting Minigame

#### Puzzle configuration

![image](https://user-images.githubusercontent.com/61749993/233215488-b53e361c-a511-468e-955b-92ef4d3f15c5.png)
![image](https://user-images.githubusercontent.com/61749993/233215498-9623b435-a53f-4208-9230-9b8a26572514.png)
![image](https://user-images.githubusercontent.com/61749993/233215501-3eccef04-fb9b-4dd9-808f-9b9cc29be2b3.png)

#### Minigame Window

![image](https://user-images.githubusercontent.com/61749993/233215662-69e285df-4816-49bf-a4d9-b8843c199d39.png)

#### Puzzle Minigame as an interactable element in a platformer scene

![image](https://user-images.githubusercontent.com/61749993/233215764-80d7166f-6727-4e1c-a4c8-bbf912341dea.png)

### Terrain Generation

#### Perlin Noise

![image](https://user-images.githubusercontent.com/61749993/233064405-c67cc33b-e034-4a66-8518-c29647001170.png)

#### Diamond-Square Noise

![image](https://user-images.githubusercontent.com/61749993/233064690-eead39c0-cf2b-4a9f-b0ff-4636c57b50fa.png)

![image](https://user-images.githubusercontent.com/61749993/233066762-8a4e7310-3af9-4b9b-8b61-177a0e168b1b.png)

#### Diamond-Square Noise with Voronoi

![image](https://user-images.githubusercontent.com/61749993/233068598-3bc2ac68-fdb8-49cc-b83f-34641b78b94d.png)

![image](https://user-images.githubusercontent.com/61749993/233072517-a1ef1b3e-71ce-43e3-8058-26595089d93f.png)

#### Island Configuration with terrain transition (chosen: c)

![image](https://user-images.githubusercontent.com/61749993/233174010-cbf85724-c568-464b-8044-ca6ff8fbceea.png)

#### Tree distribution models (chosen: c)

![image](https://user-images.githubusercontent.com/61749993/233176304-f9b04fe4-36d1-4d1e-8e1e-88a23eecba88.png)

#### Island Level Of Detail Distribution in regards to the position of the player

![image](https://user-images.githubusercontent.com/61749993/233174273-73c34835-3ca0-481a-b216-151e2a7b28b5.png)

### Vegatation: Trees

#### Branch Complexity

![image](https://user-images.githubusercontent.com/61749993/233164655-da314738-f658-4474-84d7-790ee68a93ee.png)

#### Level Of Detail variations

![image](https://user-images.githubusercontent.com/61749993/233164850-79c7ed1a-fa34-4cb6-90f5-dd9488d5ab57.png)

#### Tree Foliage

![image](https://user-images.githubusercontent.com/61749993/233171476-b10b1c06-66b4-4f91-af54-fcee0d84d827.png)

#### Tree Model with maximal and minimal LOD

![image](https://user-images.githubusercontent.com/61749993/233172032-bd45e971-1f1e-4fa2-bc83-61356a97680c.png)

#### Final Level Configuration

![image](https://user-images.githubusercontent.com/61749993/233185417-7ceb5302-24dd-4176-9adc-ecdb36049bd1.png)
![image](https://user-images.githubusercontent.com/61749993/233185438-761735e4-1c4a-4f46-807e-8834f0857a89.png)
![image](https://user-images.githubusercontent.com/61749993/233185563-e333524f-6b15-4c93-a8b1-9e52e116c7ff.png)

## Resources

1. Lague, Sebastian. “Procedural Terrain Generation.” Youtube, (2017), [Procedural Terrain Generation. Sebastian Lague](https://www.youtube.com/watch?v=wbpMiKiSKm8&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3&index=1). Accesed 14 May 2022.
2. Flick, Jasper. “Unity Procedural Meshes Tutorials.” Catlike Coding, (2021), https://catlikecoding.com/unity/tutorials/procedural-meshes/. Accesed 1 March 2022.
3. Togelius, Julian, et al. "What is procedural content generation? Mario on the borderline." Proceedings of the 2nd international workshop on procedural content generation in games. (2011).
4. Freiknecht, Jonas, and Wolfgang Effelsberg. "A survey on the procedural generation of virtual worlds." Multimodal Technologies and Interaction 1.4 (2017): 27.
5. Mantler, Stephan, Robert F. Tobler, and Anton L. Fuhrmann. "The state of the art in realtime rendering of vegetation." VRVis Center for Virtual Reality and Visualization 2 (2003).
6. Prusinkiewicz, Przemyslaw, and Aristid Lindenmayer. The algorithmic beauty of plants. Springer Science & Business Media, (2012).
7. Kim, Jinmo. "Modeling and optimization of a tree based on virtual reality for immersive virtual landscape generation." Symmetry 8.9 (2016): 93.
8. Parberry, Ian. “Designer worlds: Procedural generation of infinite terrain from real-world elevation data.” Journal of Computer Graphics Techniques 3.1 (2014).
9. Olsen, Jacob. “Realtime procedural terrain generation-realtime synthesis of eroded fractal terrain for use in computer games.” (2004).
10. Hijbeek, Renske, et al. "An evaluation of plotless sampling using vegetation simulations and field data from a mangrove forest." PloS one 8.6 (2013): e67201.



