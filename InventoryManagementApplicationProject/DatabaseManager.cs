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
        readonly static string dbFileName = "inventory.yap";
        IObjectContainer db;

        // OPERATIONS FOR MATERIALS
        public DatabaseManager()
        {
            db = Db4oEmbedded.OpenFile(dbFileName);
        }

        public IObjectSet RetrieveAllMaterials()
        {
            IObjectSet result = db.QueryByExample(typeof(Material));
            ListResult(result);
            return result;
        }

        public IObjectSet RetrieveMaterialByName(String name)
        {
            Material proto = new Material(name);
            IObjectSet result = db.QueryByExample(proto);
            ListResult(result);
            return result;
        }

        public IObjectSet RetrieveMaterialByExactAmount(double amount)
        {
            Material proto = new Material(null, null, amount);
            IObjectSet result = db.QueryByExample(proto);
            ListResult(result);
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



        public static void ListResult(IObjectSet result)
        {
            Console.WriteLine(result.Count);
            foreach (object item in result)
            {
                Console.WriteLine(item);
            }
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
            printList(mats.ToList());
            return mats;
        }

        public void printList(List<Material> whatever)
        {
            foreach (Material o in whatever)
                Console.WriteLine(o);
        }

        /**
         * Deletes the database and creates a new empty database."
         */
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
    }
}
