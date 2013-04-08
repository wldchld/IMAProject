﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.VisualBasic;
using System.ComponentModel;

namespace InventoryManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Private variables
        private ObservableCollection<Material> inventory = new ObservableCollection<Material>();
        private Material selectedItem = null;
        private List<Material> selectedItems = null;

        private ComboBoxItem comboBoxAddEditInfinite = new ComboBoxItem();
        private ComboBoxItem comboBoxAddEditUnit = new ComboBoxItem();
        
        private DatabaseManager dbManager = new DatabaseManager();

        private ObservableCollection<ShoppingList> shopLists;
        private ObservableCollection<Material> selectedShopListContent;
        private ObservableCollection<Recipe> recipesView { get; set; }
        private ObservableCollection<Material> recipesMaterials { get; set; }

        private ObservableCollection<Material> searchMaterial = new ObservableCollection<Material>();
        private ObservableCollection<Recipe> searchRecipe = new ObservableCollection<Recipe>();
        private HashSet<Material> groups = new HashSet<Material>();

        private bool addItemWindow;

        #endregion

        #region Initialize first time
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InventoryManagement_Loaded(object sender, RoutedEventArgs e)
        {
            //Added because of unresolved exception, no material found from database, works when there is data in database
            //dbManager.ReCreateDB();
            //dbManager.CreateSampleData();

            AddAllMaterialToInventoryList();
            shopLists = new ObservableCollection<ShoppingList>(dbManager.RetrieveAllShoppingLists());
        }

        private void AddAllMaterialToInventoryList()
        {
            List<Material> allItems = dbManager.RetrieveAllMaterials();
           
            foreach (Material item in allItems)
            {
                inventory.Add(
                    new Material(
                    item.Name, 
                    item.GroupName, 
                    item.Infinite, 
                    item.Amount, 
                    item.TypeOfMeasure,
                    item.LastModified, 
                    item.BestBefore, 
                    item.ExtraInfo, 
                    item.DisplayUnit,
                    item.BelongsTo));
            }
             
        }
        #endregion

        #region Search functions - Inventory Tab
        //These functions handle placeholder text "Search.."
        private void SearchFilter_LostFocus(object sender, RoutedEventArgs e)
        {
            /*
            if (SearchFilter.Text == String.Empty)
            {
                SearchFilter.Text = "Search..";
            }*/
        }
        private void SearchFilter_GotFocus(object sender, RoutedEventArgs e)
        {/*
            if (SearchFilter.Text == "Search..")
            {
                SearchFilter.Text = String.Empty;
            }
          */
        }

        private void SearchFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            if (SearchFilter.Text != "Search.." && SearchFilter.Text != String.Empty)
            {
                dbManager.SearchAll(SearchFilter.Text);
            }
            */
            SearchBox_Update();
        }
        #endregion

        #region Edit single material - Inventory Tab
        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            //Decrease selected item quantity
            if (this.selectedItem != null && selectedItem.Amount >= 1)
            {
                Material tempItem = dbManager.RetrieveMaterialByName(selectedItem.Name, Material.Connection.INVENTORY);
                selectedItem.Amount--;
                selectedItem.LastModified = DateTime.Now;
                dbManager.UpdateMaterial(tempItem, selectedItem);
                UpdateInventoryItemPanel();
            }
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            //Increase selected item quantity
            if (this.selectedItem != null)
            {
                Material tempItem = dbManager.RetrieveMaterialByName(selectedItem.Name, Material.Connection.INVENTORY);
                selectedItem.Amount++;
                selectedItem.LastModified = DateTime.Now;
                dbManager.UpdateMaterial(tempItem, selectedItem);
                UpdateInventoryItemPanel();
            }
        }

        public void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ItemNameContentEditDialog.Text = String.Empty;
            ItemDescriptionContentEditDialog.Text = String.Empty;
            ItemGroupContentEditDialog.Text = String.Empty;
            ItemQuantityContentEditDialog.Text = String.Empty;

            var comboUnit = ItemUnitEditDialog.FindName("ItemUnitComboBox") as ComboBox;
            ComboBoxAddEditUnit = comboUnit.FindName("pcs") as ComboBoxItem;

            var comboInfinite = ItemUnitEditDialog.FindName("ItemIsInfiniteComboBox") as ComboBox;
            ComboBoxAddEditInfinite = comboInfinite.FindName("no") as ComboBoxItem;

            var picker = ItemBestBefore.FindName("ItemBestBeforePicker") as DatePicker;

            picker.SelectedDate = null;

            addItemWindow = true;

            AddEditMaterialInputBox.Visibility = System.Windows.Visibility.Visible;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItem != null)
            {
                Material tempItem = dbManager.RetrieveMaterialByName(selectedItem.Name, Material.Connection.INVENTORY);

                ItemNameContentEditDialog.Text = tempItem.Name;
                ItemDescriptionContentEditDialog.Text = tempItem.ExtraInfo;
                ItemGroupContentEditDialog.Text = tempItem.GroupName;
                ItemQuantityContentEditDialog.Text = tempItem.Amount.ToString();

                if (tempItem.DisplayUnit.TypeOfMeasure == Material.MeasureType.WEIGHT)
                {
                    var combo = ItemUnitEditDialog.FindName("ItemUnitComboBox") as ComboBox;
                    ComboBoxAddEditUnit = combo.FindName("g") as ComboBoxItem;
                }
                else if (tempItem.DisplayUnit.TypeOfMeasure == Material.MeasureType.VOLUME)
                {
                    var combo = ItemUnitEditDialog.FindName("ItemUnitComboBox") as ComboBox;
                    ComboBoxAddEditUnit = combo.FindName("l") as ComboBoxItem;
                }
                else if (tempItem.DisplayUnit.TypeOfMeasure == Material.MeasureType.LENGTH)
                {
                    var combo = ItemUnitEditDialog.FindName("ItemUnitComboBox") as ComboBox;
                    ComboBoxAddEditUnit = combo.FindName("m") as ComboBoxItem;
                }
                else if (tempItem.DisplayUnit.TypeOfMeasure == Material.MeasureType.PCS)
                {
                    var combo = ItemUnitEditDialog.FindName("ItemUnitComboBox") as ComboBox;
                    ComboBoxAddEditUnit = combo.FindName("pcs") as ComboBoxItem;
                }

                if (tempItem.Infinite)
                {
                    var combo = ItemUnitEditDialog.FindName("ItemIsInfiniteComboBox") as ComboBox;
                    ComboBoxAddEditInfinite = combo.FindName("yes") as ComboBoxItem;
                }
                else
                {
                    var combo = ItemUnitEditDialog.FindName("ItemIsInfiniteComboBox") as ComboBox;
                    ComboBoxAddEditInfinite = combo.FindName("no") as ComboBoxItem;
                }

                var picker = ItemBestBefore.FindName("ItemBestBeforePicker") as DatePicker;
                if (tempItem.BestBefore.HasValue)
                {
                    picker.SelectedDate = tempItem.BestBefore;
                }

                addItemWindow = false;

                AddEditMaterialInputBox.Visibility = System.Windows.Visibility.Visible;
            }            
        }
        #endregion

        #region Add/edit material
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!addItemWindow)
                {
                    Material tempItem = dbManager.RetrieveMaterialByName(selectedItem.Name, Material.Connection.INVENTORY);

                    if (ItemNameContentEditDialog.Text == "")
                    {
                        throw new Exception("Item name cannot be null.");
                    }
                    selectedItem.Name = ItemNameContentEditDialog.Text;
                    selectedItem.ExtraInfo = ItemDescriptionContentEditDialog.Text;
                    selectedItem.GroupName = ItemGroupContentEditDialog.Text;

                    int tempAmount = 0;
                    try
                    {
                        tempAmount = Convert.ToInt32(ItemQuantityContentEditDialog.Text);
                    }
                    catch
                    {

                    }
                    selectedItem.Amount = tempAmount;

                    if (ComboBoxAddEditUnit.Name == "g")
                    {
                        selectedItem.DisplayUnit = Unit.G;
                    }
                    else if (ComboBoxAddEditUnit.Name == "l")
                    {
                        selectedItem.DisplayUnit = Unit.L;
                    }
                    else if (ComboBoxAddEditUnit.Name == "m")
                    {
                        selectedItem.DisplayUnit = Unit.M;
                    }
                    else if (ComboBoxAddEditUnit.Name == "pcs")
                    {
                        selectedItem.DisplayUnit = Unit.PCS;
                    }

                    if (ComboBoxAddEditInfinite.Name == "yes")
                    {
                        selectedItem.Infinite = true;
                    }
                    else if (ComboBoxAddEditInfinite.Name == "no")
                    {
                        selectedItem.Infinite = false;
                    }

                    selectedItem.LastModified = DateTime.Now;

                    var picker = ItemBestBefore.FindName("ItemBestBeforePicker") as DatePicker;
                    if (picker.SelectedDate.HasValue)
                    {
                        selectedItem.BestBefore = picker.SelectedDate.Value;
                    }

                    dbManager.UpdateMaterial(tempItem, selectedItem);
                    UpdateInventoryItemPanel();
                }
                else
                {
                    var newItem = new Material();

                    if (ItemNameContentEditDialog.Text == "")
                    {
                        throw new Exception("Item name cannot be null.");
                    }
                    newItem.Name = ItemNameContentEditDialog.Text;
                    newItem.ExtraInfo = ItemDescriptionContentEditDialog.Text;
                    newItem.GroupName = ItemGroupContentEditDialog.Text;

                    int tempAmount = 0;
                    try
                    {
                        tempAmount = Convert.ToInt32(ItemQuantityContentEditDialog.Text);
                    }
                    catch
                    {

                    }
                    newItem.Amount = tempAmount;

                    if (ComboBoxAddEditUnit.Name == "g")
                    {
                        newItem.DisplayUnit = Unit.G;
                    }
                    else if (ComboBoxAddEditUnit.Name == "l")
                    {
                        newItem.DisplayUnit = Unit.L;
                    }
                    else if (ComboBoxAddEditUnit.Name == "m")
                    {
                        newItem.DisplayUnit = Unit.M;
                    }
                    else if (ComboBoxAddEditUnit.Name == "pcs")
                    {
                        newItem.DisplayUnit = Unit.PCS;
                    }

                    if (ComboBoxAddEditInfinite.Name == "yes")
                    {
                        newItem.Infinite = true;
                    }
                    else
                    {
                        newItem.Infinite = false;
                    }
                    newItem.LastModified = DateTime.Now;

                    var picker = ItemBestBefore.FindName("ItemBestBeforePicker") as DatePicker;
                    if (picker.SelectedDate.HasValue)
                    {
                        newItem.BestBefore = picker.SelectedDate.Value;
                    }

                    inventory.Add(newItem);
                    dbManager.AddNewMaterial(newItem);
                }
            }
            catch
            {
                //TODO: Make exception handling
                //throw new Exception("Failed to add or edit item.");
            }

            AddEditMaterialInputBox.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AddEditMaterialInputBox.Visibility = System.Windows.Visibility.Collapsed;
        }
        #endregion

        #region Selection handling - Inventory Tab
        //Handle selections. 
        //If one item is selected, it is placed to "selectedItem" variable and "selectedItems" is null.
        //If multiple items are selected, those are placed to "selectedItems" variable as List<Material> and "selectedItem" is null.
        //If all items are deselected both variables are null.
        private void InventoryItemList_SelectionChanged(object sender, SelectionChangedEventArgs e = null)
        {
            if ((sender as ListView).SelectedItems.Count > 1)
            {
                this.selectedItem = null;

                var tempList = new List<Material>();
                foreach (var item in (sender as ListView).SelectedItems)
                {
                   tempList.Add(item as Material);
                }
                this.selectedItems = tempList;
            }
            else if ((sender as ListView).SelectedItem != null)
            {
                this.selectedItem = ((sender as ListView).SelectedItem as Material);
                this.selectedItems = null;
            }
            else
            {
                this.selectedItem = null;
                this.selectedItems = null;
            }

            UpdateInventoryItemPanel();
        }

        //This function updates rightside panel of Inventory tab
        private void UpdateInventoryItemPanel()
        {
            if (selectedItem != null)
            {
                try
                {
                    this.ItemNameContent.Content = selectedItem.Name;
                    this.ItemQuantityContent.Content = selectedItem.Amount;
                    this.ItemUnitContent.Content = selectedItem.DisplayUnit.TypeOfMeasure + " (" + selectedItem.DisplayUnit.Name + ")";
                    this.ItemGroupContent.Content = selectedItem.GroupName;
                    this.ItemDescriptionContent.Content = selectedItem.ExtraInfo;
                    this.ItemLastModifiedContent.Content = selectedItem.GetLastModifiedString();
                    if (selectedItem.BestBefore.HasValue)
                    {
                        this.ItemBestBeforeContent.Content = selectedItem.BestBefore.Value.ToString("dd.MM.yyyy");
                    }
                    else
                    {
                        this.ItemBestBeforeContent.Content = "";
                    }

                    if (selectedItem.Infinite)
                    {
                        this.ItemIsInfiniteContent.Content = "Yes";
                    }
                    else
                    {
                        this.ItemIsInfiniteContent.Content = "No";
                    }
                }
                catch
                {
                    //TODO: Perhaps some kind of error handling?
                    //throw new Exception("Selected item doesn't have all information avaible.");
                }
                
            }
            else if (selectedItems != null)
            {
                //Something..
            }
            else
            {
                this.ItemNameContent.Content = "";
                this.ItemDescriptionContent.Content = "";
                this.ItemGroupContent.Content = "";
                this.ItemQuantityContent.Content = "0";
                this.ItemUnitContent.Content = "";
                this.ItemIsInfiniteContent.Content = "";
                this.ItemLastModifiedContent.Content = "";
                this.ItemBestBeforeContent.Content = "";
            }
        }
        #endregion

        #region Context menu functions - Inventory Tab

        private void InventoryItemList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // If there are no items selected, cancel viewing the context menu
            if (InventoryItemList.SelectedItems.Count <= 0)
            {
                e.Handled = true;
            }
            else if (InventoryItemList.SelectedItems.Count > 3)
            {

                Console.WriteLine(LeftClickedMenu.Name);
            }
        }


        private void Add_Recipe(object sender, RoutedEventArgs e)
        {
            if (selectedItem != null)
            {
            }
        }

        private void Add_To_Existing_Shopping_List(object sender, RoutedEventArgs e)
        {
            if (selectedItem != null)
            {
                MenuItem item = e.OriginalSource as MenuItem;
                    string slName = item.Header.ToString();
                string text = Interaction.InputBox("Enter amount", "Add to shopping list " + slName, "1", -1, -1);
                if (text != "" && text != null)
                {
                    double amount;
                    if (Double.TryParse(text, out amount))
                    {
                        Material temp = new Material(selectedItem);
                        temp.Amount = amount;
                        temp.BelongsTo = Material.Connection.SHOPPING_LIST;
                        dbManager.AddToShoppingList(slName, temp);
                    }
                }
            }
        }

        private void Add_Shoplist(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Fuq yes!");
        }

        private void Edit_Selected_Item(object sender, RoutedEventArgs e)
        {
            if (selectedItem != null)
            {
                this.EditButton_Click(sender, e);
            }
        }

        private void Remove_Selected_Item(object sender, RoutedEventArgs e)
        {
            if (selectedItem != null)
            {
                Console.WriteLine("poistetaan paskaa");
                dbManager.DeleteMaterialByName(selectedItem.Name, Material.Connection.INVENTORY);
                inventory.Clear();
                AddAllMaterialToInventoryList();
            }
            else if(selectedItems != null)
            {
                Console.WriteLine("poistetaan monta paskaa");
                foreach (Material deleteItems in selectedItems)
                {
                    dbManager.DeleteMaterialByName(deleteItems.Name, Material.Connection.INVENTORY);
                }
                inventory.Clear();
                AddAllMaterialToInventoryList();
            }
        }

        #endregion

        #region Initialize different main window tabs
        // Method that can be used to initialize a tab when it's selected.
        private void OnTabChanged(Object sender, SelectionChangedEventArgs args)
        {
            if (MainWindowTabInventory.IsSelected)
            {
                InitInventoryTab();
            }
            else if (MainWindowTabRecipies.IsSelected)
            {
                InitRecipiesTab();
            }
            else if (MainWindowTabShoppingList.IsSelected)
            {
                InitShoppingListTab();
            }
            else if (MainWindowTabAdvancedSearch.IsSelected)
            {
                InitAdvancedSearchTab();
            }            
        }

        private void InitInventoryTab()
        {
            //throw new NotImplementedException();
        }

        private void InitRecipiesTab()
        {
            recipesView = new ObservableCollection<Recipe>(dbManager.RetrieveAllRecipes());
            RecipesView.ItemsSource = recipesView;
        }

        private void InitShoppingListTab()
        {
            shopLists = new ObservableCollection<ShoppingList>(dbManager.RetrieveAllShoppingLists());
            shoppingListsLW.ItemsSource = shopLists;
        }

        private void InitAdvancedSearchTab()
        {
            //throw new NotImplementedException();
        }
        
        #endregion        

        #region Selection handling - Shoppinglist Tab
        private void shoppingListsLW_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = shoppingListsLW.SelectedIndex;
            if (idx >= 0 && idx < shopLists.Count)
            {
                ShoppingList sl = dbManager.RetrieveShoppingListByName(shoppingListsLW.SelectedItem.ToString());
                List<Material> tempList = sl.Content;
                tempList = tempList.OrderBy(o => o.Name).ToList();
                selectedShopListContent = new ObservableCollection<Material>(tempList);
                shoppingListContentLW.ItemsSource = selectedShopListContent;
            }
        }
        #endregion

        #region Some functions - Shoppinglist Tab

        private void shoppingListContentLW_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // If there are no items selected, cancel viewing the context menu
            if (shoppingListContentLW.SelectedItems.Count <= 0)
                e.Handled = true;
        }

        private void ShopListItem_Click_Remove(object sender, RoutedEventArgs e)
        {
            RemoveFromShoplist();
        }

        private void RemoveFromShoplist()
        {
            ShoppingList sl = (ShoppingList)shoppingListsLW.SelectedItem;
            Material item = (Material)shoppingListContentLW.SelectedItem;
            // remove it from the ShoppingList object's content, update view and database
            sl.RemoveFromContent(item);
            dbManager.UpdateShoppingList(sl);
            selectedShopListContent.Remove(item);
            shoppingListContentLW.Items.Refresh();
        }

        private void Remove_Whole_Shoplist(object sender, RoutedEventArgs e)
        {
            if (selectedItem != null)
            {
                ShoppingList sl = (ShoppingList)shoppingListsLW.SelectedItem;
                dbManager.DeleteShoppingListByName(sl.Name);
                shopLists.Remove(sl);
                shoppingListsLW.Items.Refresh();
                shoppingListContentLW.ItemsSource = null;
                shoppingListContentLW.Items.Refresh();
            }
        }

        private void Print_Selected_Shoplist(object sender, RoutedEventArgs e)
        {
            ShoppingList sl = (ShoppingList)shoppingListsLW.SelectedItem;
            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog() == true)
                dialog.PrintDocument(CreateShoppingListFlowDocument(dialog.PrintableAreaWidth), "Shopping list");        
        }

        /// <summary>
        /// Creates the DocumentPaginator that can be used with PrintDialog's PrintDocument method.
        /// </summary>
        /// <param name="printAreaWidth">Makes sure that the page's width is used completely</param>
        /// <returns>Printable shoppinglist</returns>
        private DocumentPaginator CreateShoppingListFlowDocument(double printAreaWidth)
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
            foreach (Material mat in selectedShopListContent)
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

        private void Create_ShoppingList_And_Add(object sender, RoutedEventArgs e)
        {
            if (selectedItem != null)
            {
                string slName = Interaction.InputBox("Enter name", "Add to a new shopping list:", "", -1, -1);
                if (slName == null && slName == "")
                    return;
                string text = Interaction.InputBox("Enter amount", "Add to shopping list " + slName, "1", -1, -1);
                if (text != "" && text != null)
                {
                    double amount;
                    if (Double.TryParse(text, out amount))
                    {
                        dbManager.AddNewShoppingList(slName);
                        Material temp = new Material(selectedItem);
                        temp.Amount = amount;
                        temp.BelongsTo = Material.Connection.SHOPPING_LIST;
                        dbManager.AddToShoppingList(slName, temp);
                    }
                }
            }
        }


        private void AddToInventoryFromShoplist(object sender, RoutedEventArgs e)
        {
            if (selectedShopListContent != null)
            {
                foreach (Material mat in selectedShopListContent)
                    dbManager.AddToInventoryFromShoplist(mat);
                // HUONO VAIHTOEHTO MUTTA TOIMII :DD
                inventory.Clear();
                AddAllMaterialToInventoryList();
            }
        }

        private void Move_To_Existing_Shoplist(object sender, RoutedEventArgs e)
        {
            Material selectedShoplistItem = (Material)shoppingListContentLW.SelectedItem;
            MenuItem item = e.OriginalSource as MenuItem;
            string slName = item.Header.ToString();
            string text = Interaction.InputBox("Enter amount", "Add to shopping list " + slName, selectedShoplistItem.Amount.ToString(), -1, -1);
            if (text != "" && text != null)
            {
                double amount;
                if (Double.TryParse(text, out amount))
                {
                    Material temp = new Material(selectedShoplistItem);
                    temp.Amount = amount;
                    temp.BelongsTo = Material.Connection.SHOPPING_LIST;
                    dbManager.AddToShoppingList(slName, temp);
                    RemoveFromShoplist();
                }
            }
        }

        private void Move_To_New_Shoplist(object sender, RoutedEventArgs e)
        {
            Material selectedShoplistItem = (Material)shoppingListContentLW.SelectedItem;
            string slName = Interaction.InputBox("Enter name", "Add to a new shopping list:", "", -1, -1);
            if (slName == null && slName == "")
                return;
            string text = Interaction.InputBox("Enter amount", "Add to shopping list " + slName, selectedShoplistItem.Amount.ToString(), -1, -1);
            if (text != "" && text != null)
            {
                double amount;
                if (Double.TryParse(text, out amount))
                {
                    dbManager.AddNewShoppingList(slName);
                    Material temp = new Material((Material)shoppingListContentLW.SelectedItem);
                    temp.Amount = amount;
                    temp.BelongsTo = Material.Connection.SHOPPING_LIST;
                    dbManager.AddToShoppingList(slName, temp);
                    RemoveFromShoplist();
                }
            }
        }
        #endregion

        #region Recipies Tab

        private void LoadRecipiesView()
        {
            recipesView = new ObservableCollection<Recipe>(dbManager.RetrieveAllRecipes());
            RecipesView.ItemsSource = recipesView;
        }

        private void LoadInstructions()
        {
            if ((RecipesView.SelectedItem) != null)
            {
                Recipe a = (Recipe)RecipesView.SelectedItem;
                if (a == null || a.Instructions == null || a.Instructions == "")
                {
                    RecipeInstructions.Text = "";
                }
                else
                    RecipeInstructions.Text = a.Instructions;
            }
            else
                RecipeInstructions.Text = "";
        }

        private void LoadContentView()
        {
            if ((RecipesView.SelectedItem) != null && ((Recipe)RecipesView.SelectedItem).Content.Count > 0)
            {
                LoadRecipiesView();
                Recipe a = (Recipe)RecipesView.SelectedItem;
                if (a == null || a.Instructions == null || a.Instructions == "")
                {
                    RecipeInstructions.Text = "";
                }
                else
                    RecipeInstructions.Text = a.Instructions;
                if (a.Content.Count > 0)
                {
                    recipesMaterials = new ObservableCollection<Material>(a.Content);
                    RecipesMaterials.ItemsSource = recipesMaterials;
                }
            }
            else
            {
                RecipesMaterials.ItemsSource = null;
                RecipesMaterials.Items.Clear();
            }
        }



        private void RecipesView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadContentView();
            LoadInstructions();
        }

        private void DeleteContent_Click(object sender, RoutedEventArgs e)
        {
            if (RecipesMaterials.SelectedItem != null)
            {
                dbManager.DeleteMaterialFromRecipe(((Recipe)RecipesView.SelectedItem).Name, (((Material)RecipesMaterials.SelectedItem).Name));
                LoadContentView();
            }
        }

        private void DeleteRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (RecipesView.SelectedItem != null)
            {
                dbManager.DeleteRecipeByName(((Recipe)RecipesView.SelectedItem).Name);
                LoadRecipiesView();
                LoadContentView();
                LoadInstructions();
            }
        }

        private void CreateNewRecipeOkButton_Click(object sender, RoutedEventArgs e)
        {
            if (dbManager.RetrieveRecipeByName(NewRecipeName.Text).Count < 1)
            {
                dbManager.AddNewRecipe(NewRecipeName.Text, NewRecipeInstructions.Text);
                AddNewRecipeGrid.Visibility = System.Windows.Visibility.Hidden;
                AddNewRecipeStackPanel.Visibility = System.Windows.Visibility.Hidden;
                LoadRecipiesView();
                LoadInstructions();
            }
            else
            {
                System.Windows.MessageBox.Show("Recipe already exists!");
            }
        }

        private void CreateRecipe_Click(object sender, RoutedEventArgs e)
        {
            NewRecipeName.Text = "";
            NewRecipeInstructions.Text = "";
            AddNewRecipeGrid.Visibility = System.Windows.Visibility.Visible;
            AddNewRecipeStackPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void CreateNewRecipeCanselButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewRecipeGrid.Visibility = System.Windows.Visibility.Hidden;
            AddNewRecipeStackPanel.Visibility = System.Windows.Visibility.Hidden;
            AddNewMaterialToRecipeGrid.Visibility = System.Windows.Visibility.Hidden;
            AddNewMaterialToRecipeStackPanel.Visibility = System.Windows.Visibility.Hidden;
        }

        private void AddMaterialToRecipe_Click(object sender, RoutedEventArgs e)
        {
            AddNewMaterialToRecipeName.Text = "";
            AddNewMaterialToRecipeAmount.Text = "";
            AddNewMaterialToRecipeGrid.Visibility = System.Windows.Visibility.Visible;
            AddNewMaterialToRecipeStackPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void AddNewMaterialToRecipeOkButton_Click(object sender, RoutedEventArgs e)
        {
            if (dbManager.RetrieveMaterialByName(AddNewMaterialToRecipeName.Text, Material.Connection.INVENTORY) == null)
            {
                System.Windows.MessageBox.Show("Material not found!");
            }
            else if (AddNewMaterialToRecipeAmount.Text == "")
            {
                System.Windows.MessageBox.Show("Amount of material is missing");
            }
            else if (Convert.ToDouble(AddNewMaterialToRecipeAmount.Text) <= 0)
            {
                System.Windows.MessageBox.Show("Amount of material cannot be zero!");
            }
            else
            {
                dbManager.AddMaterialToRecipe(((Recipe)RecipesView.SelectedItem).Name, AddNewMaterialToRecipeName.Text, Convert.ToDouble(AddNewMaterialToRecipeAmount.Text));
                AddNewMaterialToRecipeGrid.Visibility = System.Windows.Visibility.Hidden;
                AddNewMaterialToRecipeStackPanel.Visibility = System.Windows.Visibility.Hidden;
                LoadContentView();
            }
        }

        private void UseRecipeFromInventory_Click(object sender, RoutedEventArgs e)
        {
            Recipe r = (Recipe)RecipesView.SelectedItem;
            for (int i = 0; i < r.Content.Count; i++)
            {
                Material m = new Material(dbManager.RetrieveMaterialByName(r.Content[i].Name, Material.Connection.INVENTORY));
                if (m.Amount > r.Content[i].Amount)
                    m.Amount = m.Amount - r.Content[i].Amount;
                else
                    m.Amount = 0;
                dbManager.UpdateMaterial(dbManager.RetrieveMaterialByName(r.Content[i].Name, Material.Connection.INVENTORY), m);
            }
            inventory.Clear();
            AddAllMaterialToInventoryList();
        }

        #endregion

        #region Public Properties
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public ObservableCollection<Material> Inventory { get { return this.inventory; } }
        public ObservableCollection<ShoppingList> ShopLists { get { return this.shopLists; } }
        public ComboBoxItem ComboBoxAddEditInfinite 
        { 
            get 
            { 
                return this.comboBoxAddEditInfinite; 
            } 
            set 
            {
                if (this.comboBoxAddEditInfinite != value)
                {
                    this.comboBoxAddEditInfinite = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("ComboBoxAddEditInfinite"));
                }
            } 
        }
        public ComboBoxItem ComboBoxAddEditUnit 
        { 
            get 
            { 
                return this.comboBoxAddEditUnit; 
            } 
            set 
            {
                if (this.comboBoxAddEditUnit != value)
                {
                    this.comboBoxAddEditUnit = value;
                    PropertyChanged(this, new PropertyChangedEventArgs("ComboBoxAddEditUnit"));
                }
            } 
        }

        #endregion

        #region Some functions - Advanced Search Tab
 
        private void GroupComboBox_Update()
        {
            object lastSelected = GroupComboBox.SelectedItem;

            GroupComboBox.Items.Clear();
            GroupComboBox.Items.Add("");

            groups = new HashSet<Material>(dbManager.RetrieveAllMaterials());
            IList<Material> groupList = groups.ToList();

            for (int i = 1; i < groups.Count; i++)
            {
                GroupComboBox.Items.Add(groupList[i].GroupName);
                for (int y = 1; y < i; y++)
                {
                    if (groupList[y].GroupName == groupList[i].GroupName)
                    {
                        GroupComboBox.Items.Remove(groupList[i].GroupName);
                        break;
                    }
                }
            }
            GroupComboBox.SelectedItem = lastSelected;
        }
        
        private void AdvancedSearchBox_Update()
        {
            searchRecipe.Clear();
            searchMaterial.Clear();
            AdvancedResultList.Items.Clear();

            string symbol = AdvancedSearchComboBox.Text;
            int amount = 0;
            int.TryParse(QuantityTextBox.Text, out amount);

            string group = "";
            if (GroupComboBox.SelectedIndex != -1)
            {
                group = GroupComboBox.SelectedItem.ToString();
            }

            if (MaterialCheckBox.IsChecked == true)
            {
                searchMaterial = new ObservableCollection<Material>(dbManager.SearchMats(AdvancedSearchBox.Text,
                      Material.Connection.INVENTORY, symbol, amount, group));
                foreach (Material o in searchMaterial)
                {
                    AdvancedResultList.Items.Add(o);
                }
            }

            if (RecipeCheckBox.IsChecked == true)
            {
               searchRecipe = new ObservableCollection<Recipe>(dbManager.SearchRecipes(AdvancedSearchBox.Text));
               for (int i = 0; i < searchRecipe.Count; i++)
               {
                   AdvancedResultList.Items.Add(searchRecipe[i].Name);
               }
            }
        }

        private void AdvancedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
                AdvancedSearchBox_Update();
        }
        private void AdvancedSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
        }
        private void MaterialCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RecipeCheckBox.IsChecked = false;
            AdvancedSearchBox_Update();
        }
        private void MaterialCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AdvancedSearchBox_Update();
        }
        private void RecipeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MaterialCheckBox.IsChecked = false;
            AdvancedSearchBox_Update();
        }
        private void RecipeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AdvancedSearchBox_Update();
        }
        private void AdvancedSearchComboBox_DropDownClosed(object sender, EventArgs e)
        {
            AdvancedSearchBox_Update();
        }
        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            AdvancedSearchBox_Update();
        }
        private void GroupComboBox_DropDownOpened(object sender, EventArgs e)
        {
            GroupComboBox_Update();
        }
        private void GroupComboBox_DropDownClosed(object sender, EventArgs e)
        {
            AdvancedSearchBox_Update();
        }
        #endregion

        #region main window search

        private void GroupFilter_DropDownOpened(object sender, EventArgs e)
        {
            GroupFilter_Update();
        }
        private void GroupFilter_Update()
        {
            object lastSelected = GroupFilter.SelectedItem;

            GroupFilter.Items.Clear();
            GroupFilter.Items.Add("");

            groups = new HashSet<Material>(dbManager.RetrieveAllMaterials());
            IList<Material> groupList = groups.ToList();

            for (int i = 1; i < groups.Count; i++)
            {
                GroupFilter.Items.Add(groupList[i].GroupName);
                for (int y = 1; y < i; y++)
                {
                    if (groupList[y].GroupName == groupList[i].GroupName)
                    {
                        GroupFilter.Items.Remove(groupList[i].GroupName);
                        break;
                    }
                }
            }
            GroupFilter.SelectedItem = lastSelected;
        }

        private void SearchBox_Update()
        {
            searchRecipe.Clear();
            searchMaterial.Clear();
            InventoryItemList2.Items.Clear();

            string symbol = QuantityFilterComboBox.Text;
            int amount = 0;
            int.TryParse(QuantityFilterTextBox.Text, out amount);

            string group = "";
            if (GroupFilter.SelectedIndex != -1)
            {
                group = GroupFilter.SelectedItem.ToString();
            }

            if (MCheckBox.IsChecked == true)
            {
                searchMaterial = new ObservableCollection<Material>(dbManager.SearchMats(SearchFilter.Text,
                      Material.Connection.INVENTORY, symbol, amount, group));
                foreach (Material o in searchMaterial)
                {
                    InventoryItemList2.Items.Add(o);
                }
            }

            if (RCheckBox.IsChecked == true)
            {
                searchRecipe = new ObservableCollection<Recipe>(dbManager.SearchRecipes(SearchFilter.Text));
                for (int i = 0; i < searchRecipe.Count; i++)
                {
                    InventoryItemList2.Items.Add(searchRecipe[i].Name);
                }
            }
        }

        private void MCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RCheckBox.IsChecked = false;
            SearchBox_Update();
        }
        private void MCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SearchBox_Update();
        }
        private void RCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MCheckBox.IsChecked = false;
            SearchBox_Update();
        }
        private void RCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SearchBox_Update();
        }
        private void QuantityFilterComboBox_DropDownClosed(object sender, EventArgs e)
        {
            SearchBox_Update();
        }
        private void QuantityFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchBox_Update();
        }
        private void GroupFilter_DropDownClosed(object sender, EventArgs e)
        {
            SearchBox_Update();
        }
        #endregion

        #region Menubar Functions

        private void About_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Inventory Management Application\n\nVille Hannu\nVille Minkkinen\nLauri Nykänen\nMikko Ollila\nJukka Pelander\n\nGooglaa nimellä Facebookista, jos haluat tietää lisää.");
        }

        private void MenuExit_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion 

        #region Context menu functions - Recipes Tab
        
        

        private void Print_Recipe_Click(object sender, RoutedEventArgs e)
        {
            Recipe selectedRecipe = (Recipe)RecipesView.SelectedItem;
            PrintDialog dialog = new PrintDialog();
            if (dialog.ShowDialog() == true)
                dialog.PrintDocument(CreateRecipeFlowDocument(dialog.PrintableAreaWidth, selectedRecipe), selectedRecipe.Name);  
        }

        private DocumentPaginator CreateRecipeFlowDocument(double printAreaWidth, Recipe selectedRecipe)
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
            foreach (Material mat in selectedRecipe.Content)
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
            flowDoc.Blocks.Add(new Paragraph(new Run(selectedRecipe.Instructions)));
            IDocumentPaginatorSource doc = flowDoc;
            return doc.DocumentPaginator;
        }
        #endregion

       

     

       

      

       

     

   

    }
}
