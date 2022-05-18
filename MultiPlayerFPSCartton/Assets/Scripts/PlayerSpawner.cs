using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner instance;

    //fx
    public GameObject deathEffect;

    private void Awake()
    {
        instance = this;
    }

    public float respawnTime = 5f;
    public GameObject playerPrefab;
    private GameObject player;

    
    void Start()
    {
        //check if we have already connected to the server
        if (PhotonNetwork.IsConnected) 
        {
            Debug.Log("Original spawn");
            SpawnPlayer();
        }
        
    }

   
    void Update()
    {
        
    }


    public void SpawnPlayer() 
    {
        Transform spawnPoint = SpawnManager.instance.GetSpawnPoint();
       
        player = PhotonNetwork.Instantiate(playerPrefab.name,spawnPoint.position,spawnPoint.rotation);
       
    
    }


    public void Die(string damager) 
    {
     


        //update damager name
        UIController.instance.deathText.text = "You were killed by " + damager;
        //because we are the one die, this time , we will send our own actor number
        MatchManager.instance.UpdateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
     
        if (player != null) 
        {
            StartCoroutine(DieCo());
        }

    }


    public IEnumerator DieCo() 
    {
        //play death Effect
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);

        //tell all client this player has been destroyed
        PhotonNetwork.Destroy(player);
        player = null; //we want to make sure player setting to null,otherwise after being destroy, it will be missing component,so later it won't be respawned because it's not null


        UIController.instance.deathScreen.SetActive(true);
        yield return new WaitForSeconds(respawnTime);
      
        //respawn player
        UIController.instance.deathScreen.SetActive(false);

        //if the game state is not end and we haven't spawned any player in the scene yet , we can reSpawn the player
        if (MatchManager.instance.state == MatchManager.GameState.Playing && player==null)
        {
           
            SpawnPlayer();
        }


    }

}
