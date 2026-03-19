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

        if (!visited.Contains(start))
        {
            Debug.Log("Path not found - BFSAlgorithmCROSS");
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

        Debug.Log("Time to BFSAlgorithmCROSS(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations: " + count);
        return path.ToList();
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
        
        frontier.Enqueue(end, 0);
        costToTile[end] = 0;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == start)
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
                        frontier.Enqueue(neighbour, newCost);
                        neighbour.parent = current;
                    }
                }
            }   
            count++;
        }

        if (!costToTile.ContainsKey(start))
        {
            Debug.Log("Path not found - DijkstraAlgorithmCROSS");
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

        frontier.Enqueue(end, 0);
        costToTile[end] = 0;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == start)
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

        if (!costToTile.ContainsKey(start))
        {
            Debug.Log("Path not found - DijkstraAlgorithm");
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

        frontier.Enqueue(end, 0);
        costToTile[end] = 0;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == start)
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

        if (!costToTile.ContainsKey(start))
        {
            Debug.Log("Path not found - DijkstraAlgorithmFiltered");
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
        Dictionary<Node, int> costToTile = new Dictionary<Node, int>();

        frontier.Enqueue(end, 0);
        costToTile[end] = 0;

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            if (current == start)
            {
                break;
            }

            foreach (Node neighbour in current.neighboursDiagSafe)
            {
                if (!neighbour.walkable)
                {
                    continue;
                }
                int newCost = costToTile[current] + neighbour.cost + DistanceBetweenNodes(current, neighbour);
                //int newCost = DistanceBetweenNodes(end, current) + neighbour.cost + DistanceBetweenNodes(current, neighbour);
                //if (newCost < costToTile[neighbour] || !costToTile.ContainsKey(neighbour)) //WHEN SWAPPED CAUSES ERROR, INVESTIGATE, cant check cost to neighbour as it hasnt been set yet
                if (!costToTile.ContainsKey(neighbour) || newCost < costToTile[neighbour] )
                {
                    if (grid.showfrontier && !grid.frontierList.Contains(neighbour))
                    {
                        grid.frontierList.Add(neighbour);
                    }
                    costToTile[neighbour] = newCost;
                    int priority = newCost + DistanceBetweenNodes(neighbour, start); //times 10 for the fact costs are times 10
                    frontier.Enqueue(neighbour, priority);
                    neighbour.parent = current;
                }
            }
            count++;
        }

        if (!costToTile.ContainsKey(start))
        {
            Debug.Log("Path not found - AstarAlgorithmFiltered");
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


        Debug.Log("Time to AstarAlgorithmFiltered(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well
        Debug.Log("While loop iterations: " + count);

        return path.ToList();
    }

    public List<Node> AstarAlgorithmFiltered2(Node start, Node end)
    {
        grid.PopulateNeighboursDiagExcept();
        grid.frontierList.Clear();
        int count = 0;

        Stopwatch sw = new Stopwatch();
        sw.Start();
        PriorityQueue<Node> frontier = new PriorityQueue<Node>(); //class taken from C# .net
        Dictionary<Node, int> costToTile = new Dictionary<Node, int>();

        frontier.Enqueue(end, 0);
        costToTile[end] = 0;
        

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
           
            if (current == start)
            {
                break;
            }

            foreach (Node neighbour in current.neighboursDiagSafe)
            {
                if (!neighbour.walkable)
                {
                    continue;
                }
                int newCost = costToTile[current] + neighbour.cost + (ManhattanDistance(current, neighbour) * 10);
                //int newCost = DistanceBetweenNodes(end, current) + neighbour.cost + DistanceBetweenNodes(current, neighbour);
                //if (newCost < costToTile[neighbour] || !costToTile.ContainsKey(neighbour)) //WHEN SWAPPED CAUSES ERROR, INVESTIGATE, cant check cost to neighbour as it hasnt been set yet
                if (!costToTile.ContainsKey(neighbour) || newCost < costToTile[neighbour])
                {
                    if (grid.showfrontier && !grid.frontierList.Contains(neighbour))
                    {
                        grid.frontierList.Add(neighbour);
                    }
                    costToTile[neighbour] = newCost;
                    int priority = newCost + (ManhattanDistance(neighbour, start) * 10); //times 10 for the fact costs are times 10
                    frontier.Enqueue(neighbour, priority);
                    neighbour.parent = current;
                }
            }
            count++;
        }

        if (!costToTile.ContainsKey(start))
        {
            Debug.Log("Path not found - AstarAlgorithmFiltered2");
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
