using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

public class Grid : MonoBehaviour
{
    public Vector2 gridWorldSize; //size of the grid in unity world space

    public GameObject tilePrefab;
    public GameObject unwalkabletilePrefab;
    public GameObject startPrefab;
    public GameObject endPrefab;
    public GameObject pathPrefab;

    [SerializeField] float tileHeight;
    public float nodeRadius; //radius of a node in the grid

    [SerializeField] bool drawGizmos; //for drawing the grid

    public float nodeDiameter; //radius * 2

    public int gridSizeX; //size of the grid in grid space x dimension
    public int gridSizeY; //size of the grid in grid space y dimension

    Node[,] grid; //node array for the grid
    //Tile[,] tileGrid;

    public Node startNode;
    public Node endNode;

    public List<GameObject> TileList;
    public List<Node> path;

    Vector3 bottomLeft; //bottom left of the grid

    private void Awake()
    {
        TileList = new List<GameObject>();
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        InitGrid();
        RandomWalls();
        InitTileGrid();
        PopulateNeighboursAll();//order shouldnt matter but if bugs could check
        PopulateNeighboursCross();
    }

    /*private void Update()
    {
        if (path != null)
        {
            string result = "List contents: ";
            foreach (var item in path)
            {
                result += item.x.ToString() + "," + item.y.ToString() + " ";
            }
            Debug.Log(result);
        }
       
    }*/

    void InitGrid()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        grid = new Node[gridSizeX, gridSizeY];  

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 initPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                grid[x, y] = new Node(initPoint, true, x, y); //for now all are walkable update in the future
            }
        }
        sw.Stop();
        Debug.Log("Time to InitGrid(): " + sw.ElapsedMilliseconds + "ms");
    }

    void InitTileGrid()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
               // Vector3 spawnPos = new Vector3(grid[x, y].worldPos.x, grid[x, y].worldPos.y - nodeRadius * 0.95f, grid[x, y].worldPos.z);
                Vector3 spawnPos = new Vector3(grid[x, y].worldPos.x, grid[x, y].worldPos.y, grid[x, y].worldPos.z);
                if (grid[x,y].walkable)
                {
                    tilePrefab.name = "Tile " + x + "," + y;
                    tilePrefab.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
                    var tile = Instantiate(tilePrefab, spawnPos, Quaternion.identity);
                    TileList.Add(tile);
                    //for now all are walkable update in the future
                }
                else if (!grid[x,y].walkable)
                {
                    unwalkabletilePrefab.name = "Tile " + x + "," + y;
                    unwalkabletilePrefab.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
                    var tile = Instantiate(unwalkabletilePrefab, spawnPos, Quaternion.identity);
                    TileList.Add(tile);
                }
            }
        }
        sw.Stop();
        Debug.Log("Time to InitTileGrid(): " + sw.ElapsedMilliseconds + "ms");
    }

    public void UpdateTiles() 
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        if (TileList != null)
        {
            ResetTiles();
        }

        /*if (grid != null)
        {
            foreach (Node n in grid)
            {
                if (n == startNode)
                {
                    Instantiate(startPrefab, n.worldPos, Quaternion.identity);
                }
                else if (n == endNode)
                {
                    Instantiate(endPrefab, n.worldPos, Quaternion.identity);
                }
                else if (n.walkable)
                {
                    Instantiate(tilePrefab, n.worldPos, Quaternion.identity);
                }
                else
                {
                    Instantiate(unwalkabletilePrefab, n.worldPos, Quaternion.identity);
                }
                
            }
        }*/

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 spawnPos = new Vector3(grid[x, y].worldPos.x, grid[x, y].worldPos.y, grid[x, y].worldPos.z);
                if (path != null && path.Contains(grid[x, y]))
                {
                    pathPrefab.name = "Tile " + x + "," + y;
                    pathPrefab.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
                    var tile = Instantiate(pathPrefab, spawnPos, Quaternion.identity);
                    TileList.Add(tile);
                }
                else if (grid[x, y] == startNode)
                {
                    startPrefab.name = "Tile " + x + "," + y;
                    startPrefab.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
                    var tile = Instantiate(startPrefab, spawnPos, Quaternion.identity);
                    TileList.Add(tile);
                }
                else if (grid[x, y] == endNode)
                {
                    endPrefab.name = "Tile " + x + "," + y;
                    endPrefab.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
                    var tile = Instantiate(endPrefab, spawnPos, Quaternion.identity);
                    TileList.Add(tile);
                }
                else if (grid[x, y].walkable)
                {
                    tilePrefab.name = "Tile " + x + "," + y;
                    tilePrefab.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
                    var tile = Instantiate(tilePrefab, spawnPos, Quaternion.identity);
                    TileList.Add(tile);
                    //for now all are walkable update in the future
                }
                else if (!grid[x, y].walkable)
                {
                    unwalkabletilePrefab.name = "Tile " + x + "," + y;
                    unwalkabletilePrefab.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
                    var tile = Instantiate(unwalkabletilePrefab, spawnPos, Quaternion.identity);
                    TileList.Add(tile);
                }
              
            }
        }
        sw.Stop();
        Debug.Log("Time to UpdateTiles(): " + sw.ElapsedMilliseconds + "ms");
    }

    public Node GetGridPosFromWorldPos(Vector3 worldPos)
    {
        float xPercent = ((worldPos.x / gridWorldSize.x) + 0.5f);
        float yPercent = ((worldPos.z / gridWorldSize.y) + 0.5f);

        xPercent = Mathf.Clamp01(xPercent);
        yPercent = Mathf.Clamp01(yPercent);

        int gridX = Mathf.FloorToInt((gridSizeX) * xPercent);
        int gridY = Mathf.FloorToInt((gridSizeY) * yPercent);

        return grid[gridX, gridY];
    }

    public Node GetNode(int x, int y)
    {
        return grid[x, y];
    }

    public void PlaceTile(GameObject newtile, int x, int y)
    {
        Vector3 spawnPos = new Vector3(grid[x, y].worldPos.x, grid[x, y].worldPos.y, grid[x, y].worldPos.z);
        newtile.name = "Tile " + x + "," + y;
        newtile.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
        var tile = Instantiate(newtile, spawnPos, Quaternion.identity);
        TileList.Add(tile); 
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 0, gridWorldSize.y)); //draw grid dimensions
        if (grid != null && drawGizmos)
        {
            foreach (Node n in grid)
            {
                if (n == startNode)
                {
                    Gizmos.color = Color.green;
                }
                else if (n == endNode)
                {
                    Gizmos.color = Color.red;
                }
                else if (n.walkable)
                {
                    Gizmos.color = Color.white;
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
                    Gizmos.color = Color.black;
                }
                Vector3 drawAbove = new Vector3(n.worldPos.x, n.worldPos.y -0.5f, n.worldPos.z);
                Vector3 size = new Vector3(nodeDiameter, 0.1f, nodeDiameter);
                Gizmos.DrawCube(drawAbove, size); 
                //Gizmos.DrawWireCube(n.worldPos, Vector3.one * nodeDiameter);
                
            }
        }
    }

    public void PopulateNeighboursAll() //considers all neighbours around not just N S E W
    {
        //List<Node> NeighbourNodes = new List<Node>();
        Stopwatch sw = new Stopwatch();
        sw.Start();

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
                        node.neighboursAll.Add(grid[checkX, checkY]);
                    }
                }
            }

            //node.neighbours = NeighbourNodes;
            //NeighbourNodes.Clear(); 
        }
        sw.Stop();
        Debug.Log("Time to PopulateNeighboursAll(): " + sw.ElapsedMilliseconds + "ms");
    }

    public void PopulateNeighboursCross() //x+1,y x-1,y x,y+1 x,y-1 //for use with unweighted due to diag, only considers N S E W
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        foreach (Node node in grid)
        {
            if (node.x - 1 >= 0)//left
            {
                node.neighboursCross.Add(grid[node.x - 1, node.y]); 
            }
            if (node.x + 1 < gridSizeX)//right
            {
                node.neighboursCross.Add(grid[node.x + 1, node.y]); 
            }
            if (node.y - 1 >= 0) //down
            {
                node.neighboursCross.Add(grid[node.x, node.y - 1]);
            }
            if (node.y + 1 < gridSizeY) //up
            {
                node.neighboursCross.Add(grid[node.x, node.y + 1]);
            }
        }
        sw.Stop();
        Debug.Log("Time to PopulateNeighboursCross(): " + sw.ElapsedMilliseconds + "ms");
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

    public void ResetTiles()
    {
        foreach(GameObject go in TileList)
        {
            Destroy(go);
        }
        TileList.Clear();
    }
}
