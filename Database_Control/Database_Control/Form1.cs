using System.Data.SqlClient;
using System.Data;


namespace Database_Control
{
    public partial class MainForm : Form
    {
        private SQL Connection;
        public MainForm()
        {
            Connection = new SQL("Company", "root", "root");
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
                com = new SqlConnection(@"server=.\SQLEXPRESS;Database=" + Database + ";User ID=" + Username + ";Password=" + Password + ";TrustServerCertificate=True");
                com.Open();
                MessageBox.Show("Connection Open!");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error opening Connection! " + ex.Message);
            }
        }

        public void Close()
        {
            com.Close();
        }

        public bool InsertData(string tableName, Dictionary<string, string> data)
        {
            try
            {
                
                if (com.State == ConnectionState.Closed)
                {
                    com.Open();
                }

                
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = com;

                    
                    string columns = string.Join(", ", data.Keys);
                    string values = string.Join(", ", data.Values);

                    string query = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

                    cmd.CommandText = query;

                    
                    cmd.ExecuteNonQuery();
                }

                return true; 
            }
            catch (Exception ex)
            {
                
                return false;
            }
        }

        public bool UpdateData(string tableName, Dictionary<string, string> data, string whereClause)
        {
            try
            {
                
                if (com.State == ConnectionState.Closed)
                {
                    com.Open();
                }

                
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = com;

                    
                    string setValues = string.Join(", ", data.Select(kvp => $"{kvp.Key} = '{kvp.Value}'"));
                    string query = $"UPDATE {tableName} SET {setValues} WHERE {whereClause}";

                    cmd.CommandText = query;

                    
                    cmd.ExecuteNonQuery();
                }

                return true; 
            }
            catch (Exception ex)
            {
                
                return false;
            }
        }

        public bool DeleteData(string tableName, string whereClause)
        {
            try
            {
                
                if (com.State == ConnectionState.Closed)
                {
                    com.Open();
                }

                
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = com;

                   
                    string query = $"DELETE FROM {tableName} WHERE {whereClause}";

                    cmd.CommandText = query;

                    
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                
                return false;
            }
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

        private static void AddNewConentItem(FlowLayoutPanel list, string GroupName, int Height = 70, EventHandler OnClick = null)
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
            Box.MouseEnter += (object? sender, EventArgs e) => { Item.BackColor = Color.Tan; };
            Box.MouseLeave += (object? sender, EventArgs e) => { Item.BackColor = Color.White; };
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
            for (int i = 0; i < 10; i++)
            {
                AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Test: " + i, OnClick: (object? sender, EventArgs e) => { Form.SetWindow(MainForm.WindowType.Login, new Dictionary<string, string>()); });
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