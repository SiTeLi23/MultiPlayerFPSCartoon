using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform viewPoint;
    public float mouseSensitivity=1f;

    //rotation limitation of the mouse view point
    private float verticalRotStore;
    private Vector2 mouseInput;
    public bool invertLook;


    //movement
    public float moveSpeed = 5f;
    private Vector3 moveDir,movement;


    public CharacterController cc;
   

    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cc = GetComponent<CharacterController>();
    }

    
    void Update()
    {
        #region mouse view rotation
        //get the mouse input
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"),Input.GetAxisRaw("Mouse Y"))*mouseSensitivity;


        //playerMovement
        //euler means we can transform quaternion to x,y,z version value
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,transform.rotation.eulerAngles.y+mouseInput.x,transform.rotation.eulerAngles.z);

        //cause Euler angle only have 0-360 , we need this value to be used directly in the viewpoint's rotation angle, otherwise it have bugs when angle turn to minus when using eulerangle.x directly
        verticalRotStore +=  mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);

        //clamp the up side down rotation degree
        if (invertLook) 
        { 
          viewPoint.rotation = Quaternion.Euler(verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }
        else 
        {
            viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        }
        #endregion


        #region Movement

        //get the keyboard input
        moveDir = new Vector3(Input.GetAxis("Horizontal"),0f,Input.GetAxis("Vertical"));
        //so the moveposition will awlays according to z axis which means forward,normalized will maintain the the whole value so player won't move faster in diagonaled
        movement = ((transform.forward * moveDir.z)+(transform.right*moveDir.x)).normalized;

        cc.Move( movement * moveSpeed*Time.deltaTime);



        #endregion


    }
}
