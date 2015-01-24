using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Oracle.DataAccess.Client;
using System.Diagnostics;

namespace Mars_Life
{
    [Serializable]
    public struct SaveData
    {
        public TimeSpan time;
        public float mapID;
        public int cutsceneID;
        public Vector2 playerPosition;
        public Vector2 mapPosition;
        public SaveData(TimeSpan _timeInGame, float _mapID, int _cutsceneID, Vector2 _playerPosition, Vector2 _mapPosition)
        {
            time = _timeInGame;
            mapID = _mapID;
            cutsceneID = _cutsceneID;
            playerPosition = _playerPosition;
            mapPosition = _mapPosition;
        }
    }

    public struct LoginInfo
    {
        public string username;
        public string password;
        public LoginInfo(string user, string pass)
        {
            username = user;
            password = pass;
        }
    }

    public class SaveGame
    {
        public SaveData save;
        public LoginInfo loginInfo;

        // My Documents/SavedGames/Mars Life/Saves/Player1/
        private string saveFolder = "Saves";
        private string fileExtension = ".mlssav";
        private string headerFileName = "save_";

        #region File Save Declarations
        private enum State
        {
            Innactive, RequestDevice,
            GetDevice, GetContainer,
            SaveLoad
        }

        private enum Action
        {
            None,
            Save,
            Load
        }
        private State state;
        private Action action;

        IAsyncResult result = null;
        StorageDevice device = null;
        StorageContainer container = null;
        #endregion
        #region Database Save Declarations
        private const String strCONNECTIONSTRING = "DATA SOURCE=dilbert.humber.ca:1521/grok;PASSWORD=xbox3605;PERSIST SECURITY INFO=True;USER ID=gdfn0014";
        private OracleConnection oracleConnection = new OracleConnection(strCONNECTIONSTRING);
        private OracleCommand oracleCommand;    
        private OracleDataReader oracleDataReader;

        private decimal playerID;
        private bool connectedToDatabase;
        public bool loggedInToDatabase;
        #endregion

        public SaveGame()
        {
            state = State.Innactive;
            action = Action.None;
            save = new SaveData();
            loginInfo = new LoginInfo("test","test");
            connectedToDatabase = false;
            loggedInToDatabase = false;
        }
        /// <summary>
        ///  Takes the data based on the information from LevelManager and outputs it directly to a save file sitting in memory
        /// </summary>
        private void CreateSave()
        {
            save = new SaveData(Game1.levelManager.RunningTime,
                                             Game1.levelManager.CurrentLevel.MapID, 
                                             Game1.levelManager.CurrentLevel.CutsceneID,
                                             Game1.levelManager.CurrentLevel.actor.Position,
                                             new Vector2(Game1.levelManager.CurrentLevel.mapView.X, Game1.levelManager.CurrentLevel.mapView.Y));
        }

        #region Database
        /// <summary>
        /// used to connect to the database
        /// </summary>
        public void ConnectToDatabase()
        {
            if (!connectedToDatabase)
            {
                try
                {
                    //create command and attach connection to it
                    oracleCommand = new OracleCommand();
                    oracleCommand.Connection = oracleConnection;
                    Debug.WriteLine("Connection Set..");
                    oracleConnection.Open(); //Open the connection
                    Debug.WriteLine("Connected");

                    connectedToDatabase = true;
                }
                catch (Exception objException)
                {
                    throw (objException);
                }
            }
        }

        /// <summary>
        /// Used to safely disconnect from the database
        /// </summary>
        public void DisconnectFromDatabase()
        {
            if (connectedToDatabase)
            {
                Debug.WriteLine("Disconnected from Database");
                oracleConnection.Close();
                connectedToDatabase = false;
            }
        }

        /// <summary>
        ///  Read from Database will read in the code of the Database and load any data. When Called, the save is loaded into memory waiting to be assigned.
        /// </summary>
        /// <returns>A boolean. True means that a player exists. False means that a Player has been auto-generated</returns>
        public bool ReadFromDatabase()
        { //string PlayerName, string Password)
            if (loginInfo.username != "" && loginInfo.password != "")
            {
                bool found = false;
                try
                {
                    Debug.WriteLine("Beginning Database Read");

                    String strSQL = "SELECT * From MarsLife";
                    //set command to sql statement
                    oracleCommand.CommandText = strSQL;

                    //read data from database into dataReader
                    oracleDataReader = oracleCommand.ExecuteReader();
                    //loop through dataReader rows and put data from each
                    //row into listbox
                    while (oracleDataReader.Read())
                    {
                        if (oracleDataReader["Username"].ToString() == loginInfo.username && oracleDataReader["Password"].ToString() == loginInfo.password)
                        {
                            Debug.WriteLine("Player Found: " + oracleDataReader["Username"]);
                            found = true;
                            loggedInToDatabase = true;

                            playerID = Convert.ToDecimal(oracleDataReader["PlayerID"]);
                            double mapID = Convert.ToDouble(oracleDataReader["MapID"]);
                            int cutsceneID = Convert.ToInt32(oracleDataReader["CutsceneID"]);
                            int mapX = Convert.ToInt32(oracleDataReader["MapX"]);
                            int mapY = Convert.ToInt32(oracleDataReader["MapY"]);
                            int playerX = Convert.ToInt32(oracleDataReader["PlayerX"]);
                            int playerY = Convert.ToInt32(oracleDataReader["PlayerY"]);

                            save = new SaveData(new TimeSpan(), (float)mapID, cutsceneID, new Vector2((float)playerX, (float)playerY), new Vector2((float)mapX, (float)mapY));
                            return true;
                            //break;
                        }
                    }
                    //close reader and connection when done
                    //oracleDataReader.Close();
                    //oracleConnection.Close();
                    if (!found)
                    {
                        Debug.WriteLine("Player Not Found, Creating Player: ");
                        CreatePlayer();
                        return false;
                        //loginInfo = new LoginInfo("", "");
                        //Exception objException = new Exception();
                        //throw objException;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.StackTrace.ToString());
                    throw e;
                }
            }
            return false;
        }

        /// <summary>
        /// The game will take all of the data from the LevelManager and outputs it directly to the database
        /// </summary>
        public void SaveToDatabase()
        {
            CreateSave();

            //if (!DoesPlayerExist()) { CreatePlayer(); }

            #region mapId
            string mapID = save.mapID.ToString();
            string strSQL = "UPDATE MarsLife SET MapID = " + mapID + " WHERE PlayerID = " + playerID;
            try
            {
                oracleCommand.CommandText = strSQL;
                Debug.WriteLine("Saving MapID: "+ mapID);
                oracleCommand.ExecuteNonQuery();
                Debug.WriteLine("Executed");
            }
            catch (Exception objException)
            {
                throw objException;
            }
            #endregion
            #region cutsceneID
            string cutsceneID = save.cutsceneID.ToString();
            strSQL = "UPDATE MarsLife SET CutsceneID = " + cutsceneID + " WHERE PlayerID = " + playerID;
            try
            {
                oracleCommand.CommandText = strSQL;
                Debug.WriteLine("Saving CutsceneID: "+cutsceneID);
                oracleCommand.ExecuteNonQuery();
                Debug.WriteLine("Executed");
            }
            catch (Exception objException)
            {
                throw objException;
            }
            #endregion
            #region mapX
            string mapX = save.mapPosition.X.ToString();
            strSQL = "UPDATE MarsLife SET MapX = " + mapX + " WHERE PlayerID = " + playerID;
            try
            {
                oracleCommand.CommandText = strSQL;
                Debug.WriteLine("Saving MapX: "+mapX);
                oracleCommand.ExecuteNonQuery();
                Debug.WriteLine("Executed");
            }
            catch (Exception objException)
            {
                throw objException;
            }
            #endregion
            #region mapY
            string mapY = save.mapPosition.Y.ToString();
            strSQL = "UPDATE MarsLife SET MapY = " + mapY + " WHERE PlayerID = " + playerID;
            try
            {
                oracleCommand.CommandText = strSQL;
                Debug.WriteLine("Saving MapY: "+mapY);
                oracleCommand.ExecuteNonQuery();
                Debug.WriteLine("Executed");
            }
            catch (Exception objException)
            {
                throw objException;
            }
            #endregion
            #region playerX
            string playerX = save.playerPosition.X.ToString();
            strSQL = "UPDATE MarsLife SET PlayerX = " + playerX + " WHERE PlayerID = " + playerID;
            try
            {
                oracleCommand.CommandText = strSQL;
                Debug.WriteLine("Saving PlayerX: "+playerX);
                oracleCommand.ExecuteNonQuery();
                Debug.WriteLine("Executed");
            }
            catch (Exception objException)
            {
                throw objException;
            }
            #endregion
            #region playerY
            string playerY = save.playerPosition.Y.ToString();
            strSQL = "UPDATE MarsLife SET PlayerY = " + playerY + " WHERE PlayerID = " + playerID;
            try
            {
                oracleCommand.CommandText = strSQL;
                Debug.WriteLine("Saving PlayerY: "+ playerY);
                oracleCommand.ExecuteNonQuery();
                Debug.WriteLine("Executed");
            }
            catch (Exception objException)
            {
                throw objException;
            }
            #endregion
        }

        /// <summary>
        /// Checks to see if the player exists
        /// </summary>
        /// <returns>if True, player Exists; if False, play does not exist</returns>
        public bool DoesPlayerExist()
        {
            string strSQL = "";
            #region Does Player Exist?
            try
            {
                Debug.WriteLine("Checking if Player Exists");
                strSQL = "SELECT * From MarsLife";
                //set command to sql statement
                oracleCommand.CommandText = strSQL;

                //read data from database into dataReader
                oracleDataReader = oracleCommand.ExecuteReader();
                //loop through dataReader rows and put data from each
                //row into listbox
                while (oracleDataReader.Read())
                { //oracleDataReader.
                    if (oracleDataReader["Username"].ToString() == loginInfo.username && oracleDataReader["Password"].ToString() == loginInfo.password)
                    {
                        Debug.WriteLine("Player Exists: "+oracleDataReader["Username"]);
                        loginInfo = new LoginInfo("", "");
                        return true;
                    }
                }
                return false;
            }
            catch (Exception objException)
            {
                //return false;
                Debug.WriteLine("Error!");
                throw objException;
            }
        }

        /// <summary>
        /// Will create a player based on the current information inside of LoginData
        /// </summary>
        /// <returns>False if the player already exists in the database, True if the player has been created</returns>
        public bool CreatePlayer()
        {//string PlayerName, string Password)
            string strSQL = "";
            #region Does Player Exist?
            try
            {
                Debug.WriteLine("Checking if Player Exists");
                strSQL = "SELECT * From MarsLife";
                //set command to sql statement
                oracleCommand.CommandText = strSQL;

                //read data from database into dataReader
                oracleDataReader = oracleCommand.ExecuteReader();
                //loop through dataReader rows and put data from each
                //row into listbox
                while (oracleDataReader.Read())
                { //oracleDataReader.
                    if (oracleDataReader["Username"].ToString() == loginInfo.username && oracleDataReader["Password"].ToString() == loginInfo.password)
                    {
                        Debug.WriteLine("Player Exists: "+oracleDataReader["Username"]);
                        loginInfo = new LoginInfo("", "");
                        return false;
                    }
                }
            }
            catch (Exception objException)
            {
                //return false;
                Debug.WriteLine("Error!");
                throw objException;
            }
            #endregion

            playerID = GetNextId();
            #region Temp Variables
            Level map = Game1.levelManager.CurrentLevel;
            string username = loginInfo.username;
            string password = loginInfo.password;
            float mapID = map.MapID;
            int cutsceneID = map.CutsceneID;
            int mapX = map.mapView.X;
            int mapY = map.mapView.Y;
            int playerX = (int)map.actor.Position.X;
            int playerY = (int)map.actor.Position.Y;
            #endregion

            #region Frusturating Old Code *Lights on Fire*
            //Debug.WriteLine("Creating SQL Command");
            //strSQL = "INSERT INTO MarsLife(playerID, username, password, mapID, cutsceneID, mapX, mapY, playerX, playerY)"             // "INSERT INTO MarsLife(PlayerID, Username, Password, MapID, CutsceneID, MapX, MapY, PlayerX, PlayerY)"
            //                + " VALUES(" + playerID + ", '" + username + "', " + "'" + password + "', " + "'" + mapID + "', " + "'" + cutsceneID
            //                + "', " + mapX + "', " + mapY + "', " + playerX + "', " + playerY + ")";

            //strSQL =
            //                 "INSERT INTO Player(PlayerID, PlayerName, Password, LoggedIn, RoomID, Health, Points)"
            //                 + " VALUES(" + PlayerID + ", '" + PlayerName + "', " + "'" + Password
            //                 + "', " + "'" + LoggedIn + "', " + "0, 0, " + Points + ")";
            //strSQL = "INSERT INTO DodgeBladeTable(PlayerID, PlayerName, Password, Points)"
            //     + " VALUES(" + playerID + ", '" + loginInfo.username + "', " + "'" + loginInfo.password + "', " + 4000 + ")";
            #endregion
            try
            {
                Debug.WriteLine("Assigning Command");
                
                //oracleCommand.CommandText = strSQL;

                oracleCommand.CommandText = "Insert into MarsLife VALUES (:pId, '" + username + "', '" + password + "', '" + mapID + "', '" + cutsceneID + "', " + mapX + ", " + mapY + ", " + playerX + ", " + playerY+ ")"; // '0, 100, 100, 100, 100)";//, :user, :pass, :mId, :cId, :mX, :mY, :pX, :pY)";
                #region Parameters
                //
                // Primary Key
                //    
                //Debug.WriteLine("Adding Primary Key");
                oracleCommand.Parameters.Add(new OracleParameter("pId",
                                           OracleDbType.Int32,
                                           playerID,
                                           System.Data.ParameterDirection.Input));
                #region Burn everything in this region
                //
                // VarChar2s
                //
                //Debug.WriteLine("Adding VarChars");
                //oracleCommand.Parameters.Add(new OracleParameter("user",
                //                           OracleDbType.Varchar2,
                //                           "hi",
                //                           System.Data.ParameterDirection.Input));
                //oracleCommand.Parameters.Add(new OracleParameter("pass",
                //                           OracleDbType.Varchar2,
                //                           password,
                //                           System.Data.ParameterDirection.Input));
                //oracleCommand.Parameters.Add(new OracleParameter("mId",
                //                           OracleDbType.Varchar2,
                //                           mapID,
                //                           System.Data.ParameterDirection.Input));
                //oracleCommand.Parameters.Add(new OracleParameter("cId",
                //                           OracleDbType.Varchar2,
                //                           cutsceneID,
                //                           System.Data.ParameterDirection.Input));
                ////
                //// Numbers
                ////
                ////Debug.WriteLine("Adding Decimals");
                //oracleCommand.Parameters.Add(new OracleParameter("mX",
                //                           OracleDbType.Int32,
                //                           mapX,
                //                           System.Data.ParameterDirection.Input));
                //oracleCommand.Parameters.Add(new OracleParameter("mY",
                //                           OracleDbType.Int32,
                //                           mapY,
                //                           System.Data.ParameterDirection.Input));
                //oracleCommand.Parameters.Add(new OracleParameter("pX",
                //                           OracleDbType.Int32,
                //                           playerX,
                //                           System.Data.ParameterDirection.Input));
                //oracleCommand.Parameters.Add(new OracleParameter("pY",
                //                           OracleDbType.Int32,
                //                           playerY,
                //                           System.Data.ParameterDirection.Input));
                #endregion
                #endregion

                Debug.WriteLine("Attemping to Execute...");
                oracleCommand.ExecuteNonQuery();
                Debug.Write("Player Created: "+loginInfo.username);
                return true;
            }
            catch (Exception objException)
            {
                throw objException;
                //return false;
            }
        }

        /// <summary>
        ///  Gets the next ID in the database
        /// </summary>
        /// <returns>the Primary Key of the next availiable slot</returns>
        private short GetNextId()
        {
            //find current highest id and add one to it for a new player's id
            //string strSQL = "SELECT Max(PlayerID) from DodgeBladeTable";
            string strSQL = "SELECT Max(PlayerID) from MarsLife";
            
            try
            {
                Debug.WriteLine("Getting Next ID");
                oracleCommand.CommandText = strSQL;
                short newID = 0;
                newID = Convert.ToInt16(oracleCommand.ExecuteScalar());
                Debug.WriteLine("Next ID Gotten: "+ (newID+1).ToString()+"\n");
                return (short)(newID + 1);
            }
            catch (Exception objException)
            {
                throw objException;
            }
        }
        #endregion
        #endregion

        #region File
        /// <summary>
        /// Takes the LevelManager data and throws it out to a file.
        /// </summary>
        /// <param name="saveSlot">The number of the save that you wish to make</param>
        /// <returns>true for complete and false for not complete</returns>
        public bool SaveToFile(int saveSlot)
        {
            switch (state)
            {
                case State.Innactive:
                    Debug.WriteLine(state);
                    state = State.RequestDevice;
                    break;
                case State.RequestDevice:
                    {
                        Debug.WriteLine(state);
                        // STEP 2.a: Request the device
                        result = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);
                        state = State.GetDevice;
                    }
                    break;
                case State.GetDevice:
                    {
                        Debug.WriteLine(state);
                        // If the device is ready
                        if (result.IsCompleted)
                        {
                            // STEP 2.b: Recieve the device
                            device = StorageDevice.EndShowSelector(result);
                            // STEP 3.a: Request the container
                            result.AsyncWaitHandle.Close();
                            result = null;
                            result = device.BeginOpenContainer(saveFolder, null, null);
                            state = State.GetContainer;
                        }
                    }
                    break;
                case State.GetContainer:
                    {
                        Debug.WriteLine(state);
                        // If the container is ready
                        if (result.IsCompleted)
                        {
                            Debug.WriteLine("Assigning Container");
                            // STEP 3.b: Recieve the container
                            container = device.EndOpenContainer(result);
                            Debug.WriteLine(container.ToString());
                            result.AsyncWaitHandle.Close();
                            state = State.SaveLoad;
                        }
                    }
                    break;
                case State.SaveLoad:
                    {
                        Debug.WriteLine(state);
                        Debug.WriteLine(container.ToString());
                        CreateSave();

                        string filename = headerFileName + saveSlot + fileExtension;
                        // Check to see whether
                        // the file exists.
                        if (container.FileExists(filename))
                        {
                            // Delete it so that we
                            // can create one fresh.
                            container.DeleteFile(filename);
                        }
                        // Create the file.
                        Stream stream = container.CreateFile(filename);
                        // Convert the object to XML data
                        // and put it in the stream.
                        XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
                        serializer.Serialize(stream, save);
                        // Close the file.
                        stream.Close();
                        // Dispose the container, to
                        // commit changes.
                        container.Dispose();
                        state = State.Innactive;
                        Debug.WriteLine("File Saved");
                        return true;
                        //action = Action.None;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Loads in a file and assigns the data to save.saveData
        /// </summary>
        /// <param name="saveSlot">The slot of the save you wish to open</param>
        /// <returns>-1 for file not found, 0 for not complete and 1 for complete</returns>
        public int LoadFile(int saveSlot)
        {
            switch (state)
            {
                case State.Innactive:
                    Debug.WriteLine(state);
                    state = State.RequestDevice;
                    break;
                case State.RequestDevice:
                    {
                        Debug.WriteLine(state);
                        // STEP 2.a: Request the device
                        result = StorageDevice.BeginShowSelector(PlayerIndex.One, null, null);
                        state = State.GetDevice;
                    }
                    break;
                case State.GetDevice:
                    {
                        Debug.WriteLine(state);
                        // If the device is ready
                        if (result.IsCompleted)
                        {
                            // STEP 2.b: Recieve the device
                            device = StorageDevice.EndShowSelector(result);
                            // STEP 3.a: Request the container
                            result.AsyncWaitHandle.Close();
                            result = null;
                            result = device.BeginOpenContainer(saveFolder, null, null);
                            state = State.GetContainer;
                        }
                    }
                    break;
                case State.GetContainer:
                    {
                        Debug.WriteLine(state);
                        // If the container is ready
                        if (result.IsCompleted)
                        {
                            // STEP 3.b: Recieve the container
                            container = device.EndOpenContainer(result);
                            result.AsyncWaitHandle.Close();
                            state = State.SaveLoad;
                        }
                    }
                    break;
                case State.SaveLoad:
                    {
                        Debug.WriteLine(state);
                        string filename = headerFileName + saveSlot + fileExtension;
                        // Check to see whether the save exists.
                        if (!container.FileExists(filename))
                        {
                            Debug.WriteLine("File Not Found");
                            // If not, dispose of the
                            // container and return.
                            container.Dispose();
                            return -1;
                        }
                        // Create the file.
                        Stream stream = container.OpenFile(filename, FileMode.Open);
                        // Convert the object to XML
                        // data and put it in the stream.
                        XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
                        save = (SaveData)serializer.Deserialize(stream);
                        // Close the file.
                        stream.Close();
                        // Dispose the container, to commit changes.
                        container.Dispose();
                        state = State.Innactive;
                        action = Action.None;
                        Debug.WriteLine("File Loaded");
                        return 1;
                    }
                    break;
            }
            return 0;
        }
        #endregion
    }
}