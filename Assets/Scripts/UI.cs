using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UI : MonoBehaviour
{
    Camera cam;
    public Grid grid;
    public GameObject test;
    public Pathfinding pathfinder;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("space pressed");
            List<Node> path = pathfinder.BFSAlgorithm(grid.startNode, grid.endNode);
            
            string result = "List contents: ";
            foreach (var item in path)
            {
                result += item.x.ToString() + "," + item.y.ToString() + " ";
                //Gizmos.color = Color.yellow;
            }
            Debug.Log(result);
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
            foreach (var item in n.neighbours)
            {
                result += item.x.ToString() + "," + item.y.ToString() + " ";
                //Gizmos.color = Color.yellow;
            }
            Debug.Log(result);
        }
    }

    public void SetStartNode()
    {
        RaycastHit hit;
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mousePos, out hit))
        {
            Node hitNode = grid.GetGridPosFromWorldPos(hit.point);
            if (hitNode.walkable && hitNode != grid.startNode && hitNode != grid.endNode)
            {
                grid.startNode = grid.GetGridPosFromWorldPos(hit.point);
                grid.UpdateTiles(); 
                Debug.Log("Start Node: " + grid.startNode.x.ToString() + "," + grid.startNode.y.ToString());
                Debug.DrawRay(mousePos.origin, mousePos.direction * 1000, Color.red, 10);
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
        if (Physics.Raycast(mousePos, out hit))
        {
            Node hitNode = grid.GetGridPosFromWorldPos(hit.point);
            if (hitNode.walkable && hitNode != grid.endNode && hitNode != grid.startNode)
            {
                grid.endNode = grid.GetGridPosFromWorldPos(hit.point);
                grid.UpdateTiles();
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
}
