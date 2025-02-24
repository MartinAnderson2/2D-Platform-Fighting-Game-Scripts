using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateSpawner : MonoBehaviour
{
    float spawnChance;
    [SerializeField] GameObject ball;
    [SerializeField] float secondsUntilSpawn = 120;
    [SerializeField] float mostRecentBallSpawn = 0;
    [SerializeField] float minIntervalBetweenSpawns;
    [SerializeField] float startingMinimumIntervalBetweenSpawns = 30;
        

    // Update is called once per frame
    void Update()
    {
        spawnChance = Time.deltaTime / secondsUntilSpawn;
        if ((Random.Range(0.0f, 1.0f) < spawnChance && mostRecentBallSpawn + minIntervalBetweenSpawns < Time.time && Time.time > startingMinimumIntervalBetweenSpawns) || mostRecentBallSpawn + secondsUntilSpawn <= Time.time)
        {
            Instantiate(ball, transform);
            mostRecentBallSpawn = Time.time;
        }

        if (mostRecentBallSpawn + 120.0f <= Time.time)
        {
            Instantiate(ball, transform);
            mostRecentBallSpawn = Time.time;
        }
        
    }
}
