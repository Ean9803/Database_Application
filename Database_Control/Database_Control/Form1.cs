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
            SetWindow(WindowType.Delivery, new Dictionary<string, string>());
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

        
        public Dictionary<string, List<object>> GetData(string tableName, List<string> Cols)
        {
            string sqlDelete = $"SELECT {string.Join(", ", Cols)} FROM {tableName}";
            Dictionary<string, List<object>> Ret = new Dictionary<string, List<object>>();
            using (SqlCommand cmd = new SqlCommand(sqlDelete, com))
            {
                SqlDataReader Read = cmd.ExecuteReader();

                while (Read.Read())
                {
                    for (int i = 0; i < Cols.Count; i++)
                    {
                        if (!Ret.ContainsKey(Cols[i]))
                        {
                            Ret.Add(Cols[i], new List<object>());
                        }
                        Ret[Cols[i]].Add(Read.GetValue(i));
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
            if (OnClick != null)
                Box.Click += OnClick;

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

            list.Controls.Add(Item);
        }

        private static bool OpenLogin(MainForm Form, StatusType Status, Dictionary<string, string> DataIn)
        {
            return true;
        }

        private static bool OpenDelivery(MainForm Form, StatusType Status, Dictionary<string, string> DataIn)
        {
            DeleteAllConents(Form.GetList(MainForm.List.OrderList));
            SetSelectionGroup("Test", 4, Color.Gray);
            for (int i = 0; i < 10; i++)
            {
                AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Test: " + i, OnClick: (object? sender, EventArgs e) => 
                { 
                    //Form.SetWindow(MainForm.WindowType.Login, new Dictionary<string, string>()); 
                }, SelectionGroup: "Test");
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
            return true;
        }
    }
}