using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [System.Serializable]
    public class BiomeType
    {
        public string name;             // Name des Bioms
        public GameObject[] mapTiles;   // Array der Map Tiles für dieses Biom
        public float percentage;        // Prozentuale Häufigkeit dieses Bioms
        public bool isWater;            // Gibt an, ob es sich um ein Wasser-Biom handelt
    }

    [Tooltip("Your biomes. Contains the name, mapTiles and the weight.")]
    public BiomeType[] biomes;          // Array der verschiedenen Biome
    [Tooltip("Map width size")]
    public int mapWidth = 10;           // Breite der Karte
    [Tooltip("Map height size")]
    public int mapHeight = 10;          // Höhe der Karte
    [Tooltip("Perlin noise scale")]
    public float scale = 1.0f;          // Skala für das Perlin-Rauschen
    private float xOffsetRange = 200f;  // Bereich für das zufällige x-Offset
    private float yOffsetRange = 200f;  // Bereich für das zufällige y-Offset
    [Tooltip("Ocean width relative to map size")]
    public float oceanWidth = 0.3f;     // Breite des Ozeans relativ zur Kartengröße
    [Tooltip("Spacing between tiles")]
    public float tileSpacing = 1.0f;    // Abstand zwischen den Tiles

    void Start()
    {
        Random.InitState(System.Environment.TickCount);
        GenerateMap();
    }

    void GenerateMap()
    {
        GameObject holderObject = new GameObject("MapHolder"); // Erstelle ein neues GameObject als Holder
        holderObject.transform.parent = transform; // Mache das Holder-Objekt zum Kind des TerrainGenerator-Objekts

        float xOffset = Random.Range(-xOffsetRange, xOffsetRange);
        float yOffset = Random.Range(-yOffsetRange, yOffsetRange);

        float[,] falloffMap = GenerateFalloffMap(mapWidth, mapHeight);

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float xCoord = ((float)x / mapWidth + xOffset) * scale;
                float yCoord = ((float)y / mapHeight + yOffset) * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord) - falloffMap[x, y];

                GameObject mapTile = DetermineMapTile(sample);

                Vector3 position = new Vector3(x * tileSpacing, 0, y * tileSpacing);
                GameObject tileInstance = Instantiate(mapTile, position, Quaternion.identity);
                tileInstance.transform.parent = holderObject.transform; // Mache das Tile zum Kind des Holder-Objekts

                /*// Setze den Tag basierend auf dem ausgewählten Biom
                foreach (BiomeType biome in biomes)
                {
                    float randomValue = Random.Range(0f, 100f);
                    if (randomValue <= biome.percentage)
                    {
                        tileInstance.tag = biome.name;
                        break;
                    }
                }*/
            }
        }
    }

    GameObject DetermineMapTile(float sample)
    {
        // Bestimme das Biom basierend auf dem Perlin-Rauschen-Wert
        if (sample <= 0) // Wenn der Sample-Wert kleiner oder gleich null ist, verwenden wir das Wasser-Biom
        {
            return GetWaterTile();
        }

        float cumulativePercentage = 0f;
        foreach (BiomeType biome in biomes)
        {
            cumulativePercentage += biome.percentage;
            if (sample * 100f <= cumulativePercentage)
            {
                // Wähle zufällig ein Map Tile aus dem Array dieses Bioms
                int index = Random.Range(0, biome.mapTiles.Length);
                return biome.mapTiles[index];
            }
        }

        // Standard: Gib das letzte Biom zurück, falls kein Biom ausgewählt wurde
        BiomeType defaultBiome = biomes[biomes.Length - 1];
        int defaultIndex = Random.Range(0, defaultBiome.mapTiles.Length);
        return defaultBiome.mapTiles[defaultIndex];
    }

    GameObject GetWaterTile()
    {
        // Wähle zufällig ein Wasser-Tile aus den Biomen, die als Wasser markiert sind
        foreach (BiomeType biome in biomes)
        {
            if (biome.isWater)
            {
                int index = Random.Range(0, biome.mapTiles.Length);
                return biome.mapTiles[index];
            }
        }
        return null; // Fallback, falls kein Wasser-Biom gefunden wurde
    }

    float[,] GenerateFalloffMap(int width, int height)
    {
        float[,] map = new float[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float x = i / (float)width * 2 - 1;
                float y = j / (float)height * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    float Evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;

        // Verwende oceanWidth, um den Übergang zur Wasserzone anzupassen
        return Mathf.Pow(value * oceanWidth, a) / (Mathf.Pow(value * oceanWidth, a) + Mathf.Pow(b - b * value * oceanWidth, a));
    }
}
