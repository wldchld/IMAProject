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
        public IObjectSet RetrieveAllMaterials()
        {
            IObjectSet result = db.QueryByExample(typeof(Material));
            PrintObjectSet(result);
            return result;
        }

        public IObjectSet RetrieveMaterialByName(String name)
        {
            Material proto = new Material(name);
            IObjectSet result = db.QueryByExample(proto);
            PrintObjectSet(result);
            return result;
        }

        public IObjectSet RetrieveMaterialByExactAmount(double amount)
        {
            Material proto = new Material(null, null, amount);
            IObjectSet result = db.QueryByExample(proto);
            PrintObjectSet(result);
            return result;
        }

        public void UpdateMaterial(Material mat)
        {
            IObjectSet result = db.QueryByExample(mat);
            Material found = (Material) result.Next();
            found = mat;
            db.Store(mat);
            RetrieveAllMaterials();
        }

        public void DeleteMaterialByName(String name)
        {
            IObjectSet result = db.QueryByExample(new Material(name));
            Material found = (Material)result.Next();
            db.Delete(found);
            Console.WriteLine("Deleted {0}", found);
            RetrieveAllMaterials();
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

        public IList<Material> RetreiveMaterialsInGroup(String groupName) {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.GroupName == groupName;
            });
            PrintMaterialList(mats.ToList());
            return mats;
        }

        public IList<Material> RetreiveMaterialsByBestBeforeDate(ROperator ro, DateTime date)
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
            return mats;
        }

        public IList<Material> RetreiveMaterialsByBestBeforeDateInRange(DateTime min, DateTime max)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.BestBefore >= min && mat.BestBefore <= max;
            });
            PrintMaterialList(mats.ToList());
            return mats;
        }

        public IList<Material> RetreiveMaterialsByDateBought(ROperator ro, DateTime date)
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
            return mats;
        }

        public IList<Material> RetreiveMaterialsByDateBoughtInRange(DateTime min, DateTime max)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.DateBought >= min && mat.DateBought <= max;
            });
            PrintMaterialList(mats.ToList());
            return mats;
        }

        public IList<Material> RetreiveMaterialsWithExtraInfoContainingWord(string word)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.ExtraInfo.Contains(word);
            });
            PrintMaterialList(mats.ToList());
            return mats;
        }

        public IList<Material> RetreiveMaterialsGoneBad()
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.BestBefore < DateTime.Now;
            });
            PrintMaterialList(mats.ToList());
            return mats;
        }

        public IList<Material> RetreiveByInfinity(bool infinite)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.Infinite == infinite;
            });
            PrintMaterialList(mats.ToList());
            return mats;
        }

        public IList<Material> RetreiveMaterialsByAmount(ROperator ro, double amount)
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
            return mats;
        }

        public IList<Material> RetreiveMaterialsByAmountRange(double min, double max)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.Amount >= min && mat.Amount <= max;
            });
            PrintMaterialList(mats.ToList());
            return mats;
        }

  

        //------------------------------------
        //----- SHOPPING LIST OPERATIONS -----
        //------------------------------------
        public IList<ShoppingList> RetreiveShoppingListByName(string name)
        {
            IList<ShoppingList> lists = db.Query<ShoppingList>(delegate(ShoppingList sl)
            {
                return sl.Name == name ;
            });
            return lists;
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
            AddNewMaterial("Siskonmakkarakeitto", "Ruoka", false, 1500, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(4), "Hyvää keittoa", Unit.G);
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


        /// Search
        public IList<Material> SearchMaterials(String input)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {  
                    for (int i = 0; i < input.Length; i++)
                    {
                        return mat.Name.ToUpper().Contains(input.ToUpper());
                    }

                return mat.Name == input;

            });
            PrintMaterialList(mats.ToList());
            return mats;
        }

    }
}
