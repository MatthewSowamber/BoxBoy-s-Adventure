using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject ballPrefab;
    public float spawnTime = 1f;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("SpawnBalls", 0f, 1f);
    }

    void SpawnBalls()
    {
        Instantiate(ballPrefab, transform.position, Quaternion.identity);
    }
}
