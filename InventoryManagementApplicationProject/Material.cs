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
        private String groupName;
        private String extraInfo;
        private bool infinite = false;
        private double amount = 0;
        private MeasureType typeOfMeasure = MeasureType.PCS;
        private DateTime dateBought = DateTime.Now;
        private DateTime bestBefore;
        
        public Material()
        {
        }

        public Material(String name)
        {
            this.Name = name;
        }

        public Material(String name, String groupName, double amount)
        {
            this.Name = name;
            this.Amount = amount;
            this.GroupName = groupName;
        }

        public Material(String name, String groupName, bool infinite, double amount, MeasureType typeOfMeasure, 
            DateTime dateBought, DateTime bestBefore, String extraInfo)
        {
            this.Name = name;
            this.Infinite = false;
            this.Amount = amount;
            this.GroupName = groupName;
            this.TypeOfMeasure = MeasureType.PCS;
            this.DateBought = dateBought;
            this.BestBefore = BestBefore;
            this.ExtraInfo = extraInfo;
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

        public String GroupName
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

        public String ExtraInfo
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
