using System.Collections.Generic;
using UnityEngine;

public class QuadTree
{
    public Cube boundary; // determined the size of the quadtree
    private int capacity; // determines how many points can be stored in one tree
    private List<Point> points = new List<Point>(); // list of stored points

    // list of all subtrees of the first generation
    private QuadTree[] subTrees = new QuadTree[8]; // 0 back north east | 1 back south east | 2 back south west | 3 back north east | 4 front north east | 5 front south east | 6 front south west | 7 front north west

    private bool divided = false; // if tree has subtrees / if points would be over capacity

    /// <summary>
    /// generates a quadtree based on a cube as a boundary and a given capacity per tree
    /// </summary>
    /// <param name="cube"></param>
    /// <param name="capacity"></param>
    public QuadTree(Cube cube, int capacity)
    {
        boundary = cube;
        this.capacity = capacity;
    }

    /// <summary>
    /// inserts a point into this tree. If the tree is at capacity it generates 8 new trees inside the boundary of this tree and passes the point to the correct subtree
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool Insert(Point point)
    {
        if (boundary.Contains(point) == false)
        {
            return false;
        }

        if (points.Count < capacity)
        {
            points.Add(point);
            return true;
        }
        if (divided == false)
        {
            Subdevide();
        }
        for (int i = 0; i < subTrees.Length; i++)
        {
            bool success = subTrees[i].Insert(point);
            if (success == true)
            {
                return true;
            }
        }
        Debug.LogWarning("QuadTree could not Insert Point");
        return false;
    }

    /// <summary>
    /// turns a Vector3 into a point and inserts it into the tree
    /// </summary>
    /// <param name="pointV"></param>
    /// <returns></returns>
    public bool Insert(Vector3 pointV)
    {
        Point point = new Point(pointV);
        return Insert(point);
    }

    /// <summary>
    /// returns a list of all points, which are inside the quadtree and the given cube
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public Point[] QueryCube(Cube range)
    {
        List<Point> foundPoints = new List<Point>();
        if (boundary.Intersects(range) == false)
        {
            return foundPoints.ToArray();
        }

        for (int i = 0; i < points.Count; i++)
        {
            if (range.Contains(points[i]))
            {
                foundPoints.Add(points[i]);
            }
        }

        if (divided == true)
        {
            for (int i = 0; i < subTrees.Length; i++)
            {
                foundPoints.AddRange(subTrees[i].QueryCube(range));
            }
        }

        return foundPoints.ToArray();
    }

    /// <summary>
    /// returns a list of all points, which are inside the quadtree and the given capsule
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public Point[] QueryCapsule(Capsule range)
    {
        List<Point> foundPoints = new List<Point>();
        if (boundary.IntersectsCapsule(range) == false)
        {
            return foundPoints.ToArray();
        }

        for (int i = 0; i < points.Count; i++)
        {
            if (range.Contains(points[i]))
            {
                points[i].insideCapsules.Add(range);
                foundPoints.Add(points[i]);
            }
        }

        if (divided == true)
        {
            for (int i = 0; i < subTrees.Length; i++)
            {
                foundPoints.AddRange(subTrees[i].QueryCapsule(range));
            }
        }

        return foundPoints.ToArray();
    }

    /// <summary>
    /// subdevide the quadtree into 8 evenly spread out subtrees
    /// </summary>
    private void Subdevide()
    {
        divided = true;
        // back / front / north / east / south / west
        Cube bne = new Cube(boundary.position.x + boundary.lengthX / 2, boundary.position.y + boundary.heightY / 2, boundary.position.z - boundary.widthZ / 2, boundary.lengthX / 2, boundary.heightY / 2, boundary.widthZ / 2);
        Cube bse = new Cube(boundary.position.x + boundary.lengthX / 2, boundary.position.y - boundary.heightY / 2, boundary.position.z - boundary.widthZ / 2, boundary.lengthX / 2, boundary.heightY / 2, boundary.widthZ / 2);
        Cube bsw = new Cube(boundary.position.x - boundary.lengthX / 2, boundary.position.y - boundary.heightY / 2, boundary.position.z - boundary.widthZ / 2, boundary.lengthX / 2, boundary.heightY / 2, boundary.widthZ / 2);
        Cube bnw = new Cube(boundary.position.x - boundary.lengthX / 2, boundary.position.y + boundary.heightY / 2, boundary.position.z - boundary.widthZ / 2, boundary.lengthX / 2, boundary.heightY / 2, boundary.widthZ / 2);
        Cube fne = new Cube(boundary.position.x + boundary.lengthX / 2, boundary.position.y + boundary.heightY / 2, boundary.position.z + boundary.widthZ / 2, boundary.lengthX / 2, boundary.heightY / 2, boundary.widthZ / 2);
        Cube fse = new Cube(boundary.position.x + boundary.lengthX / 2, boundary.position.y - boundary.heightY / 2, boundary.position.z + boundary.widthZ / 2, boundary.lengthX / 2, boundary.heightY / 2, boundary.widthZ / 2);
        Cube fsw = new Cube(boundary.position.x - boundary.lengthX / 2, boundary.position.y - boundary.heightY / 2, boundary.position.z + boundary.widthZ / 2, boundary.lengthX / 2, boundary.heightY / 2, boundary.widthZ / 2);
        Cube fnw = new Cube(boundary.position.x - boundary.lengthX / 2, boundary.position.y + boundary.heightY / 2, boundary.position.z + boundary.widthZ / 2, boundary.lengthX / 2, boundary.heightY / 2, boundary.widthZ / 2);

        for (int i = 0; i < subTrees.Length; i++)
        {
            switch (i)
            {
                case 0:
                    subTrees[i] = new QuadTree(bne, capacity);
                    break;

                case 1:
                    subTrees[i] = new QuadTree(bse, capacity);
                    break;

                case 2:
                    subTrees[i] = new QuadTree(bsw, capacity);
                    break;

                case 3:
                    subTrees[i] = new QuadTree(bnw, capacity);
                    break;

                case 4:
                    subTrees[i] = new QuadTree(fne, capacity);
                    break;

                case 5:
                    subTrees[i] = new QuadTree(fse, capacity);
                    break;

                case 6:
                    subTrees[i] = new QuadTree(fsw, capacity);
                    break;

                case 7:
                    subTrees[i] = new QuadTree(fnw, capacity);
                    break;
            }
        }
    }
}