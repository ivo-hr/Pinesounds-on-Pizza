using UnityEngine;

public class SpawnOnObject : MonoBehaviour
{
    [System.Serializable]
    public class assetsToSpawn
    {
        [Tooltip("Name of your asset")]
        public string assetName;
        public GameObject asset;
        [Tooltip("probability to spawn it")]
        public int probabilityToSpawn;
    }

    public assetsToSpawn[] assets;
    [Tooltip("Probability to spawn nothing")]
    public int probabilityToSpawnNothing;

    void Start()
    {
        // Asset auswählen
        GameObject selectedAsset = SelectRandomAsset();

        // Wenn ein Asset ausgewählt wurde, spawnen
        if (selectedAsset != null)
        {
            // Zufällige Position auf dem Tile spawnen
            Vector3 spawnPosition = GetRandomSpawnPosition();
            Instantiate(selectedAsset, spawnPosition, Quaternion.identity, transform);
        }
    }

    GameObject SelectRandomAsset()
    {
        int totalProbability = probabilityToSpawnNothing;
        foreach (assetsToSpawn asset in assets)
        {
            totalProbability += asset.probabilityToSpawn;
        }

        int randomValue = Random.Range(0, totalProbability);
        int cumulativeProbability = 0;

        if (randomValue < probabilityToSpawnNothing)
        {
            return null;
        }

        cumulativeProbability += probabilityToSpawnNothing;

        foreach (assetsToSpawn asset in assets)
        {
            cumulativeProbability += asset.probabilityToSpawn;
            if (randomValue < cumulativeProbability)
            {
                return asset.asset;
            }
        }

        // Falls nichts ausgewählt wurde (was unwahrscheinlich ist), das erste Asset zurückgeben
        return assets[0].asset;
    }

    Vector3 GetRandomSpawnPosition()
    {
        Bounds bounds = GetComponent<Renderer>().bounds;

        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomZ = Random.Range(bounds.min.z, bounds.max.z);
        float spawnY = bounds.max.y;

        return new Vector3(randomX, spawnY, randomZ);
    }
}
