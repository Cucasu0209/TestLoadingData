using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    PathCreator creator;
    Path path;
    private void OnSceneGUI()
    {
        Debug.Log("dasdasd");
        Handles.FreeMoveHandle(Vector3.zero, 0.1f, Vector3.zero, Handles.CylinderHandleCap);
        Drawaa();
    }
    void Drawaa()
    {
        Handles.color = Color.red;
        for (int i = 0; i < path.NumPoints; i++)
        {
            Vector2 newPos = Handles.FreeMoveHandle(path[i], 0.1f, Vector3.zero, Handles.CylinderHandleCap);
            if (path[i] != newPos)
            {
                Undo.RecordObject(creator, "Move point");
                path.MovePoint(i, newPos);
            }
        }
    }
    private void OnEnable()
    {


        creator = (PathCreator)target;
        if (creator.path == null)
        {

            creator.CreatePAth();
        }
        path = creator.path;
    }
}
