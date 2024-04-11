
using UnityEngine;

public class Test : MonoBehaviour
{
    public GameObject cube;
    // Start is called before the first frame update
    void Start()
    {
        var cube_1 = Instantiate(cube);
        var hit = new RaycastHit();
        if (Physics.Raycast(cube_1.transform.position, -cube_1.transform.up, out hit, 1))
        {
            Debug.Log(hit.transform.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
