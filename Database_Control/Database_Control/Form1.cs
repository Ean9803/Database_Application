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

        public enum PanelDetail { None, Delivery, Product, Company, Employee }
        public void SetDetailPanel(PanelDetail Detail)
        {
            DetailTabs.SelectTab((int)Detail);
        }

        private bool AutoLogin = true;

        private void LoginBtn_Click(object sender, EventArgs e)
        {
            List<Dictionary<string, object>> User = Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Username=@User AND Password=@Pass", new (string, string)[] { ("@User", AutoLogin ? "Admin" : UserName.Text), ("@Pass", AutoLogin ? "Admin" : PassWord.Text) }), "Salesman_ID", "Name", "Position");
            if (User.Count == 0)
            {
                MessageBox.Show("Login Invalid");
            }
            else
            {
                StatusType Stat = new StatusType(StatusType.CreateFrom((string)User[0]["Position"]), (int)User[0]["Salesman_ID"]);
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

                        User = Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Username=@User AND Password=@Pass", new (string, string)[] { ("@User", UserName.Text), ("@Pass", PassWord.Text) }), "Salesman_ID", "Name", "Position");

                        StatusType Stat = new StatusType(StatusType.CreateFrom((string)User[0]["Position"]), (int)User[0]["Salesman_ID"]);
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

        private void CancelOrder_Click(object sender, EventArgs e)
        {
            SetWindow(WindowType.Delivery, null);
        }

        public void FillDeliveryDisplay(Dictionary<string, object> ListItems)
        {
            DeliveryInfo.Clear();
            DeliveryInfo.AppendText("Order ID: " + ListItems["Order_ID"].ToString() + "\n--------------------------------------\nOrder Status: " + ListItems["Status"].ToString() + "\n--------------------------------------\n");
            List<Dictionary<string, object>> Employee = Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Salesman_ID=@ID", new (string, string)[] { ("@ID", ListItems["Salesman_ID"].ToString()) }), "Name", "Username", "Position");
            if (Employee.Count > 0)
                DeliveryInfo.AppendText("Order Created by: " + Employee[0]["Name"].ToString() + " | Current Position: " + Employee[0]["Position"].ToString() + "\n");
            else
                DeliveryInfo.AppendText("Order Created by: [EMPLOYEE NOT FOUND]\n");
            DeliveryInfo.AppendText("[ORDER HISTORY]:\n");
            DeliveryInfo.AppendText(ListItems["History"].ToString());
        }

        public void FillProductDisplay(Dictionary<string, object> ListItems)
        {

        }

        public void FillCompanyDisplay(Dictionary<string, object> ListItems)
        {

        }

        public void FillEmployeeDisplay(Dictionary<string, object> ListItems)
        {

        }
    }
}