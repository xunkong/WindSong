using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WindSong.Helpers;

internal abstract class DatabaseHelper
{


    private static readonly string _sqlitePath;

    public static string SqlitePath => _sqlitePath;

    private static readonly string _sqliteConnectionString;

    private static bool _initialized;

    private static List<string> UpdateSqls = new() { };


    static DatabaseHelper()
    {
        _sqlitePath = Path.Combine(AppContext.BaseDirectory, @"Data\UserData.db");
        _sqliteConnectionString = $"Data Source={_sqlitePath};";
    }



    private static void Initialize()
    {
        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, "Data"));
        using var dapper = new SqliteConnection(_sqliteConnectionString);
        var version = dapper.QueryFirstOrDefault<int>("PRAGMA USER_VERSION;");
        if (version < UpdateSqls.Count)
        {
            foreach (var sql in UpdateSqls.Skip(version))
            {
                dapper.Execute(sql);
            }
        }
        _initialized = true;
    }



    public static SqliteConnection CreateConnection()
    {
        if (!_initialized)
        {
            Initialize();
        }
        var dapper = new SqliteConnection(_sqliteConnectionString);
        dapper.Open();
        return dapper;
    }




}
