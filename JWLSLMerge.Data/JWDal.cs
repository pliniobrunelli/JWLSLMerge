using Dapper;
using JWLSLMerge.Data.Attributes;
using System.Data;
using System.Data.SQLite;

namespace JWLSLMerge.Data
{
    public class JWDal
    {
        private string connectionString;

        public JWDal(string dbPath)
        {
            connectionString = $"Data Source={dbPath}";
        }

        public IEnumerable<T> TableList<T>()
        {
            using (IDbConnection cnn = new SQLiteConnection(connectionString))
            {
                return cnn.Query<T>($"SELECT * FROM {typeof(T).Name}");
            }
        }

        public int ItemInsert<T>(T item)
        {
            using (IDbConnection con = new SQLiteConnection(connectionString))
            {
                string sql =
                    $"INSERT INTO {typeof(T).Name} ({getFieldNames<T>()}) " +
                    $"VALUES ({getFieldNames<T>(true)});" +
                    "SELECT last_insert_rowid();";

                return con.ExecuteScalar<int>(sql, getParameters<T>(item));
            }
        }

        public string SetLastModification()
        {
            string dt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");

            using (IDbConnection con = new SQLiteConnection(connectionString))
            {
                string sql = $"UPDATE LastModified SET LastModified = '{dt}'";

                con.Execute(sql);
            }

            return dt;
        }

        private DynamicParameters getParameters<T>(T objeto)
        {
            var parametros = new DynamicParameters();

            foreach (var propriedade in typeof(T).GetProperties())
            {
                if (!propriedade.GetCustomAttributes(true).Any(a => a is IgnoreAttribute))
                {
                    parametros.Add(propriedade.Name, propriedade.GetValue(objeto));
                }
            }

            return parametros;
        }

        private string getFieldNames<T>(bool includeAtSymbol = false)
        {
            List<string> names = new List<string>();

            foreach (var propriedade in typeof(T).GetProperties())
            {
                if (!propriedade.GetCustomAttributes(true).Any(a => a is IgnoreAttribute))
                {
                    names.Add((includeAtSymbol ? "@" : "") + propriedade.Name);
                }
            }

            return string.Join(",", names.ToArray());
        }
    }
}

