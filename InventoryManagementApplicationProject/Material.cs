using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement
{
    public class Material
    {
        public enum MeasureType {PCS, WEIGHT, VOLUME, LENGTH}; 
        
        private string name;
        private string groupName;
        private string extraInfo;
        private bool infinite = false;
        private double amount = 0;
        private MeasureType typeOfMeasure = MeasureType.PCS;
        private DateTime dateBought = DateTime.Now;
        private DateTime bestBefore;
        private Unit displayUnit;
        
        public Material()
        {
        }

        public Material(string name)
        {
            this.Name = name;
        }

        public Material(string name, string groupName, double amount)
        {
            this.Name = name;
            this.Amount = amount;
            this.GroupName = groupName;
        }

        public Material(string name, string groupName, bool infinite, double amount, MeasureType typeOfMeasure, 
            DateTime dateBought, DateTime bestBefore, string extraInfo, Unit displayUnit)
        {
            this.Name = name;
            this.Infinite = false;
            this.Amount = amount;
            this.GroupName = groupName;
            this.TypeOfMeasure = MeasureType.PCS;
            this.DateBought = dateBought;
            this.BestBefore = BestBefore;
            this.ExtraInfo = extraInfo;
            this.DisplayUnit = displayUnit;
        }


        override public string ToString()
        {
            return name + ", " + amount;
        }

        public void AddAmount(double amount)
        {
            this.Amount += amount;
        }

        // ACCESSORS

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

        public string GroupName
        {
            get
            {
                return groupName;
            }
            set
            {
                groupName = value;
            }
        }

        public string ExtraInfo
        {
            get
            {
                return extraInfo;
            }
            set
            {
                extraInfo = value;
            }
        }

        public bool Infinite
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

        public DateTime DateBought
        {
            get
            {
                return dateBought;
            }
            set
            {
                dateBought = value;
            }
        }

        public DateTime BestBefore
        {
            get
            {
                return bestBefore;
            }
            set
            {
                bestBefore = value;
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

        public Unit DisplayUnit
        {
            get
            {
                return displayUnit;
            }
            set
            {
                displayUnit = value;
            }
        }
    }
}
