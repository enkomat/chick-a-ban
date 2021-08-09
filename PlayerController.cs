using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private WorldController worldController;
    
    private int x;
    private int y;
    private int z;
    //camera follow variables:
    private Vector3 cameraOffset;
    private float cameraSmoothTime = 0.3F;
    private Vector3 cameraVelocity = Vector3.zero;
    

    private void Awake()
    {
        InitializeVariables();
    }

    private void Update()
    {
        CameraFollow();
        HandleMovementInput();
    }

    private void InitializeVariables()
    {
        cameraOffset = mainCamera.transform.position;
        x = Mathf.RoundToInt(this.transform.position.x); 
        y = -1; 
        z = Mathf.RoundToInt(this.transform.position.z);
    }

    private void CameraFollow()
    {
        Vector3 cameraTargetPosition = this.transform.position + cameraOffset;
        mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, cameraTargetPosition, ref cameraVelocity, cameraSmoothTime);
    }

    private void HandleMovementInput()
    {
        if(Input.GetKeyDown(KeyCode.RightArrow)) MovePlayer(1, 0, 0);
        else if(Input.GetKeyDown(KeyCode.LeftArrow)) MovePlayer(-1, 0, 0);
        else if(Input.GetKeyDown(KeyCode.UpArrow)) MovePlayer(0, 0, 1);
        else if(Input.GetKeyDown(KeyCode.DownArrow)) MovePlayer(0, 0, -1);
        else if(Input.GetKeyDown(KeyCode.S)) MovePlayer(0, 1, 0); //dig down a layer
        else if(Input.GetKeyDown(KeyCode.W)) MovePlayer(0, -1, 0); //move up a layer
    }

    private void MovePlayer(int xOffset, int yOffset, int zOffset)
    {
        if(worldController.CanMoveInDirection(x, y, z, xOffset, yOffset, zOffset))
        {
            RotatePlayerInCorrectDirection(xOffset, yOffset, zOffset);
            ChangePlayerPosition(xOffset, yOffset, zOffset);
        }
    }

    private void RotatePlayerInCorrectDirection(int xOffset, int yOffset, int zOffset)
    {
        if(xOffset == 1) this.gameObject.transform.eulerAngles = new Vector3(0f, 90f, 0f);
        else if(xOffset == -1) this.gameObject.transform.eulerAngles = new Vector3(0f, -90f, 0f);
        else if(yOffset == 1) this.gameObject.transform.eulerAngles = new Vector3(90f, 0f, 0f);
        else if(yOffset == -1) this.gameObject.transform.eulerAngles = new Vector3(-90f, 0f, 0f);
        else if(zOffset == 1) this.gameObject.transform.eulerAngles = new Vector3(0f, 0f, 0f);
        else if(zOffset == -1) this.gameObject.transform.eulerAngles = new Vector3(0f, 180f, 0f);
    }

    private void ChangePlayerPosition(int xOffset, int yOffset, int zOffset)
    {
        transform.localPosition = new Vector3(transform.localPosition.x + xOffset, transform.localPosition.y - yOffset, transform.localPosition.z + zOffset);
        UpdateOffsetToPosition(xOffset, yOffset, zOffset);
    }

    private void UpdateOffsetToPosition(int xOffset, int yOffset, int zOffset)
    {
        x += xOffset; y += yOffset; z += zOffset;
    }
}
