using System.Data.SqlClient;
using System.Data;
using System;


namespace Database_Control
{
    public partial class MainForm : Form
    {
        private SQL Connection;
        public MainForm()
        {

            Connection = new SQL("Cayden", "root", "root");

            InitializeComponent();
            LogoutBtn.Visible = false;
        }

        private WindowData DisplayControl;
        public enum WindowType { Login, Delivery, Product, List, Edit, Ordering }

        public void SetWindow(WindowType Type, Dictionary<string, string> DataIn)
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

        #region Orders

        public void InitOrderControlPanel()
        {
            ControlPanelOptions.SelectTab(0);
            groupBox1.Text = "Current Orders";
            UpdateOrderControlPanel();
        }

        public void UpdateOrderControlPanel()
        {
            List<WindowData.CollectionReturn> Item = WindowData.GetSelectedObjects("OrderSelect");
            DetailBox.ResetText();
            if (Item.Count > 0)
            {
                DeleteOrder.Enabled = true;
                EditOrderBtn.Enabled = true;
                foreach (var item in Item)
                {
                    Dictionary<string, string> Data = (Dictionary<string, string>)item();
                    foreach (var Info in Data)
                    {
                        DetailBox.AppendText(Info.Key + "\n");
                        DetailBox.AppendText(Info.Value + "\n");
                    }
                }
            }
            else
            {
                DeleteOrder.Enabled = false;
                EditOrderBtn.Enabled = false;
            }
        }

        private void EditOrderBtn_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Ordering, null);
        }

        private void CreateOrder_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Ordering, null);
        }

        private void DeleteOrder_Click(object sender, EventArgs e)
        {

        }

        private void ProductsBtn_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Delivery, new Dictionary<string, string>() { { "Type", "PRODUCTS" } });
        }

        #endregion

        #region Products

        public void InitProductsControlPanel()
        {
            ControlPanelOptions.SelectTab(1);
            groupBox1.Text = "Current Products";
            UpdateProductsPanel();
        }

        public void UpdateProductsPanel()
        {
            List<WindowData.CollectionReturn> Item = WindowData.GetSelectedObjects("ProductSelect");
            DetailBox.ResetText();
            if (Item.Count > 0)
            {
                DeleteProduct.Enabled = true;
                OpenProduct.Enabled = true;
                foreach (var item in Item)
                {
                    Dictionary<string, string> Data = (Dictionary<string, string>)item();
                    foreach (var Info in Data)
                    {
                        DetailBox.AppendText(Info.Key + "\n");
                        DetailBox.AppendText(Info.Value + "\n");
                    }
                }
            }
            else
            {
                DeleteProduct.Enabled = false;
                OpenProduct.Enabled = false;
            }
        }

        private void DeleteProduct_Click(object sender, EventArgs e)
        {

        }

        private void OpenProduct_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Product, null);
        }

        private void OpenDelivery_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Delivery, new Dictionary<string, string>() { { "Type", "ORDERS" } });
        }

        private void NewProductBtn_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Edit, null);
        }

        #endregion

        #region Edit

        private void CancelOrder_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Delivery, new Dictionary<string, string>() { { "Type", "ORDERS" } });
        }

        #endregion

        #region ProductItem

        private void ProductBack_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Delivery, new Dictionary<string, string>() { { "Type", "PRODUCTS" } });
        }

        private void ProductEdit_Click_1(object sender, EventArgs e)
        {

        }

        #endregion

        #region List

        private void CompanyBack_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Delivery, new Dictionary<string, string>() { { "Type", "ORDERS" } });
        }

        private void ItemBack_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Delivery, new Dictionary<string, string>() { { "Type", "ORDERS" } });
        }

        private void ShowCompanyList_Click(object sender, EventArgs e)
        {

        }

        private void ShowProductList_Click(object sender, EventArgs e)
        {

        }

        private void OpenListViewDelivery_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.List, null);
        }

        private void OpenListViewProduct_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.List, null);
        }

        #endregion

        #region Edit

        private void EditBack_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Delivery, new Dictionary<string, string>() { { "Type", "ORDERS" } });
        }

        #endregion

        private void Login_Click(object sender, EventArgs e)
        {
            //TESTING METHODS
            // string tableName = "EMPLOYEE";
            //string[] columns = { "Salesman_ID", "Position", "Name", "Username", "Password" };
            //object[] values = { "5", "Sales", "Cayden", "CayG", "Riskm168" };
            //Connection.InsertData(tableName, columns, values);
            //string tableName = "EMPLOYEE";
            //string whereClause = "Salesman_ID = 1"; // Specify the condition to delete a specific row
            //Connection.DeleteData(tableName, whereClause);
            //string tableName = "EMPLOYEE";
            //string[] columns = { "Position", "Name", "Password" };
            //object[] values = { "Manager", "Aaron Munson", "Riskm169" };
            //string whereClause = "Salesman_ID = 45"; // Specify the condition to update a specific row
            //Connection.UpdateData(tableName, columns, values, whereClause);

            DisplayControl = new WindowData(this, new StatusType(StatusType.DefaultAdmin));
            LogoutBtn.Visible = true;
            SetWindow(WindowType.Delivery, new Dictionary<string, string>() { { "Type", "ORDERS" } });
        }
        public enum List { OrderList, ListDisplay, OrderDiplay_Company, OrderDisplay_Product }
        public FlowLayoutPanel GetList(List Item)
        {
            switch (Item)
            {
                case List.OrderList:
                    return OrderList;
                case List.ListDisplay:
                    return ItemList;
                case List.OrderDiplay_Company:
                    return CompanyList;
                case List.OrderDisplay_Product:
                    return ProductList;
                default:
                    return OrderList;
            }
        }

        private void LogoutBtn_Click(object sender, EventArgs e)
        {
            LogoutBtn.Visible = false;
            SetWindow(WindowType.Login, null);
        }
    }

    public class SQL
    {
        private SqlConnection com;

        public SQL(string Database, string Username, string Password)
        {
            try
            {
                com = new SqlConnection(@"server=" + Database + ";User ID=" + Username + ";Password=" + Password + ";TrustServerCertificate=True");
                com.Open();
                MessageBox.Show("Connection Open!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Connection! " + ex.Message);
            }
        }

        public void InsertData(string tableName, List<(string Col, object Val)> Data)
        {
            try
            {
                string columnsString = "";
                string parameterPlaceholders = "";

                for (int i = 0; i < Data.Count; i++)
                {
                    columnsString += Data[i].Col + ",";
                    parameterPlaceholders += $"@param{i},";
                }
                columnsString = columnsString.Substring(0, columnsString.Length - 1);
                parameterPlaceholders = parameterPlaceholders.Substring(0, parameterPlaceholders.Length - 1);

                string sqlInsert = $"INSERT INTO {tableName} ({columnsString}) VALUES ({parameterPlaceholders})";
                using (SqlCommand cmd = new SqlCommand(sqlInsert, com))
                {
                    for (int i = 0; i < Data.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@param{i}", Data[i].Val);
                    }
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Data inserted successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting data! " + ex.Message);
            }
        }

        public void DeleteData(string tableName, string whereClause)
        {
            try
            {
                string sqlDelete = $"DELETE FROM {tableName} WHERE {whereClause}";
                using (SqlCommand cmd = new SqlCommand(sqlDelete, com))
                {
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"{rowsAffected} rows deleted successfully!");
                    }
                    else
                    {
                        MessageBox.Show("No rows deleted.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting data! " + ex.Message);
            }
        }

        public void UpdateData(string tableName, List<(string Col, object Val)> Data, string[] columns, object[] values, string whereClause)
        {
            try
            {
                string setClause = "";
                for (int i = 0; i < Data.Count; i++)
                {
                    setClause += $"{Data[i].Col} = @param{i},";
                }
                setClause = setClause.Substring(0, setClause.Length - 1);

                string sqlUpdate = $"UPDATE {tableName} SET {setClause} WHERE {whereClause}";


                using (SqlCommand cmd = new SqlCommand(sqlUpdate, com))
                {
                    for (int i = 0; i < columns.Length; i++)
                    {
                        cmd.Parameters.AddWithValue($"@param{i}", values[i]);
                    }
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"{rowsAffected} rows updated successfully!");
                    }
                    else
                    {
                        MessageBox.Show("No rows updated.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating data! " + ex.Message);
            }
        }


        public List<Dictionary<string, object>> GetData(string tableName, List<string> Cols)
        {
            string sqlGet = $"SELECT {string.Join(", ", Cols)} FROM {tableName}";
            List<Dictionary<string, object>> Ret = new List<Dictionary<string, object>>();
            using (SqlCommand cmd = new SqlCommand(sqlGet, com))
            {
                SqlDataReader Read = cmd.ExecuteReader();

                while (Read.Read())
                {
                    Ret.Add(new Dictionary<string, object>());
                    for (int i = 0; i < Cols.Count; i++)
                    {
                        if (!Ret[Ret.Count - 1].ContainsKey(Cols[i]))
                        {
                            Ret[Ret.Count - 1].Add(Cols[i], null);
                        }
                        Ret[Ret.Count - 1][Cols[i]] = Read.GetValue(i);
                    }
                }
            }
            return Ret;
        }

        public void Close()
        {
            com.Close();
        }

    }

    public class StatusType
    {
        public const int DefaultAdmin = int.MinValue;
        public const int DefaultViewer = 0;

        private int Stat;
        public StatusType(int Value)
        {
            Stat = Value;
        }

        public bool GetStat(int Index)
        {
            return CheckStatus(Stat, Index);
        }

        public static bool CheckStatus(int Stat, int Index)
        {
            return (Stat & (1 << Index - 1)) != 0;
        }

        public static int UpdateValue(int Current, int NewBit, int pos)
        {
            int clearBit = ~(1 << pos);
            int mask = Current & clearBit;
            return mask | (NewBit << pos);
        }

        public static string RandomString(int Length)
        {
            string Out = "";
            Random Rng = new Random();
            for (int i = 1; i < Length; i++)
            {
                Out += (char)Rng.Next((int)'!', (int)'~');
            }
            return Out;
        }
    }

    public class WindowData
    {
        private StatusType Status;
        private MainForm Form;
        public WindowData(MainForm Form, StatusType Status) { this.Form = Form; this.Status = Status; }

        private delegate bool NewWindowState(MainForm Form, StatusType Status, Dictionary<string, string> DataIn);
        public delegate object CollectionReturn();

        private Dictionary<MainForm.WindowType, NewWindowState> Windows = new Dictionary<MainForm.WindowType, NewWindowState>()
        {
            { MainForm.WindowType.Login, OpenLogin },
            { MainForm.WindowType.Delivery, OpenDelivery },
            { MainForm.WindowType.Product, OpenProduct },
            { MainForm.WindowType.List, OpenList },
            { MainForm.WindowType.Edit, OpenEdit },
            { MainForm.WindowType.Ordering, OpenOrdering },
        };

        public bool OpenWindow(MainForm.WindowType Type, Dictionary<string, string> DataIn)
        {
            if (Windows.ContainsKey(Type))
            {
                if (Windows[Type] != null)
                {
                    return Windows[Type](Form, Status, DataIn);
                }
            }
            return false;
        }

        private static void DeleteAllConents(FlowLayoutPanel list)
        {
            list.Controls.Clear();
        }

        private static Dictionary<string, (uint Max, Color GroupColor, List<(GroupBox, Action, CollectionReturn Attribute)> Collection)> selectionGroup = new Dictionary<string, (uint, Color, List<(GroupBox, Action, CollectionReturn)>)>();

        public static void SetSelectionGroup(string Name, uint MaxItems, Color colorGroup)
        {
            if (string.IsNullOrEmpty(Name))
                return;
            if (!selectionGroup.ContainsKey(Name))
            {
                selectionGroup.Add(Name, (MaxItems, colorGroup, new List<(GroupBox, Action, CollectionReturn)>()));
            }
            else
            {
                selectionGroup[Name] = (MaxItems, colorGroup, selectionGroup[Name].Collection);
            }
        }

        public static List<CollectionReturn> GetSelectedObjects(string Group)
        {
            if (string.IsNullOrEmpty(Group))
                return new List<CollectionReturn>();
            List<CollectionReturn> Ret = new List<CollectionReturn>();
            if (selectionGroup.ContainsKey(Group))
            {
                for (int i = 0; i < selectionGroup[Group].Collection.Count; i++)
                {
                    if (selectionGroup[Group].Collection[i].Attribute != null)
                    {
                        Ret.Add(selectionGroup[Group].Collection[i].Attribute);
                    }
                }
            }
            return Ret;
        }

        private static void UpdateSelectionGroups()
        {
            foreach (var item in selectionGroup)
            {
                for (int i = item.Value.Collection.Count - 1; i >= 0; i--)
                {
                    if (item.Value.Collection[i].Item1 == null || item.Value.Collection[i].Item2 == null)
                    {
                        item.Value.Collection.RemoveAt(i);
                    }
                }
                while (item.Value.Collection.Count > item.Value.Max && item.Value.Collection.Count > 0)
                {
                    item.Value.Collection[0].Item2();
                    item.Value.Collection.RemoveAt(0);
                }
            }
        }

        private static void RemoveFromCollection(string Group, GroupBox Box)
        {
            if (!string.IsNullOrEmpty(Group))
            {
                if (selectionGroup.ContainsKey(Group))
                {
                    for (int i = selectionGroup[Group].Collection.Count - 1; i >= 0; i--)
                    {
                        if (selectionGroup[Group].Collection[i].Item1 == Box)
                        {
                            selectionGroup[Group].Collection[i].Item2();
                            selectionGroup[Group].Collection.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private static bool InCollection(string Group, GroupBox Box)
        {
            if (!string.IsNullOrEmpty(Group))
            {
                if (selectionGroup.ContainsKey(Group))
                {
                    bool Add = false;
                    foreach (var item in selectionGroup[Group].Collection)
                    {
                        if (item.Item1 == Box)
                        {
                            Add = true;
                        }
                    }
                    return Add;
                }
            }
            return false;
        }

        private static void AddNewConentItem(FlowLayoutPanel list, string GroupName, int Height = 70, EventHandler OnClick = null, string SelectionGroup = null, CollectionReturn Return = null)
        {
            Panel Item = new Panel();
            GroupBox Box = new GroupBox();
            int Width = (list.Size.Width - 20);
            Item.BackColor = Color.White;
            Item.BorderStyle = BorderStyle.Fixed3D;
            int Down = 3;
            for (int i = 0; i < list.Controls.Count; i++)
            {
                Down += list.Controls[i].Size.Height;
            }
            Item.Location = new Point(3, Down);
            Item.Name = "Item#" + StatusType.RandomString(5);
            Item.Size = new Size(Width, Height);
            Item.TabIndex = 0;
            Item.Controls.Add(Box);

            Box.Dock = DockStyle.Fill;
            Box.Location = new Point(0, 0);
            Box.Name = "groupBox";
            Box.Size = new Size((int)(Width * 0.9f), (int)(Height * 0.8f));
            Box.TabIndex = 0;
            Box.TabStop = false;
            Box.Text = GroupName;
            Box.MouseEnter += (object? sender, EventArgs e) => { if (!InCollection(SelectionGroup, Box)) Item.BackColor = Color.Tan; };
            Box.MouseLeave += (object? sender, EventArgs e) => { if (!InCollection(SelectionGroup, Box)) Item.BackColor = Color.White; };

            if (!string.IsNullOrEmpty(SelectionGroup))
            {
                if (selectionGroup.ContainsKey(SelectionGroup))
                {
                    Box.Click += (object? sender, EventArgs e) =>
                    {
                        Item.BackColor = selectionGroup[SelectionGroup].GroupColor;
                        if (!InCollection(SelectionGroup, Box))
                        {
                            selectionGroup[SelectionGroup].Collection.Add((Box, () => { Item.BackColor = Color.White; }, Return));
                            UpdateSelectionGroups();
                        }
                        else
                        {
                            RemoveFromCollection(SelectionGroup, Box);
                        }
                    };
                }
            }

            if (OnClick != null)
                Box.Click += OnClick;

            list.Controls.Add(Item);
        }

        private static bool OpenLogin(MainForm Form, StatusType Status, Dictionary<string, string> DataIn)
        {
            return true;
        }

        private static bool OpenDelivery(MainForm Form, StatusType Status, Dictionary<string, string> DataIn)
        {
            DeleteAllConents(Form.GetList(MainForm.List.OrderList));
            if (DataIn.ContainsKey("Type"))
            {
                if (DataIn["Type"].Equals("PRODUCTS"))
                {
                    Form.InitProductsControlPanel();
                    SetSelectionGroup("ProductSelect", 1, Color.Gray);
                    //Grab Order items
                    for (int i = 0; i < 10; i++)
                    {
                        string Data = "Product " + i;
                        AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Product: " + i,
                        OnClick: (object? sender, EventArgs e) =>
                        {
                            Form.UpdateProductsPanel();
                        }, SelectionGroup: "ProductSelect",
                        Return:
                        () =>
                        {
                            return new Dictionary<string, string>() { { "Test Product:", Data } };
                        });
                    }
                }
                else if (DataIn["Type"].Equals("ORDERS"))
                {
                    Form.InitOrderControlPanel();
                    SetSelectionGroup("OrderSelect", 1, Color.Gray);
                    //Grab Order items
                    for (int i = 0; i < 10; i++)
                    {
                        string Data = "Order " + i;
                        AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Order: " + i,
                        OnClick: (object? sender, EventArgs e) =>
                        {
                            Form.UpdateOrderControlPanel();
                        }, SelectionGroup: "OrderSelect",
                        Return:
                        () =>
                        {
                            return new Dictionary<string, string>() { { "Test Order:", Data } };
                        });
                    }
                }
            }

            return true;
        }

        private static bool OpenProduct(MainForm Form, StatusType Status, Dictionary<string, string> DataIn)
        {
            return true;
        }

        private static bool OpenList(MainForm Form, StatusType Status, Dictionary<string, string> DataIn)
        {
            return true;
        }

        private static bool OpenEdit(MainForm Form, StatusType Status, Dictionary<string, string> DataIn)
        {
            return true;
        }

        private static bool OpenOrdering(MainForm Form, StatusType Status, Dictionary<string, string> DataIn)
        {
            DeleteAllConents(Form.GetList(MainForm.List.OrderDiplay_Company));
            DeleteAllConents(Form.GetList(MainForm.List.OrderDisplay_Product));

            SetSelectionGroup("CompanySelect", 1, Color.Blue);
            //Grab Order items
            for (int i = 0; i < 10; i++)
            {
                string Data = "Product " + i;
                AddNewConentItem(Form.GetList(MainForm.List.OrderDiplay_Company), "Company: " + i,
                OnClick: (object? sender, EventArgs e) =>
                {
                    Form.UpdateProductsPanel();
                }, SelectionGroup: "CompanySelect",
                Return:
                () =>
                {
                    return new Dictionary<string, string>() { { "Test Product:", Data } };
                });
            }

            SetSelectionGroup("OrderProductSelect", 1000, Color.Green);
            //Grab Order items
            for (int i = 0; i < 10; i++)
            {
                string Data = "Product " + i;
                AddNewConentItem(Form.GetList(MainForm.List.OrderDisplay_Product), "Product: " + i,
                OnClick: (object? sender, EventArgs e) =>
                {
                    Form.UpdateProductsPanel();
                }, SelectionGroup: "OrderProductSelect",
                Return:
                () =>
                {
                    return new Dictionary<string, string>() { { "Test Product:", Data } };
                });
            }
            return true;
        }
    }
}