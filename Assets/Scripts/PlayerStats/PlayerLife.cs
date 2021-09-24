using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLife : MonoBehaviour
{

    public float maxHealth, maxThirst, maxSaturation;
    public float saturationDecrRate, thirstIncrRate;

    float health, thirst, saturation;
   // bool canThirst, canHunger, canHealth = true;
    bool playerDead;


    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        saturation = maxSaturation;
        playerDead = false;
    }

    // Update is called once per frame
    void Update()
    {
       if (!playerDead)
        {
            thirst += thirstIncrRate * Time.deltaTime;
            saturation -= saturationDecrRate * Time.deltaTime;
        }

       if((thirst >= maxThirst || saturation <= 0) && !playerDead)
        {
            playerDead = true;
            Die();
        }
    }

    public void Die()
    {
        print("Died");
    }
}
