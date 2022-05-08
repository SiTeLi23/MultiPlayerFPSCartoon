using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIController : MonoBehaviour
{
    public static UIController instance;

    private void Awake()
    {
        instance = this;
    }



    public TMP_Text OverHeatedMessage;

    void Start()
    {
        
    }

   

    void Update()
    {
        
    }



}
