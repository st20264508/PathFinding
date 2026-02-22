using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Pathfinding : MonoBehaviour
{
    public Grid grid;

    private void Start()
    {
        grid = GetComponent<Grid>();
    }

    public List<Node> BFSAlgorithm(Node start, Node end)
    {
        Queue<Node> frontier = new Queue<Node>();
        List<Node> visited = new List<Node>();

        frontier.Enqueue(end);

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();

            foreach (Node neighbour in current.neighbours)
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
            return null;

        Queue<Node> path = new Queue<Node>();
        Node currentNode = start;

        while (currentNode != end)
        {
            currentNode = currentNode.parent;
            path.Enqueue(currentNode);
        }

        return path.ToList();
    }
}
