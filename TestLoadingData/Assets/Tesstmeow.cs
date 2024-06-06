using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tesstmeow : MonoBehaviour
{
    public MatrixMap map;
    public int numberche = 10;
    // Start is called before the first frame update
    void Start()
    {
        map.LoadDataFromFile();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < numberche; i++)
        {
            map.isMarked(Random.Range(0, map.row), Random.Range(0, map.column));
        }
    }
}
