using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{

    [Header("MovementSettings")]
    public Transform viewPoint;
    public float mouseSensitivity=1f;

    //rotation limitation of the mouse view point
    private float verticalRotStore;
    private Vector2 mouseInput;
    public bool invertLook;

    
    //movement
    public float moveSpeed = 5f,runSpeed =8f;
    public float jumpForce = 12f, gravityMod = 2.5f;
    private float activeMoveSpeed;
    private Vector3 moveDir,movement;
    public Transform groundCheckPoint;
    private bool isGrounded;
    public LayerMask groundLayers;

    [Header("Shooting")]
    //Shooting
    public GameObject bulletImpact;
    //public float timeBetweenShots =.1f;
    private float shotCounter;
    public float maxHeat = 10f,/*heatPerShot =1f,*/coolRate = 4f,overheatCoolRate =5f;
    private float heatCounter;
    private bool overHeated;
    public float muzzleDisplayTime;
    private float muzzleCounter;


    //components attached
    public CharacterController cc;
    private Camera cam;

    //gun switching
    public Gun[] allGuns;
    private int selectedGun;

    
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cc = GetComponent<CharacterController>();
        cam = Camera.main;
        UIController.instance.weaponTempSlider.maxValue = maxHeat;
        SwitchGun();
    }

    
    void Update()
    {
        //cursor lock/unlock mode
        if (Input.GetKeyDown(KeyCode.Escape))
        {

            Cursor.lockState = CursorLockMode.None;
        }
        else if (Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0)) 
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

           
        }


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

        //running or not
        if (Input.GetKey(KeyCode.LeftShift)) 
        {
            activeMoveSpeed = runSpeed;
        }
        else 
        {
            activeMoveSpeed = moveSpeed;
        }


        //stored the y axis value for gravity to use so it won't setback to 0 every frame
        float yVel = movement.y;


        //movement based on Z axis(forward) and X axis(right),normalized will maintain the the whole value so player won't move faster in diagonaled
        movement = ((transform.forward * moveDir.z)+(transform.right*moveDir.x)).normalized * activeMoveSpeed;//we don't want y axis to be influenced by activeMoveSpeed, so we put activeMoveSpeed here only for horinzontal movement
        movement.y = yVel;

        if (cc.isGrounded) 
        {
            //if character is on the ground, the y axis value woule be set back to 0 so it won't keep increasing infinitely
            movement.y = 0f;
        }

        //jumping
        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, .25f, groundLayers);

        if (Input.GetButtonDown("Jump")&&isGrounded) 
        {
            movement.y = jumpForce ;
        }

        //apply gravity

        movement.y += Physics.gravity.y * Time.deltaTime*gravityMod;

        // character movement final
        cc.Move( movement *Time.deltaTime);



        #endregion


        #region Shooting&&OverHeat System
        //shooting system && overheat system
        if (allGuns[selectedGun].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter -= Time.deltaTime;
            if (muzzleCounter <= 0)
            {
                allGuns[selectedGun].muzzleFlash.SetActive(false);//muzzle flash fx
            }
        }

        if (heatCounter <= 0)
        {
            heatCounter = 0;
        }

        if (!overHeated)
        {
            //if only pressed one time
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();

            }

            //if keep pressing ,automatic shooting
            if (Input.GetMouseButton(0)&&allGuns[selectedGun].isAutomatic)
            {
                shotCounter -= Time.deltaTime;
                if (shotCounter <= 0)
                {
                    Shoot();
                }

            }

           //cool down everysecond if not shooting
           heatCounter -= coolRate * Time.deltaTime;

        }
           //if overheated
        else 
        {
            
            Debug.Log("OverHeated,Can't Shoot");
            heatCounter -= overheatCoolRate * Time.deltaTime;
            if (heatCounter <= 0) 
            {
              
               overHeated = false;
                UIController.instance.OverHeatedMessage.gameObject.SetActive(false);

            }
        
        }

        UIController.instance.weaponTempSlider.value = heatCounter;

        #endregion


        //Gun Switching,using scroll control
        if(Input.GetAxisRaw("Mouse ScrollWheel") > 0f) 
        {

            selectedGun ++;
            if(selectedGun>= allGuns.Length) 
            {
                //return to first gun within the array
                selectedGun = 0;
            }
            SwitchGun();
        
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f) 
        {
            selectedGun--;
            if (selectedGun < 0) 
            {
                //return to last gun within the array.
                selectedGun = allGuns.Length - 1;
            }
            SwitchGun();
        }





    }


    private void LateUpdate()
    {
        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }


    private void Shoot() 
    {
        //crate a ray from the viewpoint
        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, 0f));

        ray.origin = cam.transform.position;

        // if the ray we crated before has hit something
        if(Physics.Raycast(ray,out RaycastHit hit)) 
        {
            Debug.Log("I hit the " + hit.collider.gameObject.name);
            //the bullet impact's rotation should showed on the faces being hitted
            //hit.point+(hit.normal*.002f) means the impact effect will appeared a slightly away from the exact same place of the hitted surface
           GameObject bulletImpactObject= Instantiate(bulletImpact, hit.point+(hit.normal*.002f), Quaternion.LookRotation(hit.normal,Vector3.up));
            Destroy(bulletImpactObject, 10f);
        }

        shotCounter = allGuns[selectedGun].timeBetweenShots;


        //Overheat system
        heatCounter += allGuns[selectedGun].heatPerShot;
        if (heatCounter >= maxHeat) 
        {
            heatCounter = maxHeat;
            overHeated = true;
            UIController.instance.OverHeatedMessage.gameObject.SetActive(true);
           
        }

        //muzzle flash fx
        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
    }

    public void SwitchGun() 
    {
       foreach(Gun gun in allGuns) 
        {
            gun.gameObject.SetActive(false);

        }

       allGuns[selectedGun].gameObject.SetActive(true);
        allGuns[selectedGun].muzzleFlash.SetActive(false);
    
    }



}
