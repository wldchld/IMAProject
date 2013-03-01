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

        public void RunConsoleTest()
        {
            File.Delete(dbFileName);
            AccessDb4o();
            File.Delete(dbFileName);
            using (IObjectContainer db = Db4oEmbedded.OpenFile(dbFileName))
            {
                StoreFirstMaterial(db);
                StoreSecondMaterial(db);
                RetrieveAllMaterials(db);
                RetrieveMaterialByName(db);
                RetrieveMaterialByExactPoints(db);
                UpdateMaterial(db);
                DeleteFirstMaterialByName(db);
                DeleteSecondMaterialByName(db);
            }
        }
        public static void AccessDb4o()
        {
            using (IObjectContainer db = Db4oEmbedded.OpenFile(dbFileName))
            {
                // do something with db4o
            }
        }
        public static void StoreFirstMaterial(IObjectContainer db)
        {
            Material mat1 = new Material("Nakkimakkara", 100);
            db.Store(mat1);
            Console.WriteLine("Stored {0}", mat1);
        }
        public static void StoreSecondMaterial(IObjectContainer db)
        {
            Material mat2 = new Material("Pinjansiemen", 99);
            db.Store(mat2);
            Console.WriteLine("Stored {0}", mat2);
        }
        public static void RetrieveAllMaterialQBE(IObjectContainer db)
        {
            Material proto = new Material(null, 0);
            IObjectSet result = db.QueryByExample(proto);
            ListResult(result);
        }
        public static void RetrieveAllMaterials(IObjectContainer db)
        {
            IObjectSet result = db.QueryByExample(typeof(Material));
            ListResult(result);
        }
        public static void RetrieveMaterialByName(IObjectContainer db)
        {
            Material proto = new Material("Dildo", 0);
            IObjectSet result = db.QueryByExample(proto);
            ListResult(result);
        }
        public static void RetrieveMaterialByExactPoints(IObjectContainer db)
        {
            Material proto = new Material(null, 100);
            IObjectSet result = db.QueryByExample(proto);
            ListResult(result);
        }
        public static void UpdateMaterial(IObjectContainer db)
        {
            IObjectSet result = db.QueryByExample(new Material("Nakkimakkara", 0));
            Material found = (Material)result.Next();
            found.AddAmount(11);
            db.Store(found);
            Console.WriteLine("Added 11 points for {0}", found);
            RetrieveAllMaterials(db);
        }
        public static void DeleteFirstMaterialByName(IObjectContainer db)
        {
            IObjectSet result = db.QueryByExample(new Material("Nakkimakkara", 0));
            Material found = (Material)result.Next();
            db.Delete(found);
            Console.WriteLine("Deleted {0}", found);
            RetrieveAllMaterials(db);
        }
        public static void DeleteSecondMaterialByName(IObjectContainer db)
        {
            IObjectSet result = db.QueryByExample(new Material("Pinjansiemen", 0));
            Material found = (Material)result.Next();
            db.Delete(found);
            Console.WriteLine("Deleted {0}", found);
            RetrieveAllMaterials(db);
        }

        public static void ListResult(IObjectSet result)
        {
            Console.WriteLine(result.Count);
            foreach (object item in result)
            {
                Console.WriteLine(item);
            }
        }

        /* Material:
         * (id = autoincrement)
         * string  MaterialName
         * int     UnitID
         * string  Group
         * boolean Infinite
         * string  Info
         * string  Date (format = '2013-01-31 10:00:00')
         */
        public void AddNewMaterial(string name, string groupName, int unitID, bool infinite, string info, string date)
        {
            Dictionary<String, String> data = new Dictionary<String, String>();
            data.Add("MaterialName", name);
            data.Add("UnitID", unitID.ToString());
            data.Add("Group", groupName);
            if (infinite)
                data.Add("Infinite", "1");
            else
                data.Add("Infinite", "0");
            data.Add("Info", info);
            data.Add("Date", date);
                MessageBox.Show("");
        }

        public void AddNewMaterialTest()
        {
            
        }

        // TODO muuta määrä floatiksi!
        public void AddToInventory(int materialID, int amount)
        {
            Dictionary<String, String> data = new Dictionary<String, String>();
            data.Add("MaterialID", materialID.ToString());
            data.Add("Amount", amount.ToString());
            /*
            data.Add("MaterialGroupID", );
            data.Add("UnitID", unitID.ToString);
             */
        }

        public void AddNewGroup(String name)
        {
            Dictionary<String, String> data = new Dictionary<String, String>();
            data.Add("GroupName", name);
        }
    }
}
