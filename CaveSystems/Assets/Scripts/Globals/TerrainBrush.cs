using UnityEngine;

public class TerrainBrush : MonoBehaviour
{
    [SerializeField] private Camera cam = null;
    [SerializeField] private CaveGenerator CaveGenerator = null;

    [Tooltip("The area around the hit point that will be changed")]
    [SerializeField] private int brushSize = 0;

    [Tooltip("The rate at which the terrain will be changed")]
    [SerializeField] private float brushStreangth = 0;

    private void Start()
    {
        if (cam == null)
        {
            cam = Camera.current;
        }
    }

    private void Update()
    {
        // moves the camera with wasd or the arrow keys
        transform.position = Vector3.MoveTowards(transform.position, transform.position
                           + (cam.transform.forward * Input.GetAxis("Vertical"))
                           + (transform.right * Input.GetAxis("Horizontal")), Time.deltaTime * 10);

        // rotates the camera with the mous
        cam.transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), 0, 0));
        transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));

        // Adds terrain to the aimed at spot
        if (Input.GetKey(KeyCode.E))
        {
            BrushTerrain(true);
        }

        // Removes terrain from the aimed at spot
        else if (Input.GetKey(KeyCode.Q))
        {
            BrushTerrain(false);
        }
    }

    /// <summary>
    /// Removes / Adds terrain at the aimed at spot if terrain was hit
    /// </summary>
    /// <param name="add"> Add / Remove</param>
    private void BrushTerrain(bool add)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) == false)
        {
            return;
        }
        if (hit.transform.tag != "Terrain")
        {
            return;
        }

        CaveGenerator.GetChunkFromVector3(hit.transform.position).BrushTerrain(hit.point, add, brushSize, brushStreangth);
    }
}