using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public bool isAutomatic;
    public float timeBetweenShots = .1f,heatPerShot = 1f;
    public GameObject muzzleFlash;
    public int shotDamage;
    public float adsZoom; // aiming 
    public AudioSource shotSound;


    private void Start()
    {
        shotSound = GetComponent<AudioSource>();
    }


}
