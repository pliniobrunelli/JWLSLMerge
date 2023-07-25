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

        public T? GetFirst<T>(T item, string[] FieldNames, bool SetEmptyWhenNull = false)
        {
            using (IDbConnection con = new SQLiteConnection(connectionString))
            {
                string sql = $"SELECT * FROM {typeof(T).Name} WHERE {getWhereClause(FieldNames)}";

                return con.Query<T>(sql, getParameters<T>(item, FieldNames, SetEmptyWhenNull)).FirstOrDefault();
            }
        }

        public bool ItemExists<T>(T item, string[] FieldNames, bool SetEmptyWhenNull = false)
        {
            using (IDbConnection con = new SQLiteConnection(connectionString))
            {
                string sql = $"SELECT 1 FROM {typeof(T).Name} WHERE {getWhereClause(FieldNames)}";

                return con.ExecuteScalar<int>(sql, getParameters<T>(item, FieldNames, SetEmptyWhenNull)) > 0;
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

        private DynamicParameters getParameters<T>(T objeto, string[]? FieldNames = null, bool SetEmptyWhenNull = false)
        {
            FieldNames = FieldNames?.Select(p => p.ToLower()).ToArray() ?? new string[0];
            var parameters = new DynamicParameters();

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                if (!propertyInfo.GetCustomAttributes(true).Any(a => a is IgnoreAttribute) &&
                    (FieldNames.Length == 0 || FieldNames.Contains(propertyInfo.Name.ToLower())))
                {
                    object? value = propertyInfo.GetValue(objeto);
                    if (value == null && SetEmptyWhenNull) value = "";

                    parameters.Add(propertyInfo.Name, value);
                }
            }

            return parameters;
        }

        private string getWhereClause(string[] FieldNames)
        {
            return string.Join(" AND ", FieldNames.Select(p => $"IFNULL({p}, '') = @{p}").ToArray());
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

