using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace InventoryManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Private variables
        private ObservableCollection<Material> inventory = new ObservableCollection<Material>();
        private Material selectedItem = null;
        private List<Material> selectedItems = null;

        private DatabaseManager dbManager = new DatabaseManager();

        private ObservableCollection<ShoppingList> shopLists;
        private ObservableCollection<Material> selectedShopListContent;

        private ObservableCollection<Material> search = new ObservableCollection<Material>();

        #endregion

        #region Initialize first time
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InventoryManagement_Loaded(object sender, RoutedEventArgs e)
        {
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
            if (SearchFilter.Text == String.Empty)
            {
                SearchFilter.Text = "Search..";
            }
        }
        private void SearchFilter_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchFilter.Text == "Search..")
            {
                SearchFilter.Text = String.Empty;
            }
        }

        private void SearchFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchFilter.Text != "Search.." && SearchFilter.Text != String.Empty)
            {
                dbManager.SearchMaterials(SearchFilter.Text);
            }
        }
        #endregion

        #region Edit single material - Inventory Tab
        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            //Decrease selected item quantity
            if (this.selectedItem != null && selectedItem.Amount >= 1)
            {
                Material tempItem = dbManager.RetrieveMaterialByName(selectedItem.Name);
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
                Material tempItem = dbManager.RetrieveMaterialByName(selectedItem.Name);
                selectedItem.Amount++;
                selectedItem.LastModified = DateTime.Now;
                dbManager.UpdateMaterial(tempItem, selectedItem);
                UpdateInventoryItemPanel();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            //Go to manage inventory tab and select selected item
            if (this.selectedItem != null)
            {
                tabControl.SelectedIndex = 4;
                InitManageInventoryTab(selectedItem);
            }
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
                this.ItemNameContent.Content = selectedItem.Name;
                this.ItemQuantityContent.Content = selectedItem.Amount;
                this.ItemUnitContent.Content = selectedItem.TypeOfMeasure + " (" + selectedItem.DisplayUnit.Name + ")";
                this.ItemGroupContent.Content = selectedItem.GroupName;
                this.ItemDescriptionContent.Content = selectedItem.ExtraInfo;                
                this.ItemLastModifiedContent.Content = selectedItem.GetLastModifiedString();
                if (selectedItem.BestBefore != new DateTime(0))
                {
                    this.ItemBestBeforeContent.Content = selectedItem.GetBestBeforeString();
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
                this.ItemPriceContent.Content = "";
                this.ItemPriceUnit.Content = "";
                this.ItemIsInfiniteContent.Content = "";
                this.ItemLastModifiedContent.Content = "";
                this.ItemBestBeforeContent.Content = "";
            }
        }
        #endregion

        #region Context menu functions - Inventory Tab
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
            }
        }

        //private void menuItem_CopyUsername_Click(object sender, RoutedEventArgs e)
        //{
        //    //Clipboard.SetText(mySelectedItem.Username);
        //    Console.WriteLine("Tama tuli tanne");
        //}
        //private void ListBox_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    // a better version would turn the sender into the ListBoxItem
        //    // find the list box, then find the ContextMenu that way
        //    this.LeftClickMenu.PlacementTarget = sender as UIElement;
        //    this.LeftClickMenu.IsOpen = true;
        //}

        //private void ListBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    // disables the SelectedIndex code
        //    e.Handled = true;
        //}

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
            else if (MainWindowTabManageInventory.IsSelected)
            {
                InitManageInventoryTab();
            }
        }

        private void InitInventoryTab()
        {
            //throw new NotImplementedException();
        }

        private void InitRecipiesTab()
        {
            //throw new NotImplementedException();
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

        private void InitManageInventoryTab(Material editMaterial = null)
        {
            if (editMaterial != null)
            {
                Material tempItem = dbManager.RetrieveMaterialByName(editMaterial.Name);

                //Some logic

                dbManager.UpdateMaterial(tempItem, editMaterial);
            }
        }
        #endregion        

        #region Selection handling - Shoppinglist Tab
        private void shoppingListsLW_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = shoppingListsLW.SelectedIndex;
            if (idx >= 0 && idx < shopLists.Count)
            {
                ShoppingList sl = dbManager.RetrieveShoppingListByName(shoppingListsLW.SelectedItem.ToString());
                selectedShopListContent = new ObservableCollection<Material>(sl.Content);
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
            ShoppingList sl = (ShoppingList) shoppingListsLW.SelectedItem;
            Material item = (Material) shoppingListContentLW.SelectedItem;
            // remove it from the ShoppingList object's content, update view and database
            sl.RemoveFromContent(item);
            dbManager.UpdateShoppingList(sl);
            selectedShopListContent.Remove(item);
            shoppingListContentLW.Items.Refresh();
        }

        private void Remove_Whole_Shoplist(object sender, RoutedEventArgs e)
        {
            ShoppingList sl = (ShoppingList)shoppingListsLW.SelectedItem;
            dbManager.DeleteShoppingListByName(sl.Name);
            shopLists.Remove(sl);
            shoppingListsLW.Items.Refresh();
            shoppingListContentLW.ItemsSource = null;
            shoppingListContentLW.Items.Refresh();
        }

        private void Create_ShoppingList_And_Add(object sender, RoutedEventArgs e)
        {

        }

        #endregion
        
        #region Public Properties
        public ObservableCollection<Material> Inventory { get { return this.inventory; } }
        public ObservableCollection<ShoppingList> ShopLists { get { return this.shopLists; } }
        #endregion

        #region Some functions - Advanced Search Tab
        private void QuantityEqualsButton_Click(object sender, RoutedEventArgs e)
        {
            String Qeb = this.QuantityEqualsButton.Content.ToString();
            if (Qeb == ">")
            {
                this.QuantityEqualsButton.Content = "<";
            }
            else if (Qeb == "<")
            {
                this.QuantityEqualsButton.Content = "=";
            }
            else if (Qeb == "=")
            {
                this.QuantityEqualsButton.Content = ">";
            }
        }

        private void AdvancedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AdvancedSearchBox.Text != "Search.." && AdvancedSearchBox.Text != String.Empty)
            {
                search.Clear();
               // dbManager.SearchMaterials(AdvancedSearchBox.Text);
                List<Material> result = dbManager.SearchMaterials(AdvancedSearchBox.Text);
                foreach (Material item in result)
                {
                    search.Add(
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
        }

        private void SearchTest_Click(object sender, RoutedEventArgs e)
        {
            AdvancedResultList.Items.Clear();

            foreach (Material o in search)
            AdvancedResultList.Items.Add(o);
        }

        #endregion

        #region TEST FUNCTIONS
        private void mikonTestBtn_Click(object sender, RoutedEventArgs e)
        {
            dbManager.ReCreateDB();
            dbManager.CreateSampleData();
            Console.WriteLine("Testing method RetreiveAllMaterials()");
            dbManager.RetrieveAllMaterials();
            Console.WriteLine("Testing method AddNewMaterial(hehkulamppu)");
            dbManager.AddNewMaterial(new Material("Hehkulamppu", "Varaosat", false, 100, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.MaxValue, null, Unit.G, Material.Connection.INVENTORY));
            Console.WriteLine("Testing method RetreiveAllMaterialsInGroup(Ruoka)");
            dbManager.RetrieveMaterialsInGroup("Ruoka");
            Console.WriteLine("Testing method RetreiveAllMaterials()");
            dbManager.RetrieveAllMaterials();
            Console.WriteLine("Testing method RetrieveMaterialsByAmount(E, 1)");
            dbManager.RetrieveMaterialsByAmount(DatabaseManager.ROperator.E, 1);
            Console.WriteLine("Testing method RetrieveMaterialByName(Hehkulamppu)");
            Material hehkulamppu = dbManager.RetrieveMaterialByName("Hehkulamppu");
            Console.WriteLine("Testing method UpdateMaterial(hehkulamppu)");
            dbManager.UpdateMaterial(hehkulamppu, new Material("Hehkulamppu", "Varaosat", false, 2, Material.MeasureType.PCS, DateTime.Now, DateTime.MaxValue, null, Unit.PCS, Material.Connection.INVENTORY));
            Console.WriteLine("Testing method RetrieveMaterialByName(Hehkulamppu)");
            dbManager.RetrieveMaterialByName("Hehkulamppu");
            Console.WriteLine("Testing method DeleteMaterialByName(Kovalevy)");
            dbManager.DeleteMaterialByName("Kovalevy");
            Console.WriteLine("Testing method RetreiveAllMaterials()");
            dbManager.RetrieveAllMaterials();
            Console.WriteLine("Testing method RetreiveAllShoppingLists()");
            dbManager.RetrieveAllShoppingLists();
            Console.WriteLine("Testing method ");
            Console.WriteLine("Testing method ");
            Console.WriteLine("Testing method ");
            Console.WriteLine("Testing method ");
            Console.WriteLine("Testing method ");
            Console.WriteLine("Testing method ");

            //Robbed your button Mikko ;) t. Ville
            Console.WriteLine("Testing method RetrieveAllRecipes()");
            dbManager.RetrieveAllRecipes();
            Console.WriteLine("Testing method RetrieveRecipeByMaterialList()");
            dbManager.RetrieveRecipeByMaterialList(dbManager.RetrieveMaterialsByAmount(DatabaseManager.ROperator.E, 240));
            Console.WriteLine("Testing method RetrieveRecipeByName()");
            dbManager.RetrieveRecipeByName("Resepti");
            Console.WriteLine("Testing method RetrieveRecipeByName(string)");
            dbManager.RetrieveRecipeByMaterialName("ES");
            Console.WriteLine("Testing method RetrieveRecipeByName(<Material>)");
            dbManager.RetrieveRecipeByMaterial(dbManager.RetrieveMaterialByName("Ruisleipä"));
            Console.WriteLine("Testing method RetrieveRecipeByNamePart()");
            dbManager.RetrieveRecipeByNamePart("ept");
        }
        #endregion

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            dbManager.AddToShoppingList("Ruokakauppa", new Material("Märkäsimo", "Muut", false, 1, Material.MeasureType.PCS, Unit.PCS, Material.Connection.SHOPPING_LIST));
            dbManager.AddToShoppingList("Ruokakauppa", new Material("Kakka", "Muut", false, 2, Material.MeasureType.PCS, Unit.PCS, Material.Connection.SHOPPING_LIST));
            dbManager.AddToShoppingList("Ruokakauppa", new Material("Pieru", "Muut", false, 3, Material.MeasureType.PCS, Unit.PCS, Material.Connection.SHOPPING_LIST));
            dbManager.AddToShoppingList("Ruokakauppa", new Material("Oksennus", "Muut", false, 4, Material.MeasureType.PCS, Unit.PCS, Material.Connection.SHOPPING_LIST));
        }


   



    }
}
