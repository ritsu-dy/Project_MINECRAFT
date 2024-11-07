using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using System.Linq;

public class MapEdit : MonoBehaviour
{
    [Header("»ö")]
    [SerializeField] Color[] _areaColor;
    [Header("¸Ê ±âº» Á¤º¸")]
    [SerializeField] Vector2Int _size;
    [SerializeField] int _nodeAmount = 0; 
    [SerializeField] int _loydIteratCount = 0;
    [SerializeField] int _noiseOctave = 0;
    [SerializeField] int _seed = 100;
    [Header("¸Ê View")]
    [SerializeField] SpriteRenderer _voronoiMapRenderer;
    [SerializeField] SpriteRenderer _noiseMapRender;
    [Header("¸Ê Param")]
    [SerializeField, Range(0f, 0.4f)] float _noiseFrequncy = 0; 
    [SerializeField, Range(0f, 0.4f)] float _lendNoiseThreshould = 0;
    [SerializeField, Range(0.4f, 0.6f)] float _champaignNoiseThreshould = 0.5f;
    [SerializeField, Range(0.6f, 1f)] float _alpineNoiseThreshould = 0.7f;
    [SerializeField] int _noiseMaskRadius = 0;
    [SerializeField] Color _centrodColor = Color.yellow;
    [SerializeField] Color _edgeColor = Color.black;
    [SerializeField] string _voronoiName = "Voronoi.png";
    [SerializeField] string _noiseName = "Noise.png";


    void Awake()
    {
        Voronoi vo = GenerateVoronoi(_size, _nodeAmount, _loydIteratCount);
        _voronoiMapRenderer.sprite = MapsDrawer.DrawVoronoiToSprite(vo, _centrodColor, _edgeColor, _areaColor);
    }

    void Start()
    {
        for (int n = 0; n < _areaColor.Length; n++)
        {
            float r = _areaColor[n].r;
            float g = _areaColor[n].g;
            float b = _areaColor[n].b;
            float a = _areaColor[n].a;
            Color areaC = new Color(r, g, b, a);
          
        }
    }

    void Update()
    {
        GenerateMap();

        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            MapsDrawer.CreateFileFromTexture2D(_noiseMapRender.sprite.texture, _noiseName);
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            MapsDrawer.CreateFileFromTexture2D(_voronoiMapRenderer.sprite.texture, _voronoiName);
        }
    }

    Voronoi GenerateVoronoi(Vector2Int size, int nodeAmount, int loydIteratCount)
    {
        List<Vector2> centerPoint = new List<Vector2>();
        for (int n = 0; n < nodeAmount; n++)
        {
            int rx = Random.Range(0, size.x);
            int ry = Random.Range(0, size.y);
            centerPoint.Add(new Vector2(rx, ry));
        }
        Rect rt = new Rect(0, 0, size.x, size.y);
        Voronoi vo = new Voronoi(centerPoint, rt, loydIteratCount);
        return vo;
    }
    float[] CreateMapShape(Vector2Int size, float frequency, int octave)
    {
      
        int seed = (_seed == 0) ? Random.Range(1, int.MaxValue) : _seed;

        FastNoiseLite noise = new FastNoiseLite();
        
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
     
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFrequency(frequency);
        noise.SetFractalOctaves(octave);
        noise.SetSeed(seed);

        float[] mask = MapsDrawer.GetGradientMask(size, _noiseMaskRadius);
        float[] colorDatas = new float[size.x * size.y];
        int index = 0;
        
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                float noiseColorFactor = noise.GetNoise(x, y);
                
                noiseColorFactor = (noiseColorFactor + 1.2f) * 0.5f;
               
                float color = (noiseColorFactor > _lendNoiseThreshould) ? 0.5f : 0f;
                noiseColorFactor *= mask[index];
                
                colorDatas[index] = noiseColorFactor;
                index++;
            }
        }

        return colorDatas;
    }

    public void GenerateMap()
    {
        float[] noiseColors = CreateMapShape(_size, _noiseFrequncy, _noiseOctave);
        Color[] colors = new Color[noiseColors.Length];
        for (int n = 0; n < colors.Length; n++)
        {
           
            float r = noiseColors[n];
            float g = noiseColors[n];
            float b = noiseColors[n];
            colors[n] = new Color(r, g, b, 1);
        }
        _noiseMapRender.sprite = MapsDrawer.DrawSprite(_size, colors);
    }

}
