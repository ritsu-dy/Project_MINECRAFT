using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using System.Linq;

// 이진 트리... 사진 트리.....
public class VoronoiNode
{
    public Vector2Int coord;   
    public bool isFillColor;     
    public bool isEdge;        
    public VoronoiNode parent;
    public VoronoiNode nUp;
    public VoronoiNode nDown;
    public VoronoiNode nLeft;
    public VoronoiNode nRight;

    public VoronoiNode(Vector2Int c)
    {
        coord = c;
        isFillColor = false;
        isEdge = false;
        parent = nUp = nDown = nLeft = nRight = null;
    }
}

public class MapsDrawer
{
    public static VoronoiNode[,] _voronoiNode;
    public static VoronoiNode _rootNode;


    public const float _heightScale = 20;

    public static Sprite DrawSprite(Vector2Int size, Color[] colorDatas)
    {
        Texture2D texture = new Texture2D(size.x, size.y);
        texture = new Texture2D(size.x, size.y);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorDatas);
        texture.Apply();

        Rect rect = new Rect(0, 0, size.x, size.y);
        Sprite sprite = Sprite.Create(texture, rect, Vector2.one, 0.5f);
        return sprite;
    }

    public static Sprite DrawVoronoiToSprite(Voronoi vo, Color centeroidColor, Color edgeColor, Color[] areaColor)
    {
        Rect rect = vo.PlotBounds;
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);
        Color[] pixelColors = Enumerable.Repeat(Color.white, width * height).ToArray();
        List<Vector2> siteCoords = vo.SiteCoords();

        List<int> posCenter = new List<int>();

        foreach (Vector2 coord in siteCoords)
        {
            int x = Mathf.Clamp(Mathf.RoundToInt(coord.x), 0, width - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(coord.y), 0, height - 1);
            int index = x * width + y;
            pixelColors[index] = centeroidColor;

            posCenter.Add(index);
            
        }

        Vector2Int size = new Vector2Int(width, height);
        //TODO : 선 그리기
        foreach (Site site in vo.Sites)
        {
           
            List<Site> neighbors = site.NeighborSites();
           
            foreach (Site neighbor in neighbors)
            {
                Edge edge = vo.FindEdgeFromAdjacentPolygons(site, neighbor);
                if (edge.ClippedVertices is null)
                {
                    continue;
                }
                
                Vector2 corner1 = edge.ClippedVertices[LR.LEFT];
                Vector2 corner2 = edge.ClippedVertices[LR.RIGHT];

                Vector2 tartgetPoint = corner1;
                float delta = 1 / (corner2 - corner1).magnitude;
                float lerpRotio = 0.0f;

                while ((int)tartgetPoint.x != (int)corner2.x || (int)tartgetPoint.y != (int)corner2.y)
                {
                   
                    tartgetPoint = Vector2.Lerp(corner1, corner2, lerpRotio);
                    lerpRotio += delta;
                   
                    int x = Mathf.Clamp((int)tartgetPoint.x, 0, size.x - 1);
                    int y = Mathf.Clamp((int)tartgetPoint.y, 0, size.y - 1);
                    int index = x * size.x + y;
                    pixelColors[index] = edgeColor;
                }
            }
        }
        //색칠하기
        for (int n = 0; n < posCenter.Count; n++)        
        {
            int tx = posCenter[n] % width;                
            int ty = posCenter[n] / width;               

            dfs(pixelColors, tx, ty, areaColor[n % 5], size);           
        }

        return DrawSprite(size, pixelColors);
    }

    
    public static float[] GetGradientMask(Vector2Int size, int maskRadius)
    {
        float[] coloeData = new float[size.x * size.y];

        Vector2Int center = size / 2;
        float radius = center.y;
        int index = 0;
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                Vector2Int position = new Vector2Int(x, y);
                float distFromCenter = Vector2Int.Distance(center, position) + (radius - maskRadius);
                float colorFactor = distFromCenter / radius;
                coloeData[index++] = 1 - colorFactor;
            }
        }

        return coloeData;
    }

    public static void CreateFileFromTexture2D(Texture2D texture, string name)
    {
        byte[] by = texture.EncodeToPNG();

        string path = Application.dataPath + "/" + name;
        System.IO.FileStream _fs = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
        System.IO.BinaryWriter _bw = new System.IO.BinaryWriter(_fs);

        _bw.Write(by);
        _bw.Close();
        _fs.Close();
    }

    
    static void dfs(Color[] pixelColors, int x, int y, Color targetColor, Vector2Int size)
    {
        if (x >= size.x || x < 0 || y >= size.y || y < 0)   
        {
            return;
        }
        if (pixelColors[x + size.x * y] == Color.black)   
        {
            return;
        }
        if (pixelColors[x + size.x * y] == targetColor)     
        {
            return;
        }
        pixelColors[x + size.x * y] = targetColor;       

        dfs(pixelColors, x - 1, y, targetColor, size);      
        dfs(pixelColors, x + 1, y, targetColor, size);
        dfs(pixelColors, x, y - 1, targetColor, size);
        dfs(pixelColors, x, y + 1, targetColor, size);
    }
}
