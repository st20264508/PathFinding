using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

public class Pathfinding : MonoBehaviour
{
    public Grid grid;

    private void Start()
    {
        //grid = GetComponent<Grid>();
        
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            FilteredNeighbours(grid.GetNode(0, 10));
        }
    }

    public List<Node> BFSAlgorithmALL(Node start, Node end)
    {
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
                        frontier.Enqueue(neighbour);
                        neighbour.parent = current;
                    }
                } 
            }
            visited.Add(current);
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

        return path.ToList();
    }

    public List<Node> BFSAlgorithmCROSS(Node start, Node end)
    {
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
                        frontier.Enqueue(neighbour);
                        neighbour.parent = current;
                    }

                }
            }
            visited.Add(current);
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

        return path.ToList();
    }
    public List<Node> DijkstraAlgorithmCROSS(Node start, Node end)
    {
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
                        costToTile[neighbour] = newCost;
                        frontier.Enqueue(neighbour, newCost);
                        neighbour.parent = current;
                    }
                }
            }   
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

        return path.ToList();
    }

    public List<Node> DijkstraAlgorithm(Node start, Node end)
    {
        Node temp = grid.GetNode(-3, 123123);

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
                        costToTile[neighbour] = newCost;
                        frontier.Enqueue(neighbour, newCost);
                        neighbour.parent = current;
                    }
                }
            }
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

        return path.ToList();
    }

    public List<Node> DijkstraAlgorithmFiltered(Node start, Node end)
    {
        grid.PopulateNeighboursDiagExcept();

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
                        costToTile[neighbour] = newCost;
                        frontier.Enqueue(neighbour, newCost);
                        neighbour.parent = current;
                    }
                }
            }
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

        return path.ToList();
    }
    int DistanceBetweenNodes(Node nodeA, Node nodeB)
    {
        int Xdist = Mathf.Abs(nodeA.x - nodeB.x);
        int Ydist = Mathf.Abs(nodeA.y - nodeB.y);

        if (Xdist > Ydist)
        {
            return 14 * Ydist + 10 * (Xdist - Ydist);
        }
        else
        {
            return 14 * Xdist + 10 * (Ydist - Xdist);
        }
    }

    public List<Node> FilteredNeighbours(Node cur) //make temp list an remove the non accessible so it doesnt effect the neighbours
    {
        List<Node> filteredlist = cur.neighboursAll;
        //Node temp = null;

        if (grid.GetNode(cur.x, cur.y + 1) != null && !grid.GetNode(cur.x, cur.y + 1).walkable)
        {
            if (grid.GetNode(cur.x + 1, cur.y) != null && !grid.GetNode(cur.x + 1, cur.y).walkable)
            {
                if(filteredlist.Contains(grid.GetNode(cur.x + 1, cur.y + 1)))
                {
                    filteredlist.Remove(grid.GetNode(cur.x + 1, cur.y + 1));
                }
                
            }
            if (grid.GetNode(cur.x - 1, cur.y) != null && !grid.GetNode(cur.x - 1, cur.y).walkable)
            {
                if (filteredlist.Contains(grid.GetNode(cur.x - 1, cur.y + 1)))
                {
                    filteredlist.Remove(grid.GetNode(cur.x - 1, cur.y + 1));
                }
                
            }
        }
        if (grid.GetNode(cur.x, cur.y - 1) != null && !grid.GetNode(cur.x, cur.y - 1).walkable)
        {
            if (grid.GetNode(cur.x + 1, cur.y) != null && !grid.GetNode(cur.x + 1, cur.y).walkable)
            {
                if (filteredlist.Contains(grid.GetNode(cur.x + 1, cur.y - 1)))
                {
                    filteredlist.Remove(grid.GetNode(cur.x + 1, cur.y - 1));
                }
                
            }
            if (grid.GetNode(cur.x - 1, cur.y) != null && !grid.GetNode(cur.x - 1, cur.y).walkable)
            {
                if (filteredlist.Contains(grid.GetNode(cur.x - 1, cur.y - 1)))
                {
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
        /*if (filteredlist != null)
        {
            string result = "List contents: ";
            foreach (var item in filteredlist)
            {
                result += item.x.ToString() + "," + item.y.ToString() + " ";
            }
            Debug.Log(result);
            
           
        }*/
        return filteredlist;
    }
}
