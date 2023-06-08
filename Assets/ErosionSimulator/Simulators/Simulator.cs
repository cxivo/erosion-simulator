using ErosionSimulator.Geology;
using System;
using System.Collections.Generic;
using System.Text;

namespace ErosionSimulator
{
    public abstract class Simulator : TerrainProvider
    {
        public abstract void Step();

        // the default implementation returns a value lower than the terrain height
        // to prevent water from being seen (unless you look from the underside, which isn't a problem)
        public virtual double GetWaterHeightAt(double x, double y)
        {
            return GetHeightAt(x, y) - 0.1d;
        }

        // a lot of the methods do nothing by default
        public virtual void AddWater(double height) { }

        public virtual void RemoveWater(double height) { }

        public virtual void SetErosionCoefficient(double value, double forceValue) { }

        public virtual void SetWaterSource(bool state) { }

        // simulate n steps at once
        public void Step(int n)
        {
            for (int i = 0; i < n; i++)
            {
                Step();
            }
        }
    }
}
