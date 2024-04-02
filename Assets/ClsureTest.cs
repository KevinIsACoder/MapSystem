using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClsureTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CreateTest();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateTest()
    {
        var num = 0;

        void Add()
        {
            num++;
        }

        for (int i = 0; i < 4; i++)
        {
            Add();
        }
        Debug.Log(num);
    }
}
