using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class MatrixMap : MonoBehaviour
{
    #region Variables
    public GameObject GreenBlock;
    public GameObject RedBlock;
    public Dictionary<string, GameObject> BlockDictionaty = new Dictionary<string, GameObject>();
    public Dictionary<Transform, string> BlockFollowing = new Dictionary<Transform, string>();
    public Dictionary<Transform, float> BlockExpanding = new Dictionary<Transform, float>();
    //pure data
    public float AreaSize = 13;
    public int BrushSize = 1;
    [HideInInspector] public int column = 1;
    [HideInInspector] public int row = 1;
    [HideInInspector]
    public Vector3[] c = new Vector3[4] {
        new Vector3(0,0,0),
        new Vector3(0,0,13),
        new Vector3(13,0,13),
        new Vector3(13,0,0)
    };
    private const int MAXCEL = 400;

    //data using when setup
    [HideInInspector] public string data;
    Dictionary<string, bool> CellMarked = new Dictionary<string, bool>();
    private Vector3 lastdownleft = Vector3.one * 99;
    private float lastSize = -1;

    //data using to save file
    private string RealData;
    private string filePath;
    public string dataFileName = "data";
    #endregion

    #region Mono
    private void OnEnable()
    {
        LoadDataFromFile();
    }
    #endregion

    #region public event called to setup
    public void UpdateSquare(Vector3[] _c, bool recall = false)
    {

        LoadCellMark();

        int i;
        row = Mathf.Abs(Mathf.RoundToInt((Getdownleft().x - GetupRight().x) / AreaSize));
        column = Mathf.Abs(Mathf.RoundToInt((Getdownleft().z - GetupRight().z) / AreaSize));
        for (i = 0; i < c.Length; i++)
        {
            _c[i] = new Vector3(_c[i].x, 0, _c[i].z);
            if (Vector3.Distance(_c[i], c[i]) > 0.01f)
            {


                if (row > MAXCEL)
                {
                    AreaSize = (_c[i].x - c[(i + 2) % 4].x) / MAXCEL;
                    row = MAXCEL;
                }
                if (column > MAXCEL)
                {
                    AreaSize = (_c[i].z - c[(i + 2) % 4].z) / MAXCEL;
                    column = MAXCEL;
                }
                c[i].x = c[(i + 2) % 4].x + Mathf.RoundToInt((_c[i].x - c[(i + 2) % 4].x) / AreaSize) * AreaSize;
                c[i].z = c[(i + 2) % 4].z + Mathf.RoundToInt((_c[i].z - c[(i + 2) % 4].z) / AreaSize) * AreaSize;


                if (c[(i + 1) % 4].z != c[(i + 2) % 4].z)
                {
                    c[(i + 1) % 4].z = c[i].z;
                    c[(i + 3) % 4].x = c[i].x;
                }
                else
                {
                    c[(i + 1) % 4].x = c[i].x;
                    c[(i + 3) % 4].z = c[i].z;
                }
                return;
            }
        }

        i = 0;
        if (recall)
        {
            if (row > MAXCEL)
            {
                AreaSize = (_c[i].x - c[(i + 2) % 4].x) / MAXCEL;
                row = MAXCEL;
            }
            if (column > MAXCEL)
            {
                AreaSize = (_c[i].z - c[(i + 2) % 4].z) / MAXCEL;
                column = MAXCEL;
            }
            c[i].x = c[(i + 2) % 4].x + Mathf.RoundToInt((_c[i].x - c[(i + 2) % 4].x) / AreaSize) * AreaSize;
            c[i].z = c[(i + 2) % 4].z + Mathf.RoundToInt((_c[i].z - c[(i + 2) % 4].z) / AreaSize) * AreaSize;


            if (c[(i + 1) % 4].z != c[(i + 2) % 4].z)
            {
                c[(i + 1) % 4].z = c[i].z;
                c[(i + 3) % 4].x = c[i].x;
            }
            else
            {
                c[(i + 1) % 4].x = c[i].x;
                c[(i + 3) % 4].z = c[i].z;
            }
        }
    }
    public bool isMarked(int i, int j)
    {
        return CellMarked.ContainsKey(getKey(i, j));
    }

    public bool CheckCellMarked(int i, int j)
    {
        return CellMarked.ContainsKey(getKey(i, j));
    }
    public void MarkCell(int i, int j)
    {
        if (CellMarked.ContainsKey(getKey(i, j)) == false && i < row && j < column)
        {
            CellMarked.Add(getKey(i, j), true);
        }
    }

    public void UnmarkCell(int i, int j)
    {
        CellMarked.Remove(getKey(i, j));

    }

    #endregion

    #region Get Infomation about grid
    private string getKey(int i, int j) => i + "_" + j;
    private string getKey(Vector2Int index) => index.x + "_" + index.y;
    private Vector2Int getKey(string i)
    {
        string[] a = i.Split('_');
        return new Vector2Int(int.Parse(a[0]), int.Parse(a[1]));
    }
    private Vector2 GetCenter(int i, int j) => new Vector2(Getdownleft().x + (i + 0.5f) * AreaSize, Getdownleft().z + (j + 0.5f) * AreaSize);
    public Vector3 Getupleft() => new Vector3(Mathf.Max(c[1].x, c[3].x), 0, Mathf.Min(c[1].z, c[3].z));
    public Vector3 GetupRight() => new Vector3(Mathf.Max(c[1].x, c[3].x), 0, Mathf.Max(c[1].z, c[3].z));
    public Vector3 GetdownRight() => new Vector3(Mathf.Min(c[1].x, c[3].x), 0, Mathf.Max(c[1].z, c[3].z));
    public Vector3 Getdownleft() => new Vector3(Mathf.Min(c[1].x, c[3].x), 0, Mathf.Min(c[1].z, c[3].z));
    #endregion

    #region save and load using on seting up
    private void SaveCellMark()
    {
        string result = "";

        foreach (var key in CellMarked.Keys)
        {
            SquareInfo sq = new SquareInfo()
            {
                center = GetCenter(getKey(key).x, getKey(key).y),
                size = AreaSize
            };
            result += sq.ToString() + "_";
        }

        //PlayerPrefs.SetString("Marked", result);
        data = result;

    }
    private void LoadCellMark()
    {
        if (Vector3.Distance(lastdownleft, Getdownleft()) < 0.1f && Mathf.Abs(lastSize - AreaSize) < 0.1f) return;
        SaveCellMark();
        lastdownleft = Getdownleft();
        lastSize = AreaSize;
        CellMarked = new Dictionary<string, bool>();
        //string result = PlayerPrefs.GetString("Marked", "");
        string result = data;
        if (data != null)
        {
            string[] keys = result.Split('_');
            SquareInfo newSq;
            if (keys != null && keys.Length > 0)
                for (int i = 0; i < keys.Length; i++)
                {
                    newSq = SquareInfo.GetInfoByString(keys[i]);
                    if (newSq != null)
                    {
                        List<Vector2Int> IndexMArked = newSq.getSqsWrapedInBigSquare(new Vector2(Getdownleft().x, Getdownleft().z), AreaSize);
                        foreach (var index in IndexMArked)
                        {
                            if (CellMarked.ContainsKey(getKey(index.x, index.y)) == false)
                                CellMarked.Add(getKey(index.x, index.y), true);
                        }
                    }
                }
        }

    }
    public void ClearAllCellMarked()
    {
        data = "";
        CellMarked.Clear();
        //PlayerPrefs.SetString("Marked", "");
    }
    #endregion

    #region Save and load file
    public void GetDataFromFile()
    {
        MapInfo inf = new MapInfo();
        inf.Size = AreaSize;
        inf.MaxX = GetupRight().x;
        inf.MaxZ = GetupRight().z;
        inf.MinX = Getdownleft().x;
        inf.MinZ = Getdownleft().z;
        inf.MarkedCells = new List<int>();
        int x, y;
        int size;
        Dictionary<string, bool> cache = new Dictionary<string, bool>(CellMarked);
        for (int a = 0; a < row; a++)
        {
            for (int b = 0; b < column; b++)
            {
                string key = getKey(a, b);
                if (cache.ContainsKey(key))
                {
                    size = 1;
                    x = getKey(key).x;
                    y = getKey(key).y;
                    for (int i = 2; i < 300; i++)
                    {
                        bool isNotOK = false;
                        for (int i1 = 0; i1 <= size; i1++)
                        {
                            for (int i2 = 0; i2 <= size; i2++)
                            {
                                if (cache.ContainsKey(getKey(x + i1, y + i2)) == false)
                                {
                                    isNotOK = true;
                                    break;
                                }
                            }
                            if (isNotOK) break;
                        }

                        if (isNotOK) break;
                        size++;
                    }
                    for (int i1 = 0; i1 < size; i1++)
                    {
                        for (int i2 = 0; i2 < size; i2++)
                        {
                            cache.Remove(getKey(x + i1, y + i2));
                        }
                    }
                    inf.MarkedCells.Add(x);
                    inf.MarkedCells.Add(y);
                    inf.MarkedCells.Add(size);
                }
            }
        }
        RealData = inf.ToString();
        SaveStringToFile(RealData);
    }
    public MapInfo LoadMapSaved()
    {
        return MapInfo.ParseData(RealData);
    }
    public void LoadDataFromFile()
    {
        RealData = ReadStringFromFile();
        MapInfo mapInfo = MapInfo.ParseData(RealData);

        c[0] = new Vector3(mapInfo.MinX, 0, mapInfo.MaxZ);
        c[1] = new Vector3(mapInfo.MaxX, 0, mapInfo.MaxZ);
        c[2] = new Vector3(mapInfo.MaxX, 0, mapInfo.MinZ);
        c[3] = new Vector3(mapInfo.MinX, 0, mapInfo.MinZ);
        AreaSize = mapInfo.Size;

        CellMarked = new Dictionary<string, bool>();

        for (int i = 0; i < mapInfo.MarkedCells.Count / 3; i++)
        {
            int x = mapInfo.MarkedCells[i * 3];
            int y = mapInfo.MarkedCells[i * 3 + 1];
            int size = mapInfo.MarkedCells[i * 3 + 2];
            for (int i1 = 0; i1 < size; i1++)
            {
                for (int i2 = 0; i2 < size; i2++)
                {
                    CellMarked.Add(getKey(i1 + x, i2 + y), true);
                }
            }
        }


        SaveCellMark();
        UpdateSquare(c, true);
    }
    private void SaveStringToFile(string content)
    {
        filePath = System.IO.Path.Combine(Application.dataPath, dataFileName + ".txt");

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath, false)) // false để ghi đè lên file nếu tồn tại
            {
                writer.WriteLine(content);
                Debug.Log("File written successfully.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error writing to file: " + ex.Message);
        }
    }
    private string ReadStringFromFile()
    {
        filePath = System.IO.Path.Combine(Application.dataPath, dataFileName + ".txt");
        try
        {
            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string content = reader.ReadToEnd();
                    Debug.Log("File read successfully.");
                    return content;
                }
            }
            else
            {
                Debug.LogWarning("File does not exist.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error reading from file: " + ex.Message);
            return null;
        }
    }
    #endregion

    #region GamePlayCallbacks

    //    public void RegisterTransformFollow(Transform target)
    //    {
    //#if UNITY_EDITOR
    //        if (BlockFollowing.ContainsKey(target) == false)
    //            BlockFollowing.Add(target, getKey(-1, -1));
    //        if (BlockExpanding.ContainsKey(target) == false)
    //            BlockExpanding.Add(target, -1);
    //#endif
    //    }
    //    public void UpdateFollowing(Transform target, float expand)
    //    {
    //#if UNITY_EDITOR
    //        if (getKey(GetCellIndexByPos(target.position)) != BlockFollowing[target] || expand != BlockExpanding[target])
    //        {
    //            if (Mathf.FloorToInt(expand / AreaSize) < 1)
    //            {
    //                HideCell(getKey(BlockFollowing[target]));
    //                DisplayCell(getKey(getKey(GetCellIndexByPos(target.position))));
    //            }

    //            HideCircle(getKey(BlockFollowing[target]), Mathf.FloorToInt(BlockExpanding[target] / AreaSize));
    //            DisplayCircle(GetCellIndexByPos(target.position), Mathf.FloorToInt(expand / AreaSize));


    //            BlockFollowing[target] = getKey(GetCellIndexByPos(target.position));
    //            BlockExpanding[target] = expand;
    //        }
    //#endif
    //    }
    //    public void UnsubTransformFollow(Transform target)
    //    {
    //#if UNITY_EDITOR
    //        BlockFollowing.Remove(target);
    //#endif
    //    }
    public Vector2Int GetCellIndexByPos(Vector3 pos)
    {
        return new Vector2Int(Mathf.CeilToInt((pos.x - Getdownleft().x) / AreaSize) - 1,
           Mathf.CeilToInt((pos.z - Getdownleft().z) / AreaSize) - 1);
    }
    public Vector3 GetRealWorldPosByIndex(Vector2Int index)
    {
        return new Vector3(Getdownleft().x + AreaSize * (index.x + 0.5f), 0, Getdownleft().z + AreaSize * (index.y + 0.5f));
    }
    //    public void DisplayCell(Vector2Int index)
    //    {
    //#if UNITY_EDITOR
    //        if (BlockDictionaty.ContainsKey(getKey(index)) == false)
    //        {
    //            GameObject NewObj = LeanPool.Spawn(isMarked(index.x, index.y) ? GreenBlock : RedBlock);
    //            NewObj.transform.position = Getdownleft() + Vector3.right * (index.x + 0.5f) * AreaSize + Vector3.forward * (index.y + 0.5f) * AreaSize + Vector3.up * (23.35f);
    //            NewObj.transform.localScale = new Vector3(AreaSize, 1, AreaSize);
    //            BlockDictionaty[getKey(index)] = NewObj;
    //        }

    //#endif
    //    }
    //    public void DisplayCircle(Vector2Int center, int radious)
    //    {
    //        for (int i = -radious; i <= radious; i++)
    //        {
    //            for (int j = -radious; j <= radious; j++)
    //            {
    //                if ((i * i + j * j) < radious * radious)
    //                {

    //                    DisplayCell(new Vector2Int(Mathf.Clamp(center.x + i, 0, row - 1), Mathf.Clamp(center.y + j, 0, column - 1)));
    //                }
    //            }
    //        }
    //    }
    //    public void HideCircle(Vector2Int center, int radious)
    //    {
    //        for (int i = -radious; i <= radious; i++)
    //        {
    //            for (int j = -radious; j <= radious; j++)
    //            {
    //                if ((i * i + j * j) < radious * radious)
    //                {
    //                    HideCell(new Vector2Int(Mathf.Clamp(center.x + i, 0, row - 1), Mathf.Clamp(center.y + j, 0, column - 1)));
    //                }
    //            }
    //        }
    //    }
    //    public void HideCell(Vector2Int index)
    //    {
    //#if UNITY_EDITOR
    //        if (BlockDictionaty.ContainsKey(getKey(index)))
    //        {
    //            LeanPool.Despawn(BlockDictionaty[getKey(index)]);
    //            BlockDictionaty.Remove(getKey(index));
    //        }
    //#endif
    //    }
    //    public Vector3 GetRandomPositionInCircle(Transform target, float expand)
    //    {
    //        int radious = Mathf.FloorToInt(expand / AreaSize);
    //        Vector2Int center = getKey(BlockFollowing[target]);
    //        List<Vector2Int> result = new List<Vector2Int>();
    //        for (int i = -radious; i <= radious; i++)
    //        {
    //            for (int j = -radious; j <= radious; j++)
    //            {
    //                if ((i * i + j * j) < radious * radious)
    //                {
    //                    result.Add(new Vector2Int(Mathf.Clamp(center.x + i, 0, row - 1), Mathf.Clamp(center.y + j, 0, column - 1)));
    //                }
    //            }
    //        }
    //        Vector2Int randomresult = result[UnityEngine.Random.Range(0, result.Count)];
    //        return Getdownleft() + new Vector3((randomresult.x + 0.5f) * AreaSize, 30, (randomresult.y + 0.5f) * AreaSize);
    //    }
    #endregion
}
public class SquareInfo
{
    public Vector2 center;
    public float size;
    public override string ToString()
    {
        return center.x + "," + center.y + "," + size;
    }

    public static SquareInfo GetInfoByString(string data)
    {
        string[] pieces = data.Split(',');
        if (pieces.Length >= 3)
        {
            float centerx = float.Parse(pieces[0]);
            float centery = float.Parse(pieces[1]);
            float sizea = float.Parse(pieces[2]);
            return new SquareInfo()
            {
                center = new Vector2(centerx, centery),
                size = sizea
            };
        }
        return null;
    }
    public List<Vector2Int> getSqsWrapedInBigSquare(Vector2 topleftBig, float sizePiece)
    {
        int maxX = Mathf.CeilToInt((center.x + size / 2 - topleftBig.x) / sizePiece - 0.1f);
        int minX = Mathf.FloorToInt((center.x - size / 2 - topleftBig.x) / sizePiece + 0.1f);

        int maxY = Mathf.CeilToInt((center.y + size / 2 - topleftBig.y) / sizePiece - 0.1f);
        int minY = Mathf.FloorToInt((center.y - size / 2 - topleftBig.y) / sizePiece + 0.1f);

        List<Vector2Int> result = new List<Vector2Int>();

        for (int i = minX; i < maxX; i++)
        {
            for (int j = minY; j < maxY; j++)
                result.Add(new Vector2Int(i, j));
        }
        return result;
    }
}

[Serializable]
public class MapInfo
{
    public float MaxX;
    public float MinX;
    public float MaxZ;
    public float MinZ;
    public float Size;

    public List<int> MarkedCells;



    public void SaveData()
    {

    }
    public override string ToString()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
    public static MapInfo ParseData(string data)
    {
        MapInfo info = JsonConvert.DeserializeObject<MapInfo>(data);
        return info;
    }
    public int GetRow() => Mathf.RoundToInt((MaxX - MinX) / Size);
    public int GetColumn() => Mathf.RoundToInt((MaxZ - MinZ) / Size);
}