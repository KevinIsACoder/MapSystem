using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SplineTest : MonoBehaviour
{
    private Mesh m_mesh;
    private void OnEnable()
    {
        if (!m_mesh)
            m_mesh = new Mesh();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
