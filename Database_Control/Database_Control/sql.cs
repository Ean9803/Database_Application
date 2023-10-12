using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Net.NetworkInformation;

namespace Database_Control
{
    public class SQL
    {
        private SqlConnection com;

        public bool Connect(string Database, string Username, string Password, out Exception? Ex)
        {
            if (com != null)
                com.Close();
            try
            {
                com = new SqlConnection(@"server=" + Database + ";User ID=" + Username + ";Password=" + Password + ";TrustServerCertificate=True;MultipleActiveResultSets=true");
                com.Open();
                Ex = null;
                return true;
            }
            catch (Exception ex)
            {
                Ex = ex;
                return false;
            }
        }
        private enum Direction { Reciving, Delivering }
        private delegate object Process(object Input, Direction Mode);
        private List<Dictionary<string, Process>> Gate = new List<Dictionary<string, Process>>()
        {
            new Dictionary<string, Process>()
            {
                {
                    "@Pass",
                    ProcessPassword
                }
            },
            new Dictionary<string, Process>()
            {
                {
                    "Password",
                    ProcessPassword
                }
            },
        };

        private bool HasProcess(string Col, object Val, Direction Dir, out object ValResult)
        {
            foreach (var item in Gate)
            {
                if (item.ContainsKey(Col))
                {
                    ValResult = item[Col](Val, Dir);
                    return true;
                }
            }
            
            ValResult = Val;
            return false;
        }

        private static object ProcessPassword(object Input, Direction Mode)
        {
            if (Mode == Direction.Reciving)
            {
                //decode
                var bytes = Convert.FromBase64String(Input.ToString());
                return Encoding.UTF8.GetString(bytes).ToString();
            }
            else
            {
                //encode
                var bytes = Encoding.UTF8.GetBytes(Input.ToString());
                return Convert.ToBase64String(bytes).ToString();
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
                    if (HasProcess(Data[i].Col, Data[i].Val, Direction.Delivering, out object Result))
                    {
                        Data[i].Val = Result;
                    }
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
                            if (HasProcess(whereClause.WhereParams[i].Item1, whereClause.WhereParams[i].Item2, Direction.Delivering, out object Result))
                            {
                                whereClause.WhereParams[i].Item2 = Result.ToString();
                            }
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
                    if (HasProcess(Data[i].Col, Data[i].Val, Direction.Delivering, out object Result))
                    {
                        Data[i].Val = Result;
                    }
                }
                setClause = setClause.Substring(0, setClause.Length - 1);

                string sqlUpdate = $"UPDATE {tableName} SET {setClause}" + (string.IsNullOrEmpty(whereClause.Clause) ? "" : $" WHERE {whereClause.Clause}");


                using (SqlCommand cmd = new SqlCommand(sqlUpdate, com))
                {
                    if (whereClause.WhereParams != null)
                    {
                        for (int i = 0; i < whereClause.WhereParams.Length; i++)
                        {
                            if (HasProcess(whereClause.WhereParams[i].Item1, whereClause.WhereParams[i].Item2, Direction.Delivering, out object Result))
                            {
                                whereClause.WhereParams[i].Item2 = Result.ToString();
                            }
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
                            if (HasProcess(whereClause.WhereParams[i].Item1, whereClause.WhereParams[i].Item2, Direction.Delivering, out object Result))
                            {
                                whereClause.WhereParams[i].Item2 = Result.ToString();
                            }
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
                            object In = Read.GetValue(i);
                            if (HasProcess(Cols[i], In, Direction.Reciving, out object Result))
                            {
                                In = Result;
                            }
                            Ret[Ret.Count - 1][Cols[i]] = In;
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
}
