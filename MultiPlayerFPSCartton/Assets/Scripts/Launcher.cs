using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{

    public static Launcher instance;

    private void Awake()
    {
        instance = this;
    }

    [Header("Menu UI")]
    public GameObject loadingScreen;
    public TMP_Text loadingText;

    public GameObject menuButtons;

    public GameObject createRoomScreen;
    public TMP_InputField roomNameInput;

    public GameObject roomScreen;
    public TMP_Text roomNameText,playerNameLabel;
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();

    public GameObject errorScreen;
    public TMP_Text errorText;


    public GameObject roomBrowserScreen;
    public RoomButton theRoomButton;
    [SerializeField]List<RoomButton> allRoomButtons = new List<RoomButton>();
    private Dictionary<string, RoomInfo> cachedRoomsList = new Dictionary<string, RoomInfo>();

    public GameObject nameInputScreen;
    public GameObject roomTestButton;
    public TMP_InputField nameInput;
    public static bool hasSetNick;

    public string levelToPlay;
    public GameObject startButton;


    public string[] allMaps;
    public bool changeMapBetweenRounds = true;



    // Start is called before the first frame update
    void Start()
    {
        CloseMenus();

        loadingScreen.SetActive(true);
        loadingText.text = "Connecting To Network....";

        //First thing is using pre-setted photon server setting to connect to the photon newtork
        PhotonNetwork.ConnectUsingSettings();

        //if we are in unity editor
#if UNITY_EDITOR
        roomTestButton.SetActive(true);
#endif

        //make cursor able to see again when they back to main menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

    //this method used to close all menus
    void CloseMenus() 
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
        nameInputScreen.SetActive(false);
    }

    // Secondly,once the connection to the server successed, it will call this methoed
    public override void OnConnectedToMaster()
    {

        //start to join a new lobby, which will for player waiting inside and decied which room to go next
        PhotonNetwork.JoinLobby();

        //this will allow the photon network to be able to tell us which scene we should be going to
        PhotonNetwork.AutomaticallySyncScene = true;

        loadingText.text = "Joining Lobby";
    }

    //thirdly,when we successfully joined a lobby, it will automatically call this method
    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);

        //assigning name to joined player
        PhotonNetwork.NickName = Random.Range(0,1000).ToString();

        if (!hasSetNick) 
        {
            CloseMenus();
            nameInputScreen.SetActive(true);

            //if we already set up the player name before, then set the nick name to last input name
            if (PlayerPrefs.HasKey("playerName")) 
            {
                nameInput.text = PlayerPrefs.GetString("playerName");
            
            }

        }
        else 
        {
            
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
        
    }




    //Button Functions for crating room menu
    public void OpenRoomCreate() 
    {
        CloseMenus();
        createRoomScreen.SetActive(true);
    
    }

    public void CreateRoom() 
    {

        if (!string.IsNullOrEmpty(roomNameInput.text)) //if the room name input field is not null or empty
        {
            //setting options for the maxminum player for the room
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;


            //passing the options we have set for the room and crate a room , whose name is based on inputed name
            PhotonNetwork.CreateRoom(roomNameInput.text,options);

            CloseMenus();
            loadingText.text = "Creating Room...";
            loadingScreen.SetActive(true);
            
        }


    }

    //forthly, this method will be called when we sucessfully joined a room
    #region Room Joining system
    public override void OnJoinedRoom()
    {
        CloseMenus();
        roomScreen.SetActive(true);

        //get the current connected room's name
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        ListAllPlayer();


        //check if we are the master Client of the game 
        if (PhotonNetwork.IsMasterClient) 
        {
            startButton.SetActive(true);
        }
        else 
        {
            startButton.SetActive(false);
        }
    }

    private void ListAllPlayer()
    {
        //clear previously information 
        foreach (TMP_Text player in allPlayerNames)
        {
            Destroy(player.gameObject);


        }
        allPlayerNames.Clear();

        //get the player information from photon player list
        Player[] players = PhotonNetwork.PlayerList;

        //show all the player names within the name list
        for (int i = 0; i < players.Length; i++)
        {
            //creat an instance based on reference for each player name
            TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);

            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);

            allPlayerNames.Add(newPlayerLabel);

        }

    }

    //update player list whenever a player enter the room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);

        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);

        allPlayerNames.Add(newPlayerLabel);
    }

    //update player list whenever a player leave the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //when someOne leave the room, just re-list all the current player
        ListAllPlayer();
    }


    #endregion



    //if we failed to create room
    public override void OnCreateRoomFailed(short returnCode, string message)
    {

        errorText.text = ("Failed to Crate Room : " + message);
        CloseMenus();
        errorScreen.SetActive(true);
       
    }



    public void CloseErrorScreen() 
    {
        CloseMenus();

        menuButtons.SetActive(true);
    
    }

    //leaving current room 
    public void LeaveRoom() 
    {
        PhotonNetwork.LeaveRoom();      
        CloseMenus();
        loadingText.text = "Leaving Room";
        loadingScreen.SetActive(true);
    
    }


    //once we leaved a room,it will call this function
    public override void OnLeftRoom()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }


    public void OpenRoomBrowser() 
    {
        CloseMenus();
        roomBrowserScreen.SetActive(true);
        

    }


    public void CloseRoomBrowser() 
    {
        CloseMenus();
        menuButtons.SetActive(true);
        
    
    }



    #region Room List Browser

    /*//this method will be called everytime when the list of the rooms updated while we are in the lobby
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //clean all the button gameobject and reference first
        foreach(RoomButton rb in allRoomButtons) 
        {
            Destroy(rb.gameObject);
        
        }
        //clean the list then so we have a completely empty list for later update 
        allRoomButtons.Clear();

        //we are only using theRoomButton as a reference to be instantiated as new button , so we need to make sure we never gonna use the original one
        theRoomButton.gameObject.SetActive(false);
        for(int i = 0; i < roomList.Count; i++) 
        {
            //as long as this room is not reach the maxmimun player number limitation  and  the room is not empty while being marked as removed
            if (roomList[i].PlayerCount != roomList[i].MaxPlayers && !roomList[i].RemovedFromList) 
            {
                //then we can crate a new roomButton and make is within the same parent
                RoomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
                //set the room information for the newly created button
                newButton.SetButtonDetails(roomList[i]);
                newButton.gameObject.SetActive(true);

                //also add the newly crated button into the list
                allRoomButtons.Add(newButton);

            
            }
        
        }

    }*/

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
    }

    public void UpdateCachedRoomList(List<RoomInfo> roomList) 
    {
       for(int i=0; i < roomList.Count; i++) 
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList) 
            {
                cachedRoomsList.Remove(info.Name);
            }
            else 
            {

                cachedRoomsList[info.Name] = info;
            }

        
        }

       
        RoomListButtonUpdate(cachedRoomsList);
    
    }


    void RoomListButtonUpdate(Dictionary<string,RoomInfo> cachedRoomList) 
    {
        //clean all the button gameobject and reference first
        foreach (RoomButton rb in allRoomButtons)
        {
            Destroy(rb.gameObject);

        }
        //clean the list then so we have a completely empty list for later update 
        allRoomButtons.Clear();
        //we are only using theRoomButton as a reference to be instantiated as new button , so we need to make sure we never gonna use the original one
        theRoomButton.gameObject.SetActive(false);

        foreach(KeyValuePair<string,RoomInfo> roomInfo in cachedRoomList) 
        {
            //then we can crate a new roomButton and make is within the same parent
            RoomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
            newButton.SetButtonDetails(roomInfo.Value);
            newButton.gameObject.SetActive(true);
            allRoomButtons.Add(newButton);

        
        }



    }




    #endregion


    //connect to a room
    public void JoinRoom(RoomInfo inputInfo) 
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);

        CloseMenus();
        loadingText.text = "Joining...";
        loadingScreen.SetActive(true);
    
    }


    public void SetNickName() 
    {
        if (!string.IsNullOrEmpty(nameInput.text)) 
        {

            PhotonNetwork.NickName = nameInput.text;

            PlayerPrefs.SetString("playerName", nameInput.text);

            CloseMenus();
            menuButtons.SetActive(true);
            hasSetNick = true;
        }
       
    }



    //start game

    public void StartGame() 
    {
        //this will tell the server to let all the other player load into the same level together with the host
        //PhotonNetwork.LoadLevel(levelToPlay);
        PhotonNetwork.LoadLevel(allMaps[Random.Range(0, allMaps.Length)]);

    }

    //this method will be called when the master client leave the game, and then we need to switch the master client to other player
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        //check if we are the master Client of the game 
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }


    public void QuickJoin() 
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;


        PhotonNetwork.CreateRoom("Test",options);
        CloseMenus();
        loadingText.text = "Creating Room";
        loadingScreen.SetActive(true);
    
    }



    //quit game
    public void QuitGame() 
    {

        Application.Quit();
    }

    

}
