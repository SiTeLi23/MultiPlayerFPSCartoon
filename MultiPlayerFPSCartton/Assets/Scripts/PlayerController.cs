using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform viewPoint;
    public float mouseSensitivity=1f;

    //rotation limitation of the view point
    private float verticalRotStore;
    private Vector2 mouseInput;
    
    void Start()
    {
        
    }

    
    void Update()
    {
        //get the mouse input
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"),Input.GetAxisRaw("Mouse Y"))*mouseSensitivity;


        //playerMovement
        //euler means we can transform quaternion to x,y,z version value
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,transform.rotation.eulerAngles.y+mouseInput.x,transform.rotation.eulerAngles.z);


        verticalRotStore -=  mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);

        //clamp the up side down rotation degree
        viewPoint.rotation = Quaternion.Euler(verticalRotStore, viewPoint.rotation.eulerAngles.y, viewPoint.rotation.eulerAngles.z);
        
    }
}
