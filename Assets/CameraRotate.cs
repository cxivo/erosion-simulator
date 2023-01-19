using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotate : MonoBehaviour
{
    public MeshGenerator meshGenerator;
    public Transform axis;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal") * -32f * Time.deltaTime;
        axis.Rotate(Vector3.up * x);
        
        if (Input.GetKeyUp(KeyCode.Space))
        {
            meshGenerator.newTerrain();
        }
    }
}
