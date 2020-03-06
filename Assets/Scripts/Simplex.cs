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
        points.Remove(points.Find(p => p.Equals(point)));
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
        Edge closestEdge = new Edge
        {
            distance = float.MaxValue
        };

        for (int i = 0; i < this.pointCount; i++)
        {
            // compute the next points index
            int j = i + 1 == this.pointCount ? 0 : i + 1;
            // get the current point and the next one
            var A = points[i];
            var B = points[j];
            // create the edge vector
            var E = B - A; 
                         
            var A0 = -A; 
                         // get the vector from the edge towards the origin
            var n = (A0 * Vector3.Dot(E, E) - E * (Vector3.Dot(A0, E))).normalized;
 
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
