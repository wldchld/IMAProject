using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement
{
    public class Recipe
    {
        private string name;
        private List<Material> content;
        private string instructions;

       public Recipe()
        {
            content = new List<Material>();
        }

        public void AddMaterialToRecipe(Material material)
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
                if (!add)
                {
                    content.Add(material);
                    content.Last().BelongsTo = Material.Connection.RECIPE;
                }
            }
        }

        public void RemoveMaterialFromRecipe(Material material)
        {
            content.Remove(material);
        }

        public void ClearContent()
        {
            content.Clear();
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

        public string Instructions
        {
            get
            {
                return instructions;
            }
            set
            {
                instructions = value;
            }
        }

        public List<Material> Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
            }
        }
    }
}
