using UnityEngine;

public class SpawnOnObject : MonoBehaviour
{
    [System.Serializable]
    public class AssetsToSpawn
    {
        [Tooltip("Name of your asset")]
        public string assetName;
        public GameObject asset;
        [Tooltip("Probability to spawn it")]
        public int probabilityToSpawn;
    }

    public AssetsToSpawn[] assets;
    [Tooltip("Probability to spawn nothing")]
    public int probabilityToSpawnNothing;

    void Start()
    {
        // Select an asset
        GameObject selectedAsset = SelectRandomAsset();

        // If an asset is selected, spawn it
        if (selectedAsset != null)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();

            // Create random rotation with random Y-axis
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            // Instantiate without inheriting the parent's scale
            GameObject instance = Instantiate(selectedAsset, spawnPosition, randomRotation);

            // Reset parent to prevent scale inheritance
            instance.transform.SetParent(null, true);
            }
    }

    GameObject SelectRandomAsset()
    {
        int totalProbability = probabilityToSpawnNothing;
        foreach (AssetsToSpawn asset in assets)
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

        foreach (AssetsToSpawn asset in assets)
        {
            cumulativeProbability += asset.probabilityToSpawn;
            if (randomValue < cumulativeProbability)
            {
                return asset.asset;
            }
        }

        // If no asset is selected (unlikely), return the first asset
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