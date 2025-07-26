using UnityEngine;

public class ViewMover : MonoBehaviour
{
    public float moveSpeed = 20f;
    public float screenEdge = 10f;
    public Vector2 limitXY;
    public Vector2 zoomRange;
    public float zoomPower = 20f;

    private void Update()
    {
        Vector3 move = transform.position;

        if (Input.GetKey(KeyCode.W) || Input.mousePosition.y >= Screen.height - screenEdge) move.z += moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S) || Input.mousePosition.y <= screenEdge) move.z -= moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D) || Input.mousePosition.x >= Screen.width - screenEdge) move.x += moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.A) || Input.mousePosition.x <= screenEdge) move.x -= moveSpeed * Time.deltaTime;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        move.y -= scroll * zoomPower * 100f * Time.deltaTime;

        move.x = Mathf.Clamp(move.x, -limitXY.x, limitXY.x);
        move.z = Mathf.Clamp(move.z, -limitXY.y, limitXY.y);
        move.y = Mathf.Clamp(move.y, zoomRange.x, zoomRange.y);

        transform.position = move;
    }
}