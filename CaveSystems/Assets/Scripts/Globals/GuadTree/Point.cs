using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public Vector3 position;
    private float depthPercentage;
    public List<Capsule> insideCapsules = new List<Capsule>();

    public Point(Vector3 position)
    {
        this.position = position;
        depthPercentage = Mathf.InverseLerp(CaveData.top.y, CaveData.bottom.y, position.y);
    }

    public float GetClosestCapsuleDistance()
    {
        float shortDistance = float.MaxValue;
        for (int i = 0; i < insideCapsules.Count; i++)
        {
            // Calculates the length and direction of the Line
            Vector3 line_direction = insideCapsules[i].top - insideCapsules[i].bottom;
            float line_length = line_direction.magnitude;
            line_direction.Normalize();
            // Calculates the distance between the point and the current line
            float project_length = Mathf.Clamp(Vector3.Dot(position - insideCapsules[i].bottom, line_direction), 0f, line_length);
            Vector3 pointOnLine = insideCapsules[i].bottom + line_direction * project_length;

            float distance = (pointOnLine.x - position.x) * (pointOnLine.x - position.x) * Mathf.Lerp(CaveData.topWeight.x, CaveData.bottomWeight.x, depthPercentage) +
                             (pointOnLine.y - position.y) * (pointOnLine.y - position.y) * Mathf.Lerp(CaveData.topWeight.y, CaveData.bottomWeight.y, depthPercentage) +
                             (pointOnLine.z - position.z) * (pointOnLine.z - position.z) * Mathf.Lerp(CaveData.topWeight.z, CaveData.bottomWeight.z, depthPercentage);
            if (distance < shortDistance)
            {
                shortDistance = distance;
            }
        }
        return shortDistance; 
    }
}
