using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// TODO :
/// 각각 아이템화 시키고
/// 코드 분할 정리 하면서
/// 지하 맵 생성 도전
/// </summary>
public class Blocks
{
    public enum eBlockType
    {
        Snow = 0,
        Stone,
        Forest,
        Grass,
        BaseMineral,
        Gold,
        Ruby,
    }

    //public int type;
    public eBlockType type;
    public bool _isView;
    public GameObject _objBlock;

    public Blocks(eBlockType t, bool v, GameObject blockObj)
    {
        type = t;
        _isView = v;
        _objBlock = blockObj;
    }
}

public class LoadMapTest : MonoBehaviour
{
    [Header("맵 정보")]
    [SerializeField] Texture2D _mapInfo;
    [SerializeField] Texture2D _voronoiMapInfo;
    [SerializeField] float _mapGroundHeightOffset;

    [Header("지형 블록 정보")]
    [SerializeField] Transform _root;
    [SerializeField] Transform _mineralroot;
    [SerializeField] GameObject[] _prefabMapBlocks;


    static public int _mapheight = 128;
    public float GroundHeightOffset = 20;
    Vector3 _playerSpawn;

    public Blocks[,,] worldBlock;
    Vector2Int _size;
   

    List<GameObject> _mineralList = new List<GameObject>();
    public Color[] _voroColor;

    void Start()
    {
        StartCoroutine(InitGame());
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000.0f))
            {
                Vector3 blockPos = hit.transform.position;

                if (blockPos.y <= 0)
                {
                    return;
                }

                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = null;

                if (hit.collider.CompareTag("Block"))
                {
                    Destroy(hit.collider.gameObject);
                }
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            if ((!(x == 0 && y == 0 && z == 0)))
                            {
                                if (blockPos.x + x < 0 || blockPos.x + x > _size.x) continue;
                                if (blockPos.y + y < 0 || blockPos.y + y > _mapheight) continue;
                                if (blockPos.z + z < 0 || blockPos.z + z > _size.y) continue;

                                Vector3 _neighbour = new Vector3(blockPos.x + x, blockPos.y + y, blockPos.z + z);
                                DrawBlock(_neighbour);
                            }
                        }
                    }
                }
            }

        }
    }

    IEnumerator InitGame()
    {
       yield return StartCoroutine(MapInit(_mapInfo));
    }

    IEnumerator MapInit(Texture2D dataMapShape)
    {
        int widthX = dataMapShape.width;
        int widthZ = dataMapShape.width;

        _size = new Vector2Int(dataMapShape.width, dataMapShape.height);
        worldBlock = new Blocks[widthX, _mapheight, widthZ];

        for (int x = 0; x < widthX; x++)
        {
            for (int z = 0; z < widthZ; z++)
            {
                float height = dataMapShape.GetPixel(x, z).grayscale;
                int y = Mathf.RoundToInt(height * MapsDrawer._heightScale + _mapGroundHeightOffset) + 20;
                Vector3 pos = new Vector3(x, y, z);

                StartCoroutine(CreateBlocks(y,pos,true));
                StartCoroutine(CreateBlocks(y, pos - Vector3.down, true));

                while (y > 0)
                {
                    y--;
                    pos = new Vector3(x, y, z);
                    StartCoroutine(CreateBlocks(y, pos, false));
                }
            }
            yield return new WaitForSeconds(0.02f);
        }

        yield return null;
    }
    
    IEnumerator CreateBlocks(int y, Vector3 blockPos, bool isVisual)
    {
        
        float mineralRate = Random.Range(0, 100) < 5 ? 1 : 0;
        if (y > 37)
        {
            if (isVisual)
            {
                GameObject BlockObj = Instantiate(_prefabMapBlocks[(int)Blocks.eBlockType.Snow], blockPos, Quaternion.identity, _root);
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Blocks(Blocks.eBlockType.Snow, isVisual, BlockObj);
            }
            else
            {
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Blocks(Blocks.eBlockType.Snow, isVisual, null);
            }
        }
        else if (y > 34)
        {
            if (isVisual)
            {
                GameObject BlockObj = Instantiate(_prefabMapBlocks[(int)Blocks.eBlockType.Stone], blockPos, Quaternion.identity, _root);
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Blocks(Blocks.eBlockType.Stone, isVisual, BlockObj);
            }
            else
            {
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Blocks(Blocks.eBlockType.Stone, isVisual, null);
            }
        }
        else if (y > 29)
        {
            if (isVisual)
            {
                GameObject BlockObj = Instantiate(_prefabMapBlocks[(int)Blocks.eBlockType.Forest], blockPos, Quaternion.identity, _root);
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Blocks(Blocks.eBlockType.Forest, isVisual, BlockObj);
            }
            else
            {
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Blocks(Blocks.eBlockType.Forest, isVisual, null);
            }
        }
        else 
        {
            if (isVisual)
            {
                GameObject BlockObj = Instantiate(_prefabMapBlocks[(int)Blocks.eBlockType.Grass], blockPos, Quaternion.identity, _root);
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Blocks(Blocks.eBlockType.Grass, isVisual, BlockObj);
            }
            else
            {
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Blocks(Blocks.eBlockType.Grass, isVisual, null);
            }
        }

        if (y == 0)
        {
            if (isVisual)
            {
                GameObject BlockObj = Instantiate(_prefabMapBlocks[(int)Blocks.eBlockType.BaseMineral], blockPos, Quaternion.identity, _root);
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Blocks(Blocks.eBlockType.BaseMineral, isVisual, BlockObj);
            }
            else
            {
                worldBlock[(int)blockPos.x, (int)blockPos.y, (int)blockPos.z] = new Blocks(Blocks.eBlockType.BaseMineral, false, null);
            }
        }

        yield return null;
    }

    void DrawBlock(Vector3 blockpos)
    {
        if (worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z] == null)
        {
            return; 
        }
        if (!worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z]._isView)
        {
            GameObject newBlock = null;
            worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z]._isView = true;
            if (worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z].type == Blocks.eBlockType.Snow)
            {
                newBlock = (GameObject)Instantiate(_prefabMapBlocks[(int)Blocks.eBlockType.Snow], blockpos, Quaternion.identity,_root);
            }
            else if (worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z].type == Blocks.eBlockType.Stone)
            {
                newBlock = (GameObject)Instantiate(_prefabMapBlocks[(int)Blocks.eBlockType.Stone], blockpos, Quaternion.identity, _root);
            }
            else if (worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z].type == Blocks.eBlockType.Forest)
            {
                newBlock = (GameObject)Instantiate(_prefabMapBlocks[(int)Blocks.eBlockType.Forest], blockpos, Quaternion.identity, _root);
            }
            else if (worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z].type == Blocks.eBlockType.Grass)
            {
                newBlock = (GameObject)Instantiate(_prefabMapBlocks[(int)Blocks.eBlockType.Grass], blockpos, Quaternion.identity, _root);
            }
            else 
            {
                worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z]._isView = false;
            }

            if (newBlock != null)
            {
                worldBlock[(int)blockpos.x, (int)blockpos.y, (int)blockpos.z]._objBlock = newBlock;
            }
        }
    }

}
