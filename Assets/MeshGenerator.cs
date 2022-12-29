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
    int sizeX = 200, sizeY = 200;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        Debug.Log("Started generating basic terrain...");

        System.Random random = new System.Random();
        //terrain = new SimplePerlinNoise(random.Next());
        terrain = new CompositeNoise(0.025d, new SimplePerlinNoise(random.Next()), new SimplePerlinNoise(random.Next()), new SimplePerlinNoise(random.Next()));
        //terrain = new SimpleValueNoise(sizeX, sizeY, 42d);

        simulator = new BasicParticleErosionSimulator(sizeX, sizeY, terrain);

        Debug.Log("Terrain generation done.");

        // vertices
        vertices = new Vector3[sizeX * sizeY];
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                vertices[i * sizeY + j] = new Vector3(i, 20f * (float)simulator.GetHeightAt(i, j), j);
            }
        }

        // triangles
        triangles = new int[sizeX * sizeY * 6];
        for (int i = 0; i < sizeX - 1; i++)
        {
            for (int j = 0; j < sizeY - 1; j++)
            {
                triangles[6 * (i * (sizeY - 1) + j)] = (i + 1) * sizeY + j;
                triangles[6 * (i * (sizeY - 1) + j) + 1] = i * sizeY + j;
                triangles[6 * (i * (sizeY - 1) + j) + 2] = i * sizeY + j + 1;

                triangles[6 * (i * (sizeY - 1) + j) + 3] = (i + 1) * sizeY + j;
                triangles[6 * (i * (sizeY - 1) + j) + 4] = i * sizeY + j + 1;            
                triangles[6 * (i * (sizeY - 1) + j) + 5] = (i + 1) * sizeY + j + 1;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        Debug.Log("Adding mesh done.");
    }

    private void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 265; i++)
        {
            simulator.SimulateStep();
        }

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                vertices[i * sizeY + j] = new Vector3(i, 20f * (float)simulator.GetHeightAt(i, j), j);
            }
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
