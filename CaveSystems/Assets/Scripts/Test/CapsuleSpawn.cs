using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleSpawn : MonoBehaviour
{
    private Vector3[,] lines = new Vector3[10,2];
    [SerializeField] private GameObject cylinderPref;
    [SerializeField] private GameObject spherePref;
    [SerializeField] private GameObject cubePref;
    private List<Capsule> capsules = new List<Capsule>();
    private Cube cube;
    void Start()
    {
        for (int i = 0; i < lines.GetLength(0); i++)
        {
            lines[i, 0] = new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f));
            lines[i, 1] = new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f)) + lines[i, 0];
            Capsule capsule = new Capsule(0.5f, lines[i, 0], lines[i, 1]);
            capsules.Add(capsule);
            GameObject spawnedCapsule = Instantiate(cylinderPref, capsule.position, capsule.rotation);
            GameObject topSphere = Instantiate(spherePref, capsule.top, Quaternion.identity);
            GameObject bottomSphere = Instantiate(spherePref, capsule.bottom, Quaternion.identity);
            spawnedCapsule.transform.localScale = new Vector3(capsule.radius * 2, (capsule.height - 2 * capsule.radius) / 2 , capsule.radius * 2);
            topSphere.transform.localScale = Vector3.one * capsule.radius * 2;
            bottomSphere.transform.localScale = Vector3.one * capsule.radius * 2;
        }
        cubePref = Instantiate(cubePref, Vector3.zero, Quaternion.identity);
        cubePref.GetComponent<MeshRenderer>().material.color = Color.red;
        cube = new Cube(cubePref.transform.position, cubePref.transform.localScale.x / 2, cubePref.transform.localScale.y / 2, cubePref.transform.localScale.z / 2);
    }

    // Update is called once per frame
    void Update()
    {
        cube.position = cubePref.transform.position;
        for (int i = 0; i < lines.GetLength(0); i++)
        {
            Debug.DrawLine(lines[i, 0], lines[i, 1], Color.red);
        }
        for (int i = 0; i < capsules.Count; i++)
        {
            if (cube.IntersectsCapsule(capsules[i]) == true)
            {
                cubePref.GetComponent<MeshRenderer>().material.color = Color.green;
                break;
            }
            if (i == capsules.Count - 1)
            {
                cubePref.GetComponent<MeshRenderer>().material.color = Color.red;
            }
        }
    }
}
