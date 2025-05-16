using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public int numberOfEnemies = 3;
    public Vector2 spawnArea = new Vector2(10f, 10f);
    public EnemyHealthManager healthManager;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) // Press 'N' to spawn a new wave
        {
            SpawnWave();
        }
    }

    public void SpawnWave()
    {
        EnemyHealth[] newEnemies = new EnemyHealth[numberOfEnemies];

        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 spawnPos = new Vector3(
                Random.Range(-spawnArea.x, spawnArea.x),
                0f,
                Random.Range(-spawnArea.y, spawnArea.y)
            );

            // pick a random enemy to spawn
            int randomIndex = Random.Range(0, enemyPrefabs.Length);
            Debug.Log("Spawning an enemy prefab of index " + randomIndex);
            
            // spawn
            GameObject enemyObj = Instantiate(enemyPrefabs[randomIndex], spawnPos, Quaternion.identity);
            newEnemies[i] = enemyObj.GetComponent<EnemyHealth>();
        }

        // Register this wave with the health manager
        healthManager.RegisterNewWave(newEnemies);
    }
}
