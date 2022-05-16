using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManager : MonoBehaviourPunCallbacks,IOnEventCallback
{

    public static MatchManager instance;
    private void Awake()
    {
        instance = this;
    }


    //decide what kind of message are we sending when using photon
    public enum EventCodes : byte
    {
       NewPlayer,
       ListPlayers,
       UpdateStat,
       NextMatch
    }



    //plaer info list
    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    //help us to keep track player position in the list
    private int index;


    //leaderboard list
    public List<LeaderboardPlayer> lboardPlayers = new List<LeaderboardPlayer>();

    //game state
      public enum GameState 
    {
       Waiting,
       Playing,
       Ending
      

    }

    public int killsToWin = 3;
    public Transform mapCamPoint;
    public GameState state = GameState.Waiting;
    public float waitAfterEnding = 5f;

    //wheather the game gonna continue
    public bool perertual;


    void Start()
    {
        //if lost connected,back to maim menu
        if (!PhotonNetwork.IsConnected) 
        {
            SceneManager.LoadScene(0);
        }
        else 
        {
            //send playerEvent when joined the game
            NewPlayerSend(PhotonNetwork.NickName);

            //if the match manager working correctly and a player has joined
            state = GameState.Playing;
        
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)&&state!=GameState.Ending) 
        {
            if (UIController.instance.leaderboard.activeInHierarchy) 
            {
                UIController.instance.leaderboard.SetActive(false);
            
            }

            else 
            {
                ShowLeaderboard();
            
            }
        
        }
        
    }



    #region Setting Up Event System
    //whenever a event happened or some message being sent, this function will be called and read those events
    public void OnEvent(EventData photonEvent) 
    {
        //code above 200 are those built-int photon function , so we only want to listen to our custom event which is below 200
        if (photonEvent.Code < 200) 
        {
            //if the event are sent by us, we want to convert the photon eventcode into our custom event code system,
            //so we can synchronize our own custome eventcodes with photon's built-in eventcode 
            EventCodes theEvent = (EventCodes)photonEvent.Code;

            //whatever the data we received from the event, we will convert it into an array of data that we can use and access
            object[] data = (object[])photonEvent.CustomData;

          


            //use switch system to decided which kind of eventcode should trigger related functions below
            switch (theEvent) 
            {
                //if the event is a newplayer event,we want to call the newplayerreceive function
                case EventCodes.NewPlayer:

                    NewPlayerReceive(data);
                    break;

                case EventCodes.ListPlayers:
                    ListPlayersReceive(data);
                    break;

                case EventCodes.UpdateStat:
                    UpdateStatsReceive(data);
                    break;

                case EventCodes.NextMatch:
                    NextMatchReceive();
                    break;


            
            
            }

        }

    
    }

    //whenever gameobject being enable of disable ,those functions will be called
    public override void OnEnable()
    {
        //we want to add this match manager into a list when it enabled
        //, which whenever a event happend,it will listen to it,which mean upper OnEvent() will be abled to be called
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        //we need to remove this match manger from the list when it disable,
        //if we don't do that,it will cause error when some events happened but without listener to listen to those event
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    #endregion


    #region skeleton of the eventHandler
    public void NewPlayerSend(string username) 
    {
        //package goonna be whatever information we want to send
        object[] package = new object[4];

        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;// whatever number has been assigned to this local player on the network
        package[2] = 0;  //nunber of kills
        package[3] = 0;  //number of deaths

        //sendout the event that we have packed before
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer, //we need to convert our own eventcode back into byte after synchornize it in OnEvent() so photon understand what it is
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, //make sure only the master client can receive a new player information
            new SendOptions { Reliability = true } //tell the server that this is something we want it to be reliable so it will be definitely  able to be sent to the network and all players later
            ); 
    
    }

    public void NewPlayerReceive(object[] dataReceived) 
    {
        //we need to convert all the data(value/variable) we received from type of object into value type that PlayerInfo require.
        PlayerInfo player = new PlayerInfo((string)dataReceived[0], (int)dataReceived[1], (int)dataReceived[2], (int)dataReceived[3]);
        allPlayers.Add(player);

        //as soon as we receive one new player, we update everybody else about the current player list information
        ListPlayersSend();
    }

    public void ListPlayersSend()
    {
        //pacakge allplayers list and send to all clients 

        object[] package = new object[allPlayers.Count+1];

        package[0] = state;

        for(int i =0; i < allPlayers.Count; i++) 
        {
            //create pieces information whish will stored each players' information within the list
            //stroing an array into an array
            object[] piece = new object[4];

            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i + 1] = piece; //cause package[0] is used to store game state, so all the other piece information store place move to the next
        }

        //sendout the event that we have packed before
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayers, //we need to convert our own eventcode back into byte after synchornize it in OnEvent() so photon understand what it is
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All }, //everybody going to receive the list information
            new SendOptions { Reliability = true } //tell the server that this is something we want it to be reliable so it will be definitely  able to be sent to the network and all players later
            );

    }

    public void ListPlayersReceive(object[] dataReceived)
    {
        //we want to clear the previous information evertime when we updated the list
        allPlayers.Clear();

        //we want to convert the byte information into gameState first , then received the game state
        state = (GameState)dataReceived[0];

        //we want to start receiving player inforamtion from 1 because 0 stored the game state information already
        for(int i = 1; i < dataReceived.Length; i++) 
        { 
            //pull out the piece data we received

            object[] piece = ((object[])dataReceived[i]);

            //and transfer those pulled out piece information to a new plaeryInfo 
            //that's how we decrepte the piece information and collect them to use
            PlayerInfo player = new PlayerInfo((string)piece[0], (int)piece[1], (int)piece[2], (int)piece[3]);


            allPlayers.Add(player);

            //check if our local actor number is qeual to the player actor number in the list
            if(PhotonNetwork.LocalPlayer.ActorNumber == player.actor) 
            {
                //assigned the actor number that being added to the list , so we can have a shortcut reference later for ourselves
                index = i - 1;  //the reason we can't use index = 1 is because now our player information start at 1 but not 0 when 0 means game state information
            }
        }

        //check the game state whenever we update our list throught network
        StateCheck();

    }

    public void UpdateStatsSend(int actorSending,int statToUpdate,int amountToChange)
    {
        //package all the inputed information first
        object[] package = new object[] { actorSending, statToUpdate, amountToChange }; // = new object[3]

        //sendout the event that we have packed before
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdateStat, //we need to convert our own eventcode back into byte after synchornize it in OnEvent() so photon understand what it is
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All }, //make sure only the master client can receive a new player information
            new SendOptions { Reliability = true } //tell the server that this is something we want it to be reliable so it will be definitely  able to be sent to the network and all players later
            );


    }

    public void UpdateStatsReceive(object[] dataReceived)
    {

        int actor = (int)dataReceived[0];
        int statType = (int)dataReceived[1];
        int amount = (int)dataReceived[2];

        for(int i = 0; i < allPlayers.Count; i++) 
        {
            //find the right person
            if (allPlayers[i].actor == actor) 
            {
                switch (statType) 
                {
                    //kills
                    case 0:
                        allPlayers[i].kills += amount;
                        Debug.Log("Player " + allPlayers[i].name + "  : kills" + allPlayers[i].kills);
                        break;
                    //deaths
                    case 1:
                        allPlayers[i].deaths += amount;
                        Debug.Log("Player " + allPlayers[i].name + "  : deaths" + allPlayers[i].deaths);
                        break;

                }

                //if we are the one that having a value changed
                if (i == index) 
                {
                    UpdateStatsDisplay();
                }

                //update leaderboard when leaderboard is opened
                if (UIController.instance.leaderboard.activeInHierarchy) 
                {
                    ShowLeaderboard();
                }


                break; //once we find the correct one, stop looping around
            }
        }

        ScoreCheck();//Check if we should end the game

    }

    //UI update
    public void UpdateStatsDisplay() 
    {
        //double safety control
        if (allPlayers.Count > index)
        {
            UIController.instance.killsText.text = "Kills: " + allPlayers[index].kills;
            UIController.instance.deathsText.text = "Deaths: " + allPlayers[index].deaths;
        }
        else 
        {
            UIController.instance.killsText.text = "Kills: 0";
            UIController.instance.deathsText.text = "Deaths: 0";
        }

    }


    void ShowLeaderboard() 
    {
        //clear previous information when every new start
        UIController.instance.leaderboard.SetActive(true);

        foreach(LeaderboardPlayer lp in lboardPlayers) 
        {
            Destroy(lp.gameObject);

        }
        lboardPlayers.Clear();

        UIController.instance.leaderboardPlayerDisplay.gameObject.SetActive(false);

        //re order leaderboard
        List<PlayerInfo> sorted = SortPlayers(allPlayers);


        // show leaderboard
        foreach(PlayerInfo player in sorted) 
        {

            LeaderboardPlayer newPlayerDisplay = Instantiate(UIController.instance.leaderboardPlayerDisplay, UIController.instance.leaderboardPlayerDisplay.transform.parent);
            newPlayerDisplay.SetDetails(player.name,player.kills,player.deaths);

            newPlayerDisplay.gameObject.SetActive(true);
            lboardPlayers.Add(newPlayerDisplay);
        }
    
    }

    //sort and find out the most kill player
    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players) 
    {
        //create a new list for sorted players
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        //reorder the list
        while (sorted.Count < players.Count) 
        {

            int highest = -1;

            PlayerInfo selectedPlayer = players[0];

            //find the most kill player
            foreach(PlayerInfo player in players) 
            {
                //as long as this player is not put in the sorted list, we sort the highest kill,and only for players who haven't been sorted yet
                if (!sorted.Contains(player)) 
                {

                  if(player.kills > highest) 
                  {
                    selectedPlayer = player;
                    highest = player.kills;
                
                  }
                }
            }

            //add the selected highest kill to the list one by one
            sorted.Add(selectedPlayer);
        
        }


        return sorted;
    
    }



    #endregion


    #region End Round System
    public override void OnLeftRoom()
    {
       //the reason we keep this is because we need wait for photon to be fully disconnected itself
        base.OnLeftRoom();
        //make cursor able to see again when they back to main menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //when we end the match, tell the player to leave the room and back to main scene
        SceneManager.LoadScene(0);
    }

    //whenever a player update its state,we will check if the game should be end
    void ScoreCheck() 
    {
        bool winnerFound = false;

        foreach(PlayerInfo player in allPlayers) 
        {
           if(player.kills >= killsToWin && killsToWin > 0) 
            {
                winnerFound = true;
               break;
            }
        
        
        }

        if (winnerFound) 
        {
            //if we are the master client and the game is not end
            if (PhotonNetwork.IsMasterClient&& state !=GameState.Ending) 
            {
                state = GameState.Ending;

                //we want all players to be updated with the game state,so we sent this state to master
                ListPlayersSend();
            
            }
        
        
        }
    
    }


    //check for state
    void StateCheck() 
    {
      if(state == GameState.Ending) 
        {
            //if we are in the end game state, call end game function
            EndGame();
        
        }
    
    }


    void EndGame() 
    {
        //make sure it's definitely ending the game
        state = GameState.Ending;


        //if I am the master client,destroy everthing instantiated from network including players
        if (PhotonNetwork.IsMasterClient) 
        {
            PhotonNetwork.DestroyAll();
        
        }

        UIController.instance.EndScreen.SetActive(true);
        ShowLeaderboard();

        //make cursor able to see again when they back to main menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //reset camera position to sky
        Camera.main.transform.position = MatchManager.instance.mapCamPoint.position;
        Camera.main.transform.rotation = MatchManager.instance.mapCamPoint.rotation;

        StartCoroutine(EndCo());
    
    }


    IEnumerator EndCo() 
    {
        yield return new WaitForSeconds(waitAfterEnding);


        //if the game is not going to continue
        if (!perertual)
        {
            //we don't want everybody syn scene at the end state , so we set it back to false because we set it to true when joined room
            PhotonNetwork.AutomaticallySyncScene = false;
            //this will triiger the OnLeftRoom, which will transfer us back to main menu scene
            PhotonNetwork.LeaveRoom();
        }

        else 
        {
            //if the game is goint to continue which means that if player want to keep playing together,
            if (PhotonNetwork.IsMasterClient) 
            {
                NextMatchSend();
            }
        }


        yield return null;
    
    }

    public void NextMatchSend() 
    {
      
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NextMatch, //we need to convert our own eventcode back into byte after synchornize it in OnEvent() so photon understand what it is
            null,   // null means there's no data sending with this
            new RaiseEventOptions { Receivers = ReceiverGroup.All }, //make sure only the all client can receive a new player information
            new SendOptions { Reliability = true } //tell the server that this is something we want it to be reliable so it will be definitely  able to be sent to the network and all players later
            );

    }

    public void NextMatchReceive() 
    {
        state = GameState.Playing;
        UIController.instance.EndScreen.SetActive(false);
        UIController.instance.leaderboard.SetActive(false);

        //reset all players record
        foreach(PlayerInfo player in allPlayers) 
        {
            player.kills = 0;
            player.deaths = 0;
        
        }

        UpdateStatsDisplay();

        //local player spawner for each individual person running the game will respawn a player
        PlayerSpawner.instance.SpawnPlayer();
    
    }




    #endregion



    //make sure update network player list whenever a player left game,forced to close or crashes
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        int index = allPlayers.FindIndex(x => x.name == otherPlayer.NickName);

        if(index!= -1) 
        {
            allPlayers.RemoveAt(index);

            ListPlayersSend();
        }

    }


}




//addtion class to store  player information , use constructor later to pass certain Information;

[System.Serializable]
public class PlayerInfo 
{

    public string name;
    public int actor, kills, deaths;

    public PlayerInfo(string _name,int _actor, int _kills, int _deaths) 
    {

        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _deaths;
    }

}
