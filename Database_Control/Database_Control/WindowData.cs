﻿using System;
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
                                            Form.SetDetailPanel(MainForm.PanelDetail.Delivery);
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
                                            Form.SetDetailPanel(MainForm.PanelDetail.Product);
                                        }, UnClick: () =>
                                        {
                                            DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                            if (Status.HasAbility(StatusType.Action.CanCreateProduct))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Create New", 100, 0, Direction.horizontal);
                                            }
                                            Form.SetDetailPanel(MainForm.PanelDetail.None);
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
                                            Form.SetDetailPanel(MainForm.PanelDetail.Employee);
                                        }, UnClick: () =>
                                        {
                                            DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                            if (Status.HasAbility(StatusType.Action.CanCreateEmployee))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Create New", 100, 0, Direction.horizontal);
                                            }
                                            Form.SetDetailPanel(MainForm.PanelDetail.None);
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
                                            Form.SetDetailPanel(MainForm.PanelDetail.Company);
                                        }, UnClick: () =>
                                        {
                                            DeleteAllConents(Form.GetList(MainForm.List.ControlList));
                                            if (Status.HasAbility(StatusType.Action.CanCreateDelivery))
                                            {
                                                AddNewConentItem(Form.GetList(MainForm.List.ControlList), "Create New", 100, 0, Direction.horizontal);
                                            }
                                            Form.SetDetailPanel(MainForm.PanelDetail.None);
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

            SetSelectionGroup("CompanySelect", (1, 1), Color.Orchid);
            List<Dictionary<string, object>> ListItems = Form.Connection.GetData("[Maestro].[dbo].[COMPANIES]", ("", null), "Name");
            foreach (var item in ListItems)
            {
                AddNewConentItem(Form.GetList(MainForm.List.OrderDiplay_Company), "Company: " + item["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "CompanySelect");
            }

            SetSelectionGroup("OrderProductSelect", (1, 1000), Color.Green);
            ListItems = Form.Connection.GetData("[Maestro].[dbo].[PRODUCTS]", ("", null), "Name");
            foreach (var item in ListItems)
            {
                AddNewConentItem(Form.GetList(MainForm.List.OrderDisplay_Product), "Product: " + item["Name"].ToString(), Flow: Direction.vertical, SelectionGroup: "OrderProductSelect");
            }

            return true;
        }
    }
}
