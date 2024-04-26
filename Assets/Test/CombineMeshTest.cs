using System.Collections;
using System.Collections.Generic;
using MapSystem.Runtime;
using UnityEngine;

public class CombineMeshTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MeshUtility.CombineMeshes(transform, true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
