using UnityEngine;

public class Cube
{
    public Vector3 position;
    public float lengthX;
    public float heightY;
    public float widthZ;

    /// <summary>
    /// Constructs a cube based on its position, length, height and width
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="l"></param>
    /// <param name="h"></param>
    /// <param name="w"></param>
    public Cube(Vector3 pos, float l, float h, float w)
    {
        position = pos;
        lengthX = l;
        heightY = h;
        widthZ = w;
    }

    /// <summary>
    /// Constructs a cube based on its position (split into three floats), length, height and width
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="l"></param>
    /// <param name="h"></param>
    /// <param name="w"></param>
    public Cube(float x, float y, float z, float l, float h, float w)
    {
        position.x = x;
        position.y = y;
        position.z = z;
        lengthX = l;
        heightY = h;
        widthZ = w;
    }

    /// <summary>
    /// checks if given point is inside of the cube
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool Contains(Point point)
    {
        if (point.position.x > position.x + lengthX || point.position.x < position.x - lengthX)
        {
            return false;
        }
        if (point.position.y > position.y + heightY || point.position.y < position.y - heightY)
        {
            return false;
        }
        if (point.position.z > position.z + widthZ || point.position.z < position.z - widthZ)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// checks if this cube is intersecting the given cube
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Intersects(Cube other)
    {
        float distanceX = Mathf.Abs(position.x - other.position.x);
        float distanceY = Mathf.Abs(position.y - other.position.y);
        float distanceZ = Mathf.Abs(position.z - other.position.z);

        if (distanceX <= lengthX + other.lengthX && distanceY <= heightY + other.heightY && distanceZ <= widthZ + other.widthZ)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// checks if this cube is intersecting the given capsule
    /// </summary>
    /// <param name="capsule"></param>
    /// <returns></returns>
    public bool IntersectsCapsule(Capsule capsule)
    {
        // Calculates the length and direction of the line inside the capsule
        Vector3 line_direction = capsule.top - capsule.bottom;
        float line_length = line_direction.magnitude;
        line_direction.Normalize();
        // Calculates the closest position on the line to the Cube
        float project_length = Mathf.Clamp(Vector3.Dot(position - capsule.bottom, line_direction), 0f, line_length);
        Vector3 spherepos = capsule.bottom + line_direction * project_length;

        //calculates the closest point to the line on the cube and chekcks if its inside a sphere around the closest point on the line
        float x = Mathf.Max(position.x - lengthX, Mathf.Min(spherepos.x, position.x + lengthX));
        float y = Mathf.Max(position.y - heightY, Mathf.Min(spherepos.y, position.y + heightY));
        float z = Mathf.Max(position.z - widthZ, Mathf.Min(spherepos.z, position.z + widthZ));

        float distance = Vector3.Distance(spherepos, Vector3.right * x + Vector3.up * y + Vector3.forward * z);
        if (distance < capsule.radius)
        {
            return true;
        }
        return false;
    }
}