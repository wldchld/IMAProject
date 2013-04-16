using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Query;


namespace InventoryManagement
{
    /// <summary>
    /// This class handles all the applications database operations.
    /// </summary>
    public class DatabaseManager
    {
        public enum ROperator { LT, LE, E, GT, GE };
        readonly static string dbFileName = "inventory.yap";
        IObjectContainer db;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DatabaseManager()
        {
            IEmbeddedConfiguration config = Db4oEmbedded.NewConfiguration();
            config.Common.UpdateDepth = 2;
            db = Db4oEmbedded.OpenFile(dbFileName);
        }

        //------------------------------------
        //----- OPERATIONS FOR MATERIALS -----
        //------------------------------------

        /// <summary>
        /// Retrieves all material from database. Throws exception if database is empty.
        /// </summary>
        /// <returns></returns>
        public List<Material> RetrieveAllMaterials()
        {
            IObjectSet result = db.QueryByExample(typeof(Material));
            List<Material> materials = new List<Material>();
            while (result.HasNext())
            {
                Material temp = (Material)result.Next();
                if (temp.BelongsTo == Material.Connection.INVENTORY)
                    materials.Add(temp);
            }
            //Debug method
            //PrintMaterialList(materials);
            if (materials.Any() == false)
            {
                throw new Exception("Nothing found from database!");
            }
            return materials;
        }

        /// <summary>
        /// Retrieves material identified by name from database. belongsTo means collection where material is retrieved.
        /// This function returns null if material is not found.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="belongsTo">Inventory, recipe, shopping list etc.</param>
        /// <returns></returns>
        public Material RetrieveMaterialByName(String name, Material.Connection belongsTo)
        {
            Material result = null;
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.Name == name && mat.BelongsTo == belongsTo;
            });
            if (mats.Count == 1)
                result = mats[0];
            //Console.WriteLine(result);
            return result;
        }

        /// <summary>
        /// Updates material. Needs old and new materials.
        /// </summary>
        /// <param name="oldMaterial">The material that is being updated.</param>
        /// <param name="newMaterial">New material which replace old one.</param>
        public void UpdateMaterial(Material oldMaterial, Material newMaterial)
        {
            IObjectSet result = db.QueryByExample(oldMaterial);
            if (result.HasNext())
            {
                Material found = (Material)result.Next();
                found.Amount = newMaterial.Amount;
                found.BestBefore = newMaterial.BestBefore;
                found.LastModified = newMaterial.LastModified;
                found.DisplayUnit = newMaterial.DisplayUnit;
                found.ExtraInfo = newMaterial.ExtraInfo;
                found.GroupName = newMaterial.GroupName;
                found.Infinite = newMaterial.Infinite;
                found.Name = newMaterial.Name;
                found.TypeOfMeasure = newMaterial.TypeOfMeasure;
                db.Store(found);
            }
        }

        /// <summary>
        /// Searches given material from shopping list and adds it to inventory.
        /// </summary>
        /// <param name="bought"></param>
        public void AddToInventoryFromShoplist(Material bought)
        {
            Material temp = RetrieveMaterialByName(bought.Name, Material.Connection.INVENTORY);
            temp.Amount += bought.Amount;
            db.Store(temp);
        }

        /// <summary>
        /// Deletes given material from given collection.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="belongsTo">Inventory, shopping list, recipe etc.</param>
        public void DeleteMaterialByName(String name, Material.Connection belongsTo)
        {
            db.Delete(RetrieveMaterialByName(name, belongsTo));
        }

        /// <summary>
        /// Adds new material to database with given information.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="groupName"></param>
        /// <param name="infinite"></param>
        /// <param name="amount"></param>
        /// <param name="typeOfMeasure"></param>
        /// <param name="dateBought"></param>
        /// <param name="bestBefore"></param>
        /// <param name="extraInfo"></param>
        /// <param name="unit"></param>
        /// <param name="belongsTo"></param>
        public void AddNewMaterial(String name, String groupName, bool infinite, double amount, Material.MeasureType typeOfMeasure,
            DateTime dateBought, DateTime bestBefore, String extraInfo, Unit unit, Material.Connection belongsTo)
        {
            Material mat = new Material(name, groupName, infinite, amount, typeOfMeasure,
                dateBought, bestBefore, extraInfo, unit, belongsTo);
            db.Store(mat);
        }

        /// <summary>
        /// Adds given material to database.
        /// </summary>
        /// <param name="mat"></param>
        public void AddNewMaterial(Material mat)
        {
            db.Store(mat);
        }

        /// <summary>
        /// Returns list of materials selected by given groupName.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        public List<Material> RetrieveMaterialsInGroup(String groupName)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.GroupName == groupName && mat.BelongsTo == Material.Connection.INVENTORY;
            });
            //Debug method.
            //PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        /// <summary>
        /// Returns list of materials. Selects materials using given datetime and search operator and compares it to best before date.
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="date"></param>
        /// <returns></returns>
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
            //Debug method.
            //PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        /// <summary>
        /// Returns list of material which best before date is between min and max dates.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public List<Material> RetrieveMaterialsByBestBeforeDateInRange(DateTime min, DateTime max)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.BestBefore >= min && mat.BestBefore <= max;
            });
            //Debug method.
            //PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        /// <summary>
        /// Returns list of materials. Selects materials using given datetime and search operator and compares it to bought date.
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public List<Material> RetrieveMaterialsByDateBought(ROperator ro, DateTime date)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                if (ro == ROperator.LT)
                    return mat.LastModified < date;
                else if (ro == ROperator.LE)
                    return mat.LastModified <= date;
                else if (ro == ROperator.GT)
                    return mat.LastModified > date;
                else if (ro == ROperator.GE)
                    return mat.LastModified >= date;
                return mat.LastModified == date;
            });
            //Debug method.
            //PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        /// <summary>
        /// Returns list of material which bought date is between min and max dates.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public List<Material> RetrieveMaterialsByDateBoughtInRange(DateTime min, DateTime max)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.LastModified >= min && mat.LastModified <= max;
            });
            //Debug method.
            //PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        /// <summary>
        /// Returns list of materials which extra info contains given word.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public List<Material> RetrieveMaterialsWithExtraInfoContainingWord(string word)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.ExtraInfo.Contains(word);
            });
            //Debug method.
            //PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        /// <summary>
        /// Returns list of material which best before date is older than current date.
        /// </summary>
        /// <returns></returns>
        public List<Material> RetrieveMaterialsGoneBad()
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.BestBefore < DateTime.Now;
            });
            //Debug method.
            //PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        /// <summary>
        /// Returns list of materials which are infinite or not, depending of given parameter.
        /// </summary>
        /// <param name="infinite"></param>
        /// <returns></returns>
        public List<Material> RetrieveMaterialsByInfinity(bool infinite)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.Infinite == infinite;
            });
            //Debug method.
            //PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        /// <summary>
        /// Returns list of materials selected by given operator and amount.
        /// </summary>
        /// <param name="ro"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
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
            //Debug method.
            //PrintMaterialList(mats.ToList());
            return mats.ToList();
        }

        /// <summary>
        /// Retrieves list of materials which amount is between given range.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public List<Material> RetrieveMaterialsByAmountRange(double min, double max)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.Amount >= min && mat.Amount <= max;
            });
            //Debug method.
            //PrintMaterialList(mats.ToList());
            return mats.ToList();
        }



        //------------------------------------
        //----- SHOPPING LIST OPERATIONS -----
        //------------------------------------

        /// <summary>
        /// Adds new shopping list of given name to database.
        /// </summary>
        /// <param name="name"></param>
        public void AddNewShoppingList(string name)
        {
            db.Store(new ShoppingList(name));
        }

        /// <summary>
        /// Adds new material to shopping list identified by name.
        /// </summary>
        /// <param name="listName"></param>
        /// <param name="mat"></param>
        public void AddToShoppingList(string listName, Material mat)
        {
            IObjectSet sls = db.QueryByExample(new ShoppingList(listName));
            ShoppingList list = null;
            if (sls.HasNext())
                list = (ShoppingList)sls.Next();
            if (list != null)
                list.AddToContent(mat);
            db.Ext().Store(list, Int32.MaxValue);
        }

        /// <summary>
        /// Updates modifications of given shopping list to database.
        /// </summary>
        /// <param name="list"></param>
        public void UpdateShoppingList(ShoppingList list)
        {
            IObjectSet sls = db.QueryByExample(list);
            ShoppingList oldList = null;
            if (sls.HasNext())
                oldList = (ShoppingList)sls.Next();
            if (oldList != null)
            {
                oldList = list;
                db.Ext().Store(list, Int32.MaxValue);
            }
        }

        /// <summary>
        /// Returns list of all shopping lists.
        /// </summary>
        /// <returns></returns>
        public List<ShoppingList> RetrieveAllShoppingLists()
        {
            IObjectSet result = db.QueryByExample(typeof(ShoppingList));
            List<ShoppingList> sls = new List<ShoppingList>();
            while (result.HasNext())
                sls.Add((ShoppingList)result.Next());
            //Debug method.
            //PrintShoppingListList(sls);
            return sls;
        }

        /// <summary>
        /// Returns single shopping list from database identified by given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ShoppingList RetrieveShoppingListByName(string name)
        {
            ShoppingList ret = null;
            IList<ShoppingList> lists = db.Query<ShoppingList>(delegate(ShoppingList sl)
            {
                return sl.Name == name;
            });
            if (lists.Count > 0)
                ret = lists[0];
            return ret;
        }

        /// <summary>
        /// Deletes shopping list identified by its name from database.
        /// </summary>
        /// <param name="name"></param>
        public void DeleteShoppingListByName(string name)
        {
            db.Delete(RetrieveShoppingListByName(name));
        }

        //-----------------------------
        //----- RECIPE OPERATIONS -----
        //-----------------------------

        /// <summary>
        /// Adds new recipe to database.
        /// </summary>
        /// <param name="recipe"></param>
        public void AddNewRecipe(Recipe recipe)
        {
            db.Store(recipe);
        }

        /// <summary>
        /// Adds new recipe with given name and instructions to database.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instructions"></param>
        public void AddNewRecipe(string name, string instructions)
        {
            Recipe recipe = new Recipe();
            recipe.Name = name;
            recipe.Instructions = instructions;

            db.Ext().Store(recipe, Int32.MaxValue);
        }

        /// <summary>
        /// Adds new recipe with given name, instructions and list of materials to database.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <param name="instructions"></param>
        public void AddNewRecipe(String name, IList<Material> content, String instructions)
        {
            Recipe recipe = new Recipe();
            recipe.Content = new List<Material>();
            for (int i = 0; i < content.Count; i++)
            {
                Material material = new Material(content[i]);
                material.BelongsTo = Material.Connection.RECIPE;
                recipe.Content.Add(material);

            }
            recipe.Name = name;
            recipe.Instructions = instructions;

            db.Ext().Store(recipe, Int32.MaxValue);
        }

        /// <summary>
        /// Adds given material to recipe identified by its name.
        /// </summary>
        /// <param name="recipeName"></param>
        /// <param name="material"></param>
        public void AddMaterialToRecipe(String recipeName, Material material)
        {
            IList<Recipe> recipes = RetrieveRecipeByName(recipeName);

            for (int i = 0; i < recipes.Count; i++)
            {
                Material material_ = new Material(material);
                material_.BelongsTo = Material.Connection.RECIPE;
                recipes[i].Content.Add(material_);
                db.Ext().Store(recipes[i], Int32.MaxValue);
            }
        }
        
        /// <summary>
        /// Deletes recipe identified by its name.
        /// </summary>
        /// <param name="recipeName"></param>
        public void DeleteRecipeByName(String recipeName)
        {
            IList<Recipe> recipes = RetrieveRecipeByName(recipeName);

            for (int i = 0; i < recipes.Count; i++)
            {
                db.Delete(recipes[i]);
                //Debug method.
                //Console.WriteLine(recipeName + " is deleted from recipes");
            }
        }

        /// <summary>
        /// Deletes material identified by its name from recipe identified by its naem.
        /// </summary>
        /// <param name="RecipeName"></param>
        /// <param name="MaterialName"></param>
        public void DeleteMaterialFromRecipe(String RecipeName, String MaterialName)
        {
            IList<Recipe> recipes = RetrieveRecipeByName(RecipeName);

            for (int i = 0; i < recipes.Count; i++)
            {
                for (int a = 0; a < recipes[i].Content.Count; a++)
                {
                    if (recipes[i].Content[a].Name == MaterialName && recipes[i].Content[a].BelongsTo == Material.Connection.RECIPE)
                    {
                        recipes[i].Content.RemoveAt(a);
                        db.Store(recipes[i]);
                        //Debug method.
                        //Console.WriteLine(MaterialName + " removed from recipe " + RecipeName);
                    }
                }
            }
        }

        /// <summary>
        /// Add material with amount identified by name to recipe identified by name.
        /// </summary>
        /// <param name="recipeName"></param>
        /// <param name="materialName"></param>
        /// <param name="amount"></param>
        public void AddMaterialToRecipe(String recipeName, String materialName, double amount)
        {
            IList<Recipe> recipes = RetrieveRecipeByName(recipeName);

            for (int i = 0; i < recipes.Count; i++)
            {
                Material material = new Material(RetrieveMaterialByName(materialName, Material.Connection.INVENTORY));
                material.Amount = amount;
                material.BelongsTo = Material.Connection.RECIPE;
                recipes[i].Content.Add(material);
                db.Ext().Store(recipes[i], Int32.MaxValue);
            }
        }

        /// <summary>
        /// Updates made modifications of recipe to database.
        /// </summary>
        /// <param name="recipe"></param>
        public void UpdateRecipe(Recipe recipe)
        {
            IList<Recipe> sls = RetrieveRecipeByName(recipe.Name);
            Recipe oldList = null;
            for (int i = 0; i < sls.Count; i++)
            {
                oldList = sls[i];
                if (oldList != null)
                {
                    oldList = recipe;
                    db.Ext().Store(recipe, Int32.MaxValue);
                }
            }
        }

        /// <summary>
        /// Returns list of recipes.
        /// </summary>
        /// <returns></returns>
        public IList<Recipe> RetrieveAllRecipes()
        {
            IList<Recipe> recipes = db.Query<Recipe>();

            //Debug method.
            //PrintRecipeList(recipes.ToList());
            return recipes;
        }

        /// <summary>
        /// Returns list of recipes identified by name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IList<Recipe> RetrieveRecipeByName(string name)
        {
            IList<Recipe> recipes = db.Query<Recipe>(delegate(Recipe recipe)
            {
                return recipe.Name.ToUpper() == name.ToUpper();
            });
            //Debug method.
            //PrintRecipeList(recipes.ToList());
            return recipes;
        }

        /// <summary>
        /// Returns recipe identified by given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IList<Recipe> RetrieveRecipeByNamePart(string name)
        {
            IList<Recipe> recipes = db.Query<Recipe>(delegate(Recipe recipe)
            {
                return recipe.Name.ToUpper().Contains(name.ToUpper());
            });
            //Debug method.
            //PrintRecipeList(recipes.ToList());
            return recipes;
        }

        /// <summary>
        /// Returns list of recipies which contain given material.
        /// </summary>
        /// <param name="material"></param>
        /// <returns></returns>
        public IList<Recipe> RetrieveRecipeByMaterial(Material material)
        {
            IList<Recipe> recipes = db.Query<Recipe>(delegate(Recipe recipe)
            {
                for (int i = 0; i < recipe.Content.Count; i++)
                {
                    if (recipe.Content[i].Name.Equals(material.Name))
                        return true;
                }
                return false;
            });
            //Debug method.
            //PrintRecipeList(recipes.ToList());
            return recipes;
        }

        /// <summary>
        /// Returns list of recipies containing material identified by name.
        /// </summary>
        /// <param name="materialName"></param>
        /// <returns></returns>
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
            //Debug method.
            //PrintRecipeList(recipes.ToList());
            return recipes;
        }

        /// <summary>
        /// Returns list of recipies which contains material which name contains given name.
        /// </summary>
        /// <param name="materialNamePart"></param>
        /// <returns></returns>
        public IList<Recipe> RetrieveRecipeByMaterialNamePart(string materialNamePart)
        {
            IList<Recipe> recipes = db.Query<Recipe>(delegate(Recipe recipe)
            {
                for (int i = 0; i < recipe.Content.Count; i++)
                {
                    if (recipe.Content[i].Name.ToUpper().Contains(materialNamePart.ToUpper()))
                        return true;
                }
                return false;
            });
            //Debug method.
            //PrintRecipeList(recipes.ToList());
            return recipes;
        }

        /// <summary>
        /// Deprecated function.
        /// </summary>
        /// <param name="searchMaterials"></param>
        /// <param name="recipeName"></param>
        /// <param name="materialName"></param>
        /// <returns></returns>
        /*public IList<Recipe> SearchRecipies(IList<Material> searchMaterials, string recipeName, string materialName)
        {
            IList<Recipe> recipes = db.Query<Recipe>(delegate(Recipe recipe)
            {
                int materialsFound = 0;
                for (int i = 0; i < searchMaterials.Count; i++)
                {
                    for (int a = 0; a < recipe.Content.Count; a++)
                    {
                        if (recipe.Content[a].Name.ToUpper() == searchMaterials[i].Name.ToUpper() && recipe.Content[a].Amount <= searchMaterials[i].Amount)
                        {
                            
                            materialsFound++;
                            a = recipe.Content.Count;
                        }
                    }
                }
                return materialsFound >= recipe.Content.Count;
            });
            return recipes;
        }*/

        /// <summary>
        /// Retrieves recipes which can be crafted from the materials in the list.
        /// </summary>
        /// <param name="materials"></param>
        /// <returns></returns>
        public IList<Recipe> RetrieveRecipeByMaterialList(IList<Material> materials)
        {
            IList<Recipe> recipes = db.Query<Recipe>(delegate(Recipe recipe)
            {
                int materialsFound = 0;
                for (int i = 0; i < materials.Count; i++)
                {
                    for (int a = 0; a < recipe.Content.Count; a++)
                    {
                        if (recipe.Content[a].Name == materials[i].Name && recipe.Content[a].Amount <= materials[i].Amount)
                        {
                            materialsFound++;
                            a = recipe.Content.Count;
                        }
                    }
                }
                return materialsFound >= recipe.Content.Count;
            });
            //Debug method.
            //PrintRecipeList(recipes.ToList());
            return recipes;
        }

        //--------------------------------------------
        //----- WHOLE DATABASE AFFECTING METHODS -----
        //--------------------------------------------
        
        /// <summary>
        /// Deletes the database and creates a new empty database.
        /// </summary>
        public void ReCreateDB()
        {
            db.Close();
            File.Delete(dbFileName);
            db = Db4oEmbedded.OpenFile(dbFileName);
        }

        /// <summary>
        /// Creates sample data. DEPRECATED
        /// </summary>
        public void CreateSampleData()
        {
            AddNewMaterial("Siskonmakkarakeitto", "Ruoka", false, 1500, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(4), "Hyvää keittoa. Olispa edes", Unit.G, Material.Connection.INVENTORY);
            AddNewMaterial("USB-hubi", "PC oheislaitteet", false, 3, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "4-porttinen USB-hubi", Unit.PCS, Material.Connection.INVENTORY);
            AddNewMaterial("Ruuvimeisseli +", "Työkalut", true, 5, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Tähti/ristipää", Unit.PCS, Material.Connection.INVENTORY);
            AddNewMaterial("Ruuvimeisseli -", "Työkalut", true, 3, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Talttapää", Unit.PCS, Material.Connection.INVENTORY);
            AddNewMaterial("Kahvikuppi", "Astiat", false, 25, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Pupumuki", Unit.PCS, Material.Connection.INVENTORY);
            AddNewMaterial("Kahvi", "Juoma", false, 250, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(365), "Presidenttiä", Unit.G, Material.Connection.INVENTORY);
            AddNewMaterial("Espresso", "Juoma", false, 100, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(365), "Angry Birds espressoa", Unit.G, Material.Connection.INVENTORY);
            AddNewMaterial("Läppäri", "Tietokoneet", false, 3, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Macbook ja Lenovo", Unit.PCS, Material.Connection.INVENTORY);
            AddNewMaterial("Tulostin", "PC oheislaitteet", false, 1, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "HP photosmart B1013", Unit.PCS, Material.Connection.INVENTORY);
            AddNewMaterial("Micro USB-kaapeli", "Kaapelit", false, 10, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Pituus 1m/kpl", Unit.PCS, Material.Connection.INVENTORY);
            AddNewMaterial("Näppäimisto", "PC oheislaitteet", false, 2, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Suomi-layout", Unit.PCS, Material.Connection.INVENTORY);
            AddNewMaterial("Kovalevy", "PC komponentit", false, 7, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Kokoluokka 160-500 gb", Unit.PCS, Material.Connection.INVENTORY);
            AddNewMaterial("Galaxy S2", "Puhelimet", false, 1, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, null, Unit.PCS, Material.Connection.INVENTORY);
            AddNewMaterial("Voi", "Ruoka", false, 650, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(180), null, Unit.G, Material.Connection.INVENTORY);
            AddNewMaterial("Ruisleipä", "Ruoka", false, 240, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(7), null, Unit.G, Material.Connection.INVENTORY);
            AddNewMaterial("Kalja", "Juoma", false, 240, Material.MeasureType.VOLUME, DateTime.Now, DateTime.Now.AddDays(1), "Parasta ennen huomista", Unit.L, Material.Connection.INVENTORY);
            AddNewMaterial("Kaiutinkaapeli", "Kaapelit", false, 240, Material.MeasureType.LENGTH, DateTime.Now, DateTime.MinValue, "Väri: ruskea", Unit.M, Material.Connection.INVENTORY);
            AddNewMaterial("ES", "Juoma", false, 240, Material.MeasureType.VOLUME, DateTime.Now, DateTime.Now.AddDays(720), "PÄRISEE!!!", Unit.L, Material.Connection.INVENTORY);
            AddNewMaterial("2x4 Lauta", "Rakennustarvike", false, 240, Material.MeasureType.LENGTH, DateTime.Now, DateTime.MinValue, "Sijainti: olohuone", Unit.M, Material.Connection.INVENTORY);

            AddNewRecipe("Resepti", "Käytä tätä reseptiä");
            AddMaterialToRecipe("Resepti", RetrieveMaterialByName("Kalja", Material.Connection.INVENTORY));
            AddMaterialToRecipe("Resepti", "ES", 80);
            AddNewRecipe("Ruokaa", RetrieveMaterialsInGroup("Ruoka"), "Tähän reseptiin tulee paljon ruokaa");
            AddNewRecipe("Juomia", RetrieveMaterialsInGroup("Juoma"), "Juomapuoli hoidossa");

            AddNewShoppingList("Ruokakauppa");
            AddNewShoppingList("Verkkokauppa.com");
            AddNewShoppingList("Rautakauppa");

            AddToShoppingList("Ruokakauppa", new Material("Maito", "Ruoka", false, 3, Material.MeasureType.VOLUME, DateTime.Now, DateTime.Now, null, Unit.L, Material.Connection.SHOPPING_LIST));
            AddToShoppingList("Ruokakauppa", new Material("Kerma", "Ruoka", false, 4, Material.MeasureType.VOLUME, DateTime.Now, DateTime.Now, null, Unit.DL, Material.Connection.SHOPPING_LIST));
            AddToShoppingList("Verkkokauppa.com", new Material("G27", "PS3", false, 1, Material.MeasureType.PCS, DateTime.Now, DateTime.Now, null, Unit.PCS, Material.Connection.SHOPPING_LIST));
            AddToShoppingList("Verkkokauppa.com", new Material("G27 teline", "PS3", false, 1, Material.MeasureType.PCS, DateTime.Now, DateTime.Now, "Vitun painava", Unit.PCS, Material.Connection.SHOPPING_LIST));
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


        /// <summary>
        /// Returns list of materials that contains given name from recipe and inventory.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<Material> SearchAll(String input)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.Name.ToUpper().Contains(input.ToUpper()) &&
                    mat.BelongsTo == Material.Connection.RECIPE || mat.BelongsTo == Material.Connection.INVENTORY;
            });
            return mats.ToList();
        }

        /// <summary>
        /// Returns list of materials from given collection that contains given name. 
        /// Materials are selected based on their amount with given operation.
        /// Materials must belong to given group.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="belongsTo"></param>
        /// <param name="symbol"></param>
        /// <param name="amount"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public List<Material> SearchMats(String input, Material.Connection belongsTo,
            string symbol, int amount, string group)
        {
            if (symbol == "=")
            {
                IList<Material> mats = db.Query<Material>(delegate(Material mat)
                {
                    if (group == "")
                    {
                        return mat.Name.ToUpper().Contains(input.ToUpper()) && mat.BelongsTo == belongsTo
                            && mat.Amount == amount;
                    }
                    return mat.Name.ToUpper().Contains(input.ToUpper()) && mat.BelongsTo == belongsTo
                        && mat.Amount == amount && mat.GroupName == group;
                });
                return mats.ToList();

            }
            else if (symbol == ">")
            {
                IList<Material> mats = db.Query<Material>(delegate(Material mat)
                {
                    if (group == "")
                    {
                       return mat.Name.ToUpper().Contains(input.ToUpper()) && mat.BelongsTo == belongsTo
                       && mat.Amount > amount;
                    }
                    return mat.Name.ToUpper().Contains(input.ToUpper()) && mat.BelongsTo == belongsTo
                        && mat.Amount > amount && mat.GroupName == group;
                });
                return mats.ToList();
            }
            else if (symbol == "<")
            {
                IList<Material> mats = db.Query<Material>(delegate(Material mat)
                {
                    if (group == "")
                    {
                       return mat.Name.ToUpper().Contains(input.ToUpper()) && mat.BelongsTo == belongsTo
                       && mat.Amount < amount;
                    }
                    return mat.Name.ToUpper().Contains(input.ToUpper()) && mat.BelongsTo == belongsTo
                        && mat.Amount < amount && mat.GroupName == group;
                });
                return mats.ToList();
            }
            else if (symbol == "≥")
            {
                IList<Material> mats = db.Query<Material>(delegate(Material mat)
                {
                    if (group == "")
                    {
                        return mat.Name.ToUpper().Contains(input.ToUpper()) && mat.BelongsTo == belongsTo
                        && mat.Amount >= amount;
                    }
                    return mat.Name.ToUpper().Contains(input.ToUpper()) && mat.BelongsTo == belongsTo
                        && mat.Amount >= amount && mat.GroupName == group;
                });
                return mats.ToList();
            }
            else if (symbol == "≤")
            {
                IList<Material> mats = db.Query<Material>(delegate(Material mat)
                {
                    if (group == "")
                    {
                        return mat.Name.ToUpper().Contains(input.ToUpper()) && mat.BelongsTo == belongsTo
                      && mat.Amount <= amount;
                    }
                    return mat.Name.ToUpper().Contains(input.ToUpper()) && mat.BelongsTo == belongsTo
                        && mat.Amount <= amount && mat.GroupName == group;
                });
                return mats.ToList();
            }
            else
            {
                IList<Material> mats = db.Query<Material>(delegate(Material mat)
                {
                    if (group == "")
                    {
                        return mat.Name.ToUpper().Contains(input.ToUpper()) && mat.BelongsTo == belongsTo;
                    }
                    return mat.Name.ToUpper().Contains(input.ToUpper()) && mat.BelongsTo == belongsTo
                        && mat.GroupName == group;
                });
                return mats.ToList();
            }
        }

        /// <summary>
        /// Returns list of recipies identified by their name.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public IList<Recipe> SearchRecipes(String input)
        {
            IList<Recipe> recipes = db.Query<Recipe>(delegate(Recipe rec)
            {
                return rec.Name.ToUpper().Contains(input.ToUpper());
            });
            return recipes.ToList();
        }
    }
}
