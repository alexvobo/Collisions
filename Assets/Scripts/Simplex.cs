using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Simplex
{
    public int pointCount = 0;
    public List<Vector3> points = new List<Vector3>();
    public Vector3 d;


    public void Remove(Vector3 point)
    {
        var toRemove = points.Find(p => p.Equals(point));
        points.Remove(toRemove);
        pointCount--;

    }
    public void Add(Vector3 point)
    {
        points.Add(point);
        pointCount++;

    }
    public void Add(Vector3 point, int insertion)
    {
        points.Insert(insertion, point);
        pointCount++;

    }
    public Vector3 GetLast()
    {
        return points[pointCount - 1];
    }
    public void setDirection(Vector3 dir)
    {
        this.d = dir;
    }
    public Edge ClosestEdge()
    {
        Edge closestEdge = new Edge();
        // prime the distance of the edge to the max
        closestEdge.distance = float.MaxValue;
        // s is the passed in simplex

        for (int i = 0; i < this.pointCount; i++)
        {
            // compute the next points index
            int j = i + 1 == this.pointCount ? 0 : i + 1;
            // get the current point and the next one
            var A = points[i];
            var B = points[j];
            // create the edge vector
            var E = B - A; // or a.to(b);
                           // get the vector from the origin to a
            var A0 = -A; // or a - ORIGIN
                         // get the vector from the edge towards the origin
            var n = Vector3.Cross(E, (Vector3.Cross(A0, E)));
            // normalize the vector
            n.Normalize();
            // calculate the distance from the origin to the edge
            float d = Vector3.Dot(n, A); // could use b or a here
                                         // check the distance against the other distances
            if (d < closestEdge.distance)
            {
                // if this edge is closer then use it
                closestEdge.distance = d;
                closestEdge.normal = n;
                closestEdge.index = j;
            }
        }
        // return the closest edge we found
        return closestEdge;

    }
}
