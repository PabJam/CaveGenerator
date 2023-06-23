using System.Collections.Generic;
using UnityEngine;

public static class L_StructureData
{
    // List of all positions the turtle moved to
    public static List<List<Vector3>> positions = new List<List<Vector3>>();

    // current points close to middle point
    private static List<Vector3> closePoints = new List<Vector3>();

    // current lines close to middle point
    private static List<Vector3> closeLines = new List<Vector3>();

    /// <summary>
    /// Returns the closest distance between given point and either all or a selected ammount of lines in the L_System
    /// </summary>
    /// <param name="point">given point</param>
    /// <param name="middlePoint">check against all points or only preselected ammount</param>
    /// <returns></returns>
    public static float GetClosestDistance(Vector3 point, bool middlePoint)
    {
        if (middlePoint == true)
        {
            return GetTrueShortestDistance(point);
        }
        else
        {
            return GetPreselectedShortestDistance(point);
        }
    }

    /// <summary>
    /// returns distance between given point and all lines in the L_System
    /// </summary>
    /// <param name="point">given point</param>
    /// <returns></returns>
    private static float GetTrueShortestDistance(Vector3 point)
    {
        closePoints.Clear();
        closeLines.Clear();
        // Saves all lines and points if they are close enough to get added to the preselection
        List<Vector3> lineDirections = new List<Vector3>();
        List<Vector3> pointsOnLine = new List<Vector3>();
        // The Related distances
        List<float> distances = new List<float>();
        // The current distance
        float distance = float.MaxValue;
        // The current shortest distance
        float shortestDistance = distance;

        for (int i = 0; i < positions.Count; i++)
        {
            for (int j = 0; j < positions[i].Count - 1; j++)
            {
                // Calculates the length and direction of the Line
                pointsOnLine.Add(positions[i][j]);
                Vector3 line_direction = positions[i][j + 1] - positions[i][j];
                lineDirections.Add(line_direction);
                float line_length = line_direction.magnitude;
                line_direction.Normalize();
                // Calculates the distance between the point and the current line
                float project_length = Mathf.Clamp(Vector3.Dot(point - positions[i][j], line_direction), 0f, line_length);
                Vector3 pointOnLine = positions[i][j] + line_direction * project_length;

                distance = (pointOnLine.x - point.x) * (pointOnLine.x - point.x) * CaveData.topWeight.x +
                           (pointOnLine.y - point.y) * (pointOnLine.y - point.y) * CaveData.topWeight.y +
                           (pointOnLine.z - point.z) * (pointOnLine.z - point.z) * CaveData.topWeight.z;
                distances.Add(distance);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                }
            }
        }

        // Adds close lines and points to preselection
        for (int i = 0; i < distances.Count; i++)
        {
            if (distances[i] < shortestDistance + CaveData.diagonal)
            {
                closePoints.Add(pointsOnLine[i]);
                closeLines.Add(lineDirections[i]);
            }
        }

        return shortestDistance;
    }

    /// <summary>
    /// The same process as GetTrueShortestDistance but using the preselected values
    /// </summary>
    /// <param name="point">Point the distance to gets calculated</param>
    /// <returns></returns>
    private static float GetPreselectedShortestDistance(Vector3 point)
    {
        float distance = float.MaxValue;
        float shortestDistance = distance;

        for (int i = 0; i < closeLines.Count; i++)
        {
            Vector3 line_direction = closeLines[i];
            float line_length = line_direction.magnitude;
            line_direction.Normalize();
            float project_length = Mathf.Clamp(Vector3.Dot(point - closePoints[i], line_direction), 0f, line_length);
            Vector3 pointOnLine = closePoints[i] + line_direction * project_length;

            distance = (pointOnLine.x - point.x) * (pointOnLine.x - point.x) * CaveData.topWeight.x +
                       (pointOnLine.y - point.y) * (pointOnLine.y - point.y) * CaveData.topWeight.y +
                       (pointOnLine.z - point.z) * (pointOnLine.z - point.z) * CaveData.topWeight.z;
            // adds noice to distance
            float noiceDistance = distance * PerlinNoise3D(new Vector3(closeLines[i].x, point.y, pointOnLine.z));
            distance = distance * CaveData.bumpiness + noiceDistance / (CaveData.bumpiness + 1);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
            }
        }

        return shortestDistance;
    }

    /// <summary>
    /// 3D Perlin noise to make the walls more bumpy
    /// </summary>
    /// <param name="position">Position of the point of which the noise gets calculated</param>
    /// <returns></returns>
    public static float PerlinNoise3D(Vector3 position)
    {
        float a, b, c, d, e, f;
        a = Mathf.PerlinNoise(position.x, position.y);
        b = Mathf.PerlinNoise(position.x, position.z);
        c = Mathf.PerlinNoise(position.y, position.x);
        d = Mathf.PerlinNoise(position.z, position.x);
        e = Mathf.PerlinNoise(position.y, position.z);
        f = Mathf.PerlinNoise(position.z, position.y);

        return (a + b + c + d + e + f) / 6;
    }

    public static float PerlinNoise3D(float x, float y, float z)
    {
        float a, b, c, d, e, f;
        a = Mathf.PerlinNoise(x, y);
        b = Mathf.PerlinNoise(x, z);
        c = Mathf.PerlinNoise(y, x);
        d = Mathf.PerlinNoise(z, x);
        e = Mathf.PerlinNoise(y, z);
        f = Mathf.PerlinNoise(z, y);

        return (a + b + c + d + e + f) / 6;
    }
}