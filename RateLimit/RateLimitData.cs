using System.Data.SQLite;
using Microsoft.Data.Sqlite;
using RepoDb;
using RepoDb.Enumerations;
using System.Data;
using System.Linq.Expressions;

public static class RateLimitData
{
    // public static bool StartsWithUpper(this string? str)
    // {
    //     if (string.IsNullOrWhiteSpace(str))
    //         return false;

    //     char ch = str[0];
    //     return char.IsUpper(ch);
    // }

    public static Users InsertUser()
     {
        Users user = new Users();
        user.UserId = Guid.NewGuid().ToString();
      //   using (var connection = new SqliteConnection(ConnectDetails.conString))
      //   {
      //      var id = connection.Insert(user);
      //   }
        return user;
     }
}

