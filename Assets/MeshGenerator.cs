using ErosionSimulator;
using ErosionSimulator.Simulators;
using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    // UI elements
    public Text text;
    public Text toggleButtonText;
    public Slider waterAmountSlider;
    public Text waterAmountText;
    public Slider erosionSlider;
    public Slider collisionErosionSlider;
    public Text erosionText;
    public Text collisionErosionText;
    public Text waterSourceText;
    public Dropdown simulatorDropdown;
    public Dropdown mapDropdown;

    public MeshFilter waterObject;

    Mesh terrainMesh, waterMesh;
    Vector3[] terrainVertices, waterVertices;
    int[] triangles;
    TerrainProvider terrain;
    Simulator simulator;
    System.Random random = new System.Random();
    TerrainProvider terrainProvider;

    // simulation settings
    private bool shouldSimulate = false;
    private bool waterSource = false;
    private double waterAmount = 0.1d;
    private double erosionAmount = 0.1d, collisionErosionAmount = 0.5d;
    private int seed = -1;
    private int sizeX = 256, sizeY = 256;  // literally the limit, 65k verts per object is the limit in Unity
    private float heightMultiplier = 1f;
    private double heightMultiplier2 = 20d;
    private long stepsSimulated = 0L;
    private int stepsPerFrame = 1; //1000;

    // Start is called before the first frame update
    void Start()
    {
        // create mesh
        terrainMesh = new Mesh();
        waterMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = terrainMesh;
        waterObject.mesh = waterMesh;

        Debug.Log("Started generating basic terrain...");

        // create terrain and simulator
        terrainProvider = GetTerrainFromSeed();
        terrain = terrainProvider;
        simulator = new GridBasedSimulator(sizeX, sizeY, terrain);
        UpdateSimulator();
        NewTerrain();

        Debug.Log("Primary terrain generation done.");

        // vertices
        terrainVertices = new Vector3[sizeX * sizeY];
        waterVertices = new Vector3[sizeX * sizeY];
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
        terrainMesh.Clear();
        terrainMesh.vertices = terrainVertices;
        terrainMesh.triangles = triangles;
        terrainMesh.RecalculateNormals();

        // water mesh, too
        waterMesh.Clear();
        waterMesh.vertices = waterVertices;
        waterMesh.triangles = triangles;
        waterMesh.RecalculateNormals();

        Debug.Log("Adding mesh done.");
    }

    // use a new input terrain for generation
    public void NewTerrain()
    {
        if (seed == -1)
        {
            terrainProvider = GetTerrainFromSeed();
        } else if (seed >= 0 && seed <= 2)
        {
            random = new System.Random(seed);
            terrainProvider = GetTerrainFromSeed();
        }

        stepsSimulated = 0L;

        terrain = terrainProvider;

        KeepSimulator();
    }

    public bool IsSimulating()
    {
        return shouldSimulate;
    }

    public void ToggleSimulation()
    {
        shouldSimulate = !shouldSimulate;
        toggleButtonText.text = "Simulation " + (shouldSimulate ? "on" : "off");
    }

    public void ChangeRainAmount()
    {
        waterAmount = Math.Pow(2d, waterAmountSlider.value);
        waterAmountText.text = String.Format("water amount: {0:f4}m", waterAmount);
    }

    public void ChangeErosionAmount()
    {
        erosionAmount = Math.Pow(2d, erosionSlider.value);
        erosionText.text = String.Format("force erosion: {0:f4}", erosionAmount);
        collisionErosionAmount = Math.Pow(2d, collisionErosionSlider.value);
        collisionErosionText.text = String.Format("collision erosion: {0:f4}", collisionErosionAmount);
        simulator.SetErosionCoefficient(erosionAmount, collisionErosionAmount);
    }

    public void Rain()
    {
        simulator.AddWater(waterAmount);
    }

    public void Evaporate()
    {
        simulator.RemoveWater(waterAmount);
    }

    public void ToggleWaterSource()
    {
        waterSource = !waterSource;
        waterSourceText.text = "Water source " + (waterSource ? "on" : "off");
        simulator.SetWaterSource(waterSource);
    }

    // resets the simulator - either from an existing terrain, or a new one as well
    private void ChangeOrKeepSimulator(TerrainProvider terrainProvider) {
        switch (simulatorDropdown.value)
        {
            case 0:
                simulator = new ParticleErosionSimulator(sizeX, sizeY, terrainProvider);
                stepsPerFrame = 1000;
                break;
            case 1:
                simulator = new GridBasedSimulator(sizeX, sizeY, terrainProvider, new GridBasedSimulator.TerrainProcessingModule[] {
                    new GridBasedSimulator.ForceErosionModule(),
                    new GridBasedSimulator.DissolutionErosionModule()
                });
                stepsPerFrame = 1;
                break;
            case 2:
                simulator = new GridBasedSimulator(sizeX, sizeY, terrainProvider, new GridBasedSimulator.TerrainProcessingModule[] {
                    new GridBasedSimulator.ForceErosionModule(),
                    new GridBasedSimulator.CollisionErosionModule(),
                    new GridBasedSimulator.DissolutionErosionModule()
                });
                stepsPerFrame = 1;
                break;
            case 3:
                simulator = new GridBasedSimulator(sizeX, sizeY, terrainProvider, new GridBasedSimulator.TerrainProcessingModule[] {
                    new GridBasedSimulator.ForceErosionModule(),
                    new GridBasedSimulator.VectorCollisionErosionModule(),
                    new GridBasedSimulator.DissolutionErosionModule()
                });
                stepsPerFrame = 1;
                break;
            default:
                Debug.LogError("Unknown option");
                break;
        }
        UpdateSimulator();
    }

    public void ChangeSimulator()
    {
        ChangeOrKeepSimulator(simulator);
    }

    public void KeepSimulator()
    {
        ChangeOrKeepSimulator(terrain);
    }

    public void ChangeMap()
    {
        // stop simulation
        if (shouldSimulate)
        {
            ToggleSimulation();
        }

        // input maps
        switch (mapDropdown.value)
        {
            case 0:
                seed = -1;
                random = new System.Random();
                terrainProvider = GetTerrainFromSeed();
                NewTerrain();
                break;
            case 1:
                seed = 0;
                random = new System.Random(0);
                terrainProvider = GetTerrainFromSeed();
                NewTerrain();
                break;
            case 2:
                seed = 1;
                random = new System.Random(1);
                terrainProvider = GetTerrainFromSeed();
                NewTerrain();
                break;
            case 3:
                seed = 2;
                random = new System.Random(2);
                terrainProvider = GetTerrainFromSeed();
                NewTerrain();
                break;
            case 4:
                seed = -2;
                terrainProvider = new TerrainTranslator(new TextureHeightProvider("Assets/rivers.png"), 40d, -30d);
                NewTerrain();
                break;
            case 5:
                seed = -2;
                terrainProvider = new TerrainTranslator(new TextureHeightProvider("Assets/obstacles.png"), 40d, -30d);
                NewTerrain();
                break;
            default:
                Debug.LogError("Unknown option");
                break;
        }

    }

    // returns a composite of Perlin noises of increasing frequencyc and decreasing amplitude (6 layers in this case)
    private TerrainProvider GetTerrainFromSeed()
    {
        return new TerrainTranslator(new CompositeNoise(0.01d, 2d,
            new PerlinNoise(random.Next()),
            new PerlinNoise(random.Next()),
            new PerlinNoise(random.Next()),
            new PerlinNoise(random.Next()),
            new PerlinNoise(random.Next()),
            new PerlinNoise(random.Next())
            ), heightMultiplier2);
    }

    private void UpdateSimulator()
    {
        // set values from UI
        simulator.SetWaterSource(waterSource);
        simulator.SetErosionCoefficient(erosionAmount, collisionErosionAmount);
    }

    private void UpdateVertices()
    {
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                terrainVertices[i * sizeY + j] = new Vector3(i, heightMultiplier * (float)simulator.GetHeightAt(i, j), j);
                waterVertices[i * sizeY + j] = new Vector3(i, heightMultiplier * (float)simulator.GetWaterHeightAt(i, j), j);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (shouldSimulate)
        {
            // simulate some steps every frame
            simulator.Step(stepsPerFrame);
            stepsSimulated += stepsPerFrame;
        }

        text.text = "Steps simulated: " + stepsSimulated;

        // update the mesh
        UpdateVertices();

        terrainMesh.vertices = terrainVertices;
        terrainMesh.RecalculateNormals();

        waterMesh.vertices = waterVertices;
        waterMesh.RecalculateNormals();
    }
}
