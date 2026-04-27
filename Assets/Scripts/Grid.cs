using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;
using RangeAttribute = UnityEngine.RangeAttribute;

public class Grid : MonoBehaviour
{
    public Vector2 gridWorldSize; //size of the grid in unity world space

    public GameObject tilePrefab;
    public GameObject unwalkabletilePrefab;
    public GameObject startPrefab;
    public GameObject endPrefab;
    public GameObject pathMarkerPrefab;
    public GameObject slowPrefab;
    public GameObject visitedMarkerPrefab;

    [SerializeField] float tileHeight;
    public float nodeRadius; //radius of a node in the grid

    [SerializeField] bool drawGizmos; //for drawing the grid
    [SerializeField] bool randomWallsOnStart;
    [SerializeField] bool generateLevelOnStart;
    [SerializeField] public bool showfrontier;
    public float nodeDiameter; //radius * 2

    public int gridSizeX; //size of the grid in grid space x dimension
    public int gridSizeY; //size of the grid in grid space y dimension

    public int slowcost;
    //public int normalcost;
    public Node[,] grid; //node array for the grid
    //Tile[,] tileGrid;

    public Node startNode;
    public Node endNode;

    public List<GameObject> TileList;
    public List<Node> path;
    public List<Node> frontierList;
    public List<GameObject> frontierObjects;
    public List<GameObject> pathObjects;

    [Range(0, 1)]
    [SerializeField] float wallchanceonstart;
    [Range(0, 1)]
    [SerializeField] float slowchanceonstart;

    Vector3 bottomLeft; //bottom left of the grid

    private void Awake()
    {
        slowcost *= 10;
        //normalcost *= 10;
        TileList = new List<GameObject>();
        frontierList = new List<Node>();
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        InitGrid();
        if (randomWallsOnStart)
        {
            RandomWalls();
        }
        
        InitTileGrid();
        if (generateLevelOnStart)
        {
            GenerateNewLevel(wallchanceonstart, slowchanceonstart);//added
        }

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
                grid[x, y] = new Node(initPoint, true, x, y, 0); //for now all are walkable update in the future
            }
        }
        sw.Stop();
        Debug.Log("Time to InitGrid(): " + sw.ElapsedMilliseconds + "ms");
    }

    public void NewNodeGrid()
    {
        startNode = null;
        endNode = null;
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        bottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
        grid = null;
        grid = new Node[gridSizeX, gridSizeY];
       
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 initPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                grid[x, y] = new Node(initPoint, true, x, y, 0); //for now all are walkable update in the future
            }
        }
        //UpdateTiles();
        PopulateNeighboursAll();//order shouldnt matter but if bugs could check
        PopulateNeighboursCross();
        UpdateTiles();
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
                    grid[x,y].prefab = tile;
                    //for now all are walkable update in the future
                }
                else if (!grid[x,y].walkable)
                {
                    unwalkabletilePrefab.name = "Tile " + x + "," + y;
                    unwalkabletilePrefab.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
                    var tile = Instantiate(unwalkabletilePrefab, spawnPos, Quaternion.identity);
                    TileList.Add(tile);
                    grid[x, y].prefab = tile;
                }
            }
        }
        sw.Stop();
        Debug.Log("Time to InitTileGrid(): " + sw.ElapsedMilliseconds + "ms");
    }

    public void UpdateTiles() //TODO MAKE THIS BETTER FOR VISUALISING THE FRONTIER AND PATH
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        if (TileList != null)
        {
            ResetTiles();
        }

        foreach (GameObject g in pathObjects)
        {
            Destroy(g);
        }

        foreach (GameObject g in frontierObjects)
        {
            Destroy(g);
        }

        if (pathObjects != null)
        {
            pathObjects.Clear();
        }

        if (frontierObjects != null)
        {
            frontierObjects.Clear();
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
                /*Vector3 spawnPosMarker = new Vector3(grid[x, y].worldPos.x, grid[x, y].worldPos.y + tileHeight, grid[x, y].worldPos.z);
                if (path != null && path.Contains(grid[x, y]))
                {
                    pathMarkerPrefab.name = "Tile " + x + "," + y;
                    pathMarkerPrefab.transform.localScale = new Vector3(nodeDiameter * 0.5f, tileHeight, nodeDiameter * 0.5f);
                    var tile = Instantiate(pathMarkerPrefab, spawnPosMarker, Quaternion.identity);
                    pathObjects.Add(tile);
                    //grid[x, y].prefab = tile;
                }
                if (showfrontier && frontierList != null && frontierList.Contains(grid[x, y]))
                {
                    visitedMarkerPrefab.name = "Tile " + x + "," + y;
                    visitedMarkerPrefab.transform.localScale = new Vector3(nodeDiameter * 0.5f, tileHeight, nodeDiameter * 0.5f);
                    var tile = Instantiate(visitedMarkerPrefab, spawnPosMarker, Quaternion.identity);
                    frontierObjects.Add(tile);
                    //grid[x, y].prefab = tile;
                }*/
                if (grid[x, y] == startNode)
                {
                    startPrefab.name = "Tile " + x + "," + y;
                    startPrefab.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
                    var tile = Instantiate(startPrefab, spawnPos, Quaternion.identity);
                    TileList.Add(tile);
                    grid[x, y].prefab = tile;
                }
                else if (grid[x, y] == endNode)
                {
                    endPrefab.name = "Tile " + x + "," + y;
                    endPrefab.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
                    var tile = Instantiate(endPrefab, spawnPos, Quaternion.identity);
                    TileList.Add(tile);
                    grid[x, y].prefab = tile;
                }
                else if (grid[x, y].walkable && grid[x, y].cost == slowcost)
                {
                    slowPrefab.name = "Tile " + x + "," + y;
                    slowPrefab.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
                    var tile = Instantiate(slowPrefab, spawnPos, Quaternion.identity);
                    TileList.Add(tile);
                    //for now all are walkable update in the future
                    grid[x, y].prefab = tile;//added
                }
                else if (grid[x, y].walkable)
                {
                    tilePrefab.name = "Tile " + x + "," + y;
                    tilePrefab.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
                    var tile = Instantiate(tilePrefab, spawnPos, Quaternion.identity);
                    TileList.Add(tile);
                    //for now all are walkable update in the future
                    grid[x, y].prefab = tile;//added
                }
                else if (!grid[x, y].walkable)
                {
                    unwalkabletilePrefab.name = "Tile " + x + "," + y;
                    unwalkabletilePrefab.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
                    var tile = Instantiate(unwalkabletilePrefab, spawnPos, Quaternion.identity);
                    TileList.Add(tile);
                    grid[x, y].prefab = tile;
                }
              
            }
        }
        sw.Stop();
        //Debug.Log("Time to UpdateTiles(): " + sw.ElapsedMilliseconds + "ms");
    }

    public void ShowPathAndFrontier()
    {
        foreach (GameObject g in pathObjects)
        {
            Destroy(g);
        }

        foreach (GameObject g in frontierObjects)
        {
            Destroy(g);
        }

        if (pathObjects != null)
        {
            pathObjects.Clear();
        }

        if (frontierObjects != null)
        {
            frontierObjects.Clear();
        }

        /*for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                //Vector3 spawnPos = new Vector3(grid[x, y].worldPos.x, grid[x, y].worldPos.y, grid[x, y].worldPos.z);
                Vector3 spawnPosMarker = new Vector3(grid[x, y].worldPos.x, grid[x, y].worldPos.y + tileHeight, grid[x, y].worldPos.z);
                if (path != null && path.Contains(grid[x, y]))
                {
                    pathMarkerPrefab.name = "Tile " + x + "," + y;
                    pathMarkerPrefab.transform.localScale = new Vector3(nodeDiameter * 0.5f, tileHeight, nodeDiameter * 0.5f);
                    var tile = Instantiate(pathMarkerPrefab, spawnPosMarker, Quaternion.identity);
                    pathObjects.Add(tile);
                    //grid[x, y].prefab = tile;
                }
                if (showfrontier && frontierList != null && frontierList.Contains(grid[x, y]))
                {
                    visitedMarkerPrefab.name = "Tile " + x + "," + y;
                    visitedMarkerPrefab.transform.localScale = new Vector3(nodeDiameter * 0.5f, tileHeight, nodeDiameter * 0.5f);
                    var tile = Instantiate(visitedMarkerPrefab, spawnPosMarker, Quaternion.identity);
                    frontierObjects.Add(tile);
                    //grid[x, y].prefab = tile;
                }
            }
        }*/

        
        if (path != null || path.Count == 0)
        {
            foreach (Node n in path)
            {
                Vector3 spawnPosMarker = new Vector3(n.worldPos.x, n.worldPos.y + tileHeight, n.worldPos.z);
                pathMarkerPrefab.name = "Tile " + n.x + "," + n.y;
                pathMarkerPrefab.transform.localScale = new Vector3(nodeDiameter * 0.5f, tileHeight, nodeDiameter * 0.5f);
                var tile = Instantiate(pathMarkerPrefab, spawnPosMarker, Quaternion.identity);
                pathObjects.Add(tile);
            }
        }
        
        if (frontierList != null || frontierList.Count ==0)
        {
            foreach (Node n in frontierList)
            {
                if (path != null && !path.Contains(n))
                {
                    Vector3 spawnPosMarker = new Vector3(n.worldPos.x, n.worldPos.y + tileHeight, n.worldPos.z);
                    visitedMarkerPrefab.name = "Tile " + n.x + "," + n.y;
                    visitedMarkerPrefab.transform.localScale = new Vector3(nodeDiameter * 0.5f, tileHeight, nodeDiameter * 0.5f);
                    var tile = Instantiate(visitedMarkerPrefab, spawnPosMarker, Quaternion.identity);
                    frontierObjects.Add(tile);
                }
                
            }
        }
    }
    /*
     for each node n in path
        place marker
    for each node n in frontier
        place marker
     
     */

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
        //Debug.Log(x + " " + y);

        if (x >= gridSizeX || x < 0)
            return null;
        if (y >= gridSizeY || y < 0)
            return null;
        /*if (grid[x, y] == null)
            return null;*/
        if (grid[x,y] != null)
        {
            return grid[x, y];
        }
        else { return null; }

        

    }

    public void PlaceTile(GameObject newtile, int x, int y)
    {
        Vector3 spawnPos = new Vector3(grid[x, y].worldPos.x, grid[x, y].worldPos.y, grid[x, y].worldPos.z);
        newtile.name = "Tile " + x + "," + y;
        newtile.transform.localScale = new Vector3(nodeDiameter, tileHeight, nodeDiameter);
        var tile = Instantiate(newtile, spawnPos, Quaternion.identity);
        TileList.Add(tile);
        grid[x, y].prefab = tile;
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

    public void PopulateNeighboursDiagExcept()
    {
        //List<Node> filteredlist = new List<Node> ();
        //Stopwatch sw = new Stopwatch();
        //sw.Start();

        foreach (Node cur in grid)
        {
            cur.neighboursDiagSafe.Clear();
            cur.neighboursDiagSafe.AddRange(cur.neighboursAll);
            /*Debug.Log(cur.x + "," + cur.y);
            if (cur.neighboursDiagSafe != null)
            {
                string result = "List contents: ";
                foreach (var item in cur.neighboursDiagSafe)
                {
                    result += item.x.ToString() + "," + item.y.ToString() + " ";
                }
                Debug.Log(result);


            }*/
            if (GetNode(cur.x, cur.y + 1) != null && !GetNode(cur.x, cur.y + 1).walkable)
            {
                if (GetNode(cur.x + 1, cur.y) != null && !GetNode(cur.x + 1, cur.y).walkable)
                {
                    if (cur.neighboursDiagSafe.Contains(GetNode(cur.x + 1, cur.y + 1)))
                    {
                        cur.neighboursDiagSafe.Remove(GetNode(cur.x + 1, cur.y + 1));
                    }
                }
                if (GetNode(cur.x - 1, cur.y) != null && !GetNode(cur.x - 1, cur.y).walkable)
                {
                    if (cur.neighboursDiagSafe.Contains(GetNode(cur.x - 1, cur.y + 1)))
                    {
                        cur.neighboursDiagSafe.Remove(GetNode(cur.x - 1, cur.y + 1));
                    }

                }
            }
            if (GetNode(cur.x, cur.y - 1) != null && !GetNode(cur.x, cur.y - 1).walkable)
            {
                if (GetNode(cur.x + 1, cur.y) != null && ! GetNode(cur.x + 1, cur.y).walkable)
                {
                    if (cur.neighboursDiagSafe.Contains(GetNode(cur.x + 1, cur.y - 1)))
                    {
                        cur.neighboursDiagSafe.Remove(GetNode(cur.x + 1, cur.y - 1));
                    }
                }
                if (GetNode(cur.x - 1, cur.y) != null && !GetNode(cur.x - 1, cur.y).walkable)
                {
                    if (cur.neighboursDiagSafe.Contains(GetNode(cur.x - 1, cur.y - 1)))
                    {
                        cur.neighboursDiagSafe.Remove(GetNode(cur.x - 1, cur.y - 1));
                    }

                }
            }
        }
        //sw.Stop();
        //Debug.Log("Time to PopulateNeighboursDiagExcept(): " + sw.ElapsedMilliseconds + "ms");
    }

    public void PopulateNeighboursCross() //x+1,y x-1,y x,y+1 x,y-1 //for use with unweighted due to diag, only considers N S E W
    {
        //Stopwatch sw = new Stopwatch();
        //sw.Start();

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
        //sw.Stop();
        //Debug.Log("Time to PopulateNeighboursCross(): " + sw.ElapsedMilliseconds + "ms");
    }

    public void RandomWalls()
    {
        //ResetGrid(); //for use in UI
        
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
                n.cost = 0; //normalcost;
            }
        }
    }

    public void ResetGrid()
    {
        foreach (Node n in grid)
        {
            n.walkable = true;
            n.cost = 0;
        }
    }

    public void ResetTiles()
    {
        foreach(Node n in grid)
        {
            n.prefab = null;
        }

        foreach(GameObject go in TileList)
        {
            Destroy(go);
        }
        TileList.Clear();
    }

    public void GenerateNewLevel(float wallchance, float slowchance)
    {
        if (wallchance > 1.0f || slowchance > 1.0f)
        {
            Debug.Log("chance too high");
            return;
        }

        ResetGrid();
        ResetTiles();

        foreach(Node n in grid)
        {
            float rndslow = UnityEngine.Random.Range(0f, 1f);
            if(rndslow < slowchance)
            {
                n.walkable = true;
                n.cost = slowcost;
            }

            float rndwall = UnityEngine.Random.Range(0f, 1f);
            if (rndwall < wallchance)
            {
                n.walkable = false;
                n.cost = 0;
            }
        }
        while (true)
        {
            int rndx = (int)UnityEngine.Random.Range(0f, gridSizeX);
            int rndy = (int)UnityEngine.Random.Range(0f, gridSizeY);
            if (!grid[rndx, rndy].walkable)
            {
                rndx = (int)UnityEngine.Random.Range(0f, gridSizeX);
                rndy = (int)UnityEngine.Random.Range(0f, gridSizeY);
            }
            else
            {
                startNode = grid[rndx, rndy];
                break;
            }

        }
        while (true)
        {
            int rndx = (int)UnityEngine.Random.Range(0f, gridSizeX);
            int rndy = (int)UnityEngine.Random.Range(0f, gridSizeY);
            if (!grid[rndx, rndy].walkable || grid[rndx, rndy] == startNode)
            {
                rndx = (int)UnityEngine.Random.Range(0f, gridSizeX);
                rndy = (int)UnityEngine.Random.Range(0f, gridSizeY);
            }
            else
            {
                endNode = grid[rndx, rndy];
                break;
            }

        }
        //UpdateTiles();
    }

    public void GenerateNewStartandEnd()
    {
        while (true)
        {
            int rndx = (int)UnityEngine.Random.Range(0f, gridSizeX);
            int rndy = (int)UnityEngine.Random.Range(0f, gridSizeY);
            if (!grid[rndx, rndy].walkable)
            {
                rndx = (int)UnityEngine.Random.Range(0f, gridSizeX);
                rndy = (int)UnityEngine.Random.Range(0f, gridSizeY);
            }
            else
            {
                startNode = grid[rndx, rndy];
                break;
            }

        }
        while (true)
        {
            int rndx = (int)UnityEngine.Random.Range(0f, gridSizeX);
            int rndy = (int)UnityEngine.Random.Range(0f, gridSizeY);
            if (!grid[rndx, rndy].walkable || grid[rndx, rndy] == startNode)
            {
                rndx = (int)UnityEngine.Random.Range(0f, gridSizeX);
                rndy = (int)UnityEngine.Random.Range(0f, gridSizeY);
            }
            else
            {
                endNode = grid[rndx, rndy];
                break;
            }

        }
        //Debug.Log("new start " + startNode.x + startNode.y + " new end " + endNode.x + endNode.y);
        //UpdateTiles(); //waste of resource to call where this is used
    }
}
