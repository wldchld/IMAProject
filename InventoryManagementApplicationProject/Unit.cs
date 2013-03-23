using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement
{
    public class Unit
    {
        public const byte UNIT_BOTH = 0;
        public const byte UNIT_SI = 1;
        public const byte UNIT_US = 2;
        
        private string name;
        private double factor;
        private Unit baseUnit;
        private byte metrology;
        private Material.MeasureType typeOfMeasure;

        public Unit(string name, double factor, Unit baseUnit, byte metrology, Material.MeasureType typeOfMeasure)
        {
            this.Name = name;
            this.Factor = factor;
            this.BaseUnit = baseUnit;
            this.Metrology = metrology;
            this.TypeOfMeasure = typeOfMeasure;
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public double Factor
        {
            get
            {
                return factor;
            }
            set
            {
                factor = value;
            }
        }

        public Unit BaseUnit
        {
            get
            {
                return baseUnit;
            }
            set
            {
                baseUnit = value;
            }
        }


        public byte Metrology
        {
            get
            {
                return metrology;
            }
            set
            {
                metrology = value;
            }
        }

        public Material.MeasureType TypeOfMeasure
        {
            get
            {
                return typeOfMeasure;
            }
            set
            {
                typeOfMeasure = value;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        //-----------------------------------------------

        // base units
        public static readonly Unit PCS = new Unit("pcs", 1, null, 0, Material.MeasureType.PCS);
        public static readonly Unit G = new Unit("g", 1, null, 1, Material.MeasureType.WEIGHT);
        public static readonly Unit L = new Unit("l", 1, null, 1, Material.MeasureType.VOLUME);
        public static readonly Unit M = new Unit("m", 1, null, 1, Material.MeasureType.LENGTH);

        // derived units
        public static readonly Unit KG = new Unit("kg", 1000, G, 1, Material.MeasureType.WEIGHT);
        public static readonly Unit T = new Unit("t", 1000000, G, 1, Material.MeasureType.WEIGHT);
        public static readonly Unit OZ = new Unit("oz", 28.3495231, G, 2, Material.MeasureType.WEIGHT);
        public static readonly Unit LB = new Unit("lb", 453.59237, G, 2, Material.MeasureType.WEIGHT);
        public static readonly Unit ST = new Unit("st", 6350.29318, G, 2, Material.MeasureType.WEIGHT);
        public static readonly Unit DL = new Unit("dl", 0.1, L, 1, Material.MeasureType.VOLUME);
        public static readonly Unit TSP = new Unit("tsp", 0.005, L, 0, Material.MeasureType.VOLUME);
        public static readonly Unit TBSP = new Unit("tbsp", 0.015, L, 0, Material.MeasureType.VOLUME);
        public static readonly Unit PT = new Unit("pt", 0.473176473, L, 2, Material.MeasureType.VOLUME);
        public static readonly Unit GAL = new Unit("gal", 3.78541178, L, 2, Material.MeasureType.VOLUME);
        public static readonly Unit CM = new Unit("cm", 0.01, M, 1, Material.MeasureType.LENGTH);
        public static readonly Unit DM = new Unit("dm", 0.1, M, 1, Material.MeasureType.LENGTH);
        public static readonly Unit IN = new Unit("in", 0.0254, M, 2, Material.MeasureType.LENGTH);
        public static readonly Unit FT = new Unit("ft", 0.3048, M, 2, Material.MeasureType.LENGTH);
        public static readonly Unit YD = new Unit("yd", 0.9144, M, 2, Material.MeasureType.LENGTH);
        public static readonly Unit MI = new Unit("mi", 1609.344, M, 2, Material.MeasureType.LENGTH);
    }
}
