using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Node 
{
    public Vector3 worldPos;

    public bool walkable;
    public bool displayNeighbours;

    public int x;
    public int y;

    public Node parent; //used for parenting nodes to create a path

    public List<Node> neighboursAll; //could be used if neighbours are pre calculated in grid init, could be a hashset? 
    public List<Node> neighboursCross;

    public Node(Vector3 worldPos, bool walkable, int x, int y)
    {
        this.worldPos = worldPos;
        this.walkable = walkable;
        this.x = x;
        this.y = y;
        displayNeighbours = false;
        neighboursAll = new List<Node>();
        neighboursCross = new List<Node>();
    }
    
    public List<Node> GetNeighboursAll()
    {
        if (neighboursAll != null)
        {
            return neighboursAll;
        }
        return null;
    }
     
    public List <Node> GetNeighboursCross()
    {
        if (neighboursCross != null)
        {
            return neighboursCross;
        }
        return null;
    }
}
