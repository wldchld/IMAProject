using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement
{
    class Material
    {
        public enum MeasureType {PCS, WEIGHT, VOLUME, LENGTH}; 
        
        private String name;
        private bool infinite;
        private double amount;
        private MeasureType typeOfMeasure;
        //private Dictionary<String, Object> extraInfo

        public Material(String name, double amount = 1)
        {
            this.Name = name;
            this.Infinite = false;
            this.Amount = amount;
            this.TypeOfMeasure = MeasureType.PCS;
        }

        override public String ToString()
        {
            return name + ", " + amount;
        }

        public void AddAmount(double amount)
        {
            this.Amount += amount;
        }

        // ACCESSORS

        public String Name
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

        private bool Infinite
        {
            get
            {
                return infinite;
            }
            set
            {
                infinite = value;
            }
        }

        public double Amount
        {
            get
            {
                return amount;
            }
            set
            {
                amount = value;
            }
        }

        public MeasureType TypeOfMeasure
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
    }
}
