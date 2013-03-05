using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Db4objects.Db4o;
using Db4objects.Db4o.Query;

namespace InventoryManagement
{
    public class Search
    {
       private List<string> searchContent;

        public Search()
        {
            searchContent = new List<string>();
        }

        public List<string> SearchRecipes(string input)
        {
            /*
            SqlConnection connection = new SqlConnection("...");
            connection.Open(); searchContent.Clear();

            string query = "SELECT * FROM X WHERE NAME = '" + input + "'";

            SqlCommand sqlquery = new SqlCommand(query, connection);

            SqlDataReader reader = sqlquery.ExecuteReader();

           DataTable matchedContent = new DataTable();

           matchedContent.Load(reader);

            for (int i = 0; i < 5; i++)
            {
                String matchedContentText = string.Empty;
                foreach (DataColumn column in matchedContent.Columns)
                {
                    matchedContentText += matchedContent.Rows[i][column.ColumnName] + " - ";
                    searchContent.Add(matchedContentText);
                }
            }

            connection.Close();
            */
            return searchContent;
        }

        public string SearchInventory(string input)
        {
            string result = "result";
            return result;
        }


        public List<string> SearchContent
        {
            get
            {
                return SearchContent;
            }
        }
    }
}
