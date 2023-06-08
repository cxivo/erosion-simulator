using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using ErosionSimulator;
using ErosionSimulator.Simulators;

// for more info about the library, visit https://docs.sixlabors.com/articles/imagesharp/index.html
public class ConsoleUserInterface
{
    private static int SafelyParseInt()
    {
        int number = -1;
        try
        {
            number = Int32.Parse(Console.ReadLine());
        }
        catch (FormatException)
        {
            Console.WriteLine("Please enter a number.");
        }
        catch (OverflowException)
        {
            Console.WriteLine("The number you have inputed is too large, please enter a smaller one");
        }

        return number;
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Please save your heightmap into a file called \"input.bmp\" and then press Enter");
        Console.ReadLine();

        Image<Rgba32> image;

        try
        {
            image = Image.Load<Rgba32>("input.bmp");
        }
        catch (Exception e)
        {
            Console.WriteLine("There was an error opening the file: " + e.Message + "\nThe program will now exit.");
            return;
        }

        int sizeX = image.Width;
        int sizeY = image.Height;
        double[,] heights = new double[sizeX, sizeY];

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    // yes, my height requirements are all over the place
                    // but this should produce the best results - height from 0 to ~25
                    heights[x, y] = (row[x].R) / 10d;
                }
            }
        });

        // simulator selection
        Simulator simulator;
        int recommendedSteps = 0;

        Console.WriteLine("Select which simulator to use:\n    (a) Particle-based\n    (b) Grid-based\nPress the corresponding key");
        switch(Console.ReadLine()[0])
        {
            case 'a':
                simulator = new ParticleErosionSimulator(sizeX, sizeY, new ArrayTerrain(heights));
                recommendedSteps = 100_000;
                break;
            case 'b':
                simulator = new GridBasedSimulator(sizeX, sizeY, new ArrayTerrain(heights));
                simulator.AddWater(0.3);
                simulator.SetErosionCoefficient(0.3d, 0.2d);
                recommendedSteps = 400;
                break;
            default:
                Console.WriteLine("Invalid choice");
                return;               
        }

        

        // ask the user for number of simulation steps
        int steps = -1;
        while (steps == -1)
        {
            Console.WriteLine("Input the number of erosion steps you want to simulate (around " + recommendedSteps + " is recommended): ");
            steps = SafelyParseInt();
        }

        // simulation
        simulator.Step(steps);

        // output
        Console.WriteLine("Simulation finished.");

        // get the height values
        double[,] outputHeights = new double[sizeX, sizeY];
        double min = double.PositiveInfinity, max = double.NegativeInfinity;

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                outputHeights[x, y] = simulator.GetHeightAt(x, y);

                if (outputHeights[x, y] < min)
                {
                    min = outputHeights[x, y];
                }

                if (outputHeights[x, y] > max)
                {
                    max = outputHeights[x, y];
                }
            }
        }

        // write to image
        using (Image<Rgba32> output = new Image<Rgba32>(sizeX, sizeY))
        {
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    float brightness = (float)((outputHeights[x, y] - min) / (max - min));
                    output[x, y] = new Rgba32(brightness, brightness, brightness);
                }
            }
            output.SaveAsBmp("output.bmp");
        }

        Console.WriteLine("Image has been saved as \"output.bmp\".");
    }
}
