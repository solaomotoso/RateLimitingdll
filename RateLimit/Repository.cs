using System.Data.SQLite;
using Microsoft.Data.Sqlite;
using RepoDb;
using RepoDb.Enumerations;
using System.Data;
using System.Linq.Expressions;
// using RepoDb.SqlServer;


    public class Repository:DbRepository<SqliteConnection>
    {
        private SqliteConnection connection;

        public Repository(string connectionString) : base(connectionString)
        {
            Persistency = ConnectionPersistency.Instance;
            // connection=connectionString;
            if (!RepoDb.SqliteBootstrap.IsInitialized)
            {
                RepoDb.SqliteBootstrap.Initialize();
                RepoDb.SqlServerBootstrap.Initialize();
            }

        }

        public ConnectionPersistency Persistency { get; }
        public void CreateDb()
        {
            if(!Directory.Exists("Db"))
            {
                Directory.CreateDirectory("Db");
            }
            if(!File.Exists("Db/UserDetails.db"))
            {
                SQLiteConnection.CreateFile("Db/UserDetails.db");
            }
        }
        public void SaveUserId(Users us, string connectionstring)
        {
            if(File.Exists("Db/UserDetails.db"))
            {
                OpenConnection();
                string sql = "INSERT INTO Users (UserId) VALUES (@UserId)";
                using (SQLiteConnection connection = new SQLiteConnection(connectionstring))
                {
                   connection.Open();
                   using (SQLiteCommand command = new SQLiteCommand(sql,  connection))
                  {
                   command.Parameters.AddWithValue("@UserId", us.UserId);
                   command.ExecuteNonQuery();
                  }
                }
            }
        }
        public string GetUserid(string userid, string connectionstring)
        {
            Users us=new Users();
            OpenConnection();
            // var sql = "SELECT userid FROM [users] WHERE userid = @userid;";
             using (SQLiteConnection connection = new SQLiteConnection(connectionstring))
                {
                   connection.Open();
                   using (SQLiteCommand command = connection.CreateCommand())
                      {
                           command.CommandText =  "SELECT userid FROM [users] WHERE userid = @userid;";
                           command.Parameters.AddWithValue("@userid", userid);
                           command.CommandType = CommandType.Text;
                           SQLiteDataReader myReader = command.ExecuteReader();
                           while (myReader.Read())
                           {
                            //  us.ID=(int)myReader["ID"];
                             us.UserId=(string)myReader["UserId"];
                           }
                        myReader.Close();
                      }
                }
                 return  us.UserId;
        }
       
        public void CreateTables()
        {
            CreateDb();
            var statements=GetDBScripts();
            using (SqliteTransaction trans=(SqliteTransaction)OpenConnection().BeginTransaction())
            {
                foreach (var statement in statements)
                {
                    ExecuteNonQuery(statement, null, System.Data.CommandType.Text,trans);
                    
                }
                trans.Commit();
            }
        }

        public  string[] GetDBScripts()
        {
            string[] tableSql =new[]{ @"CREATE TABLE IF NOT EXISTS Users (
                userid TEXT PRIMARY KEY
                ) WITHOUT ROWID"};
            return tableSql;
        }
        public IDbConnection OpenConnection()
        {
            if (connection == null)
            {
                connection = CreateConnection();
                connection.Open();
            }
            else if (connection.State != ConnectionState.Open)
                connection.Open();
            return connection;
        }
        public int Insert<T>(IEnumerable<T> data, IDbTransaction transaction)where T : class
        {
            int result = 0;
            try
            {
                connection = (SqliteConnection)transaction.Connection;
                result = connection.InsertAll<T>(entities: data, batchSize: 100, null, null, transaction: transaction);
            }
            catch (System.Exception ex)
            {

                throw ex;
            }
            return result;
        }

        public int Update<T>(IEnumerable<T> data,Expression<Func<T, object>> fields, IDbTransaction transaction, string key = "", object[] maps = null) where T : class
        {
            int result;
            connection = (SqliteConnection)transaction.Connection;
            result =connection.UpdateAll<T>(entities: data,qualifiers: fields, transaction: transaction);
            return result;
        }
        public List<T> Get<T>(QueryGroup where) where T:class
        {
            return OpenConnection().Query<T>(where).ToList();
        }
    }
