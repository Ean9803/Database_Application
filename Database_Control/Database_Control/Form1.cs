using System.Data.SqlClient;
using System.Data;
using System;
using Microsoft.VisualBasic;

namespace Database_Control
{
    public partial class MainForm : Form
    {
        public SQL Connection { get; internal set; }
        public MainForm()
        {
            Connection = new SQL("GameStation\\SQLEXPRESS", "root", "root");
            InitializeComponent();

            if (Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Position='Admin'", null), "Name").Count == 0)
            {
                Connection.InsertData("[Maestro].[dbo].[EMPLOYEE]", ("Position", "Admin"), ("Name", "Admin"), ("Username", "Admin"), ("Password", "Admin"), ("Security", int.MaxValue));
            }

            LogoutBtn.Visible = false;

            if (AutoLogin)
                LoginBtn_Click(this, EventArgs.Empty);
        }

        private WindowData DisplayControl;
        private StatusType Stat;
        public enum WindowType { Login, Delivery, Product, Edit, Ordering }

        public void SetWindow(WindowType Type, Dictionary<string, object> DataIn)
        {
            if (DisplayControl != null)
            {
                if (DisplayControl.OpenWindow(Type, DataIn))
                {
                    MainDisplay.SelectTab((int)Type);
                }
            }
            else
            {
                MainDisplay.SelectTab(0);
            }
        }

        public enum List { OrderList, ListDisplay, OrderDiplay_Company, OrderDisplay_Product, UIList, ControlList, ProductItemList, ProductSupplier, ProductReferences }
        public FlowLayoutPanel GetList(List Item)
        {
            switch (Item)
            {
                case List.OrderList:
                    return OrderList;
                case List.ListDisplay:
                    return OptionsList;
                case List.OrderDiplay_Company:
                    return CompanyList;
                case List.OrderDisplay_Product:
                    return ProductList;
                case List.UIList:
                    return TopUI;
                case List.ControlList:
                    return ItemOptionPanel;
                case List.ProductItemList:
                    return ProductOptionList;
                case List.ProductSupplier:
                    return SupplierList;
                case List.ProductReferences:
                    return ReferencedOrdersList;
                default:
                    return OrderList;
            }
        }

        public enum PanelDetail { None, Delivery, Product, Company, Employee }
        public void SetDetailPanel(PanelDetail Detail)
        {
            DetailTabs.SelectTab((int)Detail);
        }

        private bool AutoLogin = true;

        private void LoginBtn_Click(object sender, EventArgs e)
        {
            List<Dictionary<string, object>> User = Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Username=@User AND Password=@Pass", new (string, string)[] { ("@User", AutoLogin ? "Admin" : UserName.Text), ("@Pass", AutoLogin ? "Admin" : PassWord.Text) }), "Salesman_ID", "Name", "Position", "Password");
            if (User.Count == 0)
            {
                MessageBox.Show("Login Invalid");
            }
            else
            {
                Stat = new StatusType(StatusType.CreateFrom((string)User[0]["Position"]), (int)User[0]["Salesman_ID"], (string)User[0]["Name"], (string)User[0]["Position"], (string)User[0]["Password"]);
                DisplayControl = new WindowData(this, Stat);
                LogoutBtn.Visible = true;
                SetWindow(WindowType.Delivery, new Dictionary<string, object>() { { "INIT", (string)User[0]["Position"] }, { "NAME", (string)User[0]["Name"] } });
            }
        }

        private void LogoutBtn_Click_1(object sender, EventArgs e)
        {
            LogoutBtn.Visible = false;
            UserName.Text = "";
            PassWord.Text = "";
            SetWindow(WindowType.Login, null);
        }

        private void CreateNewUser_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(UserName.Text) && PassWord.Text.Length >= 10)
            {
                List<Dictionary<string, object>> User = Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Username=@User", new (string, string)[] { ("@User", UserName.Text) }), "Salesman_ID");
                if (User.Count > 0)
                {
                    MessageBox.Show("Username taken, please pick another");
                }
                else
                {
                    string Name = Interaction.InputBox("Enter Employee Name");
                    if (!string.IsNullOrEmpty(Name))
                    {
                        Connection.InsertData("[Maestro].[dbo].[EMPLOYEE]", ("Position", "Grunt"), ("Name", Name), ("Username", UserName.Text), ("Password", PassWord.Text), ("Security", 0));

                        User = Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Username=@User AND Password=@Pass", new (string, string)[] { ("@User", UserName.Text), ("@Pass", PassWord.Text) }), "Salesman_ID", "Name", "Position", "Password");

                        Stat = new StatusType(StatusType.CreateFrom((string)User[0]["Position"]), (int)User[0]["Salesman_ID"], (string)User[0]["Name"], (string)User[0]["Position"], (string)User[0]["Password"]);
                        DisplayControl = new WindowData(this, Stat);
                        LogoutBtn.Visible = true;
                        SetWindow(WindowType.Delivery, new Dictionary<string, object>() { { "INIT", (string)User[0]["Position"] }, { "NAME", (string)User[0]["Name"] } });
                    }
                    else
                    {
                        MessageBox.Show("Please enter valid name");
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter " + (string.IsNullOrEmpty(UserName.Text) ? ("a valid Username " + (PassWord.Text.Length < 10 ? "and " : "")) : "") + (PassWord.Text.Length < 10 ? "a Password with 10 or more characters" : ""));
            }
        }

        private void ItemSearch_TextChanged(object sender, EventArgs e)
        {
            WindowData.SortItems(GetList(List.OrderList), ItemSearch.Text);
        }

        private void CompanySearch_TextChanged(object sender, EventArgs e)
        {
            WindowData.SortItems(GetList(List.OrderDiplay_Company), CompanySearch.Text);
        }

        private void ProductSearch_TextChanged(object sender, EventArgs e)
        {
            WindowData.SortItems(GetList(List.OrderDisplay_Product), ProductSearch.Text);
        }

        private void SupplierSearch_TextChanged(object sender, EventArgs e)
        {
            WindowData.SortItems(GetList(List.ProductSupplier), SupplierSearch.Text);
        }

        private void referenceSearch_TextChanged(object sender, EventArgs e)
        {
            WindowData.SortItems(GetList(List.ProductReferences), referenceSearch.Text);
        }

        private void CancelOrder_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Delivery, null);
        }

        public void FillDeliveryDisplay(Dictionary<string, object> ListItems)
        {
            DeliveryInfo.Clear();
            DeliveryInfo.AppendText("Order ID: " + ListItems["Order_ID"].ToString() + 
                "\n--------------------------------------\n" +
                "Order Status: " + WindowData.GetOrderStatus((int)ListItems["Status"]) + 
                "\n--------------------------------------\n" +
                "Order creation date: " + ListItems["CreationDate"].ToString());
            List<Dictionary<string, object>> Employee = Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Salesman_ID=@ID", new (string, string)[] { ("@ID", ListItems["Salesman_ID"].ToString()) }), "Name", "Username", "Position");
            if (Employee.Count > 0)
                DeliveryInfo.AppendText("Order Created by: " + Employee[0]["Name"].ToString() + " | Current Position: " + Employee[0]["Position"].ToString() + "\n");
            else
                DeliveryInfo.AppendText("Order Created by: [EMPLOYEE NOT FOUND]\n");
            DeliveryInfo.AppendText("[MEMO]:\n");
            DeliveryInfo.AppendText(ListItems["Memo"].ToString() + "\n");
            DeliveryInfo.AppendText("[ORDER HISTORY]:\n");
            DeliveryInfo.AppendText(ListItems["History"].ToString());
        }

        public Action SaveOrderAction;

        private void SaveOrder_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(OrderPassword.Text))
            {
                if (OrderPassword.Text.Equals(Stat.GetPass()))
                {
                    if (SaveOrderAction != null)
                        SaveOrderAction();
                    MessageBox.Show("Action complete");
                }
                else
                {
                    MessageBox.Show("Enter correct password to complete");
                }
            }
            else
            {
                MessageBox.Show("Enter password to complete");
            }
        }

        public void resetPass()
        {
            OrderPassword.Text = "";
            ProductPassword.Text = "";
        }

        public string GetMemo()
        {
            return Memo.Text;
        }

        public void SetMemo(string MemoString)
        {
            Memo.Text = MemoString;
        }

        public void FillProductDisplay(Dictionary<string, object> ListItems)
        {
            ProductInfo.Clear();
            ProductInfo.AppendText("Product ID: " + ListItems["Product_ID"].ToString() + " | Name: " + ListItems["Name"].ToString() +
                "\n--------------------------------------\n" +
                "Available Amount: " + ListItems["Available_Amt"].ToString() +
                "\n--------------------------------------\n" +
                "Price: $" + ListItems["Price"].ToString() +
                "\n--------------------------------------\n" +
                "Prep Time: " + ListItems["Time"].ToString() + "\n");
            List<Dictionary<string, object>> Comp = Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("Company_ID=@ID", new (string, string)[] { ("@ID", ListItems["Supplier"].ToString()) }), "Name");
            ProductInfo.AppendText("Supplier: " + Comp[0]["Name"].ToString() + "\n\n");
            ProductInfo.AppendText("Description:\n" + ListItems["Description"].ToString());
        }

        public void SetReferencedNum(string Num)
        {
            ReservedLable.Text = Num;
        }

        public void SetPrice(string Price)
        {
            ProductPrice.Text = Price;
        }

        public float GetProductPrice()
        {
            if (float.TryParse(ProductPrice.Text, out float Result))
            {
                return Result;
            }
            return -1;
        }

        public string GetProductDesc()
        {
            return ProductDescript.Text;
        }

        public void SetProductDesc(string MemoString, bool ReadOnly)
        {
            ProductDescript.ReadOnly = ReadOnly;
            ProductDescript.Text = MemoString;
        }

        public int GetPrepTime()
        {
            if(int.TryParse(PrepTime.Text, out int Result))
            {
                return Math.Max(Result, 0);
            }
            return 0;
        }

        public void SetPrepTime(string Time)
        {
            PrepTime.Text = Time;
        }

        private void ProductBack_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Delivery, null);
        }

        public Action SaveProductAction;

        private void SaveProduct_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ProductPassword.Text))
            {
                if (ProductPassword.Text.Equals(Stat.GetPass()))
                {
                    if (SaveProductAction != null)
                        SaveProductAction();
                    MessageBox.Show("Action complete");
                }
                else
                {
                    MessageBox.Show("Enter correct password to complete");
                }
            }
            else
            {
                MessageBox.Show("Enter password to complete");
            }
        }

        public void SetTotalAmount(int Amount)
        {
            ProductAmount.Text = Amount.ToString();
        }

        public int GetTotalAmount()
        {
            if (int.TryParse(ProductAmount.Text, out int Result))
            {
                return Result;
            }
            return -1;
        }

        public void SetProductName(string Name, bool ReadOnly)
        {
            ProductName.ReadOnly = ReadOnly;
            ProductName.Text = Name;
        }

        public string GetProductName()
        {
            return ProductName.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            WindowData.CreateDeleteDialog("Enter Password To Delete Links", (string Text) =>
            {
                if (Text.Equals(Stat.GetPass()) && Stat.HasAbility(StatusType.Action.CanDeleteDelivery))
                {
                    List<(Control, WindowData.CollectionReturn Call)> Removes = WindowData.GetSelectedObjects("OrderProductSelect");
                    Dictionary<string, object> DataIn = null;
                    List<string> DeletedItemsProduct = new List<string>();
                    List<string> DeletedItemsOrder = new List<string>();
                    foreach (var item in Removes)
                    {
                        Dictionary<string, object> Data = item.Call();
                        string ProductID = Data["Product_ID"].ToString();
                        string OrderID = Data["Order_ID"].ToString();
                        if (!DeletedItemsProduct.Contains(Data["ProductName"].ToString()))
                            DeletedItemsProduct.Add(Data["ProductName"].ToString());

                        if (!DeletedItemsOrder.Contains(OrderID))
                            DeletedItemsOrder.Add(OrderID);

                        DataIn = (Dictionary<string, object>)Data["INFO"];

                        List<Dictionary<string, object>> Delivery = Connection.GetData("[Maestro].[dbo].[DELIVERIES]", ("Order_ID=@ID", new (string, string)[] { ("@ID", OrderID) }), "Bundle_ID");
                        List<Dictionary<string, object>> Bundles = Connection.GetData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID", new (string, string)[] { ("@ID", Delivery[0]["Bundle_ID"].ToString()) }), "Bundle_ID", "Product_ID", "Quantity");
                        int UpForDelete = 0;
                        int Restock = 0;
                        for (int i = 0; i < Bundles.Count; i++)
                        {
                            if (Bundles[i]["Product_ID"].ToString().Equals(ProductID))
                            {
                                UpForDelete++;
                                Restock += (int)Bundles[i]["Quantity"];
                            }
                        }

                        if (UpForDelete != Bundles.Count)
                        {
                            Connection.DeleteData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID AND Product_ID=@PID", new (string, string)[] { ("@ID", Delivery[0]["Bundle_ID"].ToString()), ("@PID", ProductID) }));
                        }
                        else
                        {
                            Connection.DeleteData("[Maestro].[dbo].[DELIVERIES]", ("Order_ID=@ID", new (string, string)[] { ("@ID", OrderID) }));
                            Connection.DeleteData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID AND Product_ID=@PID", new (string, string)[] { ("@ID", Delivery[0]["Bundle_ID"].ToString()), ("@PID", ProductID) }));
                        }

                        List<Dictionary<string, object>> ProductEntry = Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("Product_ID=@ID", new (string, string)[] { ("@ID", ProductID) }), "Available_Amt");
                        Connection.UpdateData("[Maestro].[dbo].[PRODUCTS]", ("Product_ID=@ID", new (string, string)[] { ("@ID", ProductID) }), ("Available_Amt", ((int)ProductEntry[0]["Available_Amt"] + Restock).ToString()));
                    }
                    WindowData.UpdateUserHistory(this, Stat.GetIDNumber(), "User deleted: " + String.Join(", ", DeletedItemsProduct) + "\n\tFrom Orders:\n\t|[" + String.Join(", ", DeletedItemsOrder) + "]");
                    MessageBox.Show("Deletion Complete");
                    SetWindow(MainForm.WindowType.Product, DataIn);
                }
                else
                {
                    if (Stat.HasAbility(StatusType.Action.CanDeleteDelivery))
                        MessageBox.Show("User does not have access to this ability");
                    else
                        MessageBox.Show("Incorrect Password");
                }
            });
        }

        public void SetPropertiesWindow(int Index)
        {
            ProductProperties.SelectTab(Index);
        }

        public void FillCompanyDisplay(Dictionary<string, object> ListItems)
        {

        }

        public void FillEmployeeDisplay(Dictionary<string, object> ListItems)
        {

        }
    }
}