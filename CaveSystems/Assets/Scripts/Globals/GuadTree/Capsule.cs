﻿using UnityEngine;

public class Capsule
{
    public float radius;
    public float height;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 top;
    public Vector3 bottom;

    /// <summary>
    /// constructs a capsule based on its radius, height, center position and rotation
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="height"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    public Capsule(float radius, float height, Vector3 position, Quaternion rotation)
    {
        this.radius = radius;
        this.height = height;
        this.position = position;
        this.rotation = rotation;
        top = position + (rotation * Vector3.up) * (height / 2 - radius);
        bottom = position + (rotation * Vector3.down) * (height / 2 - radius);
    }

    /// <summary>
    /// Constructs a capsule based on its radius and top and bottom Position
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="CylinderTop"></param>
    /// <param name="CylinderBottom"></param>
    public Capsule(float radius, Vector3 CylinderTop, Vector3 CylinderBottom)
    {
        this.radius = radius;
        height = Vector3.Distance(CylinderTop, CylinderBottom) + 2 * radius;
        top = CylinderTop;
        bottom = CylinderBottom;
        Vector3 line = CylinderTop - CylinderBottom;
        float lineLength = line.magnitude;
        line.Normalize();
        position = CylinderBottom + (lineLength / 2) * line;
        Vector3 up = Vector3.up;
        Vector3.OrthoNormalize(ref line, ref up);
        rotation = Quaternion.LookRotation(up, line);
    }

    /// <summary>
    /// chckes if given point is inside the capsule
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool Contains(Point point)
    {
        // Calculates the length and direction of the Line
        Vector3 line_direction = top - bottom;
        float line_length = line_direction.magnitude;
        line_direction.Normalize();
        // Calculates the distance between the point and the current line and adjust distance based on weight
        float project_length = Mathf.Clamp(Vector3.Dot(point.position - bottom, line_direction), 0f, line_length);
        Vector3 pointOnLine = bottom + line_direction * project_length;

        float distance = (pointOnLine.x - point.position.x) * (pointOnLine.x - point.position.x) * CaveData.topWeight.x +
                         (pointOnLine.y - point.position.y) * (pointOnLine.y - point.position.y) * CaveData.topWeight.y +
                         (pointOnLine.z - point.position.z) * (pointOnLine.z - point.position.z) * CaveData.topWeight.z;
        if (distance <= radius)
        {
            return true;
        }
        return false;
    }
}