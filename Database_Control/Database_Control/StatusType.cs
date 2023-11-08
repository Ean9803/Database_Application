﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Control
{
    public class StatusType
    {
        private int Stat;
        private int ID;
        private string Name;
        private string Position;
        private string Pass;
        public StatusType(int Value, int ID, string Name, string Position, string Password)
        {
            Stat = Value;
            this.ID = ID;
            this.Name = Name;
            this.Position = Position;
            Pass = Password;
        }

        public string GetPass()
        {
            return Pass;
        }

        public string GetPosition()
        {
            return Position;
        }

        public string GetName()
        {
            return Name;
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

        public string[] GetPresets()
        {
            return Defaults.Keys.ToArray();
        }

        public string FindMatch(List<Action> ActionItems)
        {
            int Match = 0;
            foreach (var item in Defaults)
            {
                if (item.Value.Length == ActionItems.Count)
                {
                    Match = 0;
                    foreach (var Perm in item.Value)
                    {
                        if (ActionItems.Contains(Perm))
                        {
                            Match++;
                        }
                    }
                    if (Match == item.Value.Length)
                    {
                        return item.Key;
                    }
                }
            }
            return CreateStat(ActionItems.ToArray()).ToString();
        }

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


        public List<string> PrintStats()
        {
            List<string> Ret = new List<string>();

            foreach (Action item in Enum.GetValues(typeof(Action)))
            {
                if (HasAbility(item))
                {
                    Ret.Add(item.ToString());
                }
            }

            return Ret;
        }
    }
}
