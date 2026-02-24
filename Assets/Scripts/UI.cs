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

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            RunBFSCROSS(); 
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RunBFSALL(); 
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetUnwalkable();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            SetWalkable();
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
        if (Physics.Raycast(mousePos, out hit))
        {
            Node hitNode = grid.GetGridPosFromWorldPos(hit.point);
            if (hitNode.walkable)
            {
                Debug.Log("Already Walkable");
            }
            else
            {
                grid.TileList.Remove(hit.transform.gameObject);
                Destroy(hit.transform.gameObject);
                grid.PlaceTile(grid.tilePrefab, hitNode.x, hitNode.y);
                grid.GetNode(hitNode.x, hitNode.y).walkable = true;
            }
        }
    }

    public void SetUnwalkable()
    {
        RaycastHit hit;
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mousePos, out hit))
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
            }
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
                int oldX = 0;
                int oldY = 0;

                if (grid.startNode != null)
                {
                    oldX = grid.startNode.x;
                    oldY = grid.startNode.y;
                }
                Debug.Log(oldX + " " + oldY);

                grid.TileList.Remove(hit.transform.gameObject);
                Destroy(hit.transform.gameObject);
                grid.PlaceTile(grid.startPrefab, hitNode.x, hitNode.y);
                //grid.GetNode(hitNode.x, hitNode.y).walkable = false;

                grid.startNode = grid.GetGridPosFromWorldPos(hit.point);

                
                grid.TileList.Remove(grid.GetNode(oldX, oldY).prefab);
                Destroy(grid.GetNode(oldX, oldY).prefab); 
                grid.PlaceTile(grid.tilePrefab, oldX, oldY);
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
        if (Physics.Raycast(mousePos, out hit))
        {
            Node hitNode = grid.GetGridPosFromWorldPos(hit.point);
            if (hitNode.walkable && hitNode != grid.endNode && hitNode != grid.startNode)
            {
                //grid.endNode = grid.GetGridPosFromWorldPos(hit.point);
                //grid.UpdateTiles();

                int oldX = 0;
                int oldY = 0;

                if (grid.endNode != null)
                {
                    oldX = grid.endNode.x;
                    oldY = grid.endNode.y;
                }
                Debug.Log(oldX + " " + oldY);

                grid.TileList.Remove(hit.transform.gameObject);
                Destroy(hit.transform.gameObject);
                grid.PlaceTile(grid.endPrefab, hitNode.x, hitNode.y);
                //grid.GetNode(hitNode.x, hitNode.y).walkable = false;

                grid.endNode = grid.GetGridPosFromWorldPos(hit.point);


                grid.TileList.Remove(grid.GetNode(oldX, oldY).prefab);
                Destroy(grid.GetNode(oldX, oldY).prefab);
                grid.PlaceTile(grid.tilePrefab, oldX, oldY);
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
                grid.UpdateTiles();
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
                grid.UpdateTiles();
            }
        }
        
    }
}
