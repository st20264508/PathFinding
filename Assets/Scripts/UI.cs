using Unity.VisualScripting;
using UnityEngine;

public class UI : MonoBehaviour
{
    Camera cam;
    public Grid grid;
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
            if (grid.GetGridPosFromWorldPos(hit.point).walkable)
            {
                grid.startNode = grid.GetGridPosFromWorldPos(hit.point);
                Debug.Log("Start Node: " + grid.startNode.x.ToString() + "," + grid.startNode.y.ToString());
            }
            else
            {
                Debug.Log("Start node has to be walkable");
            }
        }
    }

    public void SetEndNode()
    {
        RaycastHit hit;
        Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mousePos, out hit))
        {
            if (grid.GetGridPosFromWorldPos(hit.point).walkable)
            {
                grid.endNode = grid.GetGridPosFromWorldPos(hit.point);
                Debug.Log("End Node: " + grid.endNode.x.ToString() + "," + grid.endNode.y.ToString());
            }
            else
            {
                Debug.Log("End node has to be walkable");
            }
        }
    }
}
