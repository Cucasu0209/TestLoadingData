using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{

    public Path path;

    public void CreatePAth()
    {
        path = new Path(transform.position);
    }
}
