using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Networking;


public class FetchData : MonoBehaviour
{

#if UNITY_EDITOR

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            FetchLevelData("Shit1");
        }
    }
    private void FetchLevelData(string sheetName)
    {
        StartCoroutine(LoadLevelData(sheetName));
    }
    private string APIKey = "AIzaSyB_W8IQJvRMqwpCJDfAnipfKMMpJDSymi4"; // ko đổi
    private string GoogleSheetID = "1EPOpMVxsLl9r74rd0jv_nMzJ7muUmCEkXCEiExCxBx4";
    public GoogleSheetData GoogleSheetData;
    private IEnumerator LoadLevelData(string sheetName)
    {
        string link = $"https://sheets.googleapis.com/v4/spreadsheets/{GoogleSheetID}/values/{sheetName}?key={APIKey}";
        var www = UnityWebRequest.Get(link);
        yield return www.SendWebRequest();


        switch (www.result)
        {
            case UnityWebRequest.Result.InProgress:
                Debug.LogError("In Progress");
                break;
            case UnityWebRequest.Result.Success:
                //Debug.LogError(www.downloadHandler.text);
                GoogleSheetData = JsonConvert.DeserializeObject<GoogleSheetData>(www.downloadHandler.text);
                Debug.Log(www.downloadHandler.text);
                if (GoogleSheetData != null && GoogleSheetData.values.Count > 0)
                {
                    GoogleSheetData.values.RemoveAt(0);
                    for (int i = 0; i < GoogleSheetData.values.Count; i++)
                    {
                        for (int j = 0; j < GoogleSheetData.values[i].Count; j++)
                        {
                            Debug.LogError($"{i}-{j}-{GoogleSheetData.values[i][j]}");
                        }
                    }
                }
                else
                {
                    yield break;
                }
                break;
            case UnityWebRequest.Result.ConnectionError:
                Debug.LogError("Connection Error");
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError("Protocol Error");
                break;
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError("Data Processing Error");
                break;
        }
    }





#endif
}
public class GoogleSheetData
{
    public List<List<string>> values;
}