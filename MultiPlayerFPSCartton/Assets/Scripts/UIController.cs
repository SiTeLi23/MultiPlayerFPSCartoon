using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
public class UIController : MonoBehaviour
{
    public static UIController instance;
    public Slider weaponTempSlider;

    private void Awake()
    {
        instance = this;
    }




    public TMP_Text OverHeatedMessage;


    public GameObject deathScreen;
    public TMP_Text deathText;
    public Slider healthSlider;



    public TMP_Text killsText,deathsText;

    public GameObject leaderboard;
    public LeaderboardPlayer leaderboardPlayerDisplay;

    public GameObject EndScreen;

    //timer
    public TMP_Text timerText;

    //option screen
    public GameObject optionsScreen;
    

    void Start()
    {
        
    }

   

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            ShowHideOption();
        
        }

        //makesure when option menu pop out , we can see the cursor
        if (optionsScreen.activeInHierarchy && Cursor.lockState!=CursorLockMode.None) 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        
        }

    }


    //option menu button function
    public void ShowHideOption() 
    {
        if (!optionsScreen.activeInHierarchy) 
        {
            optionsScreen.SetActive(true);
        }
        else 
        {
            optionsScreen.SetActive(false);
        }
    
    }

    public void ReturnToMainMenu() 
    {
        //make sure when we leave the room, not syning other people to leavw together
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void QuitGame() 
    {
        Application.Quit();
    }

}
