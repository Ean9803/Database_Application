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

        public enum List { OrderList, ListDisplay, OrderDiplay_Company, OrderDisplay_Product, UIList, ControlList }
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
                default:
                    return OrderList;
            }
        }

        private void LoginBtn_Click(object sender, EventArgs e)
        {
            List<Dictionary<string, object>> User = Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Username=@User AND Password=@Pass", new (string, string)[] { ("@User", UserName.Text), ("@Pass", PassWord.Text) }), "Salesman_ID", "Name", "Position");
            if (User.Count == 0)
            {
                MessageBox.Show("Login Invalid");
            }
            else
            {
                StatusType Stat = new StatusType(StatusType.CreateFrom((string)User[0]["Position"]), (int)User[0]["Salesman_ID"]);
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

                        User = Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Username=@User AND Password=@Pass", new (string, string)[] { ("@User", UserName.Text), ("@Pass", PassWord.Text) }), "Salesman_ID", "Name", "Position");

                        StatusType Stat = new StatusType(StatusType.CreateFrom((string)User[0]["Position"]), (int)User[0]["Salesman_ID"]);
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

        private void ItemSearch_TextChanged(object sender, EventArgs e)
        {
            DisplayControl.SortItems(GetList(List.OrderList), ItemSearch.Text);
        }

        private void CompanySearch_TextChanged(object sender, EventArgs e)
        {
            DisplayControl.SortItems(GetList(List.OrderDiplay_Company), CompanySearch.Text);
        }

        private void ProductSearch_TextChanged(object sender, EventArgs e)
        {
            DisplayControl.SortItems(GetList(List.OrderDisplay_Product), ProductSearch.Text);
        }

        private void CancelOrder_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Delivery, null);
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
            return (Stat & (1 << Index)) != 0;
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

        private static Dictionary<string, Action[]> Defaults = new Dictionary<string, Action[]>()
        {
            { 
                "Admin",
                new Action[]
                {
                    Action.CanSeeDelivey,
                    Action.CanSeeProduct,
                    Action.CanSeeEmployee,
                    Action.CanSeeCompany,
                    Action.CanUpdateDelivery,
                    Action.CanCreateDelivery,
                    Action.CanDeleteDelivery,
                    Action.CanUpdateProduct,
                    Action.CanCreateProduct,
                    Action.CanDeleteProduct,
                    Action.CanUpdateEmployee,
                    Action.CanCreateEmployee,
                    Action.CanDeleteEmployee,
                    Action.CanUpdateCompany,
                    Action.CanCreateCompany,
                    Action.CanDeleteCompany
                }
            },
            {
                "Grunt",
                new Action[]
                {
                    Action.CanSeeDelivey,
                    Action.CanSeeProduct,
                    Action.CanSeeCompany
                }
            },
            {
                "Overseer",
                new Action[]
                {
                    Action.CanSeeDelivey,
                    Action.CanSeeProduct,
                    Action.CanSeeEmployee,
                    Action.CanCreateDelivery,
                    Action.CanUpdateDelivery
                }
            },
            {
                "Inventory",
                new Action[]
                {
                    Action.CanSeeProduct,
                    Action.CanSeeCompany,
                    Action.CanUpdateProduct,
                    Action.CanCreateProduct
                }
            }
        };

        public enum Action { CanSeeDelivey, CanSeeProduct, CanSeeEmployee, CanSeeCompany, CanUpdateDelivery, CanCreateDelivery, CanDeleteDelivery, CanUpdateProduct, CanCreateProduct, CanDeleteProduct, CanUpdateEmployee, CanCreateEmployee, CanDeleteEmployee, CanUpdateCompany, CanCreateCompany, CanDeleteCompany }

        public bool HasAbility(Action action)
        {
            return CheckStatus(Stat, (int)action);
        }

        public static int CreateFrom(string Position)
        {
            if (int.TryParse(Position, out int Result))
            {
                return Result;
            }
            else
            {
                if (Defaults.ContainsKey(Position))
                {
                    return CreateStat(Defaults[Position]);
                }
                return CreateStat(Defaults["Grunt"]);
            }
        }

        private static int CreateStat(params Action[] actions)
        {
            int StatOut = 0;
            foreach (var item in actions)
            {
                StatOut = UpdateValue(StatOut, 1, (int)item);
            }
            return StatOut;
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

        private static Dictionary<string, (uint Max, uint Min, Color GroupColor, List<(GroupBox, Action, CollectionReturn Attribute, int Live)> Collection)> selectionGroup = new Dictionary<string, (uint, uint, Color, List<(GroupBox, Action, CollectionReturn, int)>)>();

        public static void SetSelectionGroup(string Name, (uint MinItems, uint MaxItems) Bounds, Color colorGroup)
        {
            if (string.IsNullOrEmpty(Name))
                return;
            if (!selectionGroup.ContainsKey(Name))
            {
                selectionGroup.Add(Name, (Bounds.MaxItems, Bounds.MinItems, colorGroup, new List<(GroupBox, Action, CollectionReturn, int)>()));
            }
            else
            {
                selectionGroup[Name] = (Bounds.MaxItems, Bounds.MinItems, colorGroup, new List<(GroupBox, Action, CollectionReturn, int)>());
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
                while (item.Value.Collection.Count > item.Value.Max && item.Value.Collection.Count > item.Value.Min)
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
                    if (selectionGroup[Group].Collection.Count - 1 >= selectionGroup[Group].Min)
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
        }

        private static bool InCollection(string Group, GroupBox Box, bool Wake = false)
        {
            if (!string.IsNullOrEmpty(Group))
            {
                if (selectionGroup.ContainsKey(Group))
                {
                    bool Add = false;
                    for (int i = 0; i < selectionGroup[Group].Collection.Count; i++)
                    {
                        if (selectionGroup[Group].Collection[i].Item1 == Box)
                        {
                            if (selectionGroup[Group].Collection[i].Live == 0 && Wake)
                            {
                                selectionGroup[Group].Collection[i] = (selectionGroup[Group].Collection[i].Item1, selectionGroup[Group].Collection[i].Item2, selectionGroup[Group].Collection[i].Attribute, 1);
                            }
                            else
                            {
                                Add = true;
                            }
                            break;
                        }
                    }
                    return Add;
                }
            }
            return false;
        }

        //"Iterative with two matrix rows"
        public static int LevenshteinDistance(string source, string target)
        {
            // degenerate cases
            if (source == target) return 0;
            if (source.Length == 0) return target.Length;
            if (target.Length == 0) return source.Length;

            // create two work vectors of integer distances
            int[] v0 = new int[target.Length + 1];
            int[] v1 = new int[target.Length + 1];

            // initialize v0 (the previous row of distances)
            // this row is A[0][i]: edit distance for an empty s
            // the distance is just the number of characters to delete from t
            for (int i = 0; i < v0.Length; i++)
                v0[i] = i;

            for (int i = 0; i < source.Length; i++)
            {
                // calculate v1 (current row distances) from the previous row v0

                // first element of v1 is A[i+1][0]
                //   edit distance is delete (i+1) chars from s to match empty t
                v1[0] = i + 1;

                // use formula to fill in the rest of the row
                for (int j = 0; j < target.Length; j++)
                {
                    var cost = (source[i] == target[j]) ? 0 : 1;
                    v1[j + 1] = Math.Min(v1[j] + 1, Math.Min(v0[j + 1] + 1, v0[j] + cost));
                }

                // copy v1 (current row) to v0 (previous row) for next iteration
                for (int j = 0; j < v0.Length; j++)
                    v0[j] = v1[j];
            }

            return v1[target.Length];
        }

        public static double CalculateSimilarity(string source, string target)
        {
            if ((source == null) || (target == null)) return 0.0;
            if ((source.Length == 0) || (target.Length == 0)) return 0.0;
            if (source == target) return 1.0;

            int stepsToSame = LevenshteinDistance(source, target);
            return (1.0 - ((double)stepsToSame / (double)Math.Max(source.Length, target.Length)));
        }

        private enum Direction { vertical, horizontal }

        private static void AddNewConentItem(FlowLayoutPanel list, string GroupName, int Size = 70, int Offset = 20, Direction Flow = Direction.vertical, EventHandler OnClick = null, string SelectionGroup = null, CollectionReturn Return = null, Action UnClick = null)
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
                            selectionGroup[SelectionGroup].Collection.Add((Box, () => { Item.BackColor = Color.White; if (UnClick != null) { UnClick(); } }, Return, 0));
                            UpdateSelectionGroups();
                        }
                    };
                }
            }

            if (OnClick != null)
                Box.OnItemClick += OnClick;

            if (!string.IsNullOrEmpty(SelectionGroup))
            {
                if (selectionGroup.ContainsKey(SelectionGroup))
                {
                    Box.OnItemClick += (object? sender, EventArgs e) =>
                    {
                        Item.BackColor = selectionGroup[SelectionGroup].GroupColor;
                        if (InCollection(SelectionGroup, Box, true))
                        {
                            RemoveFromCollection(SelectionGroup, Box);
                        }
                    };
                }
            }

            list.Controls.Add(Item);
        }

        private static void SelectItem(FlowLayoutPanel list, int Item)
        {
            if (Item >= 0 && Item < list.Controls.Count)
            {
                (list.Controls[Item].Controls[0] as ItemBtn)?.ActivateClickItem();
            }
        }

        public void SortItems(FlowLayoutPanel list, string Text)
        {
            List<(double, Control)> Sorted = new List<(double, Control)>();
            for (int i = 0; i < list.Controls.Count; i++)
            {
                string Title = list.Controls[i].Controls[0].Text.Substring(list.Controls[i].Controls[0].Text.IndexOf(':') + 1).Trim();
                double Percent = CalculateSimilarity(Title, Text);
                Sorted.Add((Percent, list.Controls[i]));
            }
            Sorted.Sort((x, y) => { return (x.Item1 == y.Item1 ? 0 : MathF.Sign((float)(y.Item1 - x.Item1))); });
            list.Controls.Clear();
            foreach (var item in Sorted)
            {
                list.Controls.Add(item.Item2);
            }
        }

        private static bool OpenLogin(MainForm Form, StatusType Status, Dictionary<string, string> DataIn)
        {
            DeleteAllConents(Form.GetList(MainForm.List.UIList));
            return true;
        }

        private static bool OpenDelivery(MainForm Form, StatusType Status, Dictionary<string, string> DataIn)
        {
            if (DataIn == null)
                return true;
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

                SetSelectionGroup("OptionSelect", (1, 1), Color.Green);
                if (Status.HasAbility(StatusType.Action.CanSeeDelivey))
                {
                    AddNewConentItem(Form.GetList(MainForm.List.ListDisplay), "Delivery", Flow: Direction.horizontal, Size: 100,
                            OnClick: (object? sender, EventArgs e) =>
                            {
                                SetSelectionGroup("ItemSelect", (0, 1), Color.Green);
                                DeleteAllConents(Form.GetList(MainForm.List.OrderList));
                                List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[DELIVERIES]", ("", null), "Bundle_ID", "Order_ID");
                                foreach (var item in ListItems)
                                {
                                    AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Order_ID: " + item["Order_ID"].ToString(), Flow: Direction.vertical, SelectionGroup: "ItemSelect", 
                                        OnClick: (object? sender, EventArgs e) =>
                                        {
                                            DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                            if (Status.HasAbility(StatusType.Action.CanUpdateDelivery))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Update " + item["Order_ID"].ToString(), 200, 0, Direction.horizontal,
                                                (object? sender, EventArgs Event) =>
                                                {
                                                    Form.SetWindow(MainForm.WindowType.Ordering, null);
                                                });
                                            }
                                            if (Status.HasAbility(StatusType.Action.CanDeleteDelivery))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Delete", 100, 0, Direction.horizontal);
                                            }
                                        }, UnClick: () => 
                                        {
                                            DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                            if (Status.HasAbility(StatusType.Action.CanCreateDelivery))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Create New", 100, 0, Direction.horizontal,
                                                (object? sender, EventArgs Event) => 
                                                {
                                                    Form.SetWindow(MainForm.WindowType.Ordering, null);
                                                });
                                            }
                                        });
                                }

                                SelectItem(Form.GetList(MainForm.List.OrderList), 0);
                            },
                            SelectionGroup: "OptionSelect");
                }

                if (Status.HasAbility(StatusType.Action.CanSeeProduct))
                {
                    AddNewConentItem(Form.GetList(MainForm.List.ListDisplay), "Product", Flow: Direction.horizontal, Size: 100,
                            OnClick: (object? sender, EventArgs e) =>
                            {
                                SetSelectionGroup("ItemSelect", (0, 1), Color.Green);
                                DeleteAllConents(Form.GetList(MainForm.List.OrderList));
                                List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("", null), "Name");
                                foreach (var item in ListItems)
                                {
                                    AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Product: " + item["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "ItemSelect",
                                        OnClick: (object? sender, EventArgs e) =>
                                        {
                                            DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                            if (Status.HasAbility(StatusType.Action.CanUpdateProduct))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Update " + item["Name"].ToString(), 200, 0, Direction.horizontal);
                                            }
                                            if (Status.HasAbility(StatusType.Action.CanDeleteProduct))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Delete", 100, 0, Direction.horizontal);
                                            }
                                        }, UnClick: () =>
                                        {
                                            DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                            if (Status.HasAbility(StatusType.Action.CanCreateProduct))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Create New", 100, 0, Direction.horizontal);
                                            }
                                        });
                                }

                                SelectItem(Form.GetList(MainForm.List.OrderList), 0);
                            },
                            SelectionGroup: "OptionSelect");
                }

                if (Status.HasAbility(StatusType.Action.CanSeeEmployee))
                {
                    AddNewConentItem(Form.GetList(MainForm.List.ListDisplay), "Employee", Flow: Direction.horizontal, Size: 100,
                            OnClick: (object? sender, EventArgs e) =>
                            {
                                SetSelectionGroup("ItemSelect", (0, 1), Color.Green);
                                DeleteAllConents(Form.GetList(MainForm.List.OrderList));
                                List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("", null), "Name");
                                foreach (var item in ListItems)
                                {
                                    AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Employee: " + item["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "ItemSelect",
                                        OnClick: (object? sender, EventArgs e) =>
                                        {
                                            DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                            if (Status.HasAbility(StatusType.Action.CanUpdateEmployee))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Update " + item["Name"].ToString(), 200, 0, Direction.horizontal);
                                            }
                                            if (Status.HasAbility(StatusType.Action.CanDeleteEmployee))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Delete", 100, 0, Direction.horizontal);
                                            }
                                        }, UnClick: () =>
                                        {
                                            DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                            if (Status.HasAbility(StatusType.Action.CanCreateEmployee))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Create New", 100, 0, Direction.horizontal);
                                            }
                                        });
                                }

                                SelectItem(Form.GetList(MainForm.List.OrderList), 0);
                            },
                            SelectionGroup: "OptionSelect");
                }

                if (Status.HasAbility(StatusType.Action.CanSeeCompany))
                {
                    AddNewConentItem(Form.GetList(MainForm.List.ListDisplay), "Company", Flow: Direction.horizontal, Size: 100,
                            OnClick: (object? sender, EventArgs e) =>
                            {
                                SetSelectionGroup("ItemSelect", (0, 1), Color.Green);
                                DeleteAllConents(Form.GetList(MainForm.List.OrderList));
                                List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("", null), "Name");
                                foreach (var item in ListItems)
                                {
                                    AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Company: " + item["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "ItemSelect",
                                        OnClick: (object? sender, EventArgs e) =>
                                        {
                                            DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                            if (Status.HasAbility(StatusType.Action.CanUpdateDelivery))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Update " + item["Name"].ToString(), 200, 0, Direction.horizontal);
                                            }
                                            if (Status.HasAbility(StatusType.Action.CanDeleteDelivery))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Delete", 100, 0, Direction.horizontal);
                                            }
                                        }, UnClick: () =>
                                        {
                                            DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                            if (Status.HasAbility(StatusType.Action.CanCreateDelivery))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Create New", 100, 0, Direction.horizontal);
                                            }
                                        });
                                }

                                SelectItem(Form.GetList(MainForm.List.OrderList), 0);
                            },
                            SelectionGroup: "OptionSelect");
                }
                SelectItem(Form.GetList(MainForm.List.ListDisplay), 0);
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

            SetSelectionGroup("CompanySelect", (1,1), Color.Orchid);
            List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("", null), "Name");
            foreach (var item in ListItems)
            {
                AddNewConentItem(Form.GetList(MainForm.List.OrderDiplay_Company), "Company: " + item["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "CompanySelect");
            }

            SetSelectionGroup("OrderProductSelect", (1,1000), Color.Green);
            ListItems = Form.Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("", null), "Name");
            foreach (var item in ListItems)
            {
                AddNewConentItem(Form.GetList(MainForm.List.OrderDisplay_Product), "Product: " + item["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "OrderProductSelect");
            }

            return true;
        }
    }
}