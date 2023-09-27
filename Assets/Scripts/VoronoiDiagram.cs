using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class VoronoiDiagram : MonoBehaviour
{
    private int pixelsPerCell;
    public RawImage image;

    public BiomeData[] possibleBiomes;
    private Vector2[,] biomeSeedPositions;
    private Color[,] biomeColors;

    private int imgSize;
    [SerializeField] private int gridSize=10;

    [Header("Noise Settings")]
    [Range(0f,1f)]
    [SerializeField] private float temperatureNoiseZoom = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float humidityNoiseZoom = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float landNoiseZoom = 0.1f;
    [SerializeField] private float humidityNoiseOffset = 100f;
    [SerializeField] private float landNoiseOffset = 50f;
    [Range(0f, 1f)]
    [SerializeField] private float landThreshold = 0.45f;

    private void Awake()
    {
        image = GetComponent<RawImage>();

    }
    void Start()
    {
        GenerateWorld();
    }

    public void GenerateWorld()
    {

        imgSize = Mathf.RoundToInt(image.GetComponent<RectTransform>().sizeDelta.x);
        Texture2D texture = new Texture2D(imgSize,imgSize);
        texture.filterMode = FilterMode.Point;

        pixelsPerCell = imgSize / gridSize;

        GenerateSeeds();
        for (int x = 0; x < imgSize; x++)
        {
            for (int y = 0; y < imgSize; y++)
            {
                int gridX = x / pixelsPerCell;
                int gridY = y / pixelsPerCell;

                float nearestDistance = Mathf.Infinity;
                Vector2Int nearestPoint = new Vector2Int();
                float distortedX = x + Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                float distortedY = y + Mathf.PerlinNoise(x * 0.1f, y * 0.1f);

                for (int a = -1; a < 2; a++)
                {
                    for (int b = -1; b < 2; b++)
                    {

                        int i = gridX + a;
                        int j = gridY + b;

                        if (i < 0 || j < 0 || i >= gridSize || j >= gridSize) continue;

                        float distance = Vector2.Distance(new Vector2(distortedX, distortedY), biomeSeedPositions[i, j]);
                        if (distance < nearestDistance)
                        {
                            nearestDistance = distance;
                            nearestPoint = new Vector2Int(i, j);
                        }
                    }
                }

                texture.SetPixel(x, y, biomeColors[nearestPoint.x, nearestPoint.y]);
            }
        }

        texture.Apply();
        image.texture = texture;
    }
    
    private void GenerateSeeds()
    {
        biomeSeedPositions = new Vector2[gridSize, gridSize];
        biomeColors = new Color[gridSize, gridSize];
        for(int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                biomeSeedPositions[x, y] = new Vector2Int(x *pixelsPerCell+ UnityEngine.Random.Range(0,pixelsPerCell), y *pixelsPerCell + UnityEngine.Random.Range(0, pixelsPerCell));
                float temp = Mathf.PerlinNoise(x * temperatureNoiseZoom, y*temperatureNoiseZoom);
                float humidity = Mathf.PerlinNoise((x+humidityNoiseOffset) * humidityNoiseZoom, (y + humidityNoiseOffset) * humidityNoiseZoom);
                bool land = Mathf.PerlinNoise((x + landNoiseOffset) * landNoiseZoom, (y + landNoiseOffset) * landNoiseZoom) < landThreshold ? true : false; 
                biomeColors[x, y] = SelectBiome(temp, humidity,land);
            }
        }
    }

    private Color SelectBiome(float temp,float humidity,bool land)
    {
        foreach (var data in possibleBiomes)
        {
            if (!land) return Color.blue;
            
            if (temp > data.temperatureStartThreshold && temp < data.temperatureEndThreshold 
                && humidity > data.humidityStartThreshold && humidity < data.humidityEndThreshold)
            {
                return data.Biome;
            }
        }
        return possibleBiomes[0].Biome;
    }

}

[Serializable]
public struct BiomeData
{
    public string biomeName;
    [Range(0f, 1f)]
    public float temperatureStartThreshold, temperatureEndThreshold;
    [Range(0f, 1f)]
    public float humidityStartThreshold, humidityEndThreshold;
    public Color Biome;
}