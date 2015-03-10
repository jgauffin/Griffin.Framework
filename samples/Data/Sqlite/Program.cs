using System;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Griffin.Core.Data;
using Griffin.Core.Data.Mapper;
using Griffin.Core.Data.Mapper.CommandBuilders;
using Griffin.Data.Sqlite;

namespace Sqlite
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandBuilderFactory.Assign(mapper => new SqliteCommandBuilder(mapper));

            string cs = "URI=file:test.db";
            var connection = new SQLiteConnection(cs);
            connection.Open();

            if (!connection.TableExists("Users"))
            {
                using (var uow = new AdoNetUnitOfWork(connection))
                {
                    uow.Execute(
                        "CREATE TABLE Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, FirstName TEXT, LastName text, CreatedAtUtc INTEGER)");
                    uow.SaveChanges();
                }
            }

            var users = connection.ToList<User>(new {FirstName = "Gau%"});

            var first = connection.First<User>(new {Id = 1});



            // clear old data
            using (var uow = new AdoNetUnitOfWork(connection))
            {
                using (var cmd = uow.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Users";
                    cmd.AddParameter("id", "983498043903");
                    foreach (var entity in cmd.ToEnumerable<User>())
                    {
                        Console.WriteLine(entity.FirstName);
                    }


                }

                uow.Truncate<User>();
                for (int i = 0; i < 100; i++)
                {
                    uow.Insert(new User { FirstName = "Arne" + i });
                }

                uow.SaveChanges();
            }


        }
    }
}
