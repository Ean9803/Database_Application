using System.Data.SqlClient;
using System.Data;
using System;
using Microsoft.VisualBasic;
using System.Diagnostics.CodeAnalysis;

namespace Database_Control
{
    public partial class MainForm : Form
    {
        public SQL Connection { get; internal set; }
        public string FileName = "DataBaseOptions.options";
        public string[] Servers = new string[] { "GameStation\\SQLEXPRESS", "DESKTOP-E\\SQLEXPRESS" };
        private byte[] DefaultPicture;

        public MainForm()
        {
            Connection = new SQL();
            string DataBase = "";
            bool Quit = false;
            Exception? E = null;
            do
            {
                if (!File.Exists(Application.StartupPath + FileName))
                {
                    File.WriteAllText(Application.StartupPath + FileName, String.Join("\n", Servers));
                }
                else
                {
                    List<string> Contents = new List<string>(File.ReadAllLines(Application.StartupPath + FileName));
                    List<string> Add = new List<string>();
                    foreach (var item in Servers)
                    {
                        if (!Contents.Contains(item))
                        {
                            Add.Add(item);
                        }
                    }
                    File.AppendAllText(Application.StartupPath + FileName, "\n" + String.Join("\n", Add));
                }

                if (E != null)
                {
                    MessageBox.Show(E.Message);
                }

                CreateServerDialog((string Text) => { DataBase = Text; }, () => { Quit = true; }, Application.StartupPath + FileName);
            } while (!Connection.Connect(DataBase, "root", "root", out E) && !Quit);

            InitializeComponent();


            DefaultPicture = ImageStringEncoderDecoder.ImageBytes(ProductImage.BackgroundImage);

            if (!Quit)
            {
                if (Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Position='Admin'", null), "Name").Count == 0)
                {
                    Connection.InsertData("[Maestro].[dbo].[EMPLOYEE]", ("Position", "Admin"), ("Name", "Admin"), ("Username", "Admin"), ("Password", "Admin"), ("Image", DefaultPicture), ("History", "[" + DateTime.UtcNow.Date.ToString("dd / MM / yyyy") + "]: User Created through default"));
                }

                LogoutBtn.Visible = false;

                if (AutoLogin)
                    LoginBtn_Click(this, EventArgs.Empty);
            }
            else
            {
                this.Close();
            }
        }

        public static void CreateServerDialog(InputField.InputEnd EndAction, Action QuitAction, string File)
        {
            InitForm ServerForm = new InitForm(EndAction, QuitAction, File);

            ServerForm.ShowDialog();
        }

        private WindowData DisplayControl;
        private StatusType Stat;

        public class WindowProfile
        {
            private int Window;
            public int Tab { internal set; get; }

            public WindowProfile(int Window, int Tab)
            {
                this.Window = Window;
                this.Tab = Tab;
            }

            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                return (obj as WindowProfile)?.Window == this.Window;
            }

            public override int GetHashCode()
            {
                return this.Window;
            }
        }
        public class WindowType
        {
            public static readonly WindowProfile Login = new WindowProfile(0, 0);
            public static readonly WindowProfile Delivery = new WindowProfile(1, 1);
            public static readonly WindowProfile Product = new WindowProfile(2, 2);
            public static readonly WindowProfile Company = new WindowProfile(3, 2);
            public static readonly WindowProfile Employee = new WindowProfile(4, 2);
            public static readonly WindowProfile Ordering = new WindowProfile(5, 4);
        }
        //public enum WindowType { Login, Delivery, Product, Company, Ordering }

        public void SetWindow(WindowProfile Type, Dictionary<string, object> DataIn)
        {
            if (DisplayControl != null)
            {
                if (DisplayControl.OpenWindow(Type, DataIn))
                {
                    MainDisplay.SelectTab(Type.Tab);
                }
            }
            else
            {
                MainDisplay.SelectTab(0);
            }
        }

        public enum List { OrderList, ListDisplay, OrderDiplay_Company, OrderDisplay_Product, UIList, ControlList, ProductItemList, ProductSupplier, ProductReferences, EmployeePermissions, EmployeePresets }
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
                case List.EmployeePermissions:
                    return PermissionsList;
                case List.EmployeePresets:
                    return PresetsList;
                default:
                    return OrderList;
            }
        }

        public enum PanelDetail { None, Delivery, Product, Company, Employee }
        public void SetDetailPanel(PanelDetail Detail)
        {
            DetailTabs.SelectTab((int)Detail);
        }

        private bool AutoLogin = false;

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
            LogOut();
        }

        public void LogOut()
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
                        Connection.InsertData("[Maestro].[dbo].[EMPLOYEE]", ("Position", "Grunt"), ("Image", DefaultPicture), ("Name", Name), ("Username", UserName.Text), ("Password", PassWord.Text), ("History", "[" + DateTime.UtcNow.Date.ToString("dd / MM / yyyy") + "]: User Created through login"));

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
            List<Dictionary<string, object>> Bundles = Connection.GetData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID", new (string, string)[] { ("@ID", ListItems["Bundle_ID"].ToString()) }), "Product_ID", "Delivered", "Quantity");
            List<Dictionary<string, object>> Products;
            List<Dictionary<string, object>> Company;
            DateTime CreationDate = DateTime.ParseExact(ListItems["CreationDate"].ToString(), "dd/MM/yyyy",
                                       System.Globalization.CultureInfo.InvariantCulture);
            DeliveryInfo.AppendText("-------------[ORDER CONENTS]-------------\n");
            DeliveryInfo.AppendText(string.Format("\t|{0,15}|{1,10}|{2,10}|{3,11}|{4,12}|\n", "Name(Quantity)", "Price($)", "Supplier", "[DELIVERED]", "[DUE DATE]"));
            foreach (var item in Bundles)
            {
                Products = Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("Product_ID=@ID", new (string, string)[] { ("@ID", item["Product_ID"].ToString()) }), "Name", "Price", "Time", "Supplier");
                Company = Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("Company_ID=@ID", new (string, string)[] { ("@ID", Products[0]["Supplier"].ToString()) }), "Name");
                DeliveryInfo.AppendText(string.Format("\t|{0,15}|{1,10}|{2,10}|{3,11}|{4,12}|\n", (Products[0]["Name"].ToString() + "(" + item["Quantity"].ToString() + ")"),
                    Products[0]["Price"].ToString(), Company[0]["Name"].ToString(), ((int)item["Delivered"] == 1 ? "[YES]" : "[NO]"), (CreationDate.AddDays((int)Products[0]["Time"]).ToString("dd/MM/yyyy"))));
            }
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
            ProductInfo.AppendText("[PRODUCT HISTORY]:\n");
            ProductInfo.AppendText(ListItems["History"].ToString());
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
            if (int.TryParse(PrepTime.Text, out int Result))
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

        public void SetEmployeePass(string Name, bool PasswordChar, bool ReadOnly)
        {
            EmployeePass.ReadOnly = ReadOnly;
            EmployeePass.Enabled = !ReadOnly;
            EmployeePass.PasswordChar = PasswordChar ? '*' : '\0';
            EmployeePass.Text = Name;
        }

        public string GetEmployeePass()
        {
            return EmployeePass.Text;
        }

        public void SetEmployeeUser(string Name, bool ReadOnly)
        {
            EmployeeUser.ReadOnly = ReadOnly;
            EmployeeUser.Enabled = !ReadOnly;
            EmployeeUser.Text = Name;
        }

        public string GetEmployeeUser()
        {
            return EmployeeUser.Text;
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
                        List<Dictionary<string, object>> Bundles = Connection.GetData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID", new (string, string)[] { ("@ID", Delivery[0]["Bundle_ID"].ToString()) }), "Bundle_ID", "Product_ID", "Quantity", "Delivered");
                        int UpForDelete = 0;
                        int Restock = 0;
                        for (int i = 0; i < Bundles.Count; i++)
                        {
                            if (Bundles[i]["Product_ID"].ToString().Equals(ProductID))
                            {
                                UpForDelete++;
                                Restock += ((int)Bundles[i]["Delivered"] == 0 ? (int)Bundles[i]["Quantity"] : 0);
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

        public void SetPhone(string Phone)
        {
            CompanyPhone.Text = Phone;
        }

        public void SetEmail(string Email)
        {
            CompanyEmail.Text = Email;
        }

        public string GetPhone()
        {
            return CompanyPhone.Text;
        }

        private bool IsValidEmail(string eMail)
        {
            bool Result = false;

            try
            {
                var eMailValidator = new System.Net.Mail.MailAddress(eMail);

                Result = (eMail.LastIndexOf(".") > eMail.LastIndexOf("@"));
            }
            catch
            {
                Result = false;
            };

            return Result;
        }

        public (bool, string) GetEmail()
        {
            if (string.IsNullOrEmpty(CompanyEmail.Text))
                return (true, "");
            if (IsValidEmail(CompanyEmail.Text))
            {
                return (true, CompanyEmail.Text);
            }
            return (false, "");
        }

        public void FillCompanyDisplay(Dictionary<string, object> ListItems)
        {
            CompanyInfo.Clear();
            CompanyInfo.AppendText("Name: " + ListItems["Name"].ToString() + "\n");
            CompanyInfo.AppendText("\n[CONTACT INFO]:\n");
            CompanyInfo.AppendText("Phone: " + ListItems["Phone"].ToString() + "\n");
            CompanyInfo.AppendText("Email: " + ListItems["Email"].ToString() + "\n");
            CompanyInfo.AppendText("\nDescription:\n" + ListItems["Description"].ToString() + "\n");
        }

        public void FillEmployeeDisplay(Dictionary<string, object> ListItems)
        {
            EmployeeInfo.Clear();
            EmployeeInfo.AppendText("Employee: " + ListItems["Name"].ToString() + "\n\n");
            EmployeeInfo.AppendText("Position: " + ListItems["Position"].ToString() + "\n\n");
            EmployeeInfo.AppendText("[USERNAME]: " + ListItems["Username"].ToString() + "\n");
            EmployeeInfo.AppendText("-----------------[USER HISTORY]-----------------\n");
            EmployeeInfo.AppendText(ListItems["History"].ToString());
        }

        private void PresetsSearch_TextChanged(object sender, EventArgs e)
        {
            WindowData.SortItems(GetList(List.EmployeePresets), PresetsSearch.Text);
        }

        private void PermissionsSearch_TextChanged(object sender, EventArgs e)
        {
            WindowData.SortItems(GetList(List.EmployeePermissions), PermissionsSearch.Text);
        }

        public void OpenImage()
        {
            OpenFileDialog FileOpen = new OpenFileDialog();
            FileOpen.Filter = "Image Files | *.jpg";
            if (FileOpen.ShowDialog() == DialogResult.OK)
            {
                ProductImage.BackgroundImage = Image.FromFile(FileOpen.FileName);
            }
        }

        public byte[] GetImage()
        {
            return ImageStringEncoderDecoder.ImageBytes(ProductImage.BackgroundImage);
        }

        public void SetImage(byte[] Data)
        {
            if (Data.Length == 0)
            {
                ProductImage.BackgroundImage = ImageStringEncoderDecoder.GetImage(DefaultPicture);
            }
            else
            {
                ProductImage.BackgroundImage = ImageStringEncoderDecoder.GetImage(Data);
            }
        }
    }
}