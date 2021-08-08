using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public WorldController worldController;
    private int x;
    private int y;
    private int z;

    void Awake()
    {
        x = 8;
        y = -1;
        z = 8;
    }

    void Update()
    {
        HandleMovementInput();
    }

    void HandleMovementInput()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow) && worldController.CanMoveInDirection(x, y, z, 1, 0, 0)) MovePlayer(1, 0, 0);
        else if(Input.GetKeyDown(KeyCode.LeftArrow) && worldController.CanMoveInDirection(x, y, z, -1, 0, 0)) MovePlayer(-1, 0, 0);
        else if(Input.GetKeyDown(KeyCode.UpArrow) && worldController.CanMoveInDirection(x, y, z, 0, 0, 1)) MovePlayer(0, 0, 1);
        else if(Input.GetKeyDown(KeyCode.DownArrow) && worldController.CanMoveInDirection(x, y, z, 0, 0, -1)) MovePlayer(0, 0, -1);
        else if(Input.GetKeyDown(KeyCode.Space) && worldController.CanMoveInDirection(x, y, z, 0, 1, 0)) MovePlayer(0, 1, 0);
        else if(Input.GetKeyDown(KeyCode.C) && worldController.CanMoveInDirection(x, y, z, 0, -1, 0)) MovePlayer(0, -1, 0);
    }

    void MovePlayer(int x_offset, int y_offset, int z_offset)
    {
        RotatePlayerInCorrectDirection(x_offset, y_offset, z_offset);
        ChangePlayerPosition(x_offset, y_offset, z_offset);
    }

    void RotatePlayerInCorrectDirection(int x_offset, int y_offset, int z_offset)
    {
        if(x_offset == 1) this.gameObject.transform.eulerAngles = new Vector3(0f, 90f, 0f);
        else if(x_offset == -1) this.gameObject.transform.eulerAngles = new Vector3(0f, -90f, 0f);
        else if(y_offset == 1) this.gameObject.transform.eulerAngles = new Vector3(90f, 0f, 0f);
        else if(y_offset == -1) this.gameObject.transform.eulerAngles = new Vector3(-90f, 0f, 0f);
        else if(z_offset == 1) this.gameObject.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        else if(z_offset == -1) this.gameObject.transform.eulerAngles = new Vector3(0f, 180f, 0f);
    }

    void ChangePlayerPosition(int x_offset, int y_offset, int z_offset)
    {
        transform.localPosition = new Vector3(transform.localPosition.x + x_offset, transform.localPosition.y - y_offset, transform.localPosition.z + z_offset);
        UpdateOffsetToPosition(x_offset, y_offset, z_offset);
        Debug.Log(x + " " + " " + y + " " + z);
    }

    void UpdateOffsetToPosition(int x_offset, int y_offset, int z_offset)
    {
        x += x_offset;
        y += y_offset;
        z += z_offset;
    }
}
