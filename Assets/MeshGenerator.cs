using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    SimpleTerrainProvider terrain;
    BasicParticleErosionSimulator simulator;
    int sizeX = 256, sizeY = 256;  // literally the limit, 65k verts per object is the limit in Unity
    float heightMultiplier = 20f;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        Debug.Log("Started generating basic terrain...");

        System.Random random = new System.Random();
        //terrain = new SimplePerlinNoise(random.Next());
        terrain = new CompositeNoise(0.025d, 1d, 
            new SimplePerlinNoise(random.Next()), 
            new SimplePerlinNoise(random.Next()),
            new SimplePerlinNoise(random.Next()),
            new SimplePerlinNoise(random.Next()),
            new SimplePerlinNoise(random.Next()));
        //terrain = new SimpleValueNoise(sizeX, sizeY, 42d);

        simulator = new BasicParticleErosionSimulator(sizeX, sizeY, terrain);

        Debug.Log("Primary terrain generation done.");

        // vertices
        vertices = new Vector3[sizeX * sizeY];
        UpdateVertices();

        // triangles
        // do not try to understand this, it's just how one adds triangles between vertices
        triangles = new int[sizeX * sizeY * 6];
        for (int i = 0; i < sizeX - 1; i++)
        {
            for (int j = 0; j < sizeY - 1; j++)
            {
                // first triangle
                triangles[6 * (i * (sizeY - 1) + j)] = (i + 1) * sizeY + j;
                triangles[6 * (i * (sizeY - 1) + j) + 1] = i * sizeY + j;
                triangles[6 * (i * (sizeY - 1) + j) + 2] = i * sizeY + j + 1;

                //second triangle
                triangles[6 * (i * (sizeY - 1) + j) + 3] = (i + 1) * sizeY + j;
                triangles[6 * (i * (sizeY - 1) + j) + 4] = i * sizeY + j + 1;            
                triangles[6 * (i * (sizeY - 1) + j) + 5] = (i + 1) * sizeY + j + 1;
            }
        }

        // tell Unity to display the mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        Debug.Log("Adding mesh done.");
    }

    private void UpdateVertices()
    {
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                vertices[i * sizeY + j] = new Vector3(i, heightMultiplier * (float)simulator.GetHeightAt(i, j), j);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // simulate some steps every frame
        for (int i = 0; i < 256; i++)
        {
            simulator.SimulateStep();
        }

        // update the mesh
        UpdateVertices();
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
