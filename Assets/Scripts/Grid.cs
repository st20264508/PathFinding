using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Vector2 gridWorldSize; //size of the grid in unity world space

    public float nodeRadius; //radius of a node in the grid

    [SerializeField] bool drawGizmos; //for drawing the grid

    float nodeDiameter; //radius * 2

    int gridSizeX; //size of the grid in grid space x dimension
    int gridSizeY; //size of the grid in grid space y dimension

    Node[,] grid; //node array for the grid

    Vector3 bottomLeft; //bottom left of the grid

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        InitGrid();
        PopulateNeighbours();//order shouldnt matter but if bugs could check
        RandomWalls();
    }

    void InitGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];  

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 initPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                grid[x, y] = new Node(initPoint, true, x, y); //for now all are walkable update in the future
            }
        }

        
    }

    public Node GetGridPosFromWorldPos(Vector3 worldPos)
    {
        float xPercent = ((worldPos.x / gridWorldSize.x) + 0.5f);
        float yPercent = ((worldPos.z / gridWorldSize.y) + 0.5f);

        xPercent = Mathf.Clamp01(xPercent);
        yPercent = Mathf.Clamp01(yPercent);

        int gridX = Mathf.RoundToInt((gridSizeX - 1) * xPercent);
        int gridY = Mathf.RoundToInt((gridSizeY - 1) * yPercent);

        return grid[gridX, gridY];
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y)); //draw grid dimensions
        if (grid != null && drawGizmos)
        {
            foreach (Node n in grid)
            {
                if (n.walkable)
                {
                    Gizmos.color = Color.green;
                }
                /*if (n.displayNeighbours) //doesnt work for now
                {
                    foreach (Node ns in n.neighbours)
                    {
                        Gizmos.color = Color.yellow;
                    }
                }*/
                else
                {
                    Gizmos.color = Color.red;
                }
                Gizmos.DrawCube(n.worldPos, Vector3.one * (nodeDiameter * 0.95f)); 
            }
        }
    }

    public void PopulateNeighbours()
    {
        //List<Node> NeighbourNodes = new List<Node>();

        foreach (Node node in grid)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }

                    int checkX = node.x + x;
                    int checkY = node.y + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        node.neighbours.Add(grid[checkX, checkY]);
                    }
                }
            }

            //node.neighbours = NeighbourNodes;
            //NeighbourNodes.Clear(); 
        }
        
    }

    public void RandomWalls()
    {
        ResetGrid(); //for use in UI
        
        foreach (Node n in grid)
        {
            float randomFloat = UnityEngine.Random.Range(0f, 1f);
            if (randomFloat < 0.3f)
            {
                n.walkable = false;
            }
            else
            {
                n.walkable = true;
            }
        }
    }

    public void ResetGrid()
    {
        foreach (Node n in grid)
        {
            n.walkable = true;
        }
    }
}
