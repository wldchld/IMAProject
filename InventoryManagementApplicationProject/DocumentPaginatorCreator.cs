using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace InventoryManagement
{
    class DocumentPaginatorCreator
    {
        /// <summary>
        /// Constructs the formatted list that can be printed with the print dialog.
        /// </summary>
        /// <param name="printAreaWidth"></param>
        /// <param name="selectedRecipe"></param>
        /// <returns></returns>
        public static DocumentPaginator CreatePrintableRecipe(double printAreaWidth, Recipe recipe)
        {
            FlowDocument flowDoc = new FlowDocument();
            flowDoc.ColumnWidth = printAreaWidth;
            Table t = new Table();
            for (int i = 0; i < 3; i++)
                t.Columns.Add(new TableColumn());
            TableRow row = new TableRow();
            row.Background = Brushes.Silver;
            row.FontSize = 26;
            row.FontWeight = System.Windows.FontWeights.Bold;
            row.Cells.Add(new TableCell(new Paragraph(new Run("Name"))));
            row.Cells.Add(new TableCell(new Paragraph(new Run("Amount"))));
            row.Cells.Add(new TableCell(new Paragraph(new Run("Unit"))));
            row.Cells[0].ColumnSpan = 20;
            row.Cells[1].ColumnSpan = 5;
            row.Cells[2].ColumnSpan = 5;
            var rg = new TableRowGroup();
            rg.Rows.Add(row);
            t.RowGroups.Add(rg);
            flowDoc.Blocks.Add(t);
            foreach (Material mat in recipe.Content)
            {
                row = new TableRow();
                row.FontSize = 24;
                row.Cells.Add(new TableCell(new Paragraph(new Run(mat.Name))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(mat.Amount.ToString()))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(mat.DisplayUnit.ToString()))));
                row.Cells[0].ColumnSpan = 20;
                row.Cells[1].ColumnSpan = 5;
                row.Cells[2].ColumnSpan = 5;
                rg = new TableRowGroup();
                rg.Rows.Add(row);
                t.RowGroups.Add(rg);
                flowDoc.Blocks.Add(t);
            }
            flowDoc.Blocks.Add(new Paragraph(new Run(recipe.Instructions)));
            IDocumentPaginatorSource doc = flowDoc;
            return doc.DocumentPaginator;
        }

        /// <summary>
        /// Creates the DocumentPaginator that can be used with PrintDialog's PrintDocument method.
        /// </summary>
        /// <param name="printAreaWidth">Makes sure that the page's width is used completely</param>
        /// <returns>Printable shoppinglist</returns>
        public static DocumentPaginator CreatePrintableShoppingList(double printAreaWidth, ShoppingList list)
        {
            FlowDocument flowDoc = new FlowDocument();
            flowDoc.ColumnWidth = printAreaWidth;
            Table t = new Table();
            for (int i = 0; i < 3; i++)
                t.Columns.Add(new TableColumn());
            TableRow row = new TableRow();
            row.Background = Brushes.Silver;
            row.FontSize = 26;
            row.FontWeight = System.Windows.FontWeights.Bold;
            row.Cells.Add(new TableCell(new Paragraph(new Run("Name"))));
            row.Cells.Add(new TableCell(new Paragraph(new Run("Amount"))));
            row.Cells.Add(new TableCell(new Paragraph(new Run("Unit"))));
            row.Cells[0].ColumnSpan = 20;
            row.Cells[1].ColumnSpan = 5;
            row.Cells[2].ColumnSpan = 5;
            var rg = new TableRowGroup();
            rg.Rows.Add(row);
            t.RowGroups.Add(rg);
            flowDoc.Blocks.Add(t);
            foreach (Material mat in list.Content)
            {
                row = new TableRow();
                row.FontSize = 24;
                row.Cells.Add(new TableCell(new Paragraph(new Run(mat.Name))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(mat.Amount.ToString()))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(mat.DisplayUnit.ToString()))));
                row.Cells[0].ColumnSpan = 20;
                row.Cells[1].ColumnSpan = 5;
                row.Cells[2].ColumnSpan = 5;
                rg = new TableRowGroup();
                rg.Rows.Add(row);
                t.RowGroups.Add(rg);
                flowDoc.Blocks.Add(t);
            }
            IDocumentPaginatorSource doc = flowDoc;
            return doc.DocumentPaginator;
        }
    }
}
