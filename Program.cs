using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Data.Entity.ModelConfiguration.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Habit_Logger
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            string DbPath = "Files/habit-logger.db";
            string tableName = "HabitDateAndQuantity";
            bool endApp = false;

            if (!File.Exists(DbPath))
            {
                Console.WriteLine("Database does not exist!");
                Console.WriteLine("");
                SQLiteConnection.CreateFile(DbPath);
                Console.WriteLine("Finished creating database");
                Console.WriteLine("");
                Console.WriteLine("Creating table.");

                await using var conn = new SQLiteConnection($"Data Source={DbPath}");

                conn.Open();

                await using var cmd = new SQLiteCommand($"CREATE TABLE {tableName} (Date TEXT, Quantity INT)",conn);
                // cmd.ExecuteScalar();
                cmd.ExecuteNonQuery();
                Console.WriteLine("Finished creating table.");

                Console.WriteLine("");
            }
            else
            {
                Console.WriteLine("Database exists.");
                await using var conn = new SQLiteConnection($"Data Source={DbPath}");
                Console.WriteLine("");
            }

             
            while (!endApp)
            {
                await using var conn = new SQLiteConnection($"Data Source={DbPath}");
                Console.WriteLine("HABBIT LOGGER \r");
                Console.WriteLine("--------------------\n");
                Console.WriteLine("");
                Console.WriteLine("What would you like to do?");
                Console.WriteLine("");
                Console.WriteLine("Type 0 to Close Application.");
                Console.WriteLine("Type 1 to View All Records.");
                Console.WriteLine("Type 2 to Insert Record.");
                Console.WriteLine("Type 3 to Delete Record.");
                Console.WriteLine("Type 4 to Update Record.");
                Console.WriteLine("");

                string? op = Console.ReadLine();

                if (op == null || ! Regex.IsMatch(op, "[0|1|2|3|4]"))
                {
                    Console.WriteLine("Error: Unrecognized input.");
                }
                else
                {
                    try
                    {
                        DoOperation(op, conn);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Oh no!, An exception has occured trying to do the operation.\n - Details: " + e.Message);
                    }
                }
            }

            void DoOperation(string op, SQLiteConnection conn)
            {
                switch (op)
                {
                    case "0":
                        CloseApplication(conn);
                        break;
                    case "1":
                        ViewAllRecords(conn);
                        break;
                    case "2":
                        InsertRecord(conn);
                        break;
                    case "3":
                        DeleteRecord(conn);
                        break;
                    case "4":
                        UpdateRecord(conn);
                        break;
                    default:
                        break;
                }
            }

            async void ViewAllRecords(SQLiteConnection conn)
            {
                string sql = $"SELECT * From {tableName}";
                conn.Open();
                await using var cmd = new SQLiteCommand(sql, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("Date: " + reader["Date"] + " " + "Quantity: " + reader["Quantity"]);
                }
                conn.Close();
                Console.WriteLine("");

            } // ViewAllRecords
         

            async void InsertRecord(SQLiteConnection conn)
            {

                Console.WriteLine("Enter Date (DD/MM/YYY): ");
                string? date = Console.ReadLine();

                Console.WriteLine("Enter Quantity: ");
                string? quantityString = Console.ReadLine();
                int quantityInt;

                while (!int.TryParse(quantityString, out quantityInt))
                {
                    Console.Write("This is not valid input. Please enter a integer value: ");
                    quantityString = Console.ReadLine();
                }

                string sql = $"INSERT INTO {tableName} (Date, Quantity) VALUES (@Date, @Quantity)";
                
                conn.Open();

                await using var cmd = new SQLiteCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Date", date);
                cmd.Parameters.AddWithValue("@Quantity", quantityInt);

                int rowsAffected =  cmd.ExecuteNonQuery();

                Console.WriteLine("");

                if (rowsAffected > 0)
                {
                    Console.WriteLine("Data has successfully been added to the database!\n");
                }
                else
                {
                    Console.WriteLine("Data was not successfully added to the database!\n");
                }
                
                conn.Close();

            } // InsertRecord

            async void ViewAllRecordsIncludingRowIds(SQLiteConnection conn)
            {
                string sql = $"SELECT rowID, * From {tableName}";
                conn.Open();
                await using var cmd = new SQLiteCommand(sql, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine("RowID: " + reader["rowID"] + " " + "Date: " + reader["Date"] + " " + "Quantity: " + reader["Quantity"]);
                }
                conn.Close();
                Console.WriteLine("");

            } // ViewAllRecordsIncludingRowIds

            async void DeleteRecord(SQLiteConnection conn)
            { 

                ViewAllRecordsIncludingRowIds(conn);
                Console.WriteLine("");
                conn.Open();
                Console.WriteLine("Enter the RowID number of the row you want to delete: ");
                string? rowIDOfRecordToDeleteString = Console.ReadLine();
                int rowIDOfRecordToDeleteInt;
                while (!int.TryParse(rowIDOfRecordToDeleteString, out rowIDOfRecordToDeleteInt))
                {
                    Console.Write("This is not valid input. Please enter a integer value: ");
                    rowIDOfRecordToDeleteString = Console.ReadLine();
                }

                string sqlDelete = $"DELETE FROM {tableName} WHERE RowID = (@RowID)";

                await using var deleteCmd = new SQLiteCommand(sqlDelete, conn);
                deleteCmd.Parameters.AddWithValue("@RowID", rowIDOfRecordToDeleteInt);

                int rowsAffected = deleteCmd.ExecuteNonQuery();

                Console.WriteLine("");

                if (rowsAffected > 0)
                {
                    Console.WriteLine("Record has successfully been deleted from the database!\n");
                }
                else
                {
                    Console.WriteLine("Record was not successfully deleted from database!\n");
                }

                conn.Close();
                Console.WriteLine("");

            } // DeleteRecord

            async void UpdateRecord(SQLiteConnection conn)
            {
                ViewAllRecordsIncludingRowIds(conn);
                Console.WriteLine("");
                Console.WriteLine("Enter the RowId of the record you want to update: ");
                string? rowIdOfRecordToBeUpdatedString = Console.ReadLine();
                int rowIdOfRecordToBeUpdatedInt;
                while (!int.TryParse(rowIdOfRecordToBeUpdatedString, out rowIdOfRecordToBeUpdatedInt))
                {
                    Console.Write("This is not valid input. Please enter a integer value: ");
                    rowIdOfRecordToBeUpdatedString = Console.ReadLine();
                }
                Console.WriteLine("Enter the new Date: ");
                string? newDate = Console.ReadLine();
                Console.WriteLine("Enter the new Quantity: ");
                string? newQuantity = Console.ReadLine();
                string sqlUpdate = $"UPDATE {tableName} SET Date=(@Date), Quantity=(@Quantity) WHERE RowID = (@RowID)";

                await using var updateCmd = new SQLiteCommand(sqlUpdate, conn);
                updateCmd.Parameters.AddWithValue("@RowID", rowIdOfRecordToBeUpdatedInt);
                updateCmd.Parameters.AddWithValue("@Date", newDate);
                updateCmd.Parameters.AddWithValue("@Quantity", newQuantity);

                conn.Open();

                int rowsAffected = updateCmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Console.WriteLine("Record was succefully modified!\n");
                }
                else
                {
                    Console.WriteLine("Record was not successfully modified!\n");
                }

                conn.Close();
                Console.WriteLine("");
            }

            void CloseApplication(SQLiteConnection conn)
            {
                endApp = true;
            }// CloseApplication
        }
    }
}
