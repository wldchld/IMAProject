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

        public IObjectSet RetreiveMaterialsInGroup(String groupName)
        {
            Material proto = new Material(null, groupName, 0);
            IObjectSet result = db.QueryByExample(proto);
            ListResult(result);
            return result;
        }

    }
}
