using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class UI : MonoBehaviour
{
    Camera cam;
    public Grid grid;
    public GameObject test;
    public Pathfinding pathfinder;
    int defualtLayer;
    Pathfinding.Results results;

    [SerializeField] bool testDFS;
    [SerializeField] bool testBFS;
    [SerializeField] bool testGBFS;
    [SerializeField] bool testDijkstra;
    [SerializeField] bool testAstar;
    [SerializeField] bool testDijkstraCROSS;
    [SerializeField] bool testAstarCROSS;

    [Range(0,1)]
    [SerializeField] float wallchance;
    [Range(0, 1)]
    [SerializeField] float slowchance;
    [SerializeField] int runs;
    /*public struct Results
    {
        public List<Node> path;
        public int length;
        public int cost;
        public int iterations;
        public long time;
    }*/

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
        defualtLayer = LayerMask.GetMask("Default");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetStartNode();
        }

        if (Input.GetMouseButtonDown(1))
        {
            SetEndNode();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            RunBFSCROSS(); 
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RunDijkstraCROSS();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            RunGreedyBFS();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            RunDijkstraFiltered();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            RunAstarFiltered();
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            RunAstarCross();
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            RunAstarFiltered2();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            RunDFS();
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            RunAstarFilteredTEST();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetUnwalkable();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            SetWalkable();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SetSlow();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            DebugFilter();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            //grid.GenerateNewLevel(0.2f, 0.5f);
            TestAlgorithms(wallchance, slowchance, runs);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DebugNode();
            DebugNeighbours();
        }
    }

    void DebugNode()
    {
        RaycastHit hit;
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(mousePos, out hit))
        {
            Node n = grid.GetGridPosFromWorldPos(hit.point);
            Debug.Log("Node: " + n.x.ToString() + "," + n.y.ToString() + " Node cost: " + n.cost + " Node walkable: " + n.walkable);
        }
    }

    private void DebugFilter()
    {
        RaycastHit hit;
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(mousePos, out hit))
        {
            Node n = grid.GetGridPosFromWorldPos(hit.point);
            FilteredNeighbours(n);
        }
    }

    void DebugNeighbours()
    {
        RaycastHit hit;
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(mousePos, out hit))
        {
            Node n = grid.GetGridPosFromWorldPos(hit.point);
            n.displayNeighbours = true;
            //Debug.Log(n.GetNeighbours());
            Debug.Log(n.x.ToString() + "," + n.y.ToString());
            string result = "List contents: ";
            foreach (var item in n.neighboursAll)
            {
                result += item.x.ToString() + "," + item.y.ToString() + " ";
                //Gizmos.color = Color.yellow;
            }
            Debug.Log(result);
        }
    }

    public void SetWalkable()
    {
        RaycastHit hit;
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mousePos, out hit, 200f, defualtLayer))
        {
            Node hitNode = grid.GetGridPosFromWorldPos(hit.point);
            if (hitNode.walkable && hitNode.cost < grid.slowcost)
            {
                Debug.Log("Already Walkable");
            }
            else
            {
                grid.TileList.Remove(hit.transform.gameObject);
                Destroy(hit.transform.gameObject);
                grid.PlaceTile(grid.tilePrefab, hitNode.x, hitNode.y);
                grid.GetNode(hitNode.x, hitNode.y).walkable = true;
                grid.GetNode(hitNode.x, hitNode.y).cost = 0; //grid.normalcost;
            }
        }
    }

    public void SetUnwalkable()
    {
        RaycastHit hit;
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mousePos, out hit, 200f, defualtLayer))
        {
            Node hitNode = grid.GetGridPosFromWorldPos(hit.point);
            if (!hitNode.walkable)
            {
                Debug.Log("Already Unwalkable");
            }
            else
            {
                grid.TileList.Remove(hit.transform.gameObject);
                Destroy(hit.transform.gameObject);
                grid.PlaceTile(grid.unwalkabletilePrefab, hitNode.x, hitNode.y);
                grid.GetNode(hitNode.x, hitNode.y).walkable = false;
                grid.GetNode(hitNode.x, hitNode.y).cost = 0;//grid.normalcost;
            }
        }
    }

    public void SetSlow()
    {
        RaycastHit hit;
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mousePos, out hit, 200f, defualtLayer))
        {
            Node hitNode = grid.GetGridPosFromWorldPos(hit.point);
            if (hitNode.cost == grid.slowcost)
            {
                Debug.Log("Already Slow");
            }
            else
            {
                grid.TileList.Remove(hit.transform.gameObject);
                Destroy(hit.transform.gameObject);
                grid.PlaceTile(grid.slowPrefab, hitNode.x, hitNode.y);
                grid.GetNode(hitNode.x, hitNode.y).walkable = true;
                grid.GetNode(hitNode.x, hitNode.y).cost = grid.slowcost;
            }
        }
    }

    public void SetStartNode()
    {
        RaycastHit hit;
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mousePos, out hit, 200f, defualtLayer))
        {
            Node hitNode = grid.GetGridPosFromWorldPos(hit.point);
            if (hitNode.walkable && hitNode != grid.startNode && hitNode != grid.endNode)
            {
                int oldX = 0;
                int oldY = 0;
                int oldCost = 0;

                if (grid.startNode != null)
                {
                    oldX = grid.startNode.x;
                    oldY = grid.startNode.y;
                    oldCost = grid.startNode.cost;
                }
                Debug.Log(oldX + " " + oldY);

                grid.TileList.Remove(hit.transform.gameObject);
                Destroy(hit.transform.gameObject);
                grid.PlaceTile(grid.startPrefab, hitNode.x, hitNode.y);
                //grid.GetNode(hitNode.x, hitNode.y).walkable = false;

                grid.startNode = grid.GetGridPosFromWorldPos(hit.point);

                
                grid.TileList.Remove(grid.GetNode(oldX, oldY).prefab);
                Destroy(grid.GetNode(oldX, oldY).prefab); 
                if (oldCost == grid.slowcost)
                {
                    grid.PlaceTile(grid.slowPrefab, oldX, oldY);
                }
                else
                {
                    grid.PlaceTile(grid.tilePrefab, oldX, oldY);
                }
                grid.GetNode(oldX, oldY).walkable = true;

              

                Debug.Log("Start Node: " + grid.startNode.x.ToString() + "," + grid.startNode.y.ToString());
                Debug.DrawRay(mousePos.origin, mousePos.direction * 1000, Color.red, 10);
                //grid.startNode = grid.GetGridPosFromWorldPos(hit.point);
                //grid.UpdateTiles();
                //Debug.Log("Start Node: " + grid.startNode.x.ToString() + "," + grid.startNode.y.ToString());
            }
            else if (!hitNode.walkable)
            {
                Debug.Log("Start node has to be walkable");
                Debug.DrawRay(mousePos.origin, mousePos.direction * 1000, Color.red, 10);
            }
            else if (hitNode == grid.startNode)
            {
                Debug.Log("Already start node");
            }
            else if (hitNode == grid.endNode)
            {
                Debug.Log("Already end node");
            }
            //Destroy(hit.transform.gameObject);
            Debug.DrawRay(mousePos.origin, mousePos.direction * 1000, Color.red, 10);
            //Instantiate(test, hit.point, Quaternion.identity);
        }
    }

    public void SetEndNode()
    {
        RaycastHit hit;
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mousePos, out hit, 200f, defualtLayer))
        {
            Node hitNode = grid.GetGridPosFromWorldPos(hit.point);
            if (hitNode.walkable && hitNode != grid.endNode && hitNode != grid.startNode)
            {
                int oldX = 0;
                int oldY = 0;
                int oldCost = 0;

                if (grid.endNode != null)
                {
                    oldX = grid.endNode.x;
                    oldY = grid.endNode.y;
                    oldCost = grid.endNode.cost;
                }
                Debug.Log(oldX + " " + oldY);

                grid.TileList.Remove(hit.transform.gameObject);
                Destroy(hit.transform.gameObject);
                grid.PlaceTile(grid.endPrefab, hitNode.x, hitNode.y);
                //grid.GetNode(hitNode.x, hitNode.y).walkable = false;

                grid.endNode = grid.GetGridPosFromWorldPos(hit.point);


                grid.TileList.Remove(grid.GetNode(oldX, oldY).prefab);
                Destroy(grid.GetNode(oldX, oldY).prefab);
                if (oldCost == grid.slowcost)
                {
                    grid.PlaceTile(grid.slowPrefab, oldX, oldY);
                }
                else
                {
                    grid.PlaceTile(grid.tilePrefab, oldX, oldY);
                }
                grid.GetNode(oldX, oldY).walkable = true;
                Debug.Log("End Node: " + grid.endNode.x.ToString() + "," + grid.endNode.y.ToString());
            }
            else if (!hitNode.walkable)
            {
                Debug.Log("End node has to be walkable");
            }
             else if (hitNode == grid.startNode)
            {
                Debug.Log("Already start node");
            }
            else if (hitNode == grid.endNode)
            {
                Debug.Log("Already end node");
            }
            //Destroy(hit.transform.gameObject);
        }
    }

    public void RunBFSALL()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            grid.path = pathfinder.BFSAlgorithmALL(grid.startNode, grid.endNode);
            if (grid.path != null)
            {
                string result = "List contents: ";
                foreach (var item in grid.path)
                {
                    result += item.x.ToString() + "," + item.y.ToString() + " ";
                }
                Debug.Log(result);
                Debug.Log("Path length in Nodes: " + grid.path.Count);
                Debug.Log("Path cost: " + CalculatePathCost());
                grid.ShowPathAndFrontier();
            }
        }       
    }

    public void RunBFSCROSS()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            grid.path = pathfinder.BFSAlgorithmCROSS(grid.startNode, grid.endNode);
            if (grid.path != null)
            {
                string result = "List contents: ";
                foreach (var item in grid.path)
                {
                    result += item.x.ToString() + "," + item.y.ToString() + " ";
                }
                Debug.Log(result);
                Debug.Log("Path length in Nodes: " + grid.path.Count);
                Debug.Log("Path cost: " + CalculatePathCost());
                grid.ShowPathAndFrontier();
            }
        }
        
        //grid.path = pathfinder.BFSAlgorithmCROSSTEST(grid.startNode, grid.endNode).path;

    }

    public Pathfinding.Results RunBFSCROSSTEST()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            results = pathfinder.BFSAlgorithmCROSSTEST(grid.startNode, grid.endNode);
            grid.path = results.path;
            if (grid.path != null)
            {
                results.pathLength = results.path.Count;
                results.pathCost = CalculatePathCost();

                grid.ShowPathAndFrontier();
            }
        }
        return results;
        //grid.path = pathfinder.BFSAlgorithmCROSSTEST(grid.startNode, grid.endNode).path;

    }

    public void RunDijkstraCROSS()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            grid.path = pathfinder.DijkstraAlgorithmCROSS(grid.startNode, grid.endNode);
            if (grid.path != null)
            {
                string result = "List contents: ";
                foreach (var item in grid.path)
                {
                    result += item.x.ToString() + "," + item.y.ToString() + " ";
                }
                Debug.Log(result);
                Debug.Log("Path length in Nodes: " + grid.path.Count);
                Debug.Log("Path cost: " + CalculatePathCost());
                grid.ShowPathAndFrontier();
            }
        }

    }

    public Pathfinding.Results RunDijkstraCROSSTEST()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            results = pathfinder.DijkstraAlgorithmCROSSTEST(grid.startNode, grid.endNode);
            grid.path = results.path;
            if (grid.path != null)
            {
                results.pathLength = results.path.Count;
                results.pathCost = CalculatePathCost();
                grid.ShowPathAndFrontier(); //?
            }
        }
        return results;
    }

    public void RunDijkstra()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            grid.path = pathfinder.DijkstraAlgorithm(grid.startNode, grid.endNode);
            if (grid.path != null)
            {
                string result = "List contents: ";
                foreach (var item in grid.path)
                {
                    result += item.x.ToString() + "," + item.y.ToString() + " ";
                }
                Debug.Log(result);
                Debug.Log("Path length in Nodes: " + grid.path.Count);
                Debug.Log("Path cost: " + CalculatePathCost());
                grid.ShowPathAndFrontier();
            }
        }

    }

    public void RunDijkstraFiltered()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            grid.path = pathfinder.DijkstraAlgorithmFiltered(grid.startNode, grid.endNode);
            if (grid.path != null)
            {
                string result = "List contents: ";
                foreach (var item in grid.path)
                {
                    result += item.x.ToString() + "," + item.y.ToString() + " ";
                }
                Debug.Log(result);
                Debug.Log("Path length in Nodes: " + grid.path.Count);
                Debug.Log("Path cost: " + CalculatePathCost());
                grid.ShowPathAndFrontier();
            }
        }

    }

    public Pathfinding.Results RunDijkstraAlgorithmFilteredTEST()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            results = pathfinder.DijkstraAlgorithmFilteredTEST(grid.startNode, grid.endNode);
            grid.path = results.path;
            if (grid.path != null)
            {
                results.pathLength = results.path.Count;
                results.pathCost = CalculatePathCost();
                grid.ShowPathAndFrontier();
            }
        }
        return results;
    }

    public void RunAstarFiltered()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            grid.path = pathfinder.AstarAlgorithmFiltered(grid.startNode, grid.endNode);
            if (grid.path != null)
            {
                string result = "List contents: ";
                foreach (var item in grid.path)
                {
                    result += item.x.ToString() + "," + item.y.ToString() + " ";
                }
                Debug.Log(result);
                Debug.Log("Path length in Nodes: " + grid.path.Count);
                Debug.Log("Path cost: " + CalculatePathCost());
                grid.ShowPathAndFrontier();
            }
        }
    }

    public Pathfinding.Results RunAstarFilteredTEST()
    {
        
        if (grid.startNode != null && grid.endNode != null)
        {
            results = pathfinder.AstarAlgorithmFilteredTEST(grid.startNode, grid.endNode);
            grid.path = results.path;
            if (grid.path != null)
            {
                /*string result = "List contents: ";
                foreach (var item in grid.path)
                {
                    result += item.x.ToString() + "," + item.y.ToString() + " ";
                }
                Debug.Log(result);*/

                results.pathLength = results.path.Count;
                results.pathCost = CalculatePathCost();
                

                //Debug.Log("Path length in Nodes: " + grid.path.Count);
                //Debug.Log("Path cost: " + CalculatePathCost());

                
                grid.ShowPathAndFrontier();
            }
        }
        return results;
    }

    public void RunGreedyBFS()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            grid.path = pathfinder.GreedyBFSAlgorithm(grid.startNode, grid.endNode);
            if (grid.path != null)
            {
                string result = "List contents: ";
                foreach (var item in grid.path)
                {
                    result += item.x.ToString() + "," + item.y.ToString() + " ";
                }
                Debug.Log(result);
                Debug.Log("Path length in Nodes: " + grid.path.Count);
                Debug.Log("Path cost: " + CalculatePathCost());
                grid.ShowPathAndFrontier();
            }
        }
    }

    public Pathfinding.Results RunGreedyBFSTEST()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            results = pathfinder.GreedyBFSAlgorithmTEST(grid.startNode, grid.endNode);
            grid.path = results.path;
            if (grid.path != null)
            {
                results.pathLength = results.path.Count;
                results.pathCost = CalculatePathCost();
                grid.ShowPathAndFrontier();
            }
        }
        return results;
    }

    public void RunAstarCross()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            grid.path = pathfinder.AstarAlgorithmCross(grid.startNode, grid.endNode);
            if (grid.path != null)
            {
                string result = "List contents: ";
                foreach (var item in grid.path)
                {
                    result += item.x.ToString() + "," + item.y.ToString() + " ";
                }
                Debug.Log(result);
                Debug.Log("Path length in Nodes: " + grid.path.Count);
                Debug.Log("Path cost: " + CalculatePathCost());
                grid.ShowPathAndFrontier();
            }
        }
    }

    public Pathfinding.Results RunAstarCrossTEST()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            results = pathfinder.AstarAlgorithmCrossTEST(grid.startNode, grid.endNode);
            grid.path = results.path;
            if (grid.path != null)
            {
                results.pathLength = results.path.Count;
                results.pathCost = CalculatePathCost();
                grid.ShowPathAndFrontier();
            }
        }
        return results;
    }

    public void RunAstarFiltered2()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            grid.path = pathfinder.AstarAlgorithmFiltered2(grid.startNode, grid.endNode);
            if (grid.path != null)
            {
                string result = "List contents: ";
                foreach (var item in grid.path)
                {
                    result += item.x.ToString() + "," + item.y.ToString() + " ";
                }
                Debug.Log(result);
                Debug.Log("Path length in Nodes: " + grid.path.Count);
                Debug.Log("Path cost: " + CalculatePathCost());
                grid.ShowPathAndFrontier();
            }
        }
    }

    public void RunDFS()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            grid.path = pathfinder.DepthFirstSearchAlgorithm(grid.startNode, grid.endNode);
            if (grid.path != null)
            {
                string result = "List contents: ";
                foreach (var item in grid.path)
                {
                    result += item.x.ToString() + "," + item.y.ToString() + " ";
                }
                Debug.Log(result);
                Debug.Log("Path length in Nodes: " + grid.path.Count);
                Debug.Log("Path cost: " + CalculatePathCost());
                grid.ShowPathAndFrontier();
            }
        }
    }

    public Pathfinding.Results RunDFSTEST()
    {
        if (grid.startNode != null && grid.endNode != null)
        {
            results = pathfinder.DepthFirstSearchAlgorithmTEST(grid.startNode, grid.endNode);
            grid.path = results.path;
            if (grid.path != null)
            {
                results.pathLength = results.path.Count;
                results.pathCost = CalculatePathCost();
                grid.ShowPathAndFrontier();
            }
        }
        return results;
    }

    void TestAlgorithms(float wallchance, float slowchance, int runs)
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        if (runs <= 0)
        {
            return;
        }
        /*public List<Node> path;
        public int pathLength;
        public int pathCost;
        public int iterations;
        public long time;*/
        //average be a struct += last 10 results and then average

        //for 10 times
        //generate new level
        //set start and end (cant be on unwalkable)
        //run algorithm
        //set results += last result

        //average = results/10
        Pathfinding.Results DFSResults = new Pathfinding.Results();
        Pathfinding.Results BFSResults = new Pathfinding.Results();
        Pathfinding.Results GBFSResults = new Pathfinding.Results();
        Pathfinding.Results DijkstraResults = new Pathfinding.Results();
        Pathfinding.Results AStarResults = new Pathfinding.Results();
        Pathfinding.Results DijkstraResultsCROSS = new Pathfinding.Results();
        Pathfinding.Results AStarResultsCROSS = new Pathfinding.Results();

        //int iterations = 10;

        /*List<Node> path = new List<Node>();
        int length = 0;
        int cost = 0;
        int iterations = 0;
        long time = 0;*/

        /*results.path = new List<Node>();
        results.pathLength = 0;
        results.pathCost = 0;
        results.iterations = 0;
        results.time = 0;*/
        int test = 0;
        for (int i = 0; i < runs; i++)
        {
            test++;
            //var tempresults = new Pathfinding.Results();
            grid.GenerateNewLevel(wallchance, slowchance);
           

            for (int x = 0; x <3; x++)
            {
                grid.GenerateNewStartandEnd();
                
                if (testDFS)
                {
                    var tempresults = RunDFSTEST();
                    DFSResults = pathfinder.IncrementResult(DFSResults, tempresults);
                }
                if (testBFS)
                {
                    var tempresults = RunBFSCROSSTEST();
                    BFSResults = pathfinder.IncrementResult(BFSResults, tempresults);
                }
                if (testGBFS)
                {
                    var tempresults = RunGreedyBFSTEST();
                    GBFSResults = pathfinder.IncrementResult(GBFSResults, tempresults);
                }
                if (testDijkstra)
                {
                    var tempresults = RunDijkstraAlgorithmFilteredTEST();
                    DijkstraResults = pathfinder.IncrementResult(DijkstraResults, tempresults);
                }
                if (testAstar)
                {
                    var tempresults = RunAstarFilteredTEST();
                    AStarResults = pathfinder.IncrementResult(AStarResults, tempresults);
                }
                if (testDijkstraCROSS)
                {
                    var tempresults = RunDijkstraCROSSTEST();
                    DijkstraResultsCROSS = pathfinder.IncrementResult(DijkstraResultsCROSS, tempresults);
                }
                if (testAstarCROSS)
                {
                    var tempresults = RunAstarCrossTEST();
                    AStarResultsCROSS = pathfinder.IncrementResult(AStarResultsCROSS, tempresults);
                }
                /*results.pathLength += tempresults.pathLength;
                results.pathCost += tempresults.pathCost;
                results.iterations += tempresults.iterations;
                results.time += tempresults.time;*/

                test++;
            }

            /*var tempresults = RunAstarFilteredTEST();
            results.pathLength += tempresults.pathLength;
            results.pathCost += tempresults.pathCost;
            results.iterations += tempresults.iterations;
            results.time += tempresults.time;*/
        }

        //grid.UpdateTiles();

        float runsfloat = (float)runs;

        if (testDFS)
        {
            Debug.Log("Result DFS totals: " + " cost= " + DFSResults.pathCost + " length= " + DFSResults.pathLength + " iterations= " + DFSResults.iterations + " time= " + DFSResults.time);
            Debug.Log("Result DFS Average: " + " cost= " + (float)DFSResults.pathCost / runsfloat + " length= " + (float)DFSResults.pathLength / runsfloat + " iterations= " + (float)DFSResults.iterations / runsfloat + " time= " + (float)DFSResults.time / runsfloat);
        }
        if (testBFS)
        {
            Debug.Log("Result BFS totals: " + " cost= " + BFSResults.pathCost + " length= " + BFSResults.pathLength + " iterations= " + BFSResults.iterations + " time= " + BFSResults.time);
            Debug.Log("Result BFS Average: " + " cost= " + (float)BFSResults.pathCost / runsfloat + " length= " + (float)BFSResults.pathLength / runsfloat + " iterations= " + (float)BFSResults.iterations / runsfloat + " time= " + (float)BFSResults.time / runsfloat);
        }
        if (testGBFS)
        {
            Debug.Log("Result GBFS totals: " + " cost= " + GBFSResults.pathCost + " length= " + GBFSResults.pathLength + " iterations= " + GBFSResults.iterations + " time= " + GBFSResults.time);
            Debug.Log("Result GBFS Average: " + " cost= " + (float)GBFSResults.pathCost / runsfloat + " length= " + (float)GBFSResults.pathLength / runsfloat + " iterations= " + (float)GBFSResults.iterations / runsfloat + " time= " + (float)GBFSResults.time / runsfloat);
        }
        if (testDijkstra)
        {
            Debug.Log("Result Dijkstra totals: " + " cost= " + DijkstraResults.pathCost + " length= " + DijkstraResults.pathLength + " iterations= " + DijkstraResults.iterations + " time= " + DijkstraResults.time);
            Debug.Log("Result Dijkstra Average: " + " cost= " + (float)DijkstraResults.pathCost / runsfloat + " length= " + (float)DijkstraResults.pathLength / runsfloat + " iterations= " + (float)DijkstraResults.iterations / runsfloat + " time= " + (float)DijkstraResults.time / runsfloat);
        }
        if (testAstar)
        {
            Debug.Log("Result Astar totals: " + " cost= " + AStarResults.pathCost + " length= " + AStarResults.pathLength + " iterations= " + AStarResults.iterations + " time= " + AStarResults.time);
            Debug.Log("Result Astar Average: " + " cost= " + (float)AStarResults.pathCost / runsfloat + " length= " + (float)AStarResults.pathLength / runsfloat + " iterations= " + (float)AStarResults.iterations / runsfloat + " time= " + (float)AStarResults.time / runsfloat);
        }
        if (testDijkstraCROSS)
        {
            Debug.Log("Result DijkstraCROSS totals: " + " cost= " + DijkstraResultsCROSS.pathCost + " length= " + DijkstraResultsCROSS.pathLength + " iterations= " + DijkstraResultsCROSS.iterations + " time= " + DijkstraResultsCROSS.time);
            Debug.Log("Result DijkstraCROSS Average: " + " cost= " + (float)DijkstraResultsCROSS.pathCost / runsfloat + " length= " + (float)DijkstraResultsCROSS.pathLength / runsfloat + " iterations= " + (float)DijkstraResultsCROSS.iterations / runsfloat + " time= " + (float)DijkstraResultsCROSS.time / runsfloat);
        }
        if (testAstarCROSS)
        {
            Debug.Log("Result AstarCROSS totals: " + " cost= " + AStarResultsCROSS.pathCost + " length= " + AStarResultsCROSS.pathLength + " iterations= " + AStarResultsCROSS.iterations + " time= " + AStarResultsCROSS.time);
            Debug.Log("Result AstarCROSS Average: " + " cost= " + (float)AStarResultsCROSS.pathCost / runsfloat + " length= " + (float)AStarResultsCROSS.pathLength / runsfloat + " iterations= " + (float)AStarResultsCROSS.iterations / runsfloat + " time= " + (float)AStarResultsCROSS.time / runsfloat);
        }
        //Debug.Log("Astar time TEST " + (float)AStarResults.time / runsfloat);

        grid.GenerateNewLevel(wallchance, slowchance);
        sw.Stop();
        Debug.Log("TestAlgorithms time to complete = " + sw.ElapsedMilliseconds + "ms");
        Debug.Log("Test = " + test);
        //generate two ints within grid size, if int !walkable or is end/start generate a new one else set start node end node x,y
    }

    
    List<Node> FilteredNeighbours(Node cur) //make temp list an remove the non accessible so it doesnt effect the neighbours
    {
        //cur.neighboursDiagSafe.AddRange(cur.neighboursAll);
        List<Node> filteredlist = new List<Node>();
        filteredlist.AddRange(cur.neighboursAll);
        //Node temp = null;
        Debug.Log(cur.x + "," + cur.y);
        if (filteredlist != null)
        {
            string result = "List contents: ";
            foreach (var item in filteredlist)
            {
                result += item.x.ToString() + "," + item.y.ToString() + " ";
            }
            Debug.Log(result);


        }
        if (grid.GetNode(cur.x, cur.y + 1) != null && !grid.GetNode(cur.x, cur.y + 1).walkable)
        {
            Debug.Log("Called for up test");
            if (grid.GetNode(cur.x + 1, cur.y) != null && !grid.GetNode(cur.x + 1, cur.y).walkable)
            {
                if (filteredlist.Contains(grid.GetNode(cur.x + 1, cur.y + 1)))
                {
                    Debug.Log("Removed x+1, y+1");
                    filteredlist.Remove(grid.GetNode(cur.x + 1, cur.y + 1)); 
                }

            }
            if (grid.GetNode(cur.x - 1, cur.y) != null && !grid.GetNode(cur.x - 1, cur.y).walkable)
            {
                if (filteredlist.Contains(grid.GetNode(cur.x - 1, cur.y + 1)))
                {
                    Debug.Log("Removed x-1, y+1");
                    filteredlist.Remove(grid.GetNode(cur.x - 1, cur.y + 1));
                }

            }
        }
        if (grid.GetNode(cur.x, cur.y - 1) != null && !grid.GetNode(cur.x, cur.y - 1).walkable)
        {
            Debug.Log("Called for down test");
            if (grid.GetNode(cur.x + 1, cur.y) != null && !grid.GetNode(cur.x + 1, cur.y).walkable)
            {
                if (filteredlist.Contains(grid.GetNode(cur.x + 1, cur.y - 1)))
                {
                    Debug.Log("Removed x+1, y-1");
                    filteredlist.Remove(grid.GetNode(cur.x + 1, cur.y - 1));
                }

            }
            if (grid.GetNode(cur.x - 1, cur.y) != null && !grid.GetNode(cur.x - 1, cur.y).walkable)
            {
                if (filteredlist.Contains(grid.GetNode(cur.x - 1, cur.y - 1)))
                {
                    Debug.Log("Removed x-1, y-1");
                    filteredlist.Remove(grid.GetNode(cur.x - 1, cur.y - 1));
                }

            }
        }
        /*if (filteredlist.Contains(grid.GetNode(cur.x, cur.y + 1)))
        {
            temp = grid.GetNode(cur.x, cur.y + 1);
            if (!temp.walkable)
            {
                if (filteredlist.Contains(grid.GetNode(cur.x + 1, cur.y)))
                {
                    temp = grid.GetNode(cur.x + 1, cur.y);
                    if (!temp.walkable)
                    {
                        if (filteredlist.Contains(grid.GetNode(cur.x + 1, cur.y + 1)))
                        {
                            filteredlist.Remove(grid.GetNode(cur.x + 1, cur.y + 1));
                        }
                    }
                }
            }
            temp = grid.GetNode(cur.x, cur.y + 1);
            if (filteredlist.Contains(grid.GetNode(cur.x - 1, cur.y)))
            {
                temp = grid.GetNode(cur.x - 1, cur.y);
                if (!temp.walkable)
                {
                    if (filteredlist.Contains(grid.GetNode(cur.x - 1, cur.y + 1)))
                    {
                        filteredlist.Remove(grid.GetNode(cur.x - 1, cur.y + 1));
                    }
                }
            }
        }
        if (filteredlist.Contains(grid.GetNode(cur.x, cur.y - 1)))
        {
            temp = grid.GetNode(cur.x, cur.y - 1);
            if (!temp.walkable)
            {
                if (filteredlist.Contains(grid.GetNode(cur.x + 1, cur.y)))
                {
                    temp = grid.GetNode(cur.x + 1, cur.y);
                    if (!temp.walkable)
                    {
                        if (filteredlist.Contains(grid.GetNode(cur.x + 1, cur.y - 1)))
                        {
                            filteredlist.Remove(grid.GetNode(cur.x + 1, cur.y - 1));
                        }
                    }
                }
            }
            temp = grid.GetNode(cur.x, cur.y - 1);
            if (filteredlist.Contains(grid.GetNode(cur.x - 1, cur.y)))
            {
                temp = grid.GetNode(cur.x - 1, cur.y);
                if (!temp.walkable)
                {
                    if (filteredlist.Contains(grid.GetNode(cur.x - 1, cur.y - 1)))
                    {
                        filteredlist.Remove(grid.GetNode(cur.x - 1, cur.y - 1));
                    }
                }
            }
        }*/
        if (filteredlist != null)
        {
            string result = "List contents: ";
            foreach (var item in filteredlist)
            {
                result += item.x.ToString() + "," + item.y.ToString() + " ";
            }
            Debug.Log(result);
            
           
        }
        return filteredlist;
    }
    int CalculatePathCost()
    {
        int pathCost = 0;
        foreach (var node in grid.path)
        {
            if (node.cost == 0)
            {
                pathCost += 10;
            }
            else
            {
                pathCost += node.cost;
            }
        }
        return pathCost;
    }
}
