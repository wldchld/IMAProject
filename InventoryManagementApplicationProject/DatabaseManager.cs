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

    class DatabaseManager
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
            Material.MeasureType typeOfMeasure, DateTime dateBought, DateTime bestBefore, String extraInfo)
        {
            Material mat = new Material(name, groupName, infinite, amount, typeOfMeasure, 
                dateBought, bestBefore, extraInfo);
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
            PrintList(mats.ToList());
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
            PrintList(mats.ToList());
            return mats;
        }

        public IList<Material> RetreiveMaterialsByBestBeforeDateInRange(DateTime min, DateTime max)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.BestBefore >= min && mat.BestBefore <= max;
            });
            PrintList(mats.ToList());
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
            PrintList(mats.ToList());
            return mats;
        }

        public IList<Material> RetreiveMaterialsByDateBoughtInRange(DateTime min, DateTime max)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.DateBought >= min && mat.DateBought <= max;
            });
            PrintList(mats.ToList());
            return mats;
        }

        public IList<Material> RetreiveMaterialsWithExtraInfoContainingWord(string word)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.ExtraInfo.Contains(word);
            });
            PrintList(mats.ToList());
            return mats;
        }

        public IList<Material> RetreiveMaterialsGoneBad()
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.BestBefore < DateTime.Now;
            });
            PrintList(mats.ToList());
            return mats;
        }

        public IList<Material> RetreiveByInfinity(bool infinite)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.Infinite == infinite;
            });
            PrintList(mats.ToList());
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
            PrintList(mats.ToList());
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
            PrintList(mats.ToList());
            return mats;
        }

        public IList<Material> RetreiveMaterialsByAmountRange(double min, double max)
        {
            IList<Material> mats = db.Query<Material>(delegate(Material mat)
            {
                return mat.Amount >= min && mat.Amount <= max;
            });
            PrintList(mats.ToList());
            return mats;
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
            AddNewMaterial("Siskonmakkarakeitto", "Ruoka", false, 1500, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(4), "Hyvää keittoa");
            AddNewMaterial("USB-hubi", "PC oheislaitteet", false, 3, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "4-porttinen USB-hubi");
            AddNewMaterial("Ruuvimeisseli +", "Työkalut", true, 5, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Tähti/ristipää");
            AddNewMaterial("Ruuvimeisseli -", "Työkalut", true, 3, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Talttapää");
            AddNewMaterial("Kahvikuppi", "Astiat", false, 25, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Pupumuki");
            AddNewMaterial("Kahvi", "Juoma", false, 250, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(365), "Presidenttiä");
            AddNewMaterial("Espresso", "Juoma", false, 100, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(365), "Angry Birds espressoa");
            AddNewMaterial("Läppäri", "Tietokoneet", false, 3, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Macbook ja Lenovo");
            AddNewMaterial("Tulostin", "PC oheislaitteet", false, 1, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "HP photosmart B1013");
            AddNewMaterial("Micro USB-kaapeli", "Kaapelit", false, 10, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Pituus 1m/kpl");
            AddNewMaterial("Näppäimisto", "PC oheislaitteet", false, 2, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Suomi-layout");
            AddNewMaterial("Kovalevy", "PC komponentit", false, 7, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, "Kokoluokka 160-500 gb");
            AddNewMaterial("Galaxy S2", "Puhelimet", false, 1, Material.MeasureType.PCS, DateTime.Now, DateTime.MinValue, null);
            AddNewMaterial("Voi", "Ruoka", false, 650, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(180), null);
            AddNewMaterial("Ruisleipä", "Ruoka", false, 240, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.Now.AddDays(7), null);
            AddNewMaterial("Kalja", "Juoma", false, 240, Material.MeasureType.VOLUME, DateTime.Now, DateTime.Now.AddDays(1), "Parasta ennen huomista");
            AddNewMaterial("Kaiutinkaapeli", "Kaapelit", false, 240, Material.MeasureType.LENGTH, DateTime.Now, DateTime.MinValue, "Väri: ruskea");
            AddNewMaterial("ES", "Juoma", false, 240, Material.MeasureType.VOLUME, DateTime.Now, DateTime.Now.AddDays(720), "PÄRISEE!!!");
            AddNewMaterial("2x4 Lauta", "Rakennustarvike", false, 240, Material.MeasureType.LENGTH, DateTime.Now, DateTime.MinValue, "Sijainti: olohuone");
        }

        //-----------------------------------------
        //----- DEBUGGING CONVINIENCE METHODS -----
        //-----------------------------------------
        public void PrintList(List<Material> whatever)
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
    }
}
