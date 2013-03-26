using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement
{
    public class Material : INotifyPropertyChanged
    {
        public enum MeasureType {PCS, WEIGHT, VOLUME, LENGTH};
        public enum Connection {INVENTORY, SHOPPING_LIST, RECIPE};
        
        private string name;
        private string groupName;
        private string extraInfo;
        private bool infinite = false;
        private double amount = 0;
        private MeasureType typeOfMeasure = MeasureType.PCS;
        private DateTime lastModified = DateTime.Now;
        private DateTime? bestBefore;
        private Unit displayUnit;
        private Connection belongsTo;
        
        public Material()
        {
        }

        public Material(string name)
        {
            this.Name = name;
        }

        public Material(string name, string groupName, bool infinite, double amount, MeasureType typeofMeasure, Unit displayInit, Connection belongsTo)
        {
            this.Name = name;
            this.GroupName = groupName;
            this.Infinite = infinite;
            this.Amount = amount;
            this.TypeOfMeasure = typeofMeasure;
            this.DisplayUnit = displayUnit;
            this.belongsTo = belongsTo;
        }

        public Material(string name, string groupName, bool infinite, double amount, MeasureType typeOfMeasure, 
            DateTime lastModified, DateTime? bestBefore, string extraInfo, Unit displayUnit, Connection belongsTo)
        {
            this.Name = name;
            this.Infinite = infinite;
            this.Amount = amount;
            this.GroupName = groupName;
            this.TypeOfMeasure = typeOfMeasure;
            this.LastModified = lastModified;
            this.BestBefore = bestBefore;
            this.ExtraInfo = extraInfo;
            this.DisplayUnit = displayUnit;
            this.belongsTo = belongsTo;
        }

        public Material(Material mat)
        {
            this.Name = mat.Name;
            this.Infinite = mat.Infinite;
            this.Amount = mat.Amount;
            this.GroupName = mat.GroupName;
            this.TypeOfMeasure = mat.TypeOfMeasure;
            this.LastModified = mat.LastModified;
            this.BestBefore = mat.BestBefore;
            this.ExtraInfo = mat.ExtraInfo;
            this.DisplayUnit = mat.DisplayUnit;
            this.belongsTo = mat.BelongsTo;
        }

        override public string ToString()
        {
            return this.Name + ", " + this.amount;
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
                if (value != this.name)
                {
                    this.name = value;
                    NotifyPropertyChanged();
                }                
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
                if (value != this.groupName)
                {
                    this.groupName = value;
                    NotifyPropertyChanged();
                }
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
                if (value != this.extraInfo)
                {
                    this.extraInfo = value;
                    NotifyPropertyChanged();
                }
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
                if (value != this.infinite)
                {
                    this.infinite = value;
                    NotifyPropertyChanged();
                }
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
                if (value != this.amount)
                {
                    this.amount = value;
                    NotifyPropertyChanged();
                }
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
                if (value != this.typeOfMeasure)
                {
                    this.typeOfMeasure = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DateTime LastModified
        {
            get
            {
                return lastModified;
            }
            set
            {
                if (value != this.lastModified)
                {
                    this.lastModified = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DateTime? BestBefore
        {
            get
            {
                return bestBefore;
            }
            set
            {
                if (value != this.bestBefore)
                {
                    this.bestBefore = value;
                    NotifyPropertyChanged();
                }
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
                if (value != this.displayUnit)
                {
                    this.displayUnit = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Connection BelongsTo
        {
            get 
            {
                return belongsTo;
            }
            set
            {
                this.belongsTo = value;
            }
        }

        public String GetLastModifiedString()
        {
            return this.lastModified.ToString("dd.MM.yy - HH:mm");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
