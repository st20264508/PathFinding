using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

public class Pathfinding : MonoBehaviour
{
    public Grid grid;

    private void Start()
    {
        grid = GetComponent<Grid>();
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
            Debug.Log("Path not found");
            return null;
        }
            

        Queue<Node> path = new Queue<Node>();
        Node currentNode = start;

        while (currentNode != end)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }

        sw.Stop();
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
            Debug.Log("Path not found");
            return null;
        }

        Queue<Node> path = new Queue<Node>();
        Node currentNode = start;

        while (currentNode != end)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }

        sw.Stop();
        Debug.Log("Time to BFSAlgorithmCROSS(): " + sw.ElapsedMilliseconds + "ms"); //not entirely accurate as path calc is done here now as well

        return path.ToList();
    }
}
