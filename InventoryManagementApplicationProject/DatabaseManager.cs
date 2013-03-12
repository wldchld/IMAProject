using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Db4objects.Db4o;
using Db4objects.Db4o.Query;


namespace InventoryManagement
{
    public class DatabaseManager
    {
        public enum ROperator { LT, LE, E, GT, GE };
        readonly static string dbFileName = "inventory.yap";
        IObjectContainer db;

        public DatabaseManager()
        {
            db = Db4oEmbedded.OpenFile(dbFileName);
        }

        //------------------------------------
        //----- OPERATIONS FOR MATERIALS -----
        //------------------------------------
        public List<Material> RetrieveAllMaterials()
        {
            IObjectSet result = db.QueryByExample(typeof(Material));
            List<Material> materials = new List<Material>();
            while(result.HasNext())
                materials.Add((Material) result.Next());
            PrintMaterialList(materials);
            return materials;
        }

        public Material RetrieveMaterialByName(String name)
        {
            Material result = null;
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.Name == name;
            });
            if (mats.Count == 1)
                result = mats[0];
            Console.WriteLine(result);
            return result;
        }

        public void UpdateMaterial(Material oldMaterial, Material newMaterial)
        {
            IObjectSet result = db.QueryByExample(oldMaterial);
            if (result.HasNext())
            {
                Material found = (Material)result.Next();
                found.Amount = newMaterial.Amount;
                found.BestBefore = newMaterial.BestBefore;
                found.DateBought = newMaterial.DateBought;
                found.DisplayUnit = newMaterial.DisplayUnit;
                found.ExtraInfo = newMaterial.ExtraInfo;
                found.GroupName = newMaterial.GroupName;
                found.Infinite = newMaterial.Infinite;
                found.Name = newMaterial.Name;
                found.TypeOfMeasure = newMaterial.TypeOfMeasure;
                db.Store(found);
            }
        }

        public void DeleteMaterialByName(String name)
        {
            db.Delete(RetrieveMaterialByName(name));
        }

        public void AddNewMaterial(String name, String groupName, bool infinite, double amount, 
            Material.MeasureType typeOfMeasure, DateTime dateBought, DateTime bestBefore, String extraInfo, Unit unit)
        {
            Material mat = new Material(name, groupName, infinite, amount, typeOfMeasure, 
                dateBought, bestBefore, extraInfo, unit);
            db.Store(mat);
        }

        public void AddNewMaterial(Material mat)
        {
            db.Store(mat);
        }

        public List<Material> RetrieveMaterialsInGroup(String groupName)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.GroupName == groupName;
            });
            PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        public List<Material> RetrieveMaterialsByBestBeforeDate(ROperator ro, DateTime date)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                if (ro == ROperator.LT)
                    return mat.BestBefore < date;
                else if (ro == ROperator.LE)
                    return mat.BestBefore <= date;
                else if (ro == ROperator.GT)
                    return mat.BestBefore > date;
                else if (ro == ROperator.GE)
                    return mat.BestBefore >= date;
                return mat.BestBefore == date;
            });
            PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        public List<Material> RetrieveMaterialsByBestBeforeDateInRange(DateTime min, DateTime max)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.BestBefore >= min && mat.BestBefore <= max;
            });
            PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        public List<Material> RetrieveMaterialsByDateBought(ROperator ro, DateTime date)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                if (ro == ROperator.LT)
                    return mat.DateBought < date;
                else if (ro == ROperator.LE)
                    return mat.DateBought <= date;
                else if (ro == ROperator.GT)
                    return mat.DateBought > date;
                else if (ro == ROperator.GE)
                    return mat.DateBought >= date;
                return mat.DateBought == date;
            });
            PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        public List<Material> RetrieveMaterialsByDateBoughtInRange(DateTime min, DateTime max)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.DateBought >= min && mat.DateBought <= max;
            });
            PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        public List<Material> RetrieveMaterialsWithExtraInfoContainingWord(string word)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.ExtraInfo.Contains(word);
            });
            PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        public List<Material> RetrieveMaterialsGoneBad()
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.BestBefore < DateTime.Now;
            });
            PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        public List<Material> RetrieveMaterialsByInfinity(bool infinite)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.Infinite == infinite;
            });
            PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        public List<Material> RetrieveMaterialsByAmount(ROperator ro, double amount)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                if (mat.Infinite)
                    return true;
                else if (ro == ROperator.LT)
                    return mat.Amount < amount;
                else if (ro == ROperator.LE)
                    return mat.Amount <= amount;
                else if (ro == ROperator.GT)
                    return mat.Amount > amount;
                else if (ro == ROperator.GE)
                    return mat.Amount >= amount;
                else
                    return mat.Amount == amount;
            });
            PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        public List<Material> RetrieveMaterialsByAmountRange(double min, double max)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.Amount >= min && mat.Amount <= max;
            });
            PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

  

        //------------------------------------
        //----- SHOPPING LIST OPERATIONS -----
        //------------------------------------

        public void AddNewShoppingList(string name)
        {
            db.Store(new ShoppingList(name));
        }

        public void AddToShoppingList(string listName, Material mat)
        {
            IObjectSet sls = db.QueryByExample(new ShoppingList(listName));
            ShoppingList list = null;
            if (sls.HasNext())
                list = (ShoppingList) sls.Next();
            if (list != null)
                list.AddToContent(mat);
            db.Store(list);
        }

        public void UpdateShoppingList(ShoppingList list)
        {
            IObjectSet sls = db.QueryByExample(list);
            ShoppingList oldList = null;
            if (sls.HasNext())
                oldList = (ShoppingList)sls.Next();
            if (oldList != null)
            {
                oldList = list;
                db.Store(oldList);
            }
        }

        public List<ShoppingList> RetrieveAllShoppingLists()
        {
            IObjectSet result = db.QueryByExample(typeof(ShoppingList));
            List<ShoppingList> sls = new List<ShoppingList>();
            while (result.HasNext())
                sls.Add((ShoppingList) result.Next());
            //PrintShoppingListList(sls);
            return sls;
        }

        public ShoppingList RetrieveShoppingListByName(string name)
        {
            ShoppingList ret = null;
            IList<ShoppingList> lists = db.Query<ShoppingList>(delegate(ShoppingList sl)
            {
                return sl.Name == name ;
            });
            if(lists.Count > 0)
                ret = lists[0];
            return ret;
        }

        public void DeleteShoppingListByName(string name)
        {
            db.Delete(RetrieveShoppingListByName(name));
        }

        //-----------------------------
        //----- RECIPE OPERATIONS -----
        //-----------------------------

        public void AddNewRecipe(Recipe recipe) 
        {
            db.Store(recipe);
        }

        public void AddNewRecipe(string name, string instructions)
        {
            Recipe recipe = new Recipe();
            recipe.Name = name;
            recipe.Instructions = instructions;

            db.Store(recipe);
        }

        public void AddNewRecipe(String name, IList<Material> content, String instructions)
        {
            Recipe recipe = new Recipe();
            recipe.Content = new List<Material>(content);
            recipe.Name = name;
            recipe.Instructions = instructions;

            db.Store(recipe);
        }

        public void AddMaterialToRecipe(String recipeName, Material material)
        {
            IList<Recipe> recipes = RetrieveRecipeByName(recipeName);
            for (int i = 0; i < recipes.Count; i++) 
            {
                recipes[i].Content.Add(material);
                db.Store(recipes[i]);
            }
        }

        public void AddMaterialToRecipe(String recipeName, String materialName)
        {
            IList<Recipe> recipes = RetrieveRecipeByName(recipeName);
            Material material = RetrieveMaterialByName(materialName);
            for (int i = 0; i < recipes.Count; i++)
            {
                recipes[i].Content.Add(material);
                db.Store(recipes[i]);
            }
        }

        public IList<Recipe> RetrieveAllRecipes()
        {
            IList<Recipe> recipes = db.Query<Recipe>();

            PrintRecipeList(recipes.ToList());
            return recipes;
        }

        public IList<Recipe> RetrieveRecipeByName(string name)
        {
            IList<Recipe> recipes = db.Query<Recipe>(delegate(Recipe recipe)
            {
                return recipe.Name.ToUpper() == name.ToUpper();
            });
            PrintRecipeList(recipes.ToList());
            return recipes;
        }

        public IList<Recipe> RetrieveRecipeByNamePart(string name)
        {
            IList<Recipe> recipes = db.Query<Recipe>(delegate(Recipe recipe)
            {
                return recipe.Name.ToUpper().Contains(name.ToUpper());
            });
            PrintRecipeList(recipes.ToList());
            return recipes;
        }

        public IList<Recipe> RetrieveRecipeByMaterial(Material material)
        {
            IList<Recipe> recipes = db.Query<Recipe>(delegate(Recipe recipe)
            {
                for (int i = 0; i < recipe.Content.Count; i++)
                {
                    if (recipe.Content[i].Equals(material))
                        return true;
                }
                return false;
            });
            PrintRecipeList(recipes.ToList());
            return recipes;
        }

        public IList<Recipe> RetrieveRecipeByMaterialName(string materialName)
        {
            IList<Recipe> recipes = db.Query<Recipe>(delegate(Recipe recipe)
            {
                for (int i = 0; i < recipe.Content.Count; i++)
                {
                    if (recipe.Content[i].Name.ToUpper() == materialName.ToUpper())
                        return true;
                }
                return false;
            });
            PrintRecipeList(recipes.ToList());
            return recipes;
        }

        // Retrieves recipes which can be crafted from th materials in the list
        public IList<Recipe> RetrieveRecipeByMaterialList(IList<Material> materials)
        {
            IList<Recipe> recipes = db.Query<Recipe>(delegate(Recipe recipe)
            {
                int materialsFound = 0;
                for (int i = 0; i < materials.Count; i++)
                {
                    for (int a = 0; a < recipe.Content.Count; a++)
                    {
                        if (recipe.Content[a].Equals(materials[i]))
                        {
                            materialsFound++;
                            a = recipe.Content.Count;
                        }
                    }
                }
                return materialsFound >= recipe.Content.Count;
            });
            PrintRecipeList(recipes.ToList());
            return recipes;
        }

        //--------------------------------------------
        //----- WHOLE DATABASE AFFECTING METHODS -----
        //--------------------------------------------
        
        // Deletes the database and creates a new empty database.
        public void ReCreateDB()
        {
            db.Close();
            File.Delete(dbFileName);
            db = Db4oEmbedded.OpenFile(dbFileName);
        }

        public void CreateSampleData()
        {
            AddNewMaterial("Siskonmakkarakeitto", "Ruoka", false, 1500, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(4), "Hyvää keittoa. Olispa edes", Unit.G);
            AddNewMaterial("USB-hubi", "PC oheislaitteet", false, 3, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "4-porttinen USB-hubi", Unit.PCS);
            AddNewMaterial("Ruuvimeisseli +", "Työkalut", true, 5, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Tähti/ristipää", Unit.PCS);
            AddNewMaterial("Ruuvimeisseli -", "Työkalut", true, 3, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Talttapää", Unit.PCS);
            AddNewMaterial("Kahvikuppi", "Astiat", false, 25, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Pupumuki", Unit.PCS);
            AddNewMaterial("Kahvi", "Juoma", false, 250, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(365), "Presidenttiä", Unit.G);
            AddNewMaterial("Espresso", "Juoma", false, 100, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(365), "Angry Birds espressoa", Unit.G);
            AddNewMaterial("Läppäri", "Tietokoneet", false, 3, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Macbook ja Lenovo", Unit.PCS);
            AddNewMaterial("Tulostin", "PC oheislaitteet", false, 1, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "HP photosmart B1013", Unit.PCS);
            AddNewMaterial("Micro USB-kaapeli", "Kaapelit", false, 10, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Pituus 1m/kpl", Unit.PCS);
            AddNewMaterial("Näppäimisto", "PC oheislaitteet", false, 2, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Suomi-layout", Unit.PCS);
            AddNewMaterial("Kovalevy", "PC komponentit", false, 7, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Kokoluokka 160-500 gb", Unit.PCS);
            AddNewMaterial("Galaxy S2", "Puhelimet", false, 1, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, null, Unit.PCS);
            AddNewMaterial("Voi", "Ruoka", false, 650, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(180), null, Unit.G);
            AddNewMaterial("Ruisleipä", "Ruoka", false, 240, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(7), null, Unit.G);
            AddNewMaterial("Kalja", "Juoma", false, 240, Material.MeasureType.VOLUME, DateTime.Now, DateTime.Now.AddDays(1), "Parasta ennen huomista", Unit.L);
            AddNewMaterial("Kaiutinkaapeli", "Kaapelit", false, 240, Material.MeasureType.LENGTH, DateTime.Now, DateTime.MinValue, "Väri: ruskea", Unit.M);
            AddNewMaterial("ES", "Juoma", false, 240, Material.MeasureType.VOLUME, DateTime.Now, DateTime.Now.AddDays(720), "PÄRISEE!!!", Unit.L);
            AddNewMaterial("2x4 Lauta", "Rakennustarvike", false, 240, Material.MeasureType.LENGTH, DateTime.Now, DateTime.MinValue, "Sijainti: olohuone", Unit.M);

            AddNewRecipe("Resepti", "Käytä tätä reseptiä");
            AddMaterialToRecipe("Resepti", RetrieveMaterialByName("Kalja"));
            AddMaterialToRecipe("Resepti", "ES");
            AddNewRecipe("Ruokaa", RetrieveMaterialsInGroup("Ruoka"), "Tähän reseptiin tulee paljon ruokaa");
            AddNewRecipe("Juomia", RetrieveMaterialsInGroup("Juoma"), "Juomapuoli hoidossa");

            AddNewShoppingList("Ruokakauppa");
            AddNewShoppingList("Verkkokauppa.com");
            AddNewShoppingList("Rautakauppa");

            AddToShoppingList("Ruokakauppa", new Material("Maito", "Ruoka", false, 3, Material.MeasureType.VOLUME, DateTime.Now, DateTime.Now, null, Unit.L));
            AddToShoppingList("Ruokakauppa", new Material("Kerma", "Ruoka", false, 4, Material.MeasureType.VOLUME, DateTime.Now, DateTime.Now, null, Unit.DL));
            AddToShoppingList("Verkkokauppa.com", new Material("G27", "PS3", false, 1, Material.MeasureType.PCS, DateTime.Now, DateTime.Now, null, Unit.PCS));
            AddToShoppingList("Verkkokauppa.com", new Material("G27 teline", "PS3", false, 1, Material.MeasureType.PCS, DateTime.Now, DateTime.Now, "Vitun painava", Unit.PCS));
        }

        //-----------------------------------------
        //----- DEBUGGING CONVINIENCE METHODS -----
        //-----------------------------------------
        public void PrintMaterialList(List<Material> whatever)
        {
            foreach (Material o in whatever)
                Console.WriteLine(o);
        }

        public static void PrintObjectSet(IObjectSet result)
        {
            Console.WriteLine(result.Count);
            foreach (object item in result)
            {
                Console.WriteLine(item);
            }
        }

        public void PrintShoppingListList(List<ShoppingList> whatever)
        {
            foreach (ShoppingList o in whatever)
                Console.WriteLine(o);
        }

        public void PrintRecipeList(List<Recipe> whatever)
        {
            foreach (Recipe o in whatever)
            {
                Console.WriteLine("Nimi: " + o.Name + ", Ohje: " + o.Instructions + ", Materiaalit: ");
                for (int i = 0; i < o.Content.Count; i++)
                    Console.WriteLine(o.Content[i].Name);
            }
                
        }


        /// Search
        /// Queryn sisällä oli for silmukka, ei näin, T. Mikko
        public IList<Material> SearchMaterials(String input)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.Name.ToUpper().Contains(input.ToUpper());
            });
            PrintMaterialList(mats.ToList());
            return mats;
        }

    }
}
