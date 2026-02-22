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
            DebugNeighbours();
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
}
