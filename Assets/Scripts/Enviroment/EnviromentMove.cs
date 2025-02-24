using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnviromentMove : MonoBehaviour
{
    [SerializeField] private Transform waypointOne;
    [SerializeField] private Transform waypointOneOne; 

    [SerializeField] private Transform waypointTwo;
    [SerializeField] private Transform waypointTwoTwo;

    [SerializeField] private Transform mostRecentWaypoint;

    [SerializeField, Range(0.1f, 5f)] private float speed = 1f;

    private enum islandSide
    {
        rightIsland,
        leftIsland
    }

    [SerializeField] private islandSide island;

    private void Start()
    {
        if (island == islandSide.leftIsland)
        {
            mostRecentWaypoint = waypointOne;
        }
        else
        {
            mostRecentWaypoint = waypointTwo;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position == waypointOne.position) { mostRecentWaypoint = waypointOne; }
        if (transform.position == waypointOneOne.position) { mostRecentWaypoint = waypointOneOne; }
        if (transform.position == waypointTwo.position) { mostRecentWaypoint = waypointTwo; }
        if (transform.position == waypointTwoTwo.position) { mostRecentWaypoint = waypointTwoTwo; }
    

        if (island == islandSide.leftIsland)
        {
            if (waypointOne == mostRecentWaypoint)
            {
                TraveltoLocation(waypointOneOne);
            }
            else
            {
                TraveltoLocation(waypointOne);
            }
        }
        else
        {
            if (waypointTwo == mostRecentWaypoint)
            {
                TraveltoLocation(waypointTwoTwo);
            }
            else
            {
                TraveltoLocation( waypointTwo);
            }
        }

        
    }

    private void TraveltoLocation(Transform waypointSecond)
    {
        gameObject.transform.position = Vector3.MoveTowards(transform.position, waypointSecond.transform.position, Time.deltaTime * speed);
    }
}
