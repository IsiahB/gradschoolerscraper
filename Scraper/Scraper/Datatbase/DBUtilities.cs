using System;
using Scraper;
using Scraper.Models;
using Scraper.Database;
using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Collections.Generic;

namespace DBUtilities
{
    /// <summary>
    /// This class is used to query and update the database
    /// </summary>
    public class DBUtilities
    {

        private static volatile DBUtilities instance; // Singleton 
        private static object mutex = new Object(); // lock object

        private MySqlConnection conn;
        private DBConnector connector;

        

        /// <summary>
        /// constructor -- connects to & opens the database
        /// </summary>
        private DBUtilities()
        {
            connector = new DBConnector();
            connector.openDB();
            conn = connector.getConn();
        }//end constructor

        /// <summary>
        /// check if instance of class is null, if its null,
        /// create new instance.
        /// returns instance of class.
        /// </summary>
        public static DBUtilities Instance
        {
            get
            {
                //get lock & create instance of DBUtilies
                if (instance == null)
                {
                    lock (mutex) instance = new DBUtilities();
                }
                return instance;
            }//end get
        }//end Instance

        /// <summary>
        /// 
        /// </summary>
        public List<University> getFavUniversities(String email)
        {
            //insert into database
            string sql = null;
            //create array for faved universities
            List<University> favUnis = new List<University>();

            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Close(); //just incase it is broken
                    conn.Open(); //open the database connection
                }//if

                if (conn != null)
                {
                    MySqlDataReader reader = null;
                    sql = "SELECT gradschoolname, fundingtype, city, state, environment " + 
                    "FROM FavUniversities, University " +
                    "WHERE accountemail='" + email + "' and gradschoolname=name";
                    MySqlCommand command = new MySqlCommand(sql, conn);
                    using (reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            University p = new University
                            {
                                name = (string)reader["gradschoolname"],
                                fundingtype = (string)reader["fundingtype"],
                                city = (string)reader["city"],
                                state = (string)reader["state"],
                                environment = (string)reader["environment"]
                            };
                            favUnis.Add(p);
                        }
                    }
                }
            }
            catch (MySqlException)
            {
                Console.Write("Invalid parameters for insertion" + "\n");
            }
            finally
            {
                conn.Close();
            }
            return favUnis;
            
        }//end addFavUniversity

        public Boolean addUniversity()
        {
            //read the file and return the dictionary
            Dictionary<String, List<String>> dict = Scrape.UniversitiesByState();


            var cmd2 = new MySqlCommand();
            List<String> sqlList = new List<String>(); //in order to execute multiple sql inserts

            //insert into database
            String sqlP = "INSERT INTO University VALUES (@name, @fundingtype, @city, @state, @environment, @uniURL, @address)";
            MySqlCommand cmd = new MySqlCommand(sqlP, conn);
            string elD = "";
            string itemD = "";
            cmd.Parameters.AddWithValue("@name", elD);
            cmd.Parameters.AddWithValue("@fundingtype", "Private");
            cmd.Parameters.AddWithValue("@city", "n/a");
            cmd.Parameters.AddWithValue("@state", itemD);
            cmd.Parameters.AddWithValue("@environment", "n/a");
            cmd.Parameters.AddWithValue("@uniURL", "");
            cmd.Parameters.AddWithValue("@address", "");

            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Close(); //just incase it is broken
                    conn.Open(); //open the database connection 
                }//if

                if (conn != null)
                {
                    cmd.Prepare();
                    foreach (var item in dict)
                    {
                        //Debug.Write(item.Key + " " );
                        foreach (var el in item.Value)
                        {
                            //Debug.Write(el + ", ");
                            if (true)
                            {
                                cmd.Parameters[0].Value =  el;
                                cmd.Parameters[3].Value = item.Key;

                                //put try
                                //catch, print error message and it will continue
                                try
                                {
                                    cmd.ExecuteNonQuery();
                                }
                                catch(MySqlException e)
                                {
                                    //catching exception - is there a better way?
                                    Debug.WriteLine("Problems with sql stuff, this is the problem:  " + e);
                                }
                              
                            }
                        }
                        Debug.WriteLine("");
                    }

                    return true;
                }

            }
            catch (MySqlException e)
            {
                Debug.WriteLine("Invalid parameters for insertion    : " + e);
            }
            finally
            {
                conn.Close();
            }
            return false;
        }//end addUniversities

        public Boolean UniversityPopulated()
        {
            if(displayUniversities().Count == 0) { return false; }
            return true;

        }




        // - getUniversityList
        public List<University> displayUniversities()
        {
            List<University> unis = new List<University>();

            string sql = null;
            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Close(); //just incase it is broken
                    conn.Open(); //open the database connection
                }//if

                if(conn != null)
                {
                    MySqlDataReader reader = null;
                    sql = "SELECT name, fundingtype, city, state, environment " +
                        "FROM University ";
                    MySqlCommand command = new MySqlCommand(sql, conn);
                    using (reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            University u = new University
                            {
                                name = (string)reader["name"],
                                fundingtype = (string)reader["fundingtype"],
                                city = (string)reader["city"],
                                state = (string)reader["state"],
                                environment = (string)reader["environment"]
                            };

                            unis.Add(u);  
                        }
                    }
                }
            }
            catch (MySqlException e)
            {
                Console.Write("This is the problem: " + e + "\n");
            }
            finally
            {
                conn.Close();
            }
            return unis;
        }

        public Program[] displayPrograms(Program[] pros)
        {
            string sql = null;
            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Close(); //just incase it is broken
                    conn.Open(); //open the database connection
                }//if

                if(conn != null)
                {
                    MySqlDataReader reader = null;
                    sql = "SELECT * " +
                        "FROM Program ";
                    MySqlCommand command = new MySqlCommand(sql, conn);
                    using (reader = command.ExecuteReader())
                    {
                        int i = 0;
                        while (reader.Read() && i < pros.Length)
                        {
                            Program p = new Program
                            {
                                programname = (string)reader["programname"],
                                degree = (string)reader["degree"],
                                acceptancerate = (float)reader["acceptancerate"],
                                instatetuition = (float)reader["instatetuition"],
                                outstatetuition = (float)reader["outstatetuition"],
                                fee = (float)reader["fee"],
                                deadline = (string)reader["deadline"],
                                schoolname = (string)reader["schoolname"]
                            };
                            pros[i] = p;
                            i++;
                        }
                    }
                }
            }
            catch (MySqlException)
            {
                Console.Write("Could not display data properly" + "\n");
            }
            finally
            {
                conn.Close();
            }
            return pros;

        }

        public int tableSizes(string tablename)
        {
            string sql = null;
            int size = 0;

            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Close(); //just incase it is broken
                    conn.Open(); //open the database connection
                }//if

                if(conn != null)
                {
                    sql = "SELECT COUNT(*) " +
                    "FROM " + tablename;

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        size = Convert.ToInt32(cmd.ExecuteScalar());
                        //ExecuteScalar returns the first column of the first row in the table
                    }
                }  
            }
            catch (MySqlException e)
            {
                Console.Write(e);
            }
            finally
            {
                conn.Close();
            }

            return size;
        }

        //method to login
        public bool loginChecker(String email, String password)
        {
            bool valid = false;

            //convert plain text to hash
            //compute hash from the bytes of text
            //get hash result after compute it
            //change it into 2 hexadecimal digits
            //for each byte
            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Close(); //just incase it is broken
                    conn.Open(); //open the database connection
                }//if

                if (conn != null)
                {
                    MySqlCommand command = new MySqlCommand();
                    command.Connection = conn;
                    command.CommandText = "select password from Account where email=@email;";
                    command.Prepare();
                    command.Parameters.AddWithValue("@email", email);
                    MySqlDataReader reader = command.ExecuteReader();
                    if (reader.Read() && reader["password"].Equals(password))
                        valid = true;
                }//if
            }
            catch (Exception e)
            {
                Debug.WriteLine("SQL EXCEPTION: " + e);
            }
            finally
            {
                conn.Close();
            }

            return valid;
        }//loginChecker

        public String getAccFirstName(String accEmail)
        {
            Debug.WriteLine("passed in accEmail: " + accEmail);
            string sql = null;
            String accFirstName = "";

            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Close(); //just incase it is broken
                    conn.Open(); //open the database connection
                }//if

                if (conn != null)
                {
                    sql = "SELECT firstName " +
                    "FROM Account " + 
                    "WHERE email='" + accEmail + "'";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        accFirstName = Convert.ToString(cmd.ExecuteScalar());
                        //ExecuteScalar returns the first column of the first row in the table
                    }
                }
            }
            catch (MySqlException e)
            {
                Console.Write(e);
            }
            finally
            {
                conn.Close();
            }

            return accFirstName;
        }

        // Add a request made by the user
        public Boolean addURequest(University uR, String email, string comment){
            //insert into database
            string sqlP = null;

            try
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    conn.Close(); //just incase it is broken
                    conn.Open(); //open the database connection
                }//if

                if (conn != null)
                {
                    sqlP = "INSERT INTO URequest " +
                    "VALUES ('NULL', '" + email + "', '" + comment +
                                           "', '" + uR.name + "', '" + uR.fundingtype + "', '" +
                                           uR.city + "', '" + uR.state + "', '" + uR.environment + "')";

                    MySqlCommand cmd2 = new MySqlCommand(sqlP, conn);

                    cmd2.ExecuteNonQuery(); //to add the profile

                    return true;
                }

            }
            catch (MySqlException)
            {
                Console.Write("Invalid parameters for insertion" + "\n");
            }
            finally
            {
                conn.Close();
            }
            return false;
            
        }//end addURequest

        /// <summary>
        /// close database
        /// </summary>
        public void shutDown()
        {
            conn.Close();
        }//end shutDown

    }//end DBUtilites Class
}//namspace
