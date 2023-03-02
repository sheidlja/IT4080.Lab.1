using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InClassPowerUpSpawner : MonoBehaviour
{
    private float timeRemaining = 0f;
    public void Start()
    {
        
    }

    /*public void Update()
    {
        if(timeRemaining == 0f && curPowerUp == null)
        {
            timeRemaining = spawnDelay;
        }
        else if (timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
        }
        if timeRemaining <= 0f)
        {
            timeRemaining = 0;
            SpawnPowerUp();
        }
    }*/
}
