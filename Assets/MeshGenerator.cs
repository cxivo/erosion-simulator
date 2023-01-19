using ErosionSimulator;
using ErosionSimulator.Simulators;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    public Text text;
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    TerrainProvider terrain;
    Simulator simulator;
    int sizeX = 256, sizeY = 256;  // literally the limit, 65k verts per object is the limit in Unity
    float heightMultiplier = 20f;
    long stepsSimulated = 0L;
    const int STEPS_PER_FRAME = 1000;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        Debug.Log("Started generating basic terrain...");

        newTerrain();

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

    public void newTerrain()
    {
        stepsSimulated = 0L;

        System.Random random = new System.Random();
        terrain = new CompositeNoise(0.01d, 2d,
            new PerlinNoise(random.Next()),
            new PerlinNoise(random.Next()),
            new PerlinNoise(random.Next()),
            new PerlinNoise(random.Next()),
            new PerlinNoise(random.Next()),
            new PerlinNoise(random.Next()));

        simulator = new BasicParticleErosionSimulator(sizeX, sizeY, terrain);
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
        simulator.Step(STEPS_PER_FRAME);
        stepsSimulated += STEPS_PER_FRAME;

        text.text = "Use A & D to rotate, SPACE for next terrain. Steps simulated: " + stepsSimulated;

        // update the mesh
        UpdateVertices();
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
