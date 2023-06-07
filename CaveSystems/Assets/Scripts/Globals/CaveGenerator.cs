using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    [Header("Cave Settings")]
    [Tooltip("If cave will be generated")]
    [SerializeField] private bool generateCave = false;

    [Tooltip("L_System that will be used")]
    [SerializeField] private L_System system = null;

    //[Tooltip("Size of a single chunk the cave is made out of (only even numbers)")]
    //[SerializeField] private Vector3Int chunkSize = Vector3Int.zero;

    [Tooltip("Radius of the tunnels in the cave")]
    [SerializeField] private float tunnelRadius = 0.0f;

    [Tooltip("Weight of the tunnel radius at the top of the Cave")]
    [SerializeField] private Vector3 topWeight = Vector3.zero;

    [Tooltip("Weight of the tunnel radius at the bottom of the Cave")]
    [SerializeField] private Vector3 bottomWeight = Vector3.zero;

    [Tooltip("If the cave will have stalactites and stalagmites")]
    [SerializeField] private bool addDetails = false;

    [Tooltip("Frequency of the stalctites and stalagmites, bigger values lead to less placed")]
    [SerializeField] private int frequency = 1;

    [Tooltip("Minimum multiplier of stalctite size")]
    [SerializeField] private float minStalctiteScale = 0.0f;

    [Tooltip("Maximum multiplier of stalctite size")]
    [SerializeField] private float maxStalctiteScale = 0.0f;

    [Tooltip("Minimum multiplier of stalagmite size")]
    [SerializeField] private float minStalagmiteScale = 0.0f;

    [Tooltip("Maximum multiplier of stalagmite size")]
    [SerializeField] private float maxStalagmiteScale = 0.0f;

    [Tooltip("Bumpines of the walls higher value leads to more bumps")]
    [Range(0.0f, 0.999f)]
    [SerializeField] private float wallBumpiness = 0.187f;

    [Tooltip("Amplitude of the bumps of the wall")]
    [Range(0.0f, 100.0f)]
    [SerializeField] private float wallBumpinessAmp = 0;

    [Tooltip("Material of the cave")]
    [SerializeField] private Material material = null;

    [Tooltip("Textures of the meshes, 0 is deepest in the cave")]
    [SerializeField] private Texture2D[] textures = new Texture2D[0];

    [Tooltip("Normal maps of the textures")]
    [SerializeField] private Texture2D[] normalMaps = new Texture2D[0];

    [Tooltip("Stalctites that will be placed")]
    [SerializeField] private GameObject[] stalactites = null;

    [Tooltip("Stalagmites that will be placed")]
    [SerializeField] private GameObject[] stalagmites = null;

    // Dictionary linking all chunks with their position
    private Dictionary<Vector3Int, MarchingCubes> chunks = new Dictionary<Vector3Int, MarchingCubes>();

    /// <summary>
    /// checks for input to generate cave
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            system.GenerateLSystem();
            CaveData.system = system;

            if (generateCave == true)
            {
                Generate();

                if (addDetails == true)
                {
                    AddDetails();
                }
            }
            Debug.Log("CaveSize: " + CaveData.caveSizeX + ", " + CaveData.caveSizeY + ", " + CaveData.caveSizeZ);
        }
    }

    /// <summary>
    /// Generates Cave
    /// </summary>
    private void Generate()
    {
        float startTime = Time.realtimeSinceStartup;

        ClearCaveData();
        CaveData.terrainSurface = tunnelRadius;
        float topWeightSum = topWeight.x + topWeight.y + topWeight.z;
        if (topWeightSum == 0)
        {
            topWeightSum = 1;
        }
        float bottomWeightSum = bottomWeight.x + bottomWeight.y + bottomWeight.z;
        if (bottomWeightSum == 0)
        {
            bottomWeightSum = 1;
        }
        topWeight /= topWeightSum;
        bottomWeight /= bottomWeightSum;
        CaveData.topWeight = topWeight;
        CaveData.bottomWeight = bottomWeight;
        float capsuleRadius = CaveData.terrainSurface + Mathf.Max(Mathf.Max(topWeight.x, topWeight.y, topWeight.z) / Mathf.Min(topWeight.x, topWeight.y, topWeight.z), Mathf.Max(bottomWeight.x, bottomWeight.y, bottomWeight.z) / Mathf.Min(bottomWeight.x, bottomWeight.y, bottomWeight.z));
        int chunkSize = (int)capsuleRadius;
        Debug.Log(chunkSize);
        //if (chunksize % 2 != 0)
        //{
        //    chunksize++;
        //}
        CaveData.chunkSizeX = chunkSize;
        CaveData.chunkSizeY = chunkSize;
        CaveData.chunkSizeZ = chunkSize;
        CaveData.bumpiness = wallBumpiness;
        CaveData.bumpinessAmp = wallBumpinessAmp;
        CaveData.diagonal = CaveData.chunkSizeX * CaveData.chunkSizeX + CaveData.chunkSizeY * CaveData.chunkSizeY + CaveData.chunkSizeZ * CaveData.chunkSizeZ;
        CaveData.textures = textures;
        CaveData.normalMaps = normalMaps;
        SetCaveSize();

        for (int i = 0; i < L_StructureData.positions.Count; i++)
        {
            for (int j = 1; j < L_StructureData.positions[i].Count; j++)
            {
                CaveData.capsules.Add(new Capsule(capsuleRadius, L_StructureData.positions[i][j], L_StructureData.positions[i][j - 1]));
            }
            //CaveData.capsules.Add(new Capsule(CaveData.terrainSurface, L_StructureData.positions[i][0], L_StructureData.positions[i - 1][0]));
        }

        // The length of one depthlevel by which the texture is chosen
        CaveData.top = system.up;
        CaveData.bottom = system.down;
        CaveData.textureHeight = (system.up.y - system.down.y + 2 * CaveData.chunkSizeY) / CaveData.textures.Length;

        transform.position = new Vector3(system.left.x - CaveData.chunkSizeX,
                                         system.down.y - CaveData.chunkSizeY,
                                         system.front.z - CaveData.chunkSizeZ);

        for (int x = 0; x < CaveData.caveSizeX; x++)
        {
            for (int y = 0; y < CaveData.caveSizeY; y++)
            {
                for (int z = 0; z < CaveData.caveSizeZ; z++)
                {
                    // position the next chunk will be spawned at
                    Vector3Int chunkPos = new Vector3Int(x * CaveData.chunkSizeX + (int)transform.position.x,
                                                         y * CaveData.chunkSizeY + (int)transform.position.y,
                                                         z * CaveData.chunkSizeZ + (int)transform.position.z);
                    // if chunk is too far from any line it gets skipped
                    if (L_StructureData.GetClosestDistance(chunkPos, true) > CaveData.diagonal + CaveData.terrainSurface)
                    {
                        continue;
                    }
                    int textureID = Mathf.FloorToInt((chunkPos.y + CaveData.chunkSizeY / 2 - CaveData.bottom.y) / CaveData.textureHeight);
                    textureID = Mathf.Clamp(textureID, 0, CaveData.textures.Length - 2);
                    float botTexID = Mathf.Clamp((chunkPos.y - CaveData.bottom.y) / CaveData.textureHeight, textureID, textureID + 1);
                    float topTexID = Mathf.Clamp((chunkPos.y + CaveData.chunkSizeY - CaveData.bottom.y) / CaveData.textureHeight, textureID, textureID + 1);
                    float texDiff = topTexID - botTexID;
                    if (topTexID + 0.5 * texDiff > textureID + 1)
                    {
                        topTexID = textureID + 1;
                    }
                    if (botTexID - 0.5 * texDiff < textureID)
                    {
                        botTexID = textureID;
                    }

                    MarchingCubes chunk = new MarchingCubes(chunkPos, material, textureID, botTexID, topTexID);

                    // if no mesh was generated in the chunk it gets deleted
                    if (chunk.hasCapsulesInChunk == false)
                    {
                        continue;
                    }
                    if (chunk.chunkObject.tag != "Terrain")
                    {
                        Destroy(chunk.chunkObject);
                        continue;
                    }
                    chunks.Add(chunkPos, chunk);
                    chunks[chunkPos].chunkObject.transform.SetParent(transform);
                }
            }
        }

        Debug.Log("Time it took to generate cave: " + (Time.realtimeSinceStartup - startTime) + " seconds");
    }

    /// <summary>
    /// Places Stalactites and Stalagmites depending on frequency chosen
    /// </summary>
    private void AddDetails()
    {
        for (int i = 0; i < L_StructureData.positions.Count; i++)
        {
            for (int j = 0; j < L_StructureData.positions[i].Count; j++)
            {
                // semi random placement of objects
                if ((i + j) % frequency != 0)
                {
                    continue;
                }

                Ray rayUp = new Ray(L_StructureData.positions[i][j], Vector3.up);
                Ray rayDown = new Ray(L_StructureData.positions[i][j], Vector3.down);

                RaycastHit hitUp;
                RaycastHit hitDown;

                Physics.Raycast(rayUp, out hitUp);
                if (hitUp.transform != null)
                {
                    if (hitUp.transform.tag == "Terrain")
                    {
                        GameObject stalactite = Instantiate(stalactites[Random.Range(0, stalactites.Length)], hitUp.point, Quaternion.identity);
                        // randomization of stalactite size
                        stalactite.transform.localScale *= Random.Range(minStalctiteScale, maxStalctiteScale);
                        // moves the stalactite a bit more in to the terrain
                        stalactite.transform.Translate(stalactite.transform.localScale.y / 3 * Vector3.down, Space.World);
                        // sets texture dependent on depth
                        stalactite.GetComponent<MeshRenderer>().material.mainTexture = hitUp.transform.GetComponent<MeshRenderer>().material.mainTexture;
                        stalactite.transform.parent = hitUp.transform;
                    }
                }

                Physics.Raycast(rayDown, out hitDown);
                if (hitDown.transform != null)
                {
                    if (hitDown.transform.tag == "Terrain")
                    {
                        GameObject stalagmite = Instantiate(stalagmites[Random.Range(0, stalagmites.Length)], hitDown.point, Quaternion.identity);
                        // randomization of stalagmite size
                        stalagmite.transform.localScale *= Random.Range(minStalagmiteScale, maxStalagmiteScale);
                        // moves the stalagmite a bit more in to the terrain
                        stalagmite.transform.Translate(stalagmite.transform.localScale.y / 3 * Vector3.up, Space.World);
                        // sets texture dependent on depth
                        stalagmite.GetComponent<MeshRenderer>().material.mainTexture = hitDown.transform.GetComponent<MeshRenderer>().material.mainTexture;
                        stalagmite.transform.parent = hitDown.transform;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculates the rough size of the cave
    /// </summary>
    private void SetCaveSize()
    {
        CaveData.caveSizeX = (int)(((system.right.x - system.left.x) / CaveData.chunkSizeX) + CaveData.chunkSizeX) + 1;
        CaveData.caveSizeY = (int)(((system.up.y - system.down.y) / CaveData.chunkSizeY) + CaveData.chunkSizeY) + 1;
        CaveData.caveSizeZ = (int)(((system.back.z - system.front.z) / CaveData.chunkSizeZ) + CaveData.chunkSizeZ) + 1;
    }

    /// <summary>
    /// clears all data
    /// </summary>
    private void ClearCaveData()
    {
        foreach (var chunk in chunks)
        {
            Destroy(chunk.Value.chunkObject);
        }
        chunks.Clear();
        CaveData.caveSizeX = 0;
        CaveData.caveSizeY = 0;
        CaveData.caveSizeZ = 0;
    }

    /// <summary>
    /// returns chunk from position
    /// </summary>
    /// <param name="pos">position chunk is located at</param>
    /// <returns>chunk</returns>
    public MarchingCubes GetChunkFromVector3(Vector3 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;
        Vector3Int posInt = new Vector3Int(x, y, z);

        // if there is no chunk at the given position one is generated
        if (chunks.ContainsKey(posInt) == false)
        {
            int textureID = Mathf.FloorToInt((posInt.y + CaveData.chunkSizeY / 2 - CaveData.bottom.y) / CaveData.textureHeight);
            textureID = Mathf.Clamp(textureID, 0, CaveData.textures.Length - 2);
            float botTexID = (posInt.y - CaveData.bottom.y) / CaveData.textureHeight;
            float topTexID = (posInt.y + CaveData.chunkSizeY - CaveData.bottom.y) / CaveData.textureHeight;

            MarchingCubes chunk = new MarchingCubes(posInt, material, textureID, botTexID, topTexID);
            chunks.Add(posInt, chunk);
            chunks[posInt].chunkObject.transform.SetParent(transform);
        }

        return chunks[posInt];
    }
}