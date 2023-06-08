namespace ErosionSimulator
{
    // multiplies the height of the provided terrain and adds a constant to the height
    public class TerrainTranslator : TerrainProvider
    {
        public TerrainProvider Original { get; set; }
        public double Multiplier { get; set; } = 1d;
        public double Addition { get; set; } = 0d;

        public TerrainTranslator(TerrainProvider original)
        {
            Original = original;
        }

        public TerrainTranslator(TerrainProvider original, double multiplier)
        {
            Original = original;
            Multiplier = multiplier;
        }

        public TerrainTranslator(TerrainProvider original, double multiplier, double addition)
        {
            Original = original;
            Multiplier = multiplier;
            Addition = addition;
        }

        public override double GetHeightAt(double x, double y)
        {
            return Multiplier * Original.GetHeightAt(x, y) + Addition;
        }
    }
}