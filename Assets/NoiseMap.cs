using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NoiseMap : MonoBehaviour
{
    Dictionary<int, GameObject> tileset;
    Dictionary<int, GameObject> tile_groups;

    public GameObject lake_pfb;
    public GameObject plain_pfb;
    public GameObject forest_pfb;
    public GameObject hill_pfb;
    public GameObject mountain_pfb;

    int map_width = 31;
    int map_height = 31;

    List<List<int>> noise_grid = new List<List<int>>();
    List<List<GameObject>> tile_grid = new List<List<GameObject>>();

    float mag = 2.0f;
    int x_offset = 0;
    int y_offset = 0;

    float[] sample_data;

    void Start()
    {
        CreateTileset();
        CreateTileGroup();
        StartCoroutine(GetAndGenerateMap());
        GenerateMap();
    }

  [System.Serializable]
    public class FloatArrayWrapper
    {
        public float[] array;
    }

    IEnumerator GetRandomData()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get("http://localhost:5000/api/get_noise"))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to get random data: " + webRequest.error);
                yield break;
            }

            string jsonResponse = webRequest.downloadHandler.text;
            FloatArrayWrapper floatArrayWrapper = JsonUtility.FromJson<FloatArrayWrapper>("{\"array\":" + jsonResponse + "}");
            sample_data = floatArrayWrapper.array;

            Debug.Log("Random data retrieved successfully");
        }
    }

    IEnumerator GetAndGenerateMap()
    {
        yield return GetRandomData();
        GenerateMap();
    }

    void CreateTileset()
    {
        tileset = new Dictionary<int, GameObject>();
        tileset.Add(0, lake_pfb);
        tileset.Add(1, plain_pfb);
        tileset.Add(2, forest_pfb);
        tileset.Add(3, hill_pfb);
        tileset.Add(4, mountain_pfb);
    }

    void CreateTileGroup()
    {
        tile_groups = new Dictionary<int, GameObject>();
        foreach (KeyValuePair<int, GameObject> prefab_pair in tileset)
        {
            GameObject tile_group = new GameObject(prefab_pair.Value.name);
            tile_group.transform.parent = gameObject.transform;
            tile_group.transform.localPosition = new Vector3(0, 0, 0);
            tile_groups.Add(prefab_pair.Key, tile_group);
        }
    }

    void GenerateMap()
    {
        int index = 0;

        for (int x = 0; x < map_width; x++)
        {
            noise_grid.Add(new List<int>());
            tile_grid.Add(new List<GameObject>());

            for (int y = 0; y < map_height; y++)
            {
                //int tile_id = GetIdUsingPerlin(x, y);
                int tile_id = GetIdUsingRandom(index);
                noise_grid[x].Add(tile_id);
                CreateTile(tile_id, x, y);
                index++;
            }
        }
    }

    int GetIdUsingPerlin(int x, int y)
    {
        float raw_perlin = Mathf.PerlinNoise((x - x_offset) / mag, (y - y_offset) / mag);
        Debug.Log(raw_perlin);
        float clamp_perlin = Mathf.Clamp(raw_perlin, 0.0f, 1.0f);
        float scale_perlin = clamp_perlin * tileset.Count;
        if (scale_perlin == 4)
        {
            scale_perlin = 3;
        }
        return Mathf.FloorToInt(scale_perlin);
    }

    int GetIdUsingRandom(int index)
    {
        float raw = sample_data[index];
        float clamped = Mathf.Clamp(raw, 0.0f, 1.0f);
        float scaled = clamped * tileset.Count;
        if (scaled == 5)
        {
            scaled = 4;
        }
        return Mathf.FloorToInt(scaled);
    }

    void CreateTile(int tile_id, int x, int y)
    {
        GameObject tile_prefab = tileset[tile_id];
        GameObject tile_group = tile_groups[tile_id];
        GameObject tile = Instantiate(tile_prefab, tile_group.transform);

        tile.name = string.Format("tile_x{0}_y{1}", x, y);
        tile.transform.localPosition = new Vector3(x, y, 0);

        tile_grid[x].Add(tile);
    }
}
