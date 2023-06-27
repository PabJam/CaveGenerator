Read Me

Showcase Video: https://www.youtube.com/watch?v=1C77bbD_RxI

1. L-System
  a. The L_System object in the scene defines the structure of the resulting
  cave.
  b. Base Compression defines the length of the tunnels.
  c. Generations defines the overall size of the cave
  d. Predetermined Rules define if a new seed should be generated or the
  specified seed should be used.
  e. The Axiom works as a second seed for the cave. Caves with the same
  Rules but a different Axiom are only slightly different.
  f. The seeds will be output through the console.
  g. A good example for a cave is Rules :
  FDU+U+FFSSDLDFFLLUU>FFSD+L+FFF<>+>F , Axiom :
  FLU>FS-L+LF

2. CaveGenerator
  a. The CaveGenerator generates the mesh around the L-system.
  b. If the option Generate Cave is turned off only L-systems without a mesh
  will be generated.
  c. Tunnel Radius defines the width of the tunnels.
  d. Add details controls the placement of stalactites and stalagmites in the
  cave.
  e. Textures is the list of textures, which will be evenly distributed along the
  height of the cave.

3. Terrain Brush
  a. Brush Size influences the affected area.
  b. Brush Strength influences how fast the terrain will grow / disappear

4. General
  a. The camera can be controlled with W/A/S/D and the mouse.
  b. Q removes terrain and E adds terrain.
  c. Space generates the Cave/L-System
  d. The Unity version is 2019.4.9f1