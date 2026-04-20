using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

public class Pathfinding : MonoBehaviour
{
    public Grid grid;
    //Node highlighttile;
    public GameObject TestCube;
    private void Start()
    {
        //grid = GetComponent<Grid>();
        
    }

    public struct Results
    {
        public List<Node> path;
        public int pathLength;
        public int pathCost;
        public int iterations;
        public long time;
    }


    public List<Node> BFSAlgorithmALL(Node start, Node end)
    {
        int count = 0;
        grid.frontierList.Clear();
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Queue<Node> frontier = new Queue<Node>();
        List<Node> visited = new List<Node>();

        frontier.Enqueue(end);

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == start)
            {
                visited.Add(current);
                Debug.Log("broke");
                break;
            }

            foreach (Node neighbour in current.neighboursAll)
            {
                if (!visited.Contains(neighbour) && !frontier.Contains(neighbour))
                {
                    if (neighbour.walkable)
                    {
                        if (grid.showfrontier)
                        {
                            grid.frontierList.Add(neighbour);
                        }
                        frontier.Enqueue(neighbour);
                        neighbour.parent = current;
                    }
                } 
            }
            visited.Add(current);
            count++;
        }

        if (!visited.Contains(start))
        {
            Debug.Log("Path not found - BFSAlgorithmALL");
            return null;
        }

        sw.Stop();

        Queue<Node> path = new Queue<Node>();
        Node currentNode = start;

        while (currentNode != end)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }

        Debug.Log("Time to BFSAlgorithmALL(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations: " + count);
        return path.ToList();
    }

    public List<Node> BFSAlgorithmCROSS(Node start, Node end)
    {
        int count = 0;
        grid.frontierList.Clear();
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Queue<Node> frontier = new Queue<Node>();
        List<Node> visited = new List<Node>();

        frontier.Enqueue(start);
        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == end)
            {
                visited.Add(current);
                Debug.Log("broke");
                break;
            }

            foreach (Node neighbour in current.neighboursCross)
            {
                if (!visited.Contains(neighbour) && !frontier.Contains(neighbour))
                {
                    if (neighbour.walkable)
                    {
                        if (grid.showfrontier)
                        {
                            grid.frontierList.Add(neighbour);
                        }
                        frontier.Enqueue(neighbour);
                        neighbour.parent = current;
                    }

                }
            }
            visited.Add(current);
            count++;
        }

        if (!visited.Contains(end))
        {
            Debug.Log("Path not found - BFSAlgorithmCROSS");
            return null;
        }

        sw.Stop();
        
        Queue<Node> path = new Queue<Node>();
        Node currentNode = end;

        while (currentNode != start)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }

        Debug.Log("Time to BFSAlgorithmCROSS(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations: " + count);
        return path.ToList();
    }

    public Results BFSAlgorithmCROSSTEST(Node start, Node end)
    {
        int count = 0;
        grid.frontierList.Clear();
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Queue<Node> frontier = new Queue<Node>();
        List<Node> visited = new List<Node>();
        Results returnres = new Results();

        frontier.Enqueue(start);
        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == end)
            {
                visited.Add(current);
                Debug.Log("broke");
                break;
            }

            foreach (Node neighbour in current.neighboursCross)
            {
                if (!visited.Contains(neighbour) && !frontier.Contains(neighbour))
                {
                    if (neighbour.walkable)
                    {
                        if (grid.showfrontier)
                        {
                            grid.frontierList.Add(neighbour);
                        }
                        frontier.Enqueue(neighbour);
                        neighbour.parent = current;
                    }

                }
            }
            visited.Add(current);
            count++;
        }

        if (!visited.Contains(end))
        {
            Debug.Log("Path not found - BFSAlgorithmCROSS");
            
            return returnres;
        }

        sw.Stop();

        Queue<Node> path = new Queue<Node>();
        Node currentNode = end;

        while (currentNode != start)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }

        Debug.Log("Time to BFSAlgorithmCROSS(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations: " + count);
        returnres.path = path.ToList();
        returnres.time = sw.ElapsedMilliseconds;
        
        return returnres;
    }

    public List<Node> GreedyBFSAlgorithm(Node start, Node end)
    {
        int count = 0;
        grid.frontierList.Clear();
        Stopwatch sw = new Stopwatch();
        sw.Start();
        PriorityQueue<Node> frontier = new PriorityQueue<Node>(); //class taken from C# .net
        //Dictionary<Node, int> costToTile = new Dictionary<Node, int>();
        List<Node> visited = new List<Node>();

        frontier.Enqueue(start, 0);
        

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == end)
            {
                visited.Add(end);
                break;
            }

            foreach (Node neighbour in current.neighboursCross)
            {
                //int newCost = costToTile[current] + neighbour.cost;
                if (!visited.Contains(neighbour))
                {
                    if (neighbour.walkable)
                    {
                        if (grid.showfrontier)
                        {
                            grid.frontierList.Add(neighbour);
                        }
                        int priority = DistanceBetweenNodes(neighbour, end);
                        frontier.Enqueue(neighbour, priority);
                        neighbour.parent = current;
                    }
                }
            }
            visited.Add(current);
            count++;
        }

        if (!visited.Contains(end))
        {
            Debug.Log("Path not found - GreedyBFSAlgorithm");
            return null;
        }
        sw.Stop();

        Queue<Node> path = new Queue<Node>();
        Node currentNode = end;

        while (currentNode != start)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }


        Debug.Log("Time to GreedyBFSAlgorithm(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations: " + count);
        return path.ToList();
    }

    public List<Node> DijkstraAlgorithmCROSS(Node start, Node end)
    {
        int count = 0;
        grid.frontierList.Clear();
        Stopwatch sw = new Stopwatch();
        sw.Start();
        PriorityQueue<Node> frontier = new PriorityQueue<Node>(); //class taken from C# .net
        Dictionary<Node, int> costToTile = new Dictionary<Node, int>(); 
        
        frontier.Enqueue(start, 0);
        costToTile[start] = 0;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == end)
            { 
                break;
            }

            foreach (Node neighbour in current.neighboursCross)
            {
                int newCost = costToTile[current] + neighbour.cost + 1; //+1 due to removal of base cost for other algorithms fixes bugged path
                if (!costToTile.ContainsKey(neighbour) || newCost < costToTile[neighbour])
                {
                    if (neighbour.walkable)
                    {
                        if (grid.showfrontier)
                        {
                            grid.frontierList.Add(neighbour);
                        }
                        costToTile[neighbour] = newCost;
                        frontier.Enqueue(neighbour, newCost);
                        neighbour.parent = current;
                    }
                }
            }   
            count++;
        }

        if (!costToTile.ContainsKey(end))
        {
            Debug.Log("Path not found - DijkstraAlgorithmCROSS");
            return null;
        }
        sw.Stop();

        Queue<Node> path = new Queue<Node>();
        Node currentNode = end;

        while (currentNode != start)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }

        
        Debug.Log("Time to DijkstraAlgorithmCROSS(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations: " + count);
        return path.ToList();
    }

    public List<Node> DijkstraAlgorithm(Node start, Node end)
    {
        grid.frontierList.Clear();
        int count = 0;
        Stopwatch sw = new Stopwatch();
        sw.Start();
        PriorityQueue<Node> frontier = new PriorityQueue<Node>(); //class taken from C# .net
        Dictionary<Node, int> costToTile = new Dictionary<Node, int>();

        frontier.Enqueue(start, 0);
        costToTile[start] = 0;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == end)
            {
                break;
            }

            foreach (Node neighbour in current.neighboursAll)
            {
                int newCost = costToTile[current] + neighbour.cost + DistanceBetweenNodes(current, neighbour);
                if (!costToTile.ContainsKey(neighbour) || newCost < costToTile[neighbour])
                {
                    if (neighbour.walkable)
                    {
                        if (grid.showfrontier)
                        {
                            grid.frontierList.Add(neighbour);
                        }
                        costToTile[neighbour] = newCost;
                        frontier.Enqueue(neighbour, newCost);
                        neighbour.parent = current;
                    }
                }
            }
            count++;
        }

        if (!costToTile.ContainsKey(end))
        {
            Debug.Log("Path not found - DijkstraAlgorithm");
            return null;
        }
        sw.Stop();

        Queue<Node> path = new Queue<Node>();
        Node currentNode = end;

        while (currentNode != start)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }


        Debug.Log("Time to DijkstraAlgorithm(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations: " + count);

        return path.ToList();
    }

    public List<Node> DijkstraAlgorithmFiltered(Node start, Node end)
    {
        grid.PopulateNeighboursDiagExcept();
        grid.frontierList.Clear();
        int count = 0;

        Stopwatch sw = new Stopwatch();
        sw.Start();
        PriorityQueue<Node> frontier = new PriorityQueue<Node>(); //class taken from C# .net
        Dictionary<Node, int> costToTile = new Dictionary<Node, int>();

        frontier.Enqueue(start, 0);
        costToTile[start] = 0;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == end)
            {
                break;
            }

            foreach (Node neighbour in current.neighboursDiagSafe)
            {
                int newCost = costToTile[current] + neighbour.cost + DistanceBetweenNodes(current, neighbour);
                if (!costToTile.ContainsKey(neighbour) || newCost < costToTile[neighbour])
                {
                    if (neighbour.walkable)
                    {
                        if (grid.showfrontier)
                        {
                            grid.frontierList.Add(neighbour);
                        }
                        costToTile[neighbour] = newCost;
                        frontier.Enqueue(neighbour, newCost);
                        neighbour.parent = current;
                        
                    }
                }
            }
            count++;
        }

        if (!costToTile.ContainsKey(end))
        {
            Debug.Log("Path not found - DijkstraAlgorithmFiltered");
            return null;
        }
        sw.Stop();

        Queue<Node> path = new Queue<Node>();
        Node currentNode = end;

        while (currentNode != start)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }


        Debug.Log("Time to DijkstraAlgorithmFiltered(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations " + count);

        return path.ToList();
    }

    public List<Node> AstarAlgorithmFiltered(Node start, Node end) //feels like its exploring to many nodes
    {
        grid.PopulateNeighboursDiagExcept();
        grid.frontierList.Clear();
        int count = 0;

        Stopwatch sw = new Stopwatch();
        sw.Start();
        PriorityQueue<Node> frontier = new PriorityQueue<Node>(); //class taken from C# .net
        Dictionary<Node, int> gCost = new Dictionary<Node, int>();

        frontier.Enqueue(start, 0);
        gCost[start] = 0;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == end)
            {
                break;
            }

            foreach (Node neighbour in current.neighboursDiagSafe)
            {
                if (!neighbour.walkable)
                {
                    continue;
                }
                int newGcost = gCost[current] + neighbour.cost + DistanceBetweenNodes(current, neighbour);
                //int newCost = DistanceBetweenNodes(end, current) + neighbour.cost + DistanceBetweenNodes(current, neighbour);
                //if (newCost < costToTile[neighbour] || !costToTile.ContainsKey(neighbour)) //WHEN SWAPPED CAUSES ERROR, INVESTIGATE, cant check cost to neighbour as it hasnt been set yet
                if (!gCost.ContainsKey(neighbour) || newGcost < gCost[neighbour] )
                {
                    if (grid.showfrontier && !grid.frontierList.Contains(neighbour))
                    {
                        grid.frontierList.Add(neighbour);
                    }
                    gCost[neighbour] = newGcost;
                    int fCost = newGcost + DistanceBetweenNodes(neighbour, end); //times 10 for the fact costs are times 10
                    frontier.Enqueue(neighbour, fCost);
                    neighbour.parent = current;
                }
            }
            count++;
        }

        if (!gCost.ContainsKey(end))
        {
            Debug.Log("Path not found - AstarAlgorithmFiltered");
            return null;
        }
        sw.Stop();

        Queue<Node> path = new Queue<Node>();
        Node currentNode = end;

        while (currentNode != start)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }


        Debug.Log("Time to AstarAlgorithmFiltered(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations: " + count);

        return path.ToList();
    }

    public Results AstarAlgorithmFilteredTEST(Node start, Node end) //feels like its exploring to many nodes
    {
        grid.PopulateNeighboursDiagExcept();
        grid.frontierList.Clear();
        int count = 0;
        Results results = new Results();

        Stopwatch sw = new Stopwatch();
        sw.Start();
        PriorityQueue<Node> frontier = new PriorityQueue<Node>(); //class taken from C# .net
        Dictionary<Node, int> gCost = new Dictionary<Node, int>();

        frontier.Enqueue(start, 0);
        gCost[start] = 0;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == end)
            {
                break;
            }

            foreach (Node neighbour in current.neighboursDiagSafe)
            {
                if (!neighbour.walkable)
                {
                    continue;
                }
                int newGcost = gCost[current] + neighbour.cost + DistanceBetweenNodes(current, neighbour);
                //int newCost = DistanceBetweenNodes(end, current) + neighbour.cost + DistanceBetweenNodes(current, neighbour);
                //if (newCost < costToTile[neighbour] || !costToTile.ContainsKey(neighbour)) //WHEN SWAPPED CAUSES ERROR, INVESTIGATE, cant check cost to neighbour as it hasnt been set yet
                if (!gCost.ContainsKey(neighbour) || newGcost < gCost[neighbour])
                {
                    if (grid.showfrontier && !grid.frontierList.Contains(neighbour))
                    {
                        grid.frontierList.Add(neighbour);
                    }
                    gCost[neighbour] = newGcost;
                    int fCost = newGcost + DistanceBetweenNodes(neighbour, end); //times 10 for the fact costs are times 10
                    frontier.Enqueue(neighbour, fCost);
                    neighbour.parent = current;
                }
            }
            count++;
        }

        if (!gCost.ContainsKey(end))
        {
            Debug.Log("Path not found - AstarAlgorithmFilteredTEST");
            return results;
        }
        sw.Stop();

        Queue<Node> path = new Queue<Node>();
        Node currentNode = end;

        while (currentNode != start)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }


        Debug.Log("Time to AstarAlgorithmFilteredTEST(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations: " + count);

        results.path = path.ToList();
        results.time = sw.ElapsedMilliseconds;
        results.iterations = count;
        return results;
    }

    public List<Node> AstarAlgorithmFiltered2(Node start, Node end)
    {
        grid.PopulateNeighboursDiagExcept();
        grid.frontierList.Clear();
        int count = 0;

        Stopwatch sw = new Stopwatch();
        sw.Start();
        PriorityQueue<Node> frontier = new PriorityQueue<Node>(); //class taken from C# .net
        Dictionary<Node, int> gCost = new Dictionary<Node, int>();

        frontier.Enqueue(start, 0);
        gCost[start] = 0;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            
            if (current == end)
            {
                break;
            }

            foreach (Node neighbour in current.neighboursDiagSafe)
            {
                if (grid.showfrontier && !grid.frontierList.Contains(neighbour))
                {
                    grid.frontierList.Add(neighbour);
                }
                if (!neighbour.walkable)
                {
                    continue;
                }
                int newGcost = gCost[current] + neighbour.cost + DistanceBetweenNodes(current, neighbour);
                //int newCost = DistanceBetweenNodes(end, current) + neighbour.cost + DistanceBetweenNodes(current, neighbour);
                //if (newCost < costToTile[neighbour] || !costToTile.ContainsKey(neighbour)) //WHEN SWAPPED CAUSES ERROR, INVESTIGATE, cant check cost to neighbour as it hasnt been set yet
                if (!gCost.ContainsKey(neighbour) || newGcost < gCost[neighbour])
                {
                   
                    gCost[neighbour] = newGcost;
                    int fCost = newGcost + DistanceBetweenNodes(neighbour, end); //times 10 for the fact costs are times 10
                    frontier.Enqueue(neighbour, fCost);
                    neighbour.parent = current;
                }
            }
            count++;
        }

        if (!gCost.ContainsKey(end))
        {
            Debug.Log("Path not found - AstarAlgorithmFiltered2");
            return null;
        }
        sw.Stop();

        Queue<Node> path = new Queue<Node>();
        Node currentNode = end;

        while (currentNode != start)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }


        Debug.Log("Time to AstarAlgorithmFiltered2(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations: " + count);

        return path.ToList();
    }

    public List<Node> AstarAlgorithmCross(Node start, Node end)
    {
        int count = 0;
        grid.frontierList.Clear();
        Stopwatch sw = new Stopwatch();
        sw.Start();
        PriorityQueue<Node> frontier = new PriorityQueue<Node>(); //class taken from C# .net
        Dictionary<Node, int> costToTile = new Dictionary<Node, int>();

        frontier.Enqueue(start, 0);
        costToTile[start] = 0;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == end)
            {
                break;
            }

            foreach (Node neighbour in current.neighboursCross)
            {
                int newCost = costToTile[current] + neighbour.cost;
                if (!costToTile.ContainsKey(neighbour) || newCost < costToTile[neighbour])
                {
                    if (neighbour.walkable)
                    {
                        if (grid.showfrontier)
                        {
                            grid.frontierList.Add(neighbour);
                        }
                        costToTile[neighbour] = newCost;
                        int priority = newCost + (ManhattanDistance(end, neighbour) * 10);
                        frontier.Enqueue(neighbour, priority);
                        neighbour.parent = current;
                    }
                }
            }
            count++;
        }

        if (!costToTile.ContainsKey(end))
        {
            Debug.Log("Path not found - AstarAlgorithmCross");
            return null;
        }
        sw.Stop();

        Queue<Node> path = new Queue<Node>();
        Node currentNode = end;

        while (currentNode != start)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }


        Debug.Log("Time to AstarAlgorithmCross(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations: " + count);
        return path.ToList();
    }

    public List<Node> DepthFirstSearchAlgorithm(Node start, Node end)
    {
        int count = 0;
        grid.frontierList.Clear();
        Stopwatch sw = new Stopwatch();
        sw.Start();
        HashSet<Node> visited = new HashSet<Node>();
        Stack<Node> stack = new Stack<Node>();

        stack.Push(start);
        //start.parent = start; //fixes retracing memory error?
        while (stack.Count > 0)
        {
            Node current = stack.Pop();
            if (current == end)
            {
                visited.Add(current);
                break;
            }

            if (!visited.Contains(current))
            {
                visited.Add(current);

                foreach (Node neighbour in current.neighboursCross)
                {
                    if (neighbour.walkable && !visited.Contains(neighbour))
                    {
                        if (grid.showfrontier)
                        {
                            grid.frontierList.Add(neighbour);
                        }
                        stack.Push(neighbour);
                        neighbour.parent = current;
                    }
                }
            }
            count++;
        }

        if (!visited.Contains(end))
        {
            Debug.Log("Path not found - DepthFirstSearchAlgorithm");
            return null;
        }
        sw.Stop();

        Queue<Node> path = new Queue<Node>();
        Node currentNode = end;

        while (currentNode != start)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }

        Debug.Log("Time to DepthFirstSearchAlgorithm(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations: " + count);
        return path.ToList();
        //return null;
    }

    int DistanceBetweenNodes(Node nodeA, Node nodeB)
    {
        int Xdist = Mathf.Abs(nodeA.x - nodeB.x);
        int Ydist = Mathf.Abs(nodeA.y - nodeB.y);

        int distance = 0;

        if (Xdist > Ydist)
        {
            distance = 14 * Ydist + 10 * (Xdist - Ydist);
        }
        else
        {
            distance = 14 * Xdist + 10 * (Ydist - Xdist);
        }

        return distance;
    }

    int ManhattanDistance(Node nodeA, Node nodeB)
    {
        int distance = Mathf.Abs(nodeA.x - nodeB.x) + Mathf.Abs(nodeA.y - nodeB.y);

        return distance; 
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        if (highlighttile != null)
        {
            Gizmos.DrawCube(highlighttile.worldPos, new Vector3(grid.nodeDiameter, 0.1f, grid.nodeDiameter));
        }
        
    }*/

    
}
