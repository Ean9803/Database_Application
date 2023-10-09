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
        }

        private WindowData DisplayControl;
        public enum WindowType { Login, Delivery, Product, Edit, Ordering }

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

        public enum List { OrderList, ListDisplay, OrderDiplay_Company, OrderDisplay_Product, UIList }
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
                default:
                    return OrderList;
            }
        }

        private void LoginBtn_Click(object sender, EventArgs e)
        {
            List<Dictionary<string, object>> User = Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Username=@User AND Password=@Pass", new (string, string)[] { ("@User", UserName.Text), ("@Pass", PassWord.Text) }), "Salesman_ID", "Security", "Name", "Position");
            if (User.Count == 0)
            {
                MessageBox.Show("Login Invalid");
            }
            else
            {
                StatusType Stat = new StatusType((int)User[0]["Security"], (int)User[0]["Salesman_ID"]);
                DisplayControl = new WindowData(this, Stat);
                LogoutBtn.Visible = true;
                SetWindow(WindowType.Delivery, new Dictionary<string, string>() { { "Type", "ORDERS" }, { "INIT", (string)User[0]["Position"] }, { "NAME", (string)User[0]["Name"] } });
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

                        User = Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Username=@User AND Password=@Pass", new (string, string)[] { ("@User", UserName.Text), ("@Pass", PassWord.Text) }), "Salesman_ID", "Security", "Name", "Position");

                        StatusType Stat = new StatusType(0, (int)User[0]["Salesman_ID"]);
                        DisplayControl = new WindowData(this, Stat);
                        LogoutBtn.Visible = true;
                        SetWindow(WindowType.Delivery, new Dictionary<string, string>() { { "Type", "ORDERS" }, { "INIT", (string)User[0]["Position"] }, { "NAME", (string)User[0]["Name"] } });
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
    }

    public class SQL
    {
        private SqlConnection com;

        public SQL(string Database, string Username, string Password)
        {
            try
            {
                com = new SqlConnection(@"server=" + Database + ";User ID=" + Username + ";Password=" + Password + ";TrustServerCertificate=True;MultipleActiveResultSets=true");
                com.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Connection! " + ex.Message);
            }
        }

        public void InsertData(string tableName, params (string Col, object Val)[] Data)
        {
            try
            {
                string columnsString = "";
                string parameterPlaceholders = "";

                for (int i = 0; i < Data.Length; i++)
                {
                    columnsString += Data[i].Col + ",";
                    parameterPlaceholders += $"@param{i},";
                }
                columnsString = columnsString.Substring(0, columnsString.Length - 1);
                parameterPlaceholders = parameterPlaceholders.Substring(0, parameterPlaceholders.Length - 1);

                string sqlInsert = $"INSERT INTO {tableName} ({columnsString}) VALUES ({parameterPlaceholders})";
                using (SqlCommand cmd = new SqlCommand(sqlInsert, com))
                {
                    for (int i = 0; i < Data.Length; i++)
                    {
                        cmd.Parameters.AddWithValue($"@param{i}", Data[i].Val);
                    }
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inserting data! " + ex.Message);
            }
        }

        public void DeleteData(string tableName, (string Clause, (string, string)[] WhereParams) whereClause)
        {
            try
            {
                string sqlDelete = $"DELETE FROM {tableName} WHERE {whereClause.Clause}";
                using (SqlCommand cmd = new SqlCommand(sqlDelete, com))
                {
                    if (whereClause.WhereParams != null)
                    {
                        for (int i = 0; i < whereClause.WhereParams.Length; i++)
                        {
                            cmd.Parameters.AddWithValue(whereClause.WhereParams[i].Item1, whereClause.WhereParams[i].Item2);
                        }
                    }

                    int rowsAffected = cmd.ExecuteNonQuery();
                    /*
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"{rowsAffected} rows deleted successfully!");
                    }
                    else
                    {
                        MessageBox.Show("No rows deleted.");
                    }
                    */
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting data! " + ex.Message);
            }
        }

        public void UpdateData(string tableName, (string Clause, (string, string)[] WhereParams) whereClause, params (string Col, object Val)[] Data)
        {
            try
            {
                string setClause = "";
                for (int i = 0; i < Data.Length; i++)
                {
                    setClause += $"{Data[i].Col} = @param{i},";
                }
                setClause = setClause.Substring(0, setClause.Length - 1);

                string sqlUpdate = $"UPDATE {tableName} SET {setClause}" + (string.IsNullOrEmpty(whereClause.Clause) ? "" : $" WHERE {whereClause.Clause}");


                using (SqlCommand cmd = new SqlCommand(sqlUpdate, com))
                {
                    if (whereClause.WhereParams != null)
                    {
                        for (int i = 0; i < whereClause.WhereParams.Length; i++)
                        {
                            cmd.Parameters.AddWithValue(whereClause.WhereParams[i].Item1, whereClause.WhereParams[i].Item2);
                        }
                    }

                    for (int i = 0; i < Data.Length; i++)
                    {
                        cmd.Parameters.AddWithValue($"@param{i}", Data[i].Val);
                    }
                    int rowsAffected = cmd.ExecuteNonQuery();
                    /*
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"{rowsAffected} rows updated successfully!");
                    }
                    else
                    {
                        MessageBox.Show("No rows updated.");
                    }
                    */
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating data! " + ex.Message);
            }
        }


        public List<Dictionary<string, object>> GetData(string tableName, (string Clause, (string, string)[] WhereParams) whereClause, params string[] Cols)
        {
            string sqlGet = $"SELECT {string.Join(", ", Cols)} FROM {tableName}" + (string.IsNullOrEmpty(whereClause.Clause) ? "" : $" WHERE {whereClause.Clause}");
            List<Dictionary<string, object>> Ret = new List<Dictionary<string, object>>();
            using (SqlCommand cmd = new SqlCommand(sqlGet, com))
            {
                try
                {
                    if (whereClause.WhereParams != null)
                    {
                        for (int i = 0; i < whereClause.WhereParams.Length; i++)
                        {
                            cmd.Parameters.AddWithValue(whereClause.WhereParams[i].Item1, whereClause.WhereParams[i].Item2);
                        }
                    }
                    SqlDataReader Read = cmd.ExecuteReader();
                    while (Read.Read())
                    {
                        Ret.Add(new Dictionary<string, object>());
                        for (int i = 0; i < Cols.Length; i++)
                        {
                            if (!Ret[Ret.Count - 1].ContainsKey(Cols[i]))
                            {
                                Ret[Ret.Count - 1].Add(Cols[i], null);
                            }
                            Ret[Ret.Count - 1][Cols[i]] = Read.GetValue(i);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error getting data! " + ex.Message);
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
        public const int DefaultAdmin = int.MaxValue;
        public const int DefaultViewer = 0;

        private int Stat;
        private int ID;
        public StatusType(int Value, int ID)
        {
            Stat = Value;
            this.ID = ID;
        }

        public int GetStatNumber()
        {
            return Stat;
        }

        public int GetIDNumber()
        {
            return ID;
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
                selectionGroup[Name] = (MaxItems, colorGroup, new List<(GroupBox, Action, CollectionReturn)>());
            }
        }

        public static List<CollectionReturn> GetSelectedObjects(string Group)
        {
            if (string.IsNullOrEmpty(Group))
                return new List<CollectionReturn>();
            List<CollectionReturn> Ret = new List<CollectionReturn>();
            if (selectionGroup.ContainsKey(Group))
            {
                for (int i = selectionGroup[Group].Collection.Count - 1; i >= 0; i--)
                {
                    if (selectionGroup[Group].Collection[i].Attribute != null)
                    {
                        Ret.Add(selectionGroup[Group].Collection[i].Attribute);
                    }
                    else
                    {
                        selectionGroup[Group].Collection.RemoveAt(i);
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

        private enum Direction { vertical, horizontal }

        private static void AddNewConentItem(FlowLayoutPanel list, string GroupName, int Size = 70, int Offset = 20, Direction Flow = Direction.vertical, EventHandler OnClick = null, string SelectionGroup = null, CollectionReturn Return = null)
        {
            Panel Item = new Panel();
            ItemBtn Box = new ItemBtn();
            int Width = Flow == Direction.vertical ? (list.Size.Width - Offset) : (list.Size.Height - Offset);
            Item.BackColor = Color.White;
            Item.BorderStyle = BorderStyle.Fixed3D;
            int Down = 3;
            for (int i = 0; i < list.Controls.Count; i++)
            {
                Down += Flow == Direction.vertical ? list.Controls[i].Size.Height : list.Controls[i].Size.Width;
            }
            Item.Location = Flow == Direction.vertical ? new Point(3, Down) : new Point(Down, 3);
            Item.Name = "Item#" + StatusType.RandomString(5);
            Item.Size = Flow == Direction.vertical ? new Size(Width, Size) : new Size(Size, Width);
            Item.TabIndex = 0;
            Item.Controls.Add(Box);

            Box.Dock = DockStyle.Fill;
            Box.Location = new Point(0, 0);
            Box.Name = "groupBox";
            Box.Size = new Size((int)(Item.Size.Width * 0.9f), (int)(Item.Size.Height * 0.8f));
            Box.TabIndex = 0;
            Box.TabStop = false;
            Box.Text = GroupName;


            Box.MouseEnter += (object? sender, EventArgs e) => { if (!InCollection(SelectionGroup, Box)) Item.BackColor = Color.Tan; };
            Box.MouseLeave += (object? sender, EventArgs e) => { if (!InCollection(SelectionGroup, Box)) Item.BackColor = Color.White; };

            if (!string.IsNullOrEmpty(SelectionGroup))
            {
                if (selectionGroup.ContainsKey(SelectionGroup))
                {
                    Box.OnItemClick += (object? sender, EventArgs e) =>
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
                Box.OnItemClick += OnClick;

            list.Controls.Add(Item);
        }

        private static void SelectItem(FlowLayoutPanel list, int Item)
        {
            if (Item >= 0 && Item < list.Controls.Count)
            {
                (list.Controls[Item].Controls[0] as ItemBtn)?.ActivateClickItem();
            }
        }

        private static bool OpenLogin(MainForm Form, StatusType Status, Dictionary<string, string> DataIn)
        {
            DeleteAllConents(Form.GetList(MainForm.List.UIList));
            return true;
        }

        private static bool OpenDelivery(MainForm Form, StatusType Status, Dictionary<string, string> DataIn)
        {
            if (DataIn.ContainsKey("INIT"))
            {
                DeleteAllConents(Form.GetList(MainForm.List.OrderList));
                DeleteAllConents(Form.GetList(MainForm.List.ListDisplay));
                DeleteAllConents(Form.GetList(MainForm.List.UIList));

                AddNewConentItem(Form.GetList(MainForm.List.UIList), "Position: " + DataIn["INIT"], 200, 0, Direction.horizontal);
                if (DataIn.ContainsKey("NAME"))
                {
                    AddNewConentItem(Form.GetList(MainForm.List.UIList), "Employee: " + DataIn["NAME"], 200, 0, Direction.horizontal);
                }

                SetSelectionGroup("OptionSelect", 1, Color.Green);
                AddNewConentItem(Form.GetList(MainForm.List.ListDisplay), "Delivery", Flow: Direction.horizontal, Size: 100,
                        OnClick: (object? sender, EventArgs e) =>
                        {
                            SetSelectionGroup("ItemSelect", 1, Color.Green);
                            DeleteAllConents(Form.GetList(MainForm.List.OrderList));
                            List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[DELIVERIES]", ("", null), "Bundle_ID", "Order_ID");
                            foreach (var item in ListItems)
                            {
                                AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Order_ID: " + item["Order_ID"].ToString(), Flow: Direction.vertical, SelectionGroup: "ItemSelect");
                            }
                        }, 
                        SelectionGroup: "OptionSelect");
                AddNewConentItem(Form.GetList(MainForm.List.ListDisplay), "Product", Flow: Direction.horizontal, Size: 100,
                        OnClick: (object? sender, EventArgs e) =>
                        {
                            SetSelectionGroup("ItemSelect", 1, Color.Green);
                            DeleteAllConents(Form.GetList(MainForm.List.OrderList));
                            List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("", null), "Name");
                            foreach (var item in ListItems)
                            {
                                AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Product: " + item["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "ItemSelect");
                            }
                        }, 
                        SelectionGroup: "OptionSelect");
                AddNewConentItem(Form.GetList(MainForm.List.ListDisplay), "Employee", Flow: Direction.horizontal, Size: 100,
                        OnClick: (object? sender, EventArgs e) =>
                        {
                            SetSelectionGroup("ItemSelect", 1, Color.Green);
                            DeleteAllConents(Form.GetList(MainForm.List.OrderList));
                            List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("", null), "Name");
                            foreach (var item in ListItems)
                            {
                                AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Employee: " + item["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "ItemSelect");
                            }
                        }, 
                        SelectionGroup: "OptionSelect");
                AddNewConentItem(Form.GetList(MainForm.List.ListDisplay), "Company", Flow: Direction.horizontal, Size: 100,
                        OnClick: (object? sender, EventArgs e) =>
                        {
                            SetSelectionGroup("ItemSelect", 1, Color.Green);
                            DeleteAllConents(Form.GetList(MainForm.List.OrderList));
                            List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("", null), "Name");
                            foreach (var item in ListItems)
                            {
                                AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Company: " + item["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "ItemSelect");
                            }
                        }, 
                        SelectionGroup: "OptionSelect");
                SelectItem(Form.GetList(MainForm.List.ListDisplay), 0);
            }

            if (DataIn.ContainsKey("Type") && false)
            {
                if (DataIn["Type"].Equals("PRODUCTS"))
                {
                    SetSelectionGroup("ProductSelect", 1, Color.Gray);
                    //Grab Order items
                    for (int i = 0; i < 10; i++)
                    {
                        string Data = "Product " + i;
                        AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Product: " + i,
                        OnClick: (object? sender, EventArgs e) =>
                        {
                        }, SelectionGroup: "ProductSelect",
                        Return:
                        () =>
                        {
                            return new Dictionary<string, string>() { { "Test Product:", Data } };
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