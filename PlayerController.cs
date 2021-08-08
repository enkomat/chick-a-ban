using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Camera mainCamera;
    public WorldController worldController;
    private int x;
    private int y;
    private int z;
    //camera follow variables:
    private Vector3 cameraOffset;
    private float cameraSmoothTime = 0.3F;
    private Vector3 cameraVelocity = Vector3.zero;

    void Awake()
    {
        InitializeVariables();
    }

    void Update()
    {
        CameraFollow();
        HandleMovementInput();
    }

    void InitializeVariables()
    {
        cameraOffset = mainCamera.transform.position;
        x = 8; y = -1; z = 8;
    }

    void CameraFollow()
    {
        Vector3 cameraTargetPosition = this.transform.position + cameraOffset;
        mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, cameraTargetPosition, ref cameraVelocity, cameraSmoothTime);
    }

    void HandleMovementInput()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow)) MovePlayer(1, 0, 0);
        else if(Input.GetKeyDown(KeyCode.LeftArrow)) MovePlayer(-1, 0, 0);
        else if(Input.GetKeyDown(KeyCode.UpArrow)) MovePlayer(0, 0, 1);
        else if(Input.GetKeyDown(KeyCode.DownArrow)) MovePlayer(0, 0, -1);
        else if(Input.GetKeyDown(KeyCode.Space)) MovePlayer(0, 1, 0); //dig down a layer
        else if(Input.GetKeyDown(KeyCode.C)) MovePlayer(0, -1, 0); //move up a layer
    }

    void MovePlayer(int x_offset, int y_offset, int z_offset)
    {
        if(worldController.CanMoveInDirection(x, y, z, x_offset, y_offset, z_offset))
        {
            RotatePlayerInCorrectDirection(x_offset, y_offset, z_offset);
            ChangePlayerPosition(x_offset, y_offset, z_offset);
        }
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
    }

    void UpdateOffsetToPosition(int x_offset, int y_offset, int z_offset)
    {
        x += x_offset;
        y += y_offset;
        z += z_offset;
    }
}
