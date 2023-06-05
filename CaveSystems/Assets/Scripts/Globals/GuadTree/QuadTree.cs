using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTree
{
    public Cube boundary;
    private int capacity;
    private List<Point> points = new List<Point>(); 
    private QuadTree[] subTrees = new QuadTree[8]; // 0 back north east | 1 back south east | 2 back south west | 3 back north east | 4 front north east | 5 front south east | 6 front south west | 7 front north west
    bool divided = false;

    public QuadTree(Cube cube , int capacity)
    {
        boundary = cube;
        this.capacity = capacity;
    }

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

    public bool Insert(Vector3 pointV)
    {
        Point point = new Point(pointV);
        return Insert(point);
    }

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

    private void Subdevide()
    {
        divided = true;

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
