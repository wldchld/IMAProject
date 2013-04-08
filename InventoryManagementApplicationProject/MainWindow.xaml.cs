using System;
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
        private ObservableCollection<Recipe> recipes { get; set; }
        private ObservableCollection<Material> recipesMaterials { get; set; }

        private ObservableCollection<Material> searchMaterial = new ObservableCollection<Material>();
        private ObservableCollection<Recipe> searchRecipe = new ObservableCollection<Recipe>();
        private HashSet<string> groups = new HashSet<string>();

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
        private void SearchFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSearchResults();
        }

        private void UpdateSearchResults()
        {
            List<Material> matching = new List<Material>();
            Console.WriteLine(SearchFilter.Text);
            if (SearchFilter.Text.Length < 1 && QuantityFilterTextBox.Text.Length < 1 && GroupFilter.SelectedIndex == 0)
            {
                inventory.Clear();
                AddAllMaterialToInventoryList();
                InventoryItemList.ItemsSource = inventory;
            }
            else
            {
                if (inventory != null && InventoryItemList != null)
                {
                    for (int i = 0; i < inventory.Count; i++)
                    {
                        Material mat = inventory[i];
                        double amount = 0;
                        bool add = true;
                        if (SearchFilter.Text.Length > 0 && !mat.Name.ToLower().StartsWith(SearchFilter.Text.ToLower()))
                            add = false;
                        if (add && Double.TryParse(QuantityFilterTextBox.Text, out amount) && QuantityFilterComboBox.SelectedIndex > 0)
                        {
                            if (QuantityFilterComboBox.SelectedIndex == 1 && mat.Amount <= amount)
                                add = false;
                            else if (QuantityFilterComboBox.SelectedIndex == 2 && mat.Amount != amount)
                                add = false;
                            else if (QuantityFilterComboBox.SelectedIndex == 3 && mat.Amount >= amount)
                                add = false;
                            else if (QuantityFilterComboBox.SelectedIndex == 4 && mat.Amount < amount)
                                add = false;
                            else if (QuantityFilterComboBox.SelectedIndex == 5 && mat.Amount > amount)
                                add = false;
                        }
                        if (GroupFilter.SelectedIndex != 0 && (string)GroupFilter.SelectedItem != mat.GroupName)
                            add = false;
                        if(add)
                            matching.Add(inventory[i]);
                    }
                    InventoryItemList.ItemsSource = new ObservableCollection<Material>(matching);
                }
            }
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
            InventoryItemList.ItemsSource = inventory;
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
            InventoryItemList.ItemsSource = inventory;
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
                InventoryItemList.ItemsSource = inventory;
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
                InventoryItemList.ItemsSource = inventory;
            }
        }

        #endregion

        #region Initialize different main window tabs
        // Method that can be used to initialize a tab when it's selected.
        private void OnTabChanged(Object sender, SelectionChangedEventArgs args)
        {
            if (MainWindowTabRecipies.IsSelected)
            {
                InitRecipiesTab();
            }
            else if (MainWindowTabShoppingList.IsSelected)
            {
                InitShoppingListTab();
            }          
        }

        private void InitRecipiesTab()
        {
            recipes = new ObservableCollection<Recipe>(dbManager.RetrieveAllRecipes());
            RecipesView.ItemsSource = recipes;
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

        #region Recipes Tab

        private void LoadRecipeView()
        {
            
        }

        private void LoadContentView()
        {
            if (!(RecipesView.SelectedItem).Equals(null))
            {
                Recipe a = (Recipe)RecipesView.SelectedItem;
                RecipeInstructions.Text = a.Instructions;
                if (a.Content.Count > 0)
                {
                    recipesMaterials = new ObservableCollection<Material>(a.Content);
                    RecipesMaterials.ItemsSource = recipesMaterials;
                }
                LoadRecipeView();
            }
        }

        private void RecipesView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadContentView();
        }

        private void DeleteContent_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItem != null)
            {
                dbManager.DeleteMaterialFromRecipe(((Recipe)RecipesView.SelectedItem).Name, (((Material)RecipesMaterials.SelectedItem).Name));
                LoadContentView();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItem != null)
            {
                dbManager.DeleteRecipeByName(((Recipe)RecipesView.SelectedItem).Name);
                LoadContentView();
            }
        }

        private void CreateNewRecipeOkButton_Click(object sender, RoutedEventArgs e)
        {
            dbManager.AddNewRecipe(NewRecipeName.Text, NewRecipeInstructions.Text);
            AddNewRecipeGrid.Visibility = System.Windows.Visibility.Hidden;
            AddNewRecipeStackPanel.Visibility = System.Windows.Visibility.Hidden;
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
            dbManager.AddMaterialToRecipe(((Recipe)RecipesView.SelectedItem).Name, AddNewMaterialToRecipeName.Text, Convert.ToDouble(AddNewMaterialToRecipeAmount.Text));
            AddNewMaterialToRecipeGrid.Visibility = System.Windows.Visibility.Hidden;
            AddNewMaterialToRecipeStackPanel.Visibility = System.Windows.Visibility.Hidden;
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

        #region main window search

       

        private void GroupFilter_Update_Items()
        {
            groups.Clear();
            groups.Add("");
            List<Material> all = dbManager.RetrieveAllMaterials();

            foreach(Material mat in all)
                groups.Add(mat.GroupName);

            GroupFilter.ItemsSource = groups;
            GroupFilter.Items.Refresh();
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

        private void QuantityFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSearchResults();
        }

        private void QuantityFilterComboBox_DropDownClosed(object sender, EventArgs e)
        {
            UpdateSearchResults();
        }

        private void GroupFilter_DropDownOpened(object sender, EventArgs e)
        {
            GroupFilter_Update_Items();
        }

        private void GroupFilter_DropDownClosed(object sender, EventArgs e)
        {
            UpdateSearchResults();
        }
    }
}
