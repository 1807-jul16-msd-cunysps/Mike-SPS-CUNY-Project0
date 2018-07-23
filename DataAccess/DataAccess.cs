﻿using System;
using System.Collections.Generic;
using System.Data;              // ADO.NET lib
using System.Data.SqlClient;    // Client in ADO.NET library

namespace DataAccessADOSQL
{
    public static class DBAccess
    {
        public static string connectionString = "Data Source=rev-training-mc-dbs.database.windows.net;" +   // SQL Server
                                                "Initial Catalog=rev-training-mc-contacts-db;" +            // SQL DB
                                                "Persist Security Info=True;" +                             // Security
                                                "MultipleActiveResultSets=True;" +                          // MARS
                                                "User ID=revature;" +                                       // User name
                                                "Password=Password1";                                       // Password

        public static void InitTables()
        {
            string[] tables = new string[] {
                    "CREATE TABLE person(" +
                                          "id BIGINT PRIMARY KEY IDENTITY(1,1), " +
                                          "firstname VARCHAR(35), " +
                                          "lastname VARCHAR(35));",
                    "CREATE TABLE address(" +
                                          "id BIGINT PRIMARY KEY IDENTITY(1,1), " +
                                          "personID BIGINT FOREIGN KEY REFERENCES person(id)," +
                                          "housenum VARCHAR(25), " +
                                          "street VARCHAR(25), " +
                                          "city VARCHAR(25), " +
                                          "state VARCHAR(4), " +
                                          "country VARCHAR(25), " +
                                          "zipcode VARCHAR(5));",
                    "CREATE TABLE phone(" +
                                          "id BIGINT PRIMARY KEY IDENTITY(1,1), " +
                                          "personID BIGINT FOREIGN KEY REFERENCES person(id)," +
                                          "country VARCHAR(25), " +
                                          "areacode VARCHAR(3)," +
                                          "number VARCHAR(7)," +
                                          "ext VARCHAR(5));"
            };
            // SQL connection object
            SqlConnection connection = null;
            try
            {
                // Try and create all tables
                foreach (string s in tables)
                {
                    connection = new SqlConnection(connectionString);               // Define connection
                    connection.Open();                                              // Open connection
                    try
                    {
                        SqlCommand command = new SqlCommand(s, connection);         // Define command
                        command.ExecuteNonQuery();                                  // Send command
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"InitTables(): Failed to create table with: '{s}'");
                        Console.WriteLine($"\n{ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        public static void Add(PersonModel person)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();                                          // Open connection
                SqlTransaction transaction = connection.BeginTransaction(); // Create transaction
                SqlCommand command = connection.CreateCommand();            // Create command
                command.Transaction = transaction;                          // Assign command to transaction

                try
                {
                    // Insert Person object
                    command.CommandText = $"INSERT INTO person VALUES (" +    
                                         $"'{person.Firstname}', " +
                                         $"'{person.Lastname}'" +
                                         ");";
                    command.ExecuteNonQuery();
                    // Get Person ID
                    command.CommandText = "SELECT SCOPE_IDENTITY()";
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        reader.Read();
                        while (reader.Read())
                        {
                            Console.WriteLine($"{reader[0]}");
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to ExecuteReader");
                    }
                    reader.Close();
                    /*
                    command.CommandText = "INSERT INTO phone VALUES (" +    // Phone
                                          $"{person.Phone.Id}," +
                                          $"'{person.Phone.CountryCode}'," +
                                          $"'{person.Phone.AreaCode}'," +
                                          $"'{person.Phone.Number}'," +
                                          $"'{person.Phone.Ext}'" +
                                          ");";
                    command.ExecuteNonQuery();

                    
                    command.CommandText = "INSERT INTO address VALUES (" +  // Address
                                          $"{person.Address.Id}, " +
                                          $"'{person.Address.HouseNum}', " +
                                          $"'{person.Address.Street}', " +
                                          $"'{person.Address.City}', " +
                                          $"'{person.Address.State}', " +
                                          $"'{person.Address.Country}', " +
                                          $"'{person.Address.Zipcode}'" +
                                          ");";
                    command.ExecuteNonQuery();
                    */

                    transaction.Commit();                                   // Commit transaction
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    try
                    {
                        transaction.Rollback();                             // Roll back
                    }
                    catch (Exception exRollback)
                    {
                        Console.WriteLine(exRollback.Message);
                    }
                }
            }
        }
    }
}
