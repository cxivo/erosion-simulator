using System;
using System.IO;
using UnityEngine;


namespace ErosionSimulator
{
    // only works in Unity Engine
    // loads terrain data from a grayscale heightmap
    public class TextureHeightProvider : TerrainProvider
    {
        Texture2D texture = null;

        public TextureHeightProvider(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            texture = new Texture2D(2, 2);
            texture.LoadImage(data);
        }


        public override double GetHeightAt(double x, double y)
        {
            int lowX = (int)Math.Floor(x);
            int highX = (int)Math.Ceiling(x);
            int lowY = (int)Math.Floor(y);
            int highY = (int)Math.Ceiling(y);

            // interpolate
            return InterpolateSmooth(
                InterpolateSmooth(texture.GetPixel(lowX, lowY).r, texture.GetPixel(lowX, highY).r, y - lowY),
                InterpolateSmooth(texture.GetPixel(highX, lowY).r, texture.GetPixel(highX, highY).r, y - lowY), x - lowX);
        }
    }
}