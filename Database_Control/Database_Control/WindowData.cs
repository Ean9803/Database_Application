using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Control
{
    public class WindowData
    {
        private StatusType Status;
        private MainForm Form;
        public WindowData(MainForm Form, StatusType Status) { this.Form = Form; this.Status = Status; }

        private delegate bool NewWindowState(MainForm Form, StatusType Status, Dictionary<string, object> DataIn);
        public delegate Dictionary<string, object> CollectionReturn();
        public delegate List<(bool, Control)> CreateButonConent();

        private Dictionary<MainForm.WindowProfile, NewWindowState> Windows = new Dictionary<MainForm.WindowProfile, NewWindowState>()
        {
            { MainForm.WindowType.Login, OpenLogin },
            { MainForm.WindowType.Delivery, OpenDelivery },
            { MainForm.WindowType.Product, OpenProduct },
            { MainForm.WindowType.Company, OpenCompany },
            { MainForm.WindowType.Employee, OpenEmployee },
            { MainForm.WindowType.Ordering, OpenOrdering },
        };

        public bool OpenWindow(MainForm.WindowProfile Type, Dictionary<string, object> DataIn)
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

        private static Dictionary<string, (uint Max, uint Min, Color GroupColor, List<(Panel, Action, CollectionReturn Attribute, int Live)> Collection)> selectionGroup = new Dictionary<string, (uint, uint, Color, List<(Panel, Action, CollectionReturn, int)>)>();

        public static void SetSelectionGroup(string Name, (uint MinItems, uint MaxItems) Bounds, Color colorGroup)
        {
            if (string.IsNullOrEmpty(Name))
                return;
            if (!selectionGroup.ContainsKey(Name))
            {
                selectionGroup.Add(Name, (Bounds.MaxItems, Bounds.MinItems, colorGroup, new List<(Panel, Action, CollectionReturn, int)>()));
            }
            else
            {
                selectionGroup[Name] = (Bounds.MaxItems, Bounds.MinItems, colorGroup, new List<(Panel, Action, CollectionReturn, int)>());
            }
        }

        private static Dictionary<int, string> StatusMap = new Dictionary<int, string>() { { 1, "Init" }, { 2, "In Progress" }, { 3, "Completed" } };
        public static string GetOrderStatus(int Stat)
        {
            if (StatusMap.ContainsKey(Stat))
                return StatusMap[Stat];
            return "UNKNOWN";
        }

        public static List<(Control, CollectionReturn)> GetSelectedObjects(string Group)
        {
            if (string.IsNullOrEmpty(Group))
                return new List<(Control, CollectionReturn)>();
            List<(Control, CollectionReturn)> Ret = new List<(Control, CollectionReturn)>();
            if (selectionGroup.ContainsKey(Group))
            {
                for (int i = selectionGroup[Group].Collection.Count - 1; i >= 0; i--)
                {
                    if (selectionGroup[Group].Collection[i].Attribute != null)
                    {
                        Ret.Add((selectionGroup[Group].Collection[i].Item1, selectionGroup[Group].Collection[i].Attribute));
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

        private static void RemoveFromCollection(string Group, Panel Box)
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

        private static bool InCollection(string Group, Panel Box, bool Wake = false)
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

        private static void AddNewConentItem(FlowLayoutPanel list, string GroupName, int Size = 70, int Offset = 20, Direction Flow = Direction.vertical, EventHandler OnClick = null, string SelectionGroup = null, CollectionReturn Return = null, Action UnClick = null, CreateButonConent Content = null)
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

            EventHandler MouseEnterMeth = (object? sender, EventArgs e) => { if (!InCollection(SelectionGroup, Item)) Item.BackColor = Color.Tan; };
            EventHandler MouseLeaveMeth = (object? sender, EventArgs e) => { if (!InCollection(SelectionGroup, Item)) Item.BackColor = Color.White; };

            EventHandler EnterCollection = null;
            EventHandler ExitCollection = null;

            if (!string.IsNullOrEmpty(SelectionGroup))
            {
                if (selectionGroup.ContainsKey(SelectionGroup))
                {
                    EnterCollection += (object? sender, EventArgs e) =>
                    {
                        Item.BackColor = selectionGroup[SelectionGroup].GroupColor;
                        if (!InCollection(SelectionGroup, Item))
                        {
                            selectionGroup[SelectionGroup].Collection.Add((Item, () => { Item.BackColor = Color.White; if (UnClick != null) { UnClick(); } }, Return, 0));
                            UpdateSelectionGroups();
                        }
                    };
                    ExitCollection += (object? sender, EventArgs e) =>
                    {
                        Item.BackColor = selectionGroup[SelectionGroup].GroupColor;
                        if (InCollection(SelectionGroup, Item, true))
                        {
                            RemoveFromCollection(SelectionGroup, Item);
                        }
                    };
                }
            }

            Box.MouseEnter += MouseEnterMeth;
            Box.MouseLeave += MouseLeaveMeth;

            if (EnterCollection != null)
                Box.OnItemClick += EnterCollection;
            if (OnClick != null)
                Box.OnItemClick += OnClick;
            if (ExitCollection != null)
                Box.OnItemClick += ExitCollection;

            list.Controls.Add(Item);

            if (Content != null)
            {
                List<(bool, Control)> In = Content();
                foreach (var item in In)
                {
                    if (item.Item1)
                    {
                        item.Item2.MouseEnter += MouseEnterMeth;
                        item.Item2.MouseLeave += MouseLeaveMeth;

                        if (EnterCollection != null)
                            item.Item2.Click += EnterCollection;
                        if (OnClick != null)
                            item.Item2.Click += OnClick;
                        if (ExitCollection != null)
                            item.Item2.Click += ExitCollection;
                    }
                    Item.Controls.Add(item.Item2);
                    item.Item2.BringToFront();
                }
            }
        }

        private static void SelectItem(FlowLayoutPanel list, int Item)
        {
            if (Item >= 0 && Item < list.Controls.Count)
            {
                (list.Controls[Item].Controls[list.Controls[Item].Controls.Count - 1] as ItemBtn)?.ActivateClickItem();
            }
        }

        private static void SelectItem(FlowLayoutPanel list, string Item)
        {
            List<(double, int)> Sorted = new List<(double, int)>();
            for (int i = 0; i < list.Controls.Count; i++)
            {
                string Title = list.Controls[i].Controls[0].Text.Substring(list.Controls[i].Controls[0].Text.IndexOf(':') + 1).Trim();
                double Percent = CalculateSimilarity(Title, Item);
                Sorted.Add((Percent, i));
            }
            Sorted.Sort((x, y) => { return (x.Item1 == y.Item1 ? 0 : MathF.Sign((float)(y.Item1 - x.Item1))); });
            if (Sorted.Count > 0)
            {
                SelectItem(list, Sorted[0].Item2);
            }
        }

        public static void SortItems(FlowLayoutPanel list, string Text)
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

        private static bool OpenLogin(MainForm Form, StatusType Status, Dictionary<string, object> DataIn)
        {
            DeleteAllConents(Form.GetList(MainForm.List.UIList));
            return true;
        }

        public static void CreateDeleteDialog(string TitleText, InputField.InputEnd EndAction)
        {
            Form DeleteForm = new Form();
            DeleteForm.Size = new Size(300, 200);
            DeleteForm.StartPosition = FormStartPosition.CenterParent;
            DeleteForm.FormBorderStyle = FormBorderStyle.FixedToolWindow;

            Label Title = new Label();
            Title.Text = TitleText;
            Title.Location = new Point(0, 0);
            Title.Size = new Size(300, 200);
            Title.TextAlign = ContentAlignment.TopCenter;
            Title.ForeColor = Color.Black;

            InputField text = new InputField();
            text.Size = new Size(300, 20);
            text.Location = new Point(0, 50);
            text.PasswordChar = '*';

            text.OnEnterKey = EndAction;
            text.OnEnd = () => { DeleteForm.Close(); };

            DeleteForm.Controls.Add(text);
            DeleteForm.Controls.Add(Title);

            DeleteForm.ShowDialog();
        }

        public static void UpdateUserHistory(MainForm Form, int ID, string Log)
        {
            List<Dictionary<string, object>> ProductInfo = Form.Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("Salesman_ID=@ID", new (string, string)[] { ("@ID", ID.ToString()) }), "History");
            if (ProductInfo.Count > 0)
            {
                Form.Connection.UpdateData("[Maestro].[dbo].[EMPLOYEE]", ("Salesman_ID=@ID", new (string, string)[] { ("@ID", ID.ToString()) }), ("History", "[" + DateTime.UtcNow.Date.ToString("dd / MM / yyyy") + "]: " + Log + "\n" + ProductInfo[0]["History"]));
            }
        }

        private static void DeleteDelivery(MainForm Form, StatusType Status, Dictionary<string, object> item)
        {
            List<Dictionary<string, object>> OrderBundles = Form.Connection.GetData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID", new (string, string)[] { ("@ID", item["Bundle_ID"].ToString()) }), "Product_ID", "Quantity", "Delivered");
            List<Dictionary<string, object>> ProductInfo;
            List<string> RestoredProducts = new List<string>();
            List<string> NonRestoredProducts = new List<string>();
            foreach (var itemProduct in OrderBundles)
            {
                ProductInfo = Form.Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("Product_ID=@ID", new (string, string)[] { ("@ID", itemProduct["Product_ID"].ToString()) }), "Available_Amt", "Name");
                if ((int)itemProduct["Delivered"] == 0)
                {
                    Form.Connection.UpdateData("[Maestro].[dbo].[PRODUCTS]", ("Product_ID=@ID", new (string, string)[] { ("@ID", itemProduct["Product_ID"].ToString()) }),
                        ("Available_Amt", (((int)ProductInfo[0]["Available_Amt"]) + ((int)itemProduct["Quantity"])).ToString()));
                    RestoredProducts.Add(ProductInfo[0]["Name"].ToString() + "(" + ProductInfo[0]["Available_Amt"].ToString() + ")");
                }
                else
                {
                    NonRestoredProducts.Add(ProductInfo[0]["Name"].ToString() + "(" + ProductInfo[0]["Available_Amt"].ToString() + ")");
                }
            }
            ProductInfo = Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("Company_ID=@ID", new (string, string)[] { ("@ID", item["Company_ID"].ToString()) }), "Name");
            Form.Connection.DeleteData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID", new (string, string)[] { ("@ID", item["Bundle_ID"].ToString()) }));
            Form.Connection.DeleteData("[Maestro].[dbo].[DELIVERIES]", ("Order_ID=@ID", new (string, string)[] { ("@ID", item["Order_ID"].ToString()) }));
            UpdateUserHistory(Form, Status.GetIDNumber(), "User deleted order: " + item["Order_ID"].ToString() +
                "\n\t|Restored: [" + String.Join(", ", RestoredProducts) + "]" +
                "\n\t|Not Restored: [" + String.Join(", ", NonRestoredProducts) + "]" +
                "\n\t|To be delivered to: " + ProductInfo[0]["Name"].ToString() +
                "\n\t|Created on: " + item["CreationDate"].ToString());
        }

        private static void DeleteProduct(MainForm Form, StatusType Status, Dictionary<string, object> item)
        {
            List<Dictionary<string, object>> Included = Form.Connection.GetData("[Maestro].[dbo].[BUNDLES]", ("Product_ID=@ID", new (string, string)[] { ("@ID", item["Product_ID"].ToString()) }), "Bundle_ID");
            List<string> DeletedItemsProduct = new List<string>();
            List<string> DeletedItemsOrder = new List<string>();
            foreach (var Bundle in Included)
            {
                string OrderID = Form.Connection.GetData("[Maestro].[dbo].[DELIVERIES]", ("Bundle_ID=@ID", new (string, string)[] { ("@ID", Bundle["Bundle_ID"].ToString()) }), "Order_ID")[0]["Order_ID"].ToString();

                if (!DeletedItemsProduct.Contains(item["Name"].ToString()))
                    DeletedItemsProduct.Add(item["Name"].ToString());

                if (!DeletedItemsOrder.Contains(OrderID))
                    DeletedItemsOrder.Add(OrderID);

                List<Dictionary<string, object>> Delivery = Form.Connection.GetData("[Maestro].[dbo].[DELIVERIES]", ("Order_ID=@ID", new (string, string)[] { ("@ID", OrderID) }), "Bundle_ID");
                List<Dictionary<string, object>> Bundles = Form.Connection.GetData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID", new (string, string)[] { ("@ID", Delivery[0]["Bundle_ID"].ToString()) }), "Product_ID");
                int UpForDelete = 0;
                for (int i = 0; i < Bundles.Count; i++)
                {
                    if (Bundles[i]["Product_ID"].ToString().Equals(item["Product_ID"].ToString()))
                    {
                        UpForDelete++;
                    }
                }

                if (UpForDelete == Bundles.Count)
                {
                    Form.Connection.DeleteData("[Maestro].[dbo].[DELIVERIES]", ("Order_ID=@ID", new (string, string)[] { ("@ID", OrderID) }));
                }

                Form.Connection.DeleteData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID AND Product_ID=@PID", new (string, string)[] { ("@ID", Delivery[0]["Bundle_ID"].ToString()), ("@PID", item["Product_ID"].ToString()) }));
            }

            Form.Connection.DeleteData("[Maestro].[dbo].[PRODUCTS]", ("Product_ID=@ID", new (string, string)[] { ("@ID", item["Product_ID"].ToString()) }));

            UpdateUserHistory(Form, Status.GetIDNumber(), "User deleted: " + String.Join(", ", DeletedItemsProduct) + "\n\tAffected Orders:\n\t|[" + String.Join(", ", DeletedItemsOrder) + "]");
        }

        private static void DeleteCompany(MainForm Form, StatusType Status, Dictionary<string, object> item)
        {
            List<Dictionary<string, object>> Deliveries = Form.Connection.GetData("[Maestro].[dbo].[DELIVERIES]", ("Company_ID=@ID", new (string, string)[] { ("@ID", item["Company_ID"].ToString()) }), "Bundle_ID", "Order_ID", "Status", "Company_ID", "Salesman_ID", "History", "Memo", "CreationDate");
            List<Dictionary<string, object>> Products = Form.Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("Supplier=@ID", new (string, string)[] { ("@ID", item["Company_ID"].ToString()) }), "Name", "Product_ID", "Available_Amt", "Description", "Supplier", "Time", "Price", "Image");
            List<string> DeletedItemsOrder = new List<string>();
            List<string> DeletedItemsProduct = new List<string>();

            foreach (var Deliver in Deliveries)
            {
                DeleteDelivery(Form, Status, Deliver);
                DeletedItemsOrder.Add(Deliver["Order_ID"].ToString());
            }
            foreach (var Product in Products)
            {
                DeleteProduct(Form, Status, Product);
                DeletedItemsProduct.Add(Product["Name"].ToString());
            }
            Form.Connection.DeleteData("[Maestro].[dbo].[COMPANIES]", ("Company_ID=@ID", new (string, string)[] { ("@ID", item["Company_ID"].ToString()) }));

            UpdateUserHistory(Form, Status.GetIDNumber(), "User deleted company: " + item["Name"].ToString() +
            "\n\tAffected Orders:\n\t|[" + String.Join(", ", DeletedItemsOrder) + "]" +
            "\n\tAffected Products:\n\t|[" + String.Join(", ", DeletedItemsProduct) + "]");
        }

        private static bool OpenDelivery(MainForm Form, StatusType Status, Dictionary<string, object> DataIn)
        {
            if (DataIn == null)
                return true;
            if (DataIn.ContainsKey("INIT"))
            {
                DeleteAllConents(Form.GetList(MainForm.List.OrderList));
                DeleteAllConents(Form.GetList(MainForm.List.ListDisplay));

                if (DataIn.ContainsKey("NAME"))
                {
                    DeleteAllConents(Form.GetList(MainForm.List.UIList));
                    AddNewConentItem(Form.GetList(MainForm.List.UIList), "Position: " + DataIn["INIT"].ToString(), 200, 0, Direction.horizontal);
                    AddNewConentItem(Form.GetList(MainForm.List.UIList), "Employee: " + DataIn["NAME"].ToString(), 200, 0, Direction.horizontal);
                }

                SetSelectionGroup("OptionSelect", (1, 1), Color.Green);
                if (Status.HasAbility(StatusType.Action.CanSeeDelivey))
                {
                    AddNewConentItem(Form.GetList(MainForm.List.ListDisplay), "Delivery", Flow: Direction.horizontal, Size: 100,
                            OnClick: (object? sender, EventArgs e) =>
                            {
                                SetSelectionGroup("ItemSelect", (0, 1), Color.Green);
                                DeleteAllConents(Form.GetList(MainForm.List.OrderList));
                                List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[DELIVERIES]", ("", null), "Bundle_ID", "Order_ID", "Status", "Company_ID", "Salesman_ID", "History", "Memo", "CreationDate");
                                if (ListItems.Count != 0)
                                {
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
                                                        Form.SetWindow(MainForm.WindowType.Ordering, item);
                                                    });
                                                }
                                                if (Status.HasAbility(StatusType.Action.CanDeleteDelivery))
                                                {
                                                    AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Delete", 100, 0, Direction.horizontal,
                                                    (object? sender, EventArgs Event) =>
                                                    {
                                                        CreateDeleteDialog("Enter Password To Delete Delivery", (string Text) =>
                                                        {
                                                            if (Text.Equals(Status.GetPass()))
                                                            {
                                                                DeleteDelivery(Form, Status, item);
                                                                Form.SetWindow(MainForm.WindowType.Delivery, new Dictionary<string, object>() { { "INIT", "" } });
                                                                MessageBox.Show("Deleted Order");
                                                            }
                                                            else
                                                            {
                                                                MessageBox.Show("Incorrect Password");
                                                            }
                                                        });
                                                    });
                                                }
                                                Form.SetDetailPanel(MainForm.PanelDetail.Delivery);
                                                Form.FillDeliveryDisplay(item);
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
                                                Form.SetDetailPanel(MainForm.PanelDetail.None);
                                            }, Content: () =>
                                            {
                                                List<(bool, Control)> Controls = new List<(bool, Control)>();

                                                Label L = new Label();
                                                List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("Company_ID=@ID", new (string, string)[] { ("@ID", item["Company_ID"].ToString()) }), "Name");
                                                string To = "";
                                                if (ListItems.Count > 0)
                                                {
                                                    To = " | Company: " + ListItems[0]["Name"].ToString();
                                                }
                                                L.Text = "Order Status: " + GetOrderStatus((int)item["Status"]) + To;
                                                L.Location = new Point(5, 20);
                                                L.Size = new Size(350, 30);
                                                L.ForeColor = Color.Black;
                                                Controls.Add((true, L));
                                                return Controls;
                                            });
                                    }
                                }
                                else
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
                                    Form.SetDetailPanel(MainForm.PanelDetail.None);
                                }
                                if (DataIn.ContainsKey("SEARCH"))
                                {
                                    SortItems(Form.GetList(MainForm.List.OrderList), DataIn["SEARCH"].ToString());
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
                                List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("", null), "Name", "Product_ID", "Available_Amt", "Description", "Supplier", "Time", "Price", "Image", "History");
                                if (ListItems.Count != 0)
                                {
                                    foreach (var item in ListItems)
                                    {
                                        AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Product: " + item["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "ItemSelect",
                                            OnClick: (object? sender, EventArgs e) =>
                                            {
                                                DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                                if (Status.HasAbility(StatusType.Action.CanUpdateProduct))
                                                {
                                                    AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Update " + item["Name"].ToString(), 200, 0, Direction.horizontal,
                                                    (object? sender, EventArgs Event) =>
                                                    {
                                                        Form.SetWindow(MainForm.WindowType.Product, item);
                                                    });
                                                }
                                                if (Status.HasAbility(StatusType.Action.CanDeleteProduct))
                                                {
                                                    AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Delete", 100, 0, Direction.horizontal,
                                                    (object? sender, EventArgs e) =>
                                                    {
                                                        CreateDeleteDialog("Enter Password To Delete Links", (string Text) =>
                                                        {
                                                            if (Text.Equals(Status.GetPass()))
                                                            {
                                                                DeleteProduct(Form, Status, item);
                                                                Form.SetWindow(MainForm.WindowType.Delivery, new Dictionary<string, object>() { { "INIT", "" } });
                                                                MessageBox.Show("Deletion of product Complete");
                                                            }
                                                            else
                                                            {
                                                                MessageBox.Show("Incorrect Password");
                                                            }
                                                        });
                                                    });
                                                }
                                                Form.SetDetailPanel(MainForm.PanelDetail.Product);
                                                Form.FillProductDisplay(item);
                                            }, UnClick: () =>
                                            {
                                                DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                                if (Status.HasAbility(StatusType.Action.CanCreateProduct))
                                                {
                                                    AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Create New", 100, 0, Direction.horizontal,
                                                    (object? sender, EventArgs Event) =>
                                                    {
                                                        Form.SetWindow(MainForm.WindowType.Product, null);
                                                    });
                                                }
                                                Form.SetDetailPanel(MainForm.PanelDetail.None);
                                            }, Content: () =>
                                            {
                                                List<(bool, Control)> Controls = new List<(bool, Control)>();

                                                Label L = new Label();
                                                L.Text = "Amt: " + item["Available_Amt"];
                                                L.Location = new Point(5, 20);
                                                L.Size = new Size(350, 30);
                                                L.ForeColor = Color.Black;
                                                Controls.Add((true, L));
                                                return Controls;
                                            });
                                    }
                                }
                                else
                                {
                                    DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                    if (Status.HasAbility(StatusType.Action.CanCreateProduct))
                                    {
                                        AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Create New", 100, 0, Direction.horizontal,
                                        (object? sender, EventArgs Event) =>
                                        {
                                            Form.SetWindow(MainForm.WindowType.Product, null);
                                        });
                                    }
                                    Form.SetDetailPanel(MainForm.PanelDetail.None);
                                }
                                if (DataIn.ContainsKey("SEARCH"))
                                {
                                    SortItems(Form.GetList(MainForm.List.OrderList), DataIn["SEARCH"].ToString());
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
                                List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[EMPLOYEE]", ("", null), "Name", "Username", "Position", "History", "Salesman_ID");
                                if (ListItems.Count != 0)
                                {
                                    foreach (var item in ListItems)
                                    {
                                        if (!item["Salesman_ID"].ToString().Equals(Status.GetIDNumber().ToString()))
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
                                                    Form.SetDetailPanel(MainForm.PanelDetail.Employee);
                                                    Form.FillEmployeeDisplay(item);
                                                }, UnClick: () =>
                                                {
                                                    DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                                    if (Status.HasAbility(StatusType.Action.CanCreateEmployee))
                                                    {
                                                        AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Create New", 100, 0, Direction.horizontal,
                                                        (object? sender, EventArgs Event) =>
                                                        {
                                                            Form.SetWindow(MainForm.WindowType.Employee, null);
                                                        });
                                                    }
                                                    Form.SetDetailPanel(MainForm.PanelDetail.None);
                                                });
                                        }
                                    }
                                }
                                else
                                {
                                    DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                    if (Status.HasAbility(StatusType.Action.CanCreateEmployee))
                                    {
                                        AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Create New", 100, 0, Direction.horizontal,
                                        (object? sender, EventArgs Event) =>
                                        {
                                            Form.SetWindow(MainForm.WindowType.Employee, null);
                                        });
                                    }
                                    Form.SetDetailPanel(MainForm.PanelDetail.None);
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
                                List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("", null), "Name", "Company_ID", "Description", "Phone", "Email", "Image");
                                if (ListItems.Count != 0)
                                {
                                    foreach (var item in ListItems)
                                    {
                                        AddNewConentItem(Form.GetList(MainForm.List.OrderList), "Company: " + item["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "ItemSelect",
                                            OnClick: (object? sender, EventArgs e) =>
                                            {
                                                DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                                if (Status.HasAbility(StatusType.Action.CanUpdateCompany))
                                                {
                                                    AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Update " + item["Name"].ToString(), 200, 0, Direction.horizontal,
                                                    (object? sender, EventArgs Event) =>
                                                    {
                                                        Form.SetWindow(MainForm.WindowType.Company, item);
                                                    });
                                                }
                                                if (Status.HasAbility(StatusType.Action.CanDeleteCompany))
                                                {
                                                    AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Delete", 100, 0, Direction.horizontal,
                                                    (object? sender, EventArgs Event) =>
                                                    {
                                                        CreateDeleteDialog("Enter Password To Delete Company", (string Text) =>
                                                        {
                                                            if (Text.Equals(Status.GetPass()))
                                                            {
                                                                DeleteCompany(Form, Status, item);
                                                                Form.SetWindow(MainForm.WindowType.Delivery, new Dictionary<string, object>() { { "INIT", "" } });
                                                                MessageBox.Show("Deletion of company Complete");
                                                            }
                                                            else
                                                            {
                                                                MessageBox.Show("Incorrect Password");
                                                            }
                                                        });
                                                    });
                                                }
                                                Form.SetDetailPanel(MainForm.PanelDetail.Company);
                                                Form.FillCompanyDisplay(item);
                                            }, UnClick: () =>
                                            {
                                                DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                                if (Status.HasAbility(StatusType.Action.CanCreateCompany))
                                                {
                                                    AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Create New", 100, 0, Direction.horizontal,
                                                    (object? sender, EventArgs Event) =>
                                                    {
                                                        Form.SetWindow(MainForm.WindowType.Company, null);
                                                    });
                                                }
                                                Form.SetDetailPanel(MainForm.PanelDetail.None);
                                            });
                                    }
                                }
                                else
                                {
                                    DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                    if (Status.HasAbility(StatusType.Action.CanCreateCompany))
                                    {
                                        AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Create New", 100, 0, Direction.horizontal,
                                        (object? sender, EventArgs Event) =>
                                        {
                                            Form.SetWindow(MainForm.WindowType.Company, null);
                                        });
                                    }
                                    Form.SetDetailPanel(MainForm.PanelDetail.None);
                                }

                                SelectItem(Form.GetList(MainForm.List.OrderList), 0);
                            },
                            SelectionGroup: "OptionSelect");
                }
                SelectItem(Form.GetList(MainForm.List.ListDisplay), DataIn.ContainsKey("PAGE") ? DataIn["PAGE"].ToString() : "Delivery");
            }

            return true;
        }

        private static bool OpenProduct(MainForm Form, StatusType Status, Dictionary<string, object> DataIn)
        {
            DeleteAllConents(Form.GetList(MainForm.List.ProductItemList));
            DeleteAllConents(Form.GetList(MainForm.List.ProductSupplier));
            DeleteAllConents(Form.GetList(MainForm.List.ProductReferences));
            Form.resetPass();
            Form.SetPropertiesWindow(0);
            Form.SetReferencedNum("-");
            Form.SetPrice("-");
            Form.SetProductName("", false);
            Form.SetPrepTime("0");

            AddNewConentItem(Form.GetList(MainForm.List.ProductItemList), "Set Supplier", Offset: 0, Size: 25, Flow: Direction.vertical, OnClick: (object? sender, EventArgs Args) => { Form.SetPropertiesWindow(1); });
            AddNewConentItem(Form.GetList(MainForm.List.ProductItemList), "Set Properties", Offset: 0, Size: 25, Flow: Direction.vertical, OnClick: (object? sender, EventArgs Args) => { Form.SetPropertiesWindow(2); });
            AddNewConentItem(Form.GetList(MainForm.List.ProductItemList), "Set Picture", Offset: 0, Size: 25, Flow: Direction.vertical, OnClick: (object? sender, EventArgs Args) => { MessageBox.Show("This picks a pic"); });

            
            SetSelectionGroup("SupplierSelect", (1, 1), Color.Orchid);
            List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("", null), "Name", "Company_ID");
            (string, int) SelectComp = ("", -1);
            bool Added = false;
            for (int i = 0; i < ListItems.Count; i++)
            {
                object CompName = ListItems[i]["Name"];
                object CompID = ListItems[i]["Company_ID"];
                if (DataIn != null)
                {
                    if (ListItems[i]["Company_ID"].Equals(DataIn["Supplier"]))
                    {
                        SelectComp = (ListItems[i]["Name"].ToString(), i);
                    }
                }
                AddNewConentItem(Form.GetList(MainForm.List.ProductSupplier), "Company: " + ListItems[i]["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "SupplierSelect", Return: () =>
                {
                    return new Dictionary<string, object>() { { "Name", CompName }, { "Company_ID", CompID } };
                });
            }

            if (SelectComp.Item2 >= 0)
            {
                SelectItem(Form.GetList(MainForm.List.ProductSupplier), SelectComp.Item2);
                SortItems(Form.GetList(MainForm.List.ProductSupplier), SelectComp.Item1);
            }

            int reservedAmount = 0;
            SetSelectionGroup("OrderProductSelect", (0, 1000), Color.Red);
            if (DataIn != null)
            {
                Form.SetProductDesc(DataIn["Description"].ToString(), false);
                int TotalAmount = 0;
                List<Dictionary<string, object>> ProductBundle = Form.Connection.GetData("[Maestro].[dbo].[BUNDLES]", ("Product_ID=@ID", new (string, string)[] { ("@ID", DataIn["Product_ID"].ToString()) }), "Quantity", "Bundle_ID", "Delivered");
                List<Dictionary<string, object>> OrderBundle;
                List<Dictionary<string, object>> CompanyBundle;
                foreach (var item in ProductBundle)
                {
                    reservedAmount += int.Parse(item["Quantity"].ToString());
                    OrderBundle = Form.Connection.GetData("[Maestro].[dbo].[DELIVERIES]", ("Bundle_ID=@ID", new (string, string)[] { ("@ID", item["Bundle_ID"].ToString()) }), "Order_ID", "Company_ID", "Status");
                    CompanyBundle = Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("Company_ID=@ID", new (string, string)[] { ("@ID", OrderBundle[0]["Company_ID"].ToString()) }), "Name");

                    AddNewConentItem(Form.GetList(MainForm.List.ProductReferences), "Order ID: " + OrderBundle[0]["Order_ID"].ToString(), Size: 150, Offset: 0, Flow: Direction.vertical, Content: () =>
                    {
                        List<(bool, Control)> Items = new List<(bool, Control)>();

                        Label L = new Label();
                        L.Text = "Company to: " + CompanyBundle[0]["Name"] + "\n|Amt: " + item["Quantity"].ToString() + "\n|Status: " + GetOrderStatus((int)OrderBundle[0]["Status"]) + "\n|Delivered: " + ((int)item["Delivered"] == 0 ? "[NO]" : "[YES]");
                        L.Location = new Point(5, 20);
                        L.Size = new Size(150, 120);
                        L.ForeColor = Color.Black;
                        L.BackColor = Color.Transparent;
                        Items.Add((true, L));

                        return Items;
                    }, SelectionGroup: "OrderProductSelect", Return: () =>
                    {
                        return new Dictionary<string, object>() { { "Order_ID", OrderBundle[0]["Order_ID"] }, { "Product_ID", DataIn["Product_ID"] }, { "ProductName", DataIn["Name"] }, { "INFO", DataIn } };
                    });
                }
                Form.SetReferencedNum(reservedAmount.ToString());
                Form.SetPrice(DataIn["Price"].ToString());
                Form.SetProductName(DataIn["Name"].ToString(), false);
                Form.SetPrepTime(DataIn["Time"].ToString());
                ProductBundle = Form.Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("Product_ID=@ID", new (string, string)[] { ("@ID", DataIn["Product_ID"].ToString()) }), "Available_Amt");
                TotalAmount = reservedAmount + int.Parse(ProductBundle[0]["Available_Amt"].ToString());

                Form.SetTotalAmount(TotalAmount);
            }
            else
            {
                Form.SetProductDesc("[Enter Product Description]", false);
                Form.SetTotalAmount(0);
            }

            Action SaveAction;

            if (DataIn != null)
            {
                SaveAction = () =>
                {
                    if (!string.IsNullOrEmpty(Form.GetProductName()))
                    {
                        if (Form.GetProductPrice() >= 0)
                        {
                            if (Form.GetTotalAmount() - reservedAmount > 0)
                            {
                                List<(Control, CollectionReturn)> Company = GetSelectedObjects("SupplierSelect");
                                if (Company.Count > 0)
                                {
                                    Dictionary<string, object> Comp = Company[0].Item2();
                                    string CompName = Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("Company_ID=@ID", new (string, string)[] { ("@ID", Comp["Company_ID"].ToString()) }), "Name")[0]["Name"].ToString();
                                    string Profile = "\n\t|Price: " + Form.GetProductPrice() +
                                                     "\n\t|Time: " + Form.GetPrepTime() +
                                                     "\n\t|Available: " + ((int)(Form.GetTotalAmount() - reservedAmount)).ToString() +
                                                     "\n\t|Supplier: " + CompName;
                                    UpdateUserHistory(Form, Status.GetIDNumber(), "User updated product: " + Form.GetProductName() + Profile);
                                    string HistData = "[" + DateTime.UtcNow.Date.ToString("dd/MM/yyyy") + "] Modified by: " + Status.GetName() + ", position: " + Status.GetPosition();

                                    Form.Connection.UpdateData("[Maestro].[dbo].[PRODUCTS]", ("Product_ID=@ID", new (string, string)[] { ("@ID", DataIn["Product_ID"].ToString()) }), ("Name", Form.GetProductName()),
                                        ("Description", Form.GetProductDesc()), ("Price", Form.GetProductPrice()), ("Time", Form.GetPrepTime()),
                                        ("Available_Amt", (int)(Form.GetTotalAmount() - reservedAmount)), ("Image", ""), ("Supplier", Comp["Company_ID"]), ("History", HistData + Profile + "\n" + DataIn["History"].ToString()));

                                    Form.SetWindow(MainForm.WindowType.Delivery, new Dictionary<string, object>() { { "INIT", "" } });
                                }
                                else
                                {
                                    MessageBox.Show("Must have supplier");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Invalid Inventory Amount");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Enter valid price");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Enter Name");
                    }
                };
            }
            else
            {
                SaveAction = () =>
                {
                    if (!string.IsNullOrEmpty(Form.GetProductName()))
                    {
                        if (Form.GetProductPrice() >= 0)
                        {
                            if (Form.GetTotalAmount() - reservedAmount > 0)
                            {
                                List<(Control, CollectionReturn)> Company = GetSelectedObjects("SupplierSelect");
                                if (Company.Count > 0)
                                {
                                    Random Rand = new Random();
                                    int ProductID = Rand.Next(int.MinValue, int.MaxValue);
                                    while (Form.Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("Product_ID=@ID", new (string, string)[] { ("@ID", ProductID.ToString()) }), "Product_ID").Count != 0)
                                    {
                                        ProductID = Rand.Next(int.MinValue, int.MaxValue);
                                    }
                                    Dictionary<string, object> Comp = Company[0].Item2();
                                    string CompName = Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("Company_ID=@ID", new (string, string)[] { ("@ID", Comp["Company_ID"].ToString()) }), "Name")[0]["Name"].ToString();
                                    string Profile = "\n\t|Price: " + Form.GetProductPrice() +
                                                     "\n\t|Time: " + Form.GetPrepTime() +
                                                     "\n\t|Available: " + ((int)(Form.GetTotalAmount() - reservedAmount)).ToString() +
                                                     "\n\t|Supplier: " + CompName;
                                    UpdateUserHistory(Form, Status.GetIDNumber(), "User created product: " + Form.GetProductName() + Profile);
                                    string HistData = "[" + DateTime.UtcNow.Date.ToString("dd/MM/yyyy") + "] Created by: " + Status.GetName() + ", position: " + Status.GetPosition();
                                    Form.Connection.InsertData("[Maestro].[dbo].[PRODUCTS]", ("Product_ID", ProductID), ("Name", Form.GetProductName()), ("Description", Form.GetProductDesc()),
                                        ("Price", Form.GetProductPrice()), ("Time", Form.GetPrepTime()), ("Available_Amt", (int)(Form.GetTotalAmount() - reservedAmount)), ("Image", ""), ("Supplier", Comp["Company_ID"]),
                                        ("History", HistData + Profile + "\n" + DataIn["History"].ToString()));

                                    Form.SetWindow(MainForm.WindowType.Delivery, new Dictionary<string, object>() { { "INIT", "" } });
                                }
                                else
                                {
                                    MessageBox.Show("Must have supplier");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Invalid Inventory Amount");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Enter valid price");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Enter Name");
                    }
                };
            }
            Form.SaveProductAction = SaveAction;

            return true;
        }

        private static bool OpenCompany(MainForm Form, StatusType Status, Dictionary<string, object> DataIn)
        {
            DeleteAllConents(Form.GetList(MainForm.List.ProductItemList));

            Form.resetPass();
            Form.SetPropertiesWindow(0);
            Form.SetProductName("", false);
            Form.SetEmail("");
            Form.SetPhone("");

            AddNewConentItem(Form.GetList(MainForm.List.ProductItemList), "Contact Info", Offset: 0, Size: 25, Flow: Direction.vertical, OnClick: (object? sender, EventArgs Args) => { Form.SetPropertiesWindow(3); });
            if (Status.HasAbility(StatusType.Action.CanSeeDelivey))
            {
                AddNewConentItem(Form.GetList(MainForm.List.ProductItemList), "Current Orders", Offset: 0, Size: 25, Flow: Direction.vertical, OnClick: (object? sender, EventArgs Args) =>
                {
                    Form.SetPropertiesWindow(1);

                    DeleteAllConents(Form.GetList(MainForm.List.ProductSupplier));
                    SetSelectionGroup("CompanyAttributeSelect", (0, 1), Color.Green);
                    if (DataIn != null)
                    {
                        AddNewConentItem(Form.GetList(MainForm.List.ProductSupplier), "OPEN ITEM", Offset: 0, Size: 25, Flow: Direction.vertical, OnClick: (object? sender, EventArgs Args) =>
                        {
                            List<(Control, CollectionReturn)> Att = GetSelectedObjects("CompanyAttributeSelect");
                            if (Att.Count > 0)
                            {
                                Dictionary<string, object> Comp = Att[0].Item2();
                                Form.SetWindow(MainForm.WindowType.Delivery, new Dictionary<string, object>() { { "INIT", "" }, { "SEARCH", Comp["SEARCH"] }, { "PAGE", "Delivery" } });
                            }
                        });

                        List<Dictionary<string, object>> Orders = Form.Connection.GetData("[Maestro].[dbo].[DELIVERIES]", ("Company_ID=@ID", new (string, string)[] { ("@ID", DataIn["Company_ID"].ToString()) }), "Order_ID", "Status");
                        foreach (var item in Orders)
                        {
                            AddNewConentItem(Form.GetList(MainForm.List.ProductSupplier), "Order_ID: " + item["Order_ID"].ToString(), Flow: Direction.vertical, SelectionGroup: "CompanyAttributeSelect",
                            Content: () =>
                            {
                                List<(bool, Control)> Controls = new List<(bool, Control)>();
                                Label L = new Label();
                                L.Text = "Order Status: " + GetOrderStatus((int)item["Status"]);
                                L.Location = new Point(5, 20);
                                L.Size = new Size(350, 30);
                                L.ForeColor = Color.Black;
                                Controls.Add((true, L));
                                return Controls;
                            }, Return: () =>
                            {
                                return new Dictionary<string, object>() { { "SEARCH", item["Order_ID"].ToString() } };
                            });
                        }
                    }
                });
            }
            if (Status.HasAbility(StatusType.Action.CanSeeProduct))
            {
                AddNewConentItem(Form.GetList(MainForm.List.ProductItemList), "Current Products", Offset: 0, Size: 25, Flow: Direction.vertical, OnClick: (object? sender, EventArgs Args) =>
                {
                    Form.SetPropertiesWindow(1);

                    DeleteAllConents(Form.GetList(MainForm.List.ProductSupplier));
                    SetSelectionGroup("CompanyAttributeSelect", (0, 1), Color.Green);
                    if (DataIn != null)
                    {
                        AddNewConentItem(Form.GetList(MainForm.List.ProductSupplier), "OPEN ITEM", Offset: 0, Size: 25, Flow: Direction.vertical, OnClick: (object? sender, EventArgs Args) =>
                        {
                            List<(Control, CollectionReturn)> Att = GetSelectedObjects("CompanyAttributeSelect");
                            if (Att.Count > 0)
                            {
                                Dictionary<string, object> Comp = Att[0].Item2();
                                Form.SetWindow(MainForm.WindowType.Delivery, new Dictionary<string, object>() { { "INIT", "" }, { "SEARCH", Comp["SEARCH"] }, { "PAGE", "Product" } });
                            }
                        });

                        List<Dictionary<string, object>> Orders = Form.Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("Supplier=@ID", new (string, string)[] { ("@ID", DataIn["Company_ID"].ToString()) }), "Name", "Available_Amt");
                        foreach (var item in Orders)
                        {
                            AddNewConentItem(Form.GetList(MainForm.List.ProductSupplier), "Product: " + item["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "CompanyAttributeSelect",
                            Content: () =>
                            {
                                List<(bool, Control)> Controls = new List<(bool, Control)>();

                                Label L = new Label();
                                L.Text = "Amt: " + item["Available_Amt"];
                                L.Location = new Point(5, 20);
                                L.Size = new Size(350, 30);
                                L.ForeColor = Color.Black;
                                Controls.Add((true, L));
                                return Controls;
                            }, Return: () =>
                            {
                                return new Dictionary<string, object>() { { "SEARCH", item["Name"].ToString() } };
                            });
                        }
                    }
                });
            }
            AddNewConentItem(Form.GetList(MainForm.List.ProductItemList), "Set Picture", Offset: 0, Size: 25, Flow: Direction.vertical, OnClick: (object? sender, EventArgs Args) => { MessageBox.Show("This picks a pic"); });

            if (DataIn != null)
            {
                Form.SetProductDesc(DataIn["Description"].ToString(), false);
                Form.SetProductName(DataIn["Name"].ToString(), false);
                Form.SetEmail(DataIn["Email"].ToString());
                Form.SetPhone(DataIn["Phone"].ToString());
            }
            else
            {
                Form.SetProductDesc("[Enter Company Description]", false);
            }

            Action SaveAction;

            if (DataIn != null)
            {
                SaveAction = () =>
                {
                    if (!string.IsNullOrEmpty(Form.GetProductName()))
                    {
                        (bool, string) Em = Form.GetEmail();
                        if (Em.Item1)
                        {
                            Form.Connection.UpdateData("[Maestro].[dbo].[COMPANIES]", ("Company_ID=@ID", new (string, string)[] { ("@ID", DataIn["Company_ID"].ToString()) }), ("Name", Form.GetProductName()), ("Description", Form.GetProductDesc()),
                                            ("Email", Em.Item2), ("Phone", Form.GetPhone()), ("Image", ""));
                            UpdateUserHistory(Form, Status.GetIDNumber(), "User updated company profile: " + Form.GetProductName() + "\n\t|Email: " + Em.Item2 + "\n\t|Phone: " + Form.GetPhone());
                            Form.SetWindow(MainForm.WindowType.Delivery, new Dictionary<string, object>() { { "INIT", "" } });
                        }
                        else
                        {
                            MessageBox.Show("Invalid Email");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Enter Name");
                    }
                };
            }
            else
            {
                SaveAction = () =>
                {
                    if (!string.IsNullOrEmpty(Form.GetProductName()))
                    {
                        (bool, string) Em = Form.GetEmail();
                        if (Em.Item1)
                        {
                            Random Rand = new Random();
                            int CompanyID = Rand.Next(int.MinValue, int.MaxValue);
                            while (Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("Company_ID=@ID", new (string, string)[] { ("@ID", CompanyID.ToString()) }), "Company_ID").Count != 0)
                            {
                                CompanyID = Rand.Next(int.MinValue, int.MaxValue);
                            }
                            Form.Connection.InsertData("[Maestro].[dbo].[COMPANIES]", ("Company_ID", CompanyID), ("Name", Form.GetProductName()), ("Description", Form.GetProductDesc()),
                                            ("Email", Em.Item2), ("Phone", Form.GetPhone()), ("Image", ""));
                            UpdateUserHistory(Form, Status.GetIDNumber(), "User created company profile: " + Form.GetProductName() + "\n\t|Email: " + Em.Item2 + "\n\t|Phone: " + Form.GetPhone());
                            Form.SetWindow(MainForm.WindowType.Delivery, new Dictionary<string, object>() { { "INIT", "" } });
                        }
                        else
                        {
                            MessageBox.Show("Invalid Email");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Enter Name");
                    }
                };
            }
            Form.SaveProductAction = SaveAction;

            return true;
        }

        private static bool OpenEmployee(MainForm Form, StatusType Status, Dictionary<string, object> DataIn)
        {
            return true;
        }

        private static bool OpenOrdering(MainForm Form, StatusType Status, Dictionary<string, object> DataIn)
        {
            DeleteAllConents(Form.GetList(MainForm.List.OrderDiplay_Company));
            DeleteAllConents(Form.GetList(MainForm.List.OrderDisplay_Product));

            Form.resetPass();
            if (DataIn != null)
            {
                Form.SetMemo(DataIn["Memo"].ToString());
            }
            else
            {
                Form.SetMemo("New Order");
            }

            SetSelectionGroup("CompanySelect", (1, 1), Color.Orchid);
            List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("", null), "Name", "Company_ID");
            bool Added = false;
            for (int i = 0; i < ListItems.Count; i++)
            {
                object CompName = ListItems[i]["Name"];
                object CompID = ListItems[i]["Company_ID"];
                if (DataIn != null)
                {
                    if (ListItems[i]["Company_ID"].Equals(DataIn["Company_ID"]))
                    {
                        AddNewConentItem(Form.GetList(MainForm.List.OrderDiplay_Company), "Company: " + ListItems[i]["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "CompanySelect", Return: () =>
                        {
                            return new Dictionary<string, object>() { { "Name", CompName }, { "Company_ID", CompID } };
                        });
                        SelectItem(Form.GetList(MainForm.List.OrderDiplay_Company), 0);
                    }
                }
                else
                {
                    AddNewConentItem(Form.GetList(MainForm.List.OrderDiplay_Company), "Company: " + ListItems[i]["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "CompanySelect", Return: () =>
                    {
                        return new Dictionary<string, object>() { { "Name", CompName }, { "Company_ID", CompID } };
                    });
                }
            }

            SetSelectionGroup("OrderProductSelect", (0, 1000), Color.Green);

            List<Dictionary<string, object>> Bundles = DataIn != null ? Form.Connection.GetData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID", new (string, string)[] { ("@ID", DataIn["Bundle_ID"].ToString()) }), "Product_ID", "Delivered") : new List<Dictionary<string, object>>();
            List<int> SelectItems = new List<int>();
            ListItems = Form.Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("Available_Amt>0", null), "Name", "Product_ID", "Available_Amt", "Time");
            Added = false;
            for (int i = 0; i < ListItems.Count; i++)
            {
                foreach (var Bundle in Bundles)
                {
                    if (ListItems[i]["Product_ID"].Equals(Bundle["Product_ID"]))
                    {
                        Added = false;
                        if ((int)Bundle["Delivered"] == 0)
                            SelectItems.Add(SelectItems.Count);
                        break;
                    }
                    else
                    {
                        Added = true;
                    }
                }
                if (!Added)
                {
                    object ProductID = ListItems[i]["Product_ID"];
                    object ProductName = ListItems[i]["Name"];
                    object ProductAmt = ListItems[i]["Available_Amt"];
                    object ProductPrep = ListItems[i]["Time"];
                    AddNewConentItem(Form.GetList(MainForm.List.OrderDisplay_Product), "Product: " + ListItems[i]["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "OrderProductSelect", Return: () =>
                    {
                        return new Dictionary<string, object>() { { "Product_ID", ProductID }, { "Name", ProductName }, { "Avalable", ProductAmt }, { "Time", ProductPrep } };
                    }, Content: () =>
                    {
                        List<(bool, Control)> Controls = new List<(bool, Control)>();

                        if (DataIn == null)
                        {
                            Label L = new Label();
                            L.Text = "Avalable: " + ProductAmt.ToString();
                            L.Location = new Point(5, 18);
                            L.Size = new Size(350, 30);
                            L.ForeColor = Color.Black;
                            Controls.Add((true, L));

                            TextBox textBox = new TextBox();
                            textBox.Location = new Point(5, 35);
                            textBox.Size = new Size(350, 30);
                            textBox.ForeColor = Color.Black;
                            Controls.Add((false, textBox));
                        }
                        else
                        {
                            List<Dictionary<string, object>> ProductItem = Form.Connection.GetData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID AND Product_ID=@PID",
                                new (string, string)[] { ("@ID", DataIn["Bundle_ID"].ToString()), ("@PID", ProductID.ToString()) }), "Quantity", "Delivered");
                            if ((int)ProductItem[0]["Delivered"] == 1)
                            {
                                Label L = new Label();
                                L.Text = "[Product delivered, Qt: " + ProductItem[0]["Quantity"].ToString() + "]";
                                L.Location = new Point(5, 20);
                                L.Size = new Size(350, 30);
                                L.ForeColor = Color.Black;
                                Controls.Add((true, L));
                            }
                            else
                            {
                                TextBox textBox = new TextBox();
                                textBox.Text = ProductItem[0]["Quantity"].ToString();
                                textBox.ReadOnly = true;
                                textBox.Location = new Point(5, 20);
                                textBox.Size = new Size(350, 30);
                                textBox.ForeColor = Color.Black;
                                Controls.Add((false, textBox));
                            }
                        }

                        return Controls;
                    });
                }
            }

            foreach (var item in SelectItems)
            {
                SelectItem(Form.GetList(MainForm.List.OrderDisplay_Product), item);
            }

            Action SaveAction;

            if (DataIn != null)
            {
                SaveAction = () =>
                {
                    List<(Control, CollectionReturn Call)> Products = GetSelectedObjects("OrderProductSelect");
                    List<(Control, CollectionReturn)> Company = GetSelectedObjects("CompanySelect");
                    Form.Connection.UpdateData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID", new (string, string)[] { ("@ID", DataIn["Bundle_ID"].ToString()) }), ("Delivered", 1));
                    Dictionary<string, object> Undelivered;
                    foreach (var item in Products)
                    {
                        Undelivered = (Dictionary<string, object>)item.Call();
                        Form.Connection.UpdateData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID AND Product_ID=@PID", new (string, string)[] { ("@ID", DataIn["Bundle_ID"].ToString()), ("@PID", Undelivered["Product_ID"].ToString()) }), ("Delivered", 0));
                    }
                    List<Dictionary<string, object>> Items = Form.Connection.GetData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID", new (string, string)[] { ("@ID", DataIn["Bundle_ID"].ToString()) }), "Product_ID", "Delivered", "Quantity");
                    List<Dictionary<string, object>> Product;
                    string HistData = "[" + DateTime.UtcNow.Date.ToString("dd/MM/yyyy") + "] Modified by: " + Status.GetName() + ", position: " + Status.GetPosition() + "\n";
                    int Stat = 1;
                    string Log = "";
                    foreach (var item in Items)
                    {
                        if ((int)item["Delivered"] == 1)
                        {
                            Product = Form.Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("Product_ID=@ID", new (string, string)[] { ("@ID", item["Product_ID"].ToString()) }), "Name");
                            HistData += "\t" + Product[0]["Name"].ToString() + " Marked as Delivered\n";
                            Log += "\n\t|" + Product[0]["Name"].ToString() + "(" + item["Quantity"].ToString() + ")" + " Marked as Delivered";
                            if (Stat != 2)
                            {
                                Stat = 3;
                            }
                        }
                        else
                        {
                            Stat = 2;
                        }
                    }
                    Form.Connection.UpdateData("[Maestro].[dbo].[DELIVERIES]", ("Order_ID=@ID", new (string, string)[] { ("@ID", DataIn["Order_ID"].ToString()) }), ("Memo", Form.GetMemo()), ("History", HistData + DataIn["History"]), ("Status", Stat));
                    Dictionary<string, object> Subject = (Dictionary<string, object>)Company[0].Item2();
                    UpdateUserHistory(Form, Status.GetIDNumber(), "User updated delivery to: " + Subject["Name"].ToString() + Log);
                    Form.SetWindow(MainForm.WindowType.Delivery, new Dictionary<string, object>() { { "INIT", "" } });
                };
            }
            else
            {
                SaveAction = () =>
                {
                    List<(Control Panel, CollectionReturn Call)> Products = GetSelectedObjects("OrderProductSelect");
                    List<(Control, CollectionReturn)> Company = GetSelectedObjects("CompanySelect");
                    if (Company.Count > 0 && Products.Count > 0)
                    {
                        Dictionary<string, object> Subject;
                        string ProductNames = "";
                        string ProductBounds = "";
                        List<(object, int, int, int, string)> ProductCollection = new List<(object, int, int, int, string)>();
                        foreach (var item in Products)
                        {
                            Subject = (Dictionary<string, object>)item.Call();
                            TextBox text = item.Panel.Controls[0] as TextBox;
                            if (string.IsNullOrEmpty(text.Text))
                            {
                                ProductNames += Subject["Name"].ToString() + "\n";
                            }
                            else
                            {
                                if (int.TryParse(text.Text, out int Result))
                                {
                                    if (Result > 0 && Result <= int.Parse(Subject["Avalable"].ToString()))
                                    {
                                        ProductCollection.Add((Subject["Product_ID"], Result, (int)Subject["Time"], int.Parse(Subject["Avalable"].ToString()) - Result, Subject["Name"].ToString()));
                                    }
                                    else
                                    {
                                        ProductBounds += Subject["Name"].ToString() + "\n";
                                    }
                                }
                                else
                                {
                                    ProductBounds += Subject["Name"].ToString() + "\n";
                                }
                            }
                        }

                        if (ProductNames.Length > 0 || ProductBounds.Length > 0)
                        {
                            MessageBox.Show((ProductNames.Length > 0 ? "Please enter amount for:\n" + ProductNames : "") + (ProductBounds.Length > 0 ? "Please enter an accepted amount [1, Avalable] for:\n" + ProductBounds : ""));
                        }
                        else
                        {
                            Random Rand = new Random();
                            int BundleID = Rand.Next(int.MinValue, int.MaxValue);
                            while (Form.Connection.GetData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID=@ID", new (string, string)[] { ("@ID", BundleID.ToString()) }), "Bundle_ID").Count != 0)
                            {
                                BundleID = Rand.Next(int.MinValue, int.MaxValue);
                            }
                            List<string> CollectionProduct = new List<string>();
                            foreach (var item in ProductCollection)
                            {
                                Form.Connection.InsertData("[Maestro].[dbo].[BUNDLES]", ("Bundle_ID", BundleID), ("Product_ID", item.Item1), ("Quantity", item.Item2),
                                    ("Delivery_Date", DateTime.UtcNow.Date.AddDays(item.Item3).ToString("dd/MM/yyyy")), ("Delivered", 0));
                                Form.Connection.UpdateData("[Maestro].[dbo].[PRODUCTS]", ("Product_ID=@PID", new (string, string)[] { ("@PID", item.Item1.ToString()) }), ("Available_Amt", item.Item4));
                                CollectionProduct.Add(item.Item5 + "(" + item.Item2.ToString() + ")");
                            }

                            int OrderID = Rand.Next(int.MinValue, int.MaxValue);
                            while (Form.Connection.GetData("[Maestro].[dbo].[DELIVERIES]", ("Order_ID=@ID", new (string, string)[] { ("@ID", OrderID.ToString()) }), "Order_ID").Count != 0)
                            {
                                OrderID = Rand.Next(int.MinValue, int.MaxValue);
                            }

                            Subject = (Dictionary<string, object>)Company[0].Item2();
                            Form.Connection.InsertData("[Maestro].[dbo].[DELIVERIES]", ("Order_ID", OrderID), ("Salesman_ID", Status.GetIDNumber()), ("Bundle_ID", BundleID),
                                ("Status", 1), ("History", "[" + DateTime.UtcNow.Date.ToString("dd/MM/yyyy") + "] Created by: " + Status.GetName()), ("Company_ID", Subject["Company_ID"]),
                                ("Memo", Form.GetMemo()), ("CreationDate", DateTime.UtcNow.Date.ToString("dd/MM/yyyy")));
                            Subject = Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("Company_ID=@ID", new (string, string)[] { ("@ID", Subject["Company_ID"].ToString()) }), "Name")[0];
                            UpdateUserHistory(Form, Status.GetIDNumber(), "User created delivery to: " + Subject["Name"].ToString() + 
                                "\n\t|Products: " + String.Join(", ", CollectionProduct));
                            Form.SetWindow(MainForm.WindowType.Delivery, new Dictionary<string, object>() { { "INIT", "" } });
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select" + (Products.Count == 0 ? " at least 1 Product" + (Company.Count == 0 ? " and" : "") : "") + (Company.Count == 0 ? " 1 company" : ""));
                    }
                };
            }

            Form.SaveOrderAction = SaveAction;

            return true;
        }
    }
}
