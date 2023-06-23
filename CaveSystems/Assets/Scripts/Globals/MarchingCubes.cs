using System.Collections.Generic;
using UnityEngine;

public class MarchingCubes
{
    // Game Object of the chunk
    public GameObject chunkObject;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;
    private Vector3Int chunkPosition;
    private Dictionary<Vector3, Point> pointsInCapsules = new Dictionary<Vector3, Point>();
    public bool hasCapsulesInChunk = false;
    private List<Capsule> capsulesInChunk = new List<Capsule>();
    private QuadTree qt;

    // Distance from the closest point on a line
    private float[,,] wallDistance;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    // Dimensions of the chunk
    private int length
    { get { return CaveData.chunkSizeX; } }

    private int height
    { get { return CaveData.chunkSizeY; } }

    private int width
    { get { return CaveData.chunkSizeZ; } }

    // Radius of the tunnels
    private float terrainSurface
    { get { return CaveData.terrainSurface; } }

    // Id of the texture array which will be applied
    public int textureID = 0;

    /// <summary>
    /// Constructs a chunk and runs the marching cubes algorithm inside the chunk
    /// </summary>
    /// <param name="_position"></param>
    /// <param name="material"></param>
    /// <param name="_textureID"></param>
    /// <param name="botTexId"></param>
    /// <param name="topTexID"></param>
    public MarchingCubes(Vector3Int _position, Material material, int _textureID, float botTexId, float topTexID)
    {
        chunkPosition = _position;
        wallDistance = new float[length + 1, height + 1, width + 1];
        textureID = _textureID;
        GenerateQuadTree();
        if (CheckCapsulesInChunk() == false)
        {
            return;
        }
        CheckPointsInCapsules();
        PopulateTerrainMapQuad();

        chunkObject = new GameObject();
        chunkObject.transform.position = chunkPosition;
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        SetMaterialProperties(material, botTexId, topTexID);
        GenerateMeshData();
        chunkObject.name = string.Format("Chunk {0} / {1} / {2} / {3} / {4} / {5}", chunkPosition.x, chunkPosition.y, chunkPosition.z, textureID, botTexId, topTexID);
    }

    /// <summary>
    /// Goes through every cube in the chunk and generates the mesh
    /// </summary>
    private void GenerateMeshData()
    {
        ClearMeshData();

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    MarchCube(new Vector3Int(x, y, z));
                }
            }
        }

        BuildMesh();
    }

    /// <summary>
    /// adds a value to each corner for marching cubes. The value is the distance to the closest center line of a capsule
    /// </summary>
    private void PopulateTerrainMapQuad()
    {
        for (int x = 0; x < wallDistance.GetLength(0); x++)
        {
            for (int y = 0; y < wallDistance.GetLength(1); y++)
            {
                for (int z = 0; z < wallDistance.GetLength(2); z++)
                {
                    wallDistance[x, y, z] = CaveData.terrainSurface + 0.5f * CaveData.terrainSurface;
                    Vector3 key = Vector3.right * (chunkPosition.x + x) + Vector3.up * (chunkPosition.y + y) + Vector3.forward * (chunkPosition.z + z);
                    if (pointsInCapsules.ContainsKey(key) == true)
                    {
                        wallDistance[x, y, z] = pointsInCapsules[key].GetClosestCapsuleDistance();
                        wallDistance[x, y, z] += (L_StructureData.PerlinNoise3D(key * CaveData.bumpiness) - 0.5f) * CaveData.bumpinessAmp;
                    }
                }
            }
        }
    }

    /// <summary>
    /// depending on the material the correct textures get passed to the shader
    /// </summary>
    /// <param name="material"></param>
    /// <param name="botTexID"></param>
    /// <param name="topTexID"></param>
    private void SetMaterialProperties(Material material, float botTexID, float topTexID)
    {
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshRenderer.material.SetTexture("_MainTex", CaveData.textures[textureID]);
        meshRenderer.material.SetTexture("_MainTex2", CaveData.textures[textureID + 1]);

        if (CaveData.useNormalMaps == true)
        {
            meshRenderer.material.SetTexture("_BumpMap", CaveData.normalMaps[textureID]);
            meshRenderer.material.SetTexture("_BumpMap2", CaveData.normalMaps[textureID + 1]);
        }
        else
        {
            meshRenderer.material.SetFloat("_TexScale", CaveData.texScale);
        }
        meshRenderer.material.SetFloat("_BotPercentage", botTexID - textureID);
        meshRenderer.material.SetFloat("_TopPercentage", topTexID - textureID);
        meshRenderer.material.SetFloat("_BotChunkPosY", chunkPosition.y);
        meshRenderer.material.SetFloat("_TopChunkPosY", chunkPosition.y + height);
    }

    /// <summary>
    /// generates a quadre of the size of the chunk and inserts all corners for marching cubes
    /// </summary>
    private void GenerateQuadTree()
    {
        Cube cube = new Cube(chunkPosition, length, height, width);
        qt = new QuadTree(cube, 8);
        for (int x = 0; x <= length; x++)
        {
            for (int y = 0; y <= height; y++)
            {
                for (int z = 0; z <= width; z++)
                {
                    qt.Insert(Vector3.right * (chunkPosition.x + x) + Vector3.up * (chunkPosition.y + y) + Vector3.forward * (chunkPosition.z + z));
                }
            }
        }
    }

    /// <summary>
    /// checks which capsules are inside of this chunk
    /// </summary>
    /// <returns>returns false if 0 capsules are in this chunk</returns>
    private bool CheckCapsulesInChunk()
    {
        for (int i = 0; i < CaveData.capsules.Count; i++)
        {
            if (qt.boundary.IntersectsCapsule(CaveData.capsules[i]))
            {
                capsulesInChunk.Add(CaveData.capsules[i]);
            }
        }

        if (capsulesInChunk.Count == 0)
        {
            hasCapsulesInChunk = false;
            return false;
        }
        hasCapsulesInChunk = true;
        return true;
    }

    /// <summary>
    /// checks which point is inside of which capsule
    /// </summary>
    private void CheckPointsInCapsules()
    {
        for (int i = 0; i < capsulesInChunk.Count; i++)
        {
            Point[] pointsInCapsule = qt.QueryCapsule(capsulesInChunk[i]);
            for (int j = 0; j < pointsInCapsule.Length; j++)
            {
                if (pointsInCapsules.ContainsKey(pointsInCapsule[j].position) == false)
                {
                    pointsInCapsules.Add(pointsInCapsule[j].position, pointsInCapsule[j]);
                }
                else
                {
                    pointsInCapsules[pointsInCapsule[j].position].insideCapsules.Add(capsulesInChunk[i]);
                }
            }
        }
    }

    /// <summary>
    /// (DEPRICATED method without quadtrees) Calculates the walldistance for every cube corner in the chunk
    /// </summary>
    private void PopulateTerrainMap()
    {
        // Calculates the walldistance for every middlepoint in a 3x3x3 radius and fills an array with all lines in range of the diagonal of a chunk
        for (int x = 1; x < length; x += 2)
        {
            for (int y = 1; y < height; y += 2)
            {
                for (int z = 1; z < width; z += 2)
                {
                    wallDistance[x, y, z] = CaveData.GetTerrainHeight(new Vector3(x + chunkPosition.x, y + chunkPosition.y, z + chunkPosition.z), true);
                    PopulateSurroundingPoints(x, y, z);
                }
            }
        }
    }

    /// <summary>
    /// Calculates the walldistance for all surrounding points of a given point with a the preselected lines
    /// </summary>
    /// <param name="_x">position x of middle point</param>
    /// <param name="_y">position y of middle point</param>
    /// <param name="_z">position z of middle point</param>
    private void PopulateSurroundingPoints(int _x, int _y, int _z)
    {
        for (int x = _x - 1; x < _x + 2; x++)
        {
            for (int y = _y - 1; y < _y + 2; y++)
            {
                for (int z = _z - 1; z < _z + 2; z++)
                {
                    // point is allready populated
                    if (wallDistance[x, y, z] != 0)
                    {
                        continue;
                    }
                    // middle point is allready populated
                    if (x == _x && y == _y && z == _z)
                    {
                        continue;
                    }

                    wallDistance[x, y, z] = CaveData.GetTerrainHeight(new Vector3(x + chunkPosition.x, y + chunkPosition.y, z + chunkPosition.z), false);
                }
            }
        }
    }

    /// <summary>
    /// Fills a single cube with the correct polygon
    /// </summary>
    /// <param name="position">position of the cube</param>
    private void MarchCube(Vector3Int position)
    {
        // Gets the walldistance for every corner of the cube
        float[] cube = new float[8];
        for (int i = 0; i < 8; i++)
        {
            cube[i] = wallDistance[position.x + CaveData.CornerTable[i].x,
                                   position.y + CaveData.CornerTable[i].y,
                                   position.z + CaveData.CornerTable[i].z];
        }

        // Get right polygon number for the triangle table
        int configIndex = GetCubeConfiguration(cube);

        // Cube is completly inside or outside of the terrain
        if (configIndex == 0 || configIndex == 255)
        {
            return;
        }

        // Loop through all triangles in the cube the biggest polygon consists of 5 triangles
        int edgeIndex = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                // Get the current indice
                int indice = CaveData.TriangleTable[configIndex, edgeIndex];

                // -1 in the triangle table means there are no more triangles
                if (indice == -1)
                {
                    return;
                }

                // Get the vertices at the start and end of this edge
                Vector3 vert1 = position + CaveData.CornerTable[CaveData.EdgeIndexes[indice, 0]];
                Vector3 vert2 = position + CaveData.CornerTable[CaveData.EdgeIndexes[indice, 1]];

                Vector3 vertPosition;

                // Get the wall distance at both ends of the current edge
                float vert1Sample = cube[CaveData.EdgeIndexes[indice, 0]];
                float vert2Sample = cube[CaveData.EdgeIndexes[indice, 1]];

                float difference = vert2Sample - vert1Sample;

                // If the difference is 0 then the terrain passes through the middle this stops dividing by 0
                if (difference == 0)
                {
                    difference = terrainSurface;
                }
                else
                {
                    difference = (terrainSurface - vert1Sample) / difference;
                }

                // Calculates the point along the edge the terrain passes through using interpolation
                vertPosition = vert1 + ((vert2 - vert1) * difference);

                triangles.Add(VertForIndice(vertPosition));

                edgeIndex++;
            }
        }
    }

    /// <summary>
    /// Calculates the right polygon number to get from the triangle table
    /// </summary>
    /// <param name="cube">walldistances at every corner</param>
    /// <returns></returns>
    private int GetCubeConfiguration(float[] cube)
    {
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            // if the walldistance at the corner is less than the terrainsurface a 1 gets added to the bit at the i. position from the right
            //  00011101 -> corners 0,2,3,4 are less than the surface value = 29
            if (cube[i] < terrainSurface)
            {
                configurationIndex |= 1 << i;
            }
        }
        return configurationIndex;
    }

    /// <summary>
    /// adds or removes terrain at the given point
    /// </summary>
    /// <param name="point">point arround which terrain will be added/removed</param>
    /// <param name="add">add or remove</param>
    public void BrushTerrain(Vector3 point, bool add, int brushSize, float brushStreangth)
    {
        Vector3Int posInt;

        // estimates the correct point by rounding
        posInt = new Vector3Int((int)point.x, (int)point.y, (int)point.z);

        posInt -= chunkPosition;

        for (int x = posInt.x - brushSize; x < posInt.x + brushSize + 1; x++)
        {
            // edges of the chunk wont get changed to stop holes in the mesh
            if (x <= 0 || x >= length)
            {
                continue;
            }

            for (int y = posInt.y - brushSize; y < posInt.y + brushSize + 1; y++)
            {
                if (y <= 0 || y >= height)
                {
                    continue;
                }

                for (int z = posInt.z - brushSize; z < posInt.z + brushSize + 1; z++)
                {
                    if (z <= 0 || z >= width)
                    {
                        continue;
                    }

                    // changes the walldistance to add / remove terrain
                    if (add == true)
                    {
                        wallDistance[x, y, z] += brushStreangth;
                    }
                    else
                    {
                        wallDistance[x, y, z] -= brushStreangth;
                    }
                }
            }
        }

        GenerateMeshData();
    }

    /// <summary>
    /// Adds vertex to vertices if it doesnt allready contain it and returns index
    /// </summary>
    /// <param name="vert">vertex that gets added if it is not allready in the list</param>
    /// <returns>index of the given indice in the list</returns>
    private int VertForIndice(Vector3 vert)
    {
        if (vertices.Contains(vert))
        {
            return vertices.IndexOf(vert);
        }

        // If no match was found this vertex is added to the list
        vertices.Add(vert);
        return vertices.Count - 1;
    }

    /// <summary>
    /// Clears the mesh data
    /// </summary>
    private void ClearMeshData()
    {
        vertices.Clear();
        triangles.Clear();
    }

    /// <summary>
    /// Builds the mesh from the calculated data
    /// </summary>
    private void BuildMesh()
    {
        // if the mesh is not empty the chunk gets the Terrain tag
        if (vertices.Count != 0)
        {
            chunkObject.tag = "Terrain";
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}