using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement
{
    public class ShoppingList
    {
        private string name;
        private List<Material> content;

        public ShoppingList()
        {
            content = new List<Material>();
        }

        public ShoppingList(string name)
        {
            this.Name = name;
            content = new List<Material>();
        }

        public void AddToContent(Material material)
        {
            if (material != null)
            {
                bool add = false;
                for (int i = 0; i < content.Count; i++)
                {
                    if (content[i].Name == material.Name)
                    {
                        content[i].Amount += material.Amount;
                        add = true;
                    }
                }
                if(!add)
                    content.Add(material);
            }
        }

        public void RemoveFromContent(Material material)
        {
            content.Remove(material);
        }

        public void ClearContent()
        {
            content.Clear();
        }

        public List<Material> Content
        {
            get
            {
                return content;
            }
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

        public override string ToString()
        {
            return name.ToString();
        }
    }
}
