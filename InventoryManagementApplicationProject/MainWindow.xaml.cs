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

namespace InventoryManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        private ObservableCollection<Material> inventory = new ObservableCollection<Material>();
        private List<ShoppingList> shopLists;
        private List<Material> selectedShopListContent;

        private Material selectedItem = null;
        private List<Material> selectedItems = null;
        private DatabaseManager dbManager = new DatabaseManager();
        private Search search = new Search();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InventoryManagement_Loaded(object sender, RoutedEventArgs e)
        {
            //When Inventory Management window is loaded 
            //(application is started)
            InventoryItemList.ItemsSource = this.inventory;
            
            /*
            dbManager.ReCreateDB();
            dbManager.CreateSampleData();
            */
            /*
            //Some test data
            inventory.Add(new Material("testi1", "peikonpallit", 1));
            inventory.Add(new Material("asd", "nutturat", 1));
            inventory.Add(new Material("qwe", null, 1));
            inventory.Add(new Material("rty", null, 1));
            */

            AddAllMaterialToInventoryList();
            //List<Material> allItems = dbManager.RetrieveAllMaterials();
            ////dbManager.PrintMaterialList(allItems);

            //foreach (Material item in allItems)
            //{
            //    inventory.Add(new Material(item.Name, item.GroupName, item.Infinite, item.Amount, item.TypeOfMeasure, item.DateBought, item.BestBefore, item.ExtraInfo, item.DisplayUnit));
            //}
        }

        private void AddAllMaterialToInventoryList()
        {
            List<Material> allItems = dbManager.RetrieveAllMaterials();
            //dbManager.PrintMaterialList(allItems);

            foreach (Material item in allItems)
            {
                inventory.Add(new Material(item.Name, item.GroupName, item.Infinite, item.Amount, item.TypeOfMeasure, item.DateBought, item.BestBefore, item.ExtraInfo, item.DisplayUnit));
            }
        }
        private void SearchFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchFilter.Text != "Search.." && SearchFilter.Text != String.Empty)
            {
                //Search here
                dbManager.SearchMaterials(SearchFilter.Text);
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            //Decrease selected item quantity
            if (this.selectedItem != null && selectedItem.Amount >= 1)
            {
                Material tempItem = dbManager.RetrieveMaterialByName(selectedItem.Name);
                selectedItem.AddAmount(-1);
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
                selectedItem.AddAmount(1);
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

        //Handle selections. 
        //If one item is selected, it is placed to "selectedItem" variable and "selectedItems" is null.
        //If multiple items are selected, those are placed to "selectedItems" variable as List<Material> and "selectedItem" is null.
        //If all items are deselected both variables are null.
        private void InventoryItemList_SelectionChanged(object sender, SelectionChangedEventArgs e = null)
        {
            Console.WriteLine(sender);
            Console.WriteLine(e);

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
                this.ItemBestBeforeContent.Content = selectedItem.BestBefore;
                Console.WriteLine(selectedItem.BestBefore);

                this.ItemLastModifiedContent.Content = selectedItem.DateBought;

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

        private void mikonTestBtn_Click(object sender, RoutedEventArgs e)
        {
            dbManager.ReCreateDB();
            dbManager.CreateSampleData();
            Console.WriteLine("Testing method RetreiveAllMaterials()");
            dbManager.RetrieveAllMaterials();
            Console.WriteLine("Testing method AddNewMaterial(hehkulamppu)");
            dbManager.AddNewMaterial(new Material("Hehkulamppu", "Varaosat", false, 100, Material.MeasureType.WEIGHT, DateTime.Now, DateTime.MaxValue, null, Unit.G));
            Console.WriteLine("Testing method RetreiveAllMaterialsInGroup(Ruoka)");
            dbManager.RetrieveMaterialsInGroup("Ruoka");
            Console.WriteLine("Testing method RetreiveAllMaterials()");
            dbManager.RetrieveAllMaterials();
            Console.WriteLine("Testing method RetrieveMaterialsByAmount(E, 1)");
            dbManager.RetrieveMaterialsByAmount(DatabaseManager.ROperator.E, 1);
            Console.WriteLine("Testing method RetrieveMaterialByName(Hehkulamppu)");
            Material hehkulamppu = dbManager.RetrieveMaterialByName("Hehkulamppu");
            Console.WriteLine("Testing method UpdateMaterial(hehkulamppu)");
            dbManager.UpdateMaterial(hehkulamppu, new Material("Hehkulamppu", "Varaosat", false, 2, Material.MeasureType.PCS, DateTime.Now, DateTime.MaxValue, null, Unit.PCS));
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
        }

        // Method that can be used to initialize a tab when it's selected.
        private void OnTabChanged(Object sender, SelectionChangedEventArgs args)
        {
            if (MainWindowTabShoppingList.IsSelected)
                InitShoppingListTab();
            else if (MainWindowTabManageInventory.IsSelected)
                InitManageInventoryTab();
            
            /*
            else if (MainWindowTabAdvancedSearch.IsSelected) ;
            else if (MainWindowTabInventory.IsSelected) ;
            else if (MainWindowTabRecipies.IsSelected) ;
            */
        }

        private void InitShoppingListTab()
        {
            shopLists = dbManager.RetrieveAllShoppingLists();
            shoppingListsLW.ItemsSource = shopLists;
        }

        private void InitManageInventoryTab(Material editMaterial = null)
        {
            Console.WriteLine(editMaterial);
            // Lisäsin null tarkistuksen, ettei ohjelma kaadu jos vaihtaa tähän välilehteen.
            if (editMaterial != null)
            {
                Material tempItem = dbManager.RetrieveMaterialByName(editMaterial.Name);

                //Some logic
                dbManager.UpdateMaterial(tempItem, editMaterial);
            }
        }

        private void jukanNabbula_Click(object sender, RoutedEventArgs e)
        {
            inventory.Clear(); 
            AddAllMaterialToInventoryList();
        }

        private void shoppingListsLW_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = shoppingListsLW.SelectedIndex;
            if (idx >= 0 && idx < shopLists.Count)
            {
                ShoppingList sl = dbManager.RetrieveShoppingListByName(shoppingListsLW.SelectedItem.ToString());
                selectedShopListContent = sl.Content;
                shoppingListContentLW.ItemsSource = selectedShopListContent;
            }
        }

        private void shoppingListContentLW_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // If there are no items selected, cancel viewing the context menu
            if (shoppingListContentLW.SelectedItems.Count <= 0)
            {
                e.Handled = true;
            }
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
    }
}
