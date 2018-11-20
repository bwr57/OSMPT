using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Demo.WindowsForms;
using GMap.NET;

namespace Demo.WindowsPresentation.Cash
{

    public class CashHelper : IDisposable
    {
        private static CashHelper _Cash = new CashHelper();

        public static CashHelper Cash
        {
            get { return CashHelper._Cash; }
        } 

           public const string BASE_NAME = "Routes.db3";
        private SQLiteConnection _Connection;

        public static SQLiteConnection TrasferToMemory(string fileName)
        {
            SQLiteConnection destination = new SQLiteConnection("Data Source=:memory:");
            destination.Open();

            if (File.Exists(fileName))
            {
                // copy db file to memory
                using (SQLiteConnection source = new SQLiteConnection("Data Source=" + fileName))
                {
                    source.Open();
                    source.BackupDatabase(destination, "main", "main", -1, null, 0);
                    source.Close();
                }
            }
            else
                CreateDB(destination);
            return destination;
        }

        public static void TrasferToFile(string fileName, SQLiteConnection connection)
        {
            if (!File.Exists(fileName))
            {
                SQLiteConnection.CreateFile(fileName);
            }

            using (SQLiteConnection destination = new SQLiteConnection("Data Source=" + fileName))
            {
                destination.Open();

                // save memory db to file
                connection.BackupDatabase(destination, "main", "main", -1, null, 0);
                destination.Close();
            }
        }
        

        public CashHelper()
        {
            _Connection = TrasferToMemory(BASE_NAME);
                /*
            if (File.Exists(BASE_NAME))
            {
                SQLiteFactory factory = new SQLiteFactory(); //)DbProviderFactories.GetFactory("System.Data.SQLite");
                _Connection = (SQLiteConnection)factory.CreateConnection();
                _Connection.ConnectionString = "Data Source = " + BASE_NAME;
                _Connection.Open();
            }
            else
            {
                _Connection = CreateDB();
            }
                 * */
        }

        public static void CreateDB(SQLiteConnection connection)
        {
        	try
        	{
                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                    CREATE TABLE [Squares] (
                        NorthLatitude double NOT NULL
                        , WestLongtitude double NOT NULL
                        , SouthLatitude double NOT NULL
                        , EastLongtitude double NOT NULL
                        , time NOT NULL
                    );
                    CREATE TABLE [Nodes] (
                        [id] long NOT NULL
                        , isWay bit
                        , latitude double NOT NULL
                        , longtitude double NOT NULL
                        , Name varchar(50) NULL
                        , Type int NOT NULL DEFAULT 0
                        , PRIMARY KEY ([id], isWay)
                     );
                    CREATE TABLE [Ways] (
                        [id] long PRIMARY KEY NOT NULL
                        , Name varchar(50) NULL
                        , Type int NOT NULL DEFAULT 0
                     );
                    CREATE TABLE [Places] (
                        id long PRIMARY KEY NOT NULL
                        , Name varchar(50) NOT NULL
                        , CountryName varchar(50) NOT NULL
                        , RegionName varchar(50) NOT NULL
                        , DistrictName varchar(50) NOT NULL
                        , Type tinyint NOT NULL
                        , time datetime NULL
                    );
                    CREATE TABLE [Routes] (
                        [id] long PRIMARY KEY NOT NULL
                        , ref varchar(50) NOT NULL
                        , name varchar(50) NULL
                        , Operator varchar(50) NULL
                        , Network varchar(50) NULL
                        , Type int NOT NULL DEFAULT 0
                        , masterRouteID long DEFAULT 0
                     );
                    CREATE TABLE [MasterRoutes] (
                        [id] long PRIMARY KEY NOT NULL
                        , ref varchar(50) NOT NULL
                        , name varchar(50) NULL
                        , Operator varchar(50) NULL
                        , Network varchar(50) NULL
                        , Type int NOT NULL DEFAULT 0
                     );
                    CREATE TABLE [Platforms] (
                        [id] long PRIMARY KEY NOT NULL
                        , ref varchar(50) NOT NULL
                        , name varchar(50) NULL
                        , Operator varchar(50) NULL
                        , Network varchar(50) NULL
                        , Type int NOT NULL DEFAULT 0
                     );
                    CREATE TABLE [RouteWays] (
                        [RouteId] long NOT NULL,
                        [WayId] long NOT NULL
                        , [Index] long NOT NULL
                     );
                    CREATE TABLE [WaysNodes] (
                        [WayId] long NOT NULL
                        , [NodeId] long NOT NULL
                        , [Index] long NOT NULL
                     );
                    
                    CREATE TABLE [PlatformNodes] (
                        [PlatformId] long NOT NULL,
                        [NodeId] long NOT NULL
                     );
                    CREATE TABLE [RouteNodes] (
                        [RouteId] long NOT NULL
                        , [NodeId] long NOT NULL
                        , [Index] long NOT NULL
                        , [IsWay] bit Not Null
                        , Type shortint default 0
                    );
                    CREATE TABLE [PlaceInnerNodes] (
                        [PlaceId] long NOT NULL
                        , [NodeId] long NOT NULL
                        , [IsWay] bit Not Null
                    );
";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();

                }
        	}
        	catch(Exception ex)
        	{
        		MessageBox.Show(ex.Message);
        	}
}

        public static bool TestExists(SQLiteConnection connection, string tableName, string id, bool isWay)
        {
        	string code = String.Format("SELECT COUNT(id) FROM {0} WHERE id={1} And IsWay = {2}", tableName, id, isWay ? 1 : 0);
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText = code;
                command.CommandType = CommandType.Text;
                return (long)command.ExecuteScalar() > 0;
            }
        }

        public static bool TestExists(SQLiteConnection connection, string tableName, string id)
        {
        	string code = String.Format("SELECT COUNT(id) FROM {0} WHERE id={1}", tableName, id);
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText = code;
                command.CommandType = CommandType.Text;
                return (long)command.ExecuteScalar() > 0;
            }
        }

        public static SQLiteConnection CreateDB()
        {

            SQLiteConnection.CreateFile(BASE_NAME);

            SQLiteFactory factory = new SQLiteFactory(); //)DbProviderFactories.GetFactory("System.Data.SQLite");
            SQLiteConnection connection = (SQLiteConnection)factory.CreateConnection();
            {
                connection.ConnectionString = "Data Source = " + BASE_NAME;
                connection.Open();
                CreateDB(connection);
                //connection.Close();
                return connection;
            }
        }

        public void RegisterSquare(double w, double n, double e, double s)
        {
            string code = String.Format(new System.Globalization.CultureInfo("en-US"), "INSERT INTO Squares (NorthLatitude, WestLongtitude, SouthLatitude, EastLongtitude , time) VALUES ( {0}, {1}, {2}, {3}, '{4}')", n, w, s, e, DateTime.Now);
            using (SQLiteCommand command = new SQLiteCommand(_Connection))
            {
                command.CommandText = code;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
        }

        public bool TestSquare(double latitude, double longtitude)
        {
            using (SQLiteCommand command = new SQLiteCommand(_Connection))
            {
                string code = String.Format(new System.Globalization.CultureInfo("en-US"), "SELECT COUNT(*) FROM Squares WHERE NorthLatitude >= {0} AND SouthLatitude <= {0} AND WestLongtitude <= {1} AND EastLongtitude >= {1} ", latitude, longtitude);
                command.CommandText = code;
                command.CommandType = CommandType.Text;
                return (Int64)command.ExecuteScalar() > 0;
            }
        }

        public static string GetStringOrNull(string str)
        {
            return str == null ? "NULL" : String.Concat("'", str, "'");
        }

        public void SaveNode(XmlNode node)
        {
        	SaveNode(node, false, 0, 0);
        }
        
        public void SaveNode(XmlNode node, bool isWay, double latitude, double longtitude)
        {
            if (TestExists(_Connection, "Nodes", node.Attributes["id"].Value, isWay))
                return;
            XmlNodeList tags = node.SelectNodes("tag");
            string name = null;
            int type = 0;
            foreach (XmlNode tagNode in tags)
            {
                string key = tagNode.Attributes["k"].Value;
                string value = tagNode.Attributes["v"].Value;
                if (key == "name")
                {
                    name = value;
                }
                else
                    if (key == "highway")
                    {
                        if (value == "bus_stop")
                        {
                            type += (int)StopTypes.BusStop;
                        }
                    }
                if (key == "railway")
                {
                    if (value == "tram_stop")
                    {
                        type += (int)StopTypes.TramStop;
                    }
                    else
                        if (value == "station")
                        {
                            type += (int)StopTypes.RailroadStation;
                        }
                        else
                            if (value == "halt")
                            {
                                type += (int)StopTypes.RailroadHalt;
                            }
                }
                else
                    if (key == "public_transport")
                    {
                        if (value == "stop_position")
                        {
                            type += (int)StopTypes.StopPoint;
                        }
                        else
                            if (value == "platform")
                            {
                                type += (int)StopTypes.Platform;
                            }
                        else
                        	if(value == "station")
                        {
                        	type += (int)StopTypes.BusStation;
                        }
                    }
                else
                        if (key == "amenity")
                        {
                            if (value == "bus_station")
                            {
                                type += (int)StopTypes.BusStation;
                            }
                        }
                else
                	if(String.Equals(key, "transport", StringComparison.CurrentCultureIgnoreCase)
                	   && String.Equals(value, "subway", StringComparison.CurrentCultureIgnoreCase))
                {
                	type += (int)StopTypes.SubwayStation;
                }
                	   
            }
            CultureInfo usCulture = new System.Globalization.CultureInfo("en-US");
            string code = String.Format(usCulture, "INSERT INTO Nodes (id, isWay, latitude, longtitude, name, type) Values ({0}, {1}, {2}, {3}, {4}, {5})", node.Attributes["id"].Value, isWay ? 1 : 0, isWay ? latitude.ToString(usCulture) : node.Attributes["lat"].Value, isWay ? longtitude.ToString(usCulture) : node.Attributes["lon"].Value, GetStringOrNull(name), type);
            using (SQLiteCommand command = new SQLiteCommand(_Connection))
            {
                command.CommandText = code;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
            }
        }

        public void SaveWay(XmlNode node)
        {
            string id = node.Attributes["id"].Value;
            if (TestExists(_Connection, "Ways", id))
                return;
            XmlNodeList tags = node.SelectNodes("tag");
            string name = null;
            int type = 0;
            bool toNode = false;
            foreach (XmlNode tagNode in tags)
            {
                string key = tagNode.Attributes["k"].Value;
                string value = tagNode.Attributes["v"].Value;
                if (key == "name")
                {
                    name = value;
                }
                else
                    if (key == "highway")
                    {
                        if (value == "primary")
                        {
                            type += 2;
                        }
                        else
                            if (value == "trunc")
                            {
                                type += 1;
                            }
                            else
                            {
                                type += 4;
                            }
                    }
                else
                if (key == "railway")
                {
                    if (value == "tram")
                    {
                        type += 8;
                    }
                    else
                    	if(value == "station" || value == "halt")
                    {
                    	toNode = true;
                    }
                    else
                        type += 16;
                }
                if (key == "place")
                {
                    type += 64;
                    SavePlace(id, tags, DateTime.Now);
                }
                    
            }
            string code = String.Format(new System.Globalization.CultureInfo("en-US"), "INSERT INTO Ways (id, name, type) Values ({0}, {1}, {2})", id, GetStringOrNull(name), type);
            using (SQLiteCommand command = new SQLiteCommand(_Connection))
            {
                command.CommandText = code;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
                XmlNodeList ndNodes = node.SelectNodes("nd");
                uint index = 0;
                foreach (XmlNode ndNode in ndNodes)
                {
                    code = String.Format(new System.Globalization.CultureInfo("en-US"), "INSERT INTO WaysNodes ([WayId], [NodeId], [Index]) Values ({0}, {1}, {2})", id, ndNode.Attributes["ref"].Value, index++);
                    command.CommandText = code;
                    command.ExecuteNonQuery();
                }
                /*
                if(type == 0 || toNode)
                {
                	code = String.Format("SELECT SUM(Nodes.latitude) as latitude, SUM(Nodes.longtitude) as longtitude, COUNT(Nodes.id) from Nodes as Nodes join WayNodes as WayNodes on WayNodes.NodeId = Nodes.Id AND Node.IsWay = 0 WHERE WayNodes.WayId = {0}", id);
                	command.CommandText = code;
                	SQLiteDataReader reader = command.ExecuteReader();
                	while(reader.Read())
                	{
                		long count = (long)reader[2];
                		if(count > 0)
                		{
                			SaveNode(node, true, (double)reader[0] / count, (double)reader[1] / count);
                		}
                	}
                	reader.Close();
                	reader.Dispose();
                }
                */
            }
        }

        public void SaveRoute(XmlNode node)
        {
            string id = node.Attributes["id"].Value;
            if (TestExists(_Connection, "Routes", id))
                return;
            XmlNodeList tags = node.SelectNodes("tag");
            string name = null;
            int type = 0;
            string refId = "";
            string operatorName = null;
            string networkName = null;
            string from = null;
            string to = null;
            foreach (XmlNode tagNode in tags)
            {
                string key = tagNode.Attributes["k"].Value;
                string value = tagNode.Attributes["v"].Value;
                if (key == "name")
                {
                    name = value;
                }
                else
                    if (key == "ref")
                    {
                        refId = value;
                    }
                    else
                        if(key == "operator")
                            operatorName = value;
                        else
                        if(key == "network")
                            networkName = value;
                        else
                        if(key == "from")
                            from = value;
                        else
                        if(key == "to")
                            to = value;
                        else
                            if(key == "route")
                            {
                        if (value == "tram")
                        {
                            type += (int)OSMOTRouteTypes.Tramway;
                        }
                        else
                            if (value == "trolleybus")
                            {
                                type += (int)OSMOTRouteTypes.Trolleybus;
                            }
                            else
                                if (value == "bus")
                                {
                                    type += (int)OSMOTRouteTypes.Bus;
                                }
                                else
                                    if (value == "shared_taxi")
                                    {
                                        type += (int)OSMOTRouteTypes.SharedTaxi;
                                    }
                                    else
                                        if (value == "train")
                                        {
                                            type += (int)OSMOTRouteTypes.Train;
                                        }
                                    else
                                        if (value == "light_rail")
                                        {
                                            type += (int)OSMOTRouteTypes.Light_rail;
                                        }
                                    else
                                        if (value == "subway")
                                        {
                                            type += (int)OSMOTRouteTypes.Subway;
                                        }
                                    else 
                                    	if(value == "ferry")
                                    		type += (int)OSMOTRouteTypes.Ferry;
                    }
                    else
                    	if(key=="service")
                    {
                    	if(value=="local")
                    		type += (int)OSMOTRouteTypes.Local;
                    	else
                    		if(value=="regional")
                    			type += (int)OSMOTRouteTypes.Regional;
                    	else
                    			if(value == "long_distance")
                    				type += (int)OSMOTRouteTypes.LongDistance;
                    	
                    }
            }
            string code = String.Format(new System.Globalization.CultureInfo("en-US"), "INSERT INTO Routes (id, ref, name, operator, network, type) Values ({0}, {1}, {2}, {3}, {4}, {5})", id, GetStringOrNull(refId), GetStringOrNull(name), GetStringOrNull(operatorName), GetStringOrNull(networkName), type);
            using (SQLiteCommand command = new SQLiteCommand(_Connection))
            {
                command.CommandText = code;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
                XmlNodeList memberNodes = node.SelectNodes("member");
                uint wayIndex = 0;
                uint nodeIndex = 0;
                foreach (XmlNode memberNode in memberNodes)
                {
                	code = null;
                    string refName = memberNode.Attributes["type"].Value;
                    string refTargetId = memberNode.Attributes["ref"].Value;
                    string refRole = memberNode.Attributes["role"].Value;
                    int refType = 0;
                    if (refRole == "" || String.Equals(refRole, "forward") || String.Equals(refRole, "backward"))
	                {
    	                if (refName == "way")
                        {
							code = String.Format(new System.Globalization.CultureInfo("en-US"), "INSERT INTO RouteWays ([RouteId], [WayId], [Index]) Values ({0}, {1}, {2})", id, refTargetId, wayIndex++);
    	                }
                    }
                    else
                    {
                            if (refRole == "stop")
                            {
                                refType += 1;
                            }
                            else
                                if (refRole == "forward_stop" || refRole == "forward:stop")
                                {
                                    refType += 5;
                                }
                                else
                                    if (refRole == "backward_stop" || refRole == "backward:stop")
                                    {
                                        refType += 9;
                                    }
                                    else
                                        if (refRole == "stop_entry_only")
                                        {
                                            refType += 17;
                                        }
                                        else
                                            if (refRole == "stop_exit_only")
                                            {
                                                refType += 33;
                                            }
                                            else
                                                if (refRole == "platform")
                                                {
                                                    refType += 2;
                                                }
                                                else
                                                    if (refRole == "platform_entry_only")
                                                    {
                                                        refType += 18;
                                                    }
                                                    else
                                                        if (refRole == "platform_exit_only")
                                                        {
                                                            refType += 34;
                                                        }
                        if(refType > 0)
                        {
                        	bool isWay = refName == "way";
		                    if (isWay)
		                    {
		                    	refType += 64;
		                    }
		                    code = String.Format("INSERT INTO RouteNodes ([RouteId], [NodeId], IsWay, Type, [Index]) Values ({0}, {1}, {2}, {3}, {4})", id, refTargetId, isWay ? 1 : 0,  refType, nodeIndex++);
                        }
                    }
                    if(code != null)
                    {
                    	command.CommandText = code;
                    	command.ExecuteNonQuery();
                    }
                }


            }
        }

        public virtual void SavePlace(string id, XmlNodeList tags, DateTime loadTime)
        {
            string name = "";
            byte type = 0;
            string countryName = "";
            string districtName = "";
            string regionName = "";

            foreach (XmlNode tagNode in tags)
            {
                string key = tagNode.Attributes["k"].Value;
                string value = tagNode.Attributes["v"].Value;
                switch (key)
                {
                    case "id":
                        {
                            id = value;
                            break;
                        }
                    case "name":
                        {
                            name = value;
                            break;
                        }
                    case "place":
                        {
                            switch (value)
                            {
                                case "city":
                                    {
                                        type = 1;
                                        break;
                                    }
                                case "town":
                                    {
                                        type = 2;
                                        break;
                                    }
                                case "village":
                                    {
                                        type = 3;
                                        break;
                                    }
                                case "hamlet":
                                    {
                                        type = 4;
                                        break;
                                    }
                            }
                            break;
                        }
                    case "addr:country":
                        {
                            countryName = value;
                            break;
                        }
                    case "addr:district":
                        {
                            districtName = value;
                            break;
                        }
                    case "addr:region":
                        {
                            regionName = value;
                            break;
                        }
                }
            }
            if (!String.IsNullOrEmpty(name))
            {
                string code = String.Format(new System.Globalization.CultureInfo("en-US"), "SELECT time FROM Places WHERE id={0}", id);
                using (SQLiteCommand command = new SQLiteCommand(_Connection))
                {
                    command.CommandText = code;
                    command.CommandType = CommandType.Text;
                    SQLiteDataReader reader = command.ExecuteReader();
                    DateTime prevTime = new DateTime(1900, 1, 1);
                    bool isExists = false;
                    if (reader.Read())
                    {
//                        prevTime = (DateTime)reader["Time"];
                        isExists = true;
                        
                    }
                    reader.Close();
                    reader.Dispose();
                    if(!isExists || prevTime.Year == 1900)// (prevTime < loadTime)
                    {
                        code = String.Format(new System.Globalization.CultureInfo("en-US"), "INSERT INTO Places (id, name, type, CountryName, RegionName, DistrictName, time) VALUES ({0}, '{1}', {2}, '{3}', '{4}', '{5}', '{6}')", id, name, type, countryName, districtName, regionName, loadTime);
                        command.CommandText = code;
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public OSMStopPoint ReadPoint(Int64 id, bool isWay)
        {
            OSMStopPoint point = null;
            using (SQLiteCommand command = new SQLiteCommand(_Connection))
            {
            	string code = String.Format(new System.Globalization.CultureInfo("en-US"), "SELECT id, latitude, longtitude, name, type FROM Nodes WHERE id={0} and isway={1}", id, isWay ? 1 : 0);
                command.CommandText = code;
                command.CommandType = CommandType.Text;
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    point = LoadPointValues(point, reader);
                }
            }
            return point;
        }

        private static OSMStopPoint LoadPointValues(OSMStopPoint point, SQLiteDataReader reader)
        {
            if(point == null)
                point = new OSMStopPoint();
            point.ID = (Int64)reader["id"];
//            point.IsWay = (byte)reader["IsWay"] == 1;
            point.Latitude = (double)reader["Latitude"];
            point.Longtitude = (double)reader["Longtitude"];
            point.Name = reader["name"] is DBNull ? "" : (string)reader["name"];
            point.StopType = (int)reader["type"];
            return point;
        }

        public OSMOTRoute ReadRoute(Int64 id)
        {
            OSMOTRoute route = null;
            using (SQLiteCommand command = new SQLiteCommand(_Connection))
            {
                string code = String.Format(new System.Globalization.CultureInfo("en-US"), "SELECT id, ref, name, operator, network, type FROM Routes WHERE id={0}", id);
                command.CommandText = code;
                command.CommandType = CommandType.Text;
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    route = new OSMOTRoute();
                    route.Ref = (string)reader["ref"];
                    route.id = (Int64)reader["id"];
                    route.Name = reader["name"] is DBNull ? "" : (string)reader["name"];
                                        route.RouteType = reader["type"] is DBNull ? 0 : (OSMOTRouteTypes)reader["type"];
                                        route.Operator = reader["operator"] is DBNull ? "" : (string)reader["operator"];
                                        route.Network = reader["network"] is DBNull ? "" : (string)reader["network"];
//                                        route.From = reader["from"] is DBNull ? "" : (string)reader["from"];
//                                        route.To = reader["to"] is DBNull ? "" : (string)reader["to"];
                                        Debug.Print(route.Ref + " " + route.Name);
                }
                reader.Close();
                if (route != null)
                {

                    SQLiteCommand command1 = new SQLiteCommand(_Connection);
                    code = String.Format(new System.Globalization.CultureInfo("en-US"), "SELECT NodeId, type FROM RouteNodes WHERE RouteId={0}", route.id);
                    command1.CommandText = code;
                    command1.CommandType = CommandType.Text;
                    reader = command1.ExecuteReader();
                    while (reader.Read())
                    {
                        	OSMStopPoint point = ReadPoint((Int64)reader["NodeId"], (Convert.ToInt16(reader["type"]) & 64) != 0);
                        	if(point != null)
                        	{
                        	OSMOTRouteStop stop = new OSMOTRouteStop();
                        	stop.StopPoints.Add(point.ID, point);
                        	stop.Name = point.Name;
                        	if (point != null)
                            	route.Stops.Add(stop);
                        	}
                    }
                }
            }
            return route;
        }

        public IList<OSMStopPoint> ReadStops(RectLatLng viewRect)
        {
            IList<OSMStopPoint> stopPoints = new List<OSMStopPoint>();
            using (SQLiteCommand command = new SQLiteCommand(_Connection))
            {

                string code = String.Format(new System.Globalization.CultureInfo("en-US"), "SELECT id, latitude, longtitude, name, type FROM Nodes WHERE Type>0 AND (latitude >= {0} ) AND (latitude <= {1}) AND (longtitude >= {2})  AND (longtitude <= {3})", viewRect.Lat - viewRect.HeightLat, (viewRect.Lat), (viewRect.Lng), (viewRect.Lng + viewRect.WidthLng));
                command.CommandText = code;
                command.CommandType = CommandType.Text;
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    OSMStopPoint stop = LoadPointValues(null, reader);
                    SelectRoutesForStop(stop.ID, stop.Routes);
                    stopPoints.Add(stop);
                }
                reader.Close();
            }
            return stopPoints;
        }

        public void TestNodeInPlace()
        {
            string code = String.Format(new System.Globalization.CultureInfo("en-US"), "SELECT id FROM Places");
        //    List<uint> 
            using (SQLiteCommand command = new SQLiteCommand(_Connection))
            {
                command.CommandText = code;
                command.CommandType = CommandType.Text;
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        uint id = (uint)reader["id"];
                        code = String.Format(new System.Globalization.CultureInfo("en-US"), "SELECT [Node].id, [Node].[latitude], [Node].[longtitude] FROM WaysNodes JOIN Nodes ON WaysNodes.NodeID = Nodes.ID AND Nodes.IsWay = 0 WHERE WaysNodes.WayID = {0} ORDER BY WaysNodes.Index", id);
                    }

                }
            }

        }

        public void Dispose()
        {
            if (_Connection != null)
            {
                if (_Connection.State == ConnectionState.Open)
                    _Connection.Close();
                _Connection.Dispose();
                _Connection = null;
            }
        }

        public void TransferToDisk()
        {
            TrasferToFile(BASE_NAME, _Connection);
        }

        public IList<OSMOTRouteLite> SelectRoutesForStop(Int64 stopId, IList<OSMOTRouteLite> routes)
        {
            if(routes == null)
                routes = new List<OSMOTRouteLite>();
            using (SQLiteCommand command = new SQLiteCommand(_Connection))
            {
                string code = String.Format(new System.Globalization.CultureInfo("en-US"), "SELECT r.id, r.ref, r.name, r.operator, r.network, r.type as routeType, rn.type as stopType  FROM Routes as r JOIN RouteNodes as rn ON r.id = rn.RouteId  WHERE rn.NodeId={0} AND rn.type > 0", stopId);
                command.CommandText = code;
                command.CommandType = CommandType.Text;
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                	if((from route in routes where route.id == (Int64)reader["id"] select route).Count() == 0)
                	{
                    OSMOTRouteLite route = new OSMOTRouteLite();
                    route.Ref = reader["ref"] is DBNull ? "" : (string)reader["ref"];
                    route.id = (Int64)reader["id"];
                    route.RouteType = (OSMOTRouteTypes)reader["routeType"];
                    route.Name = reader["name"] is DBNull ? "" : (string)reader["name"];
                    routes.Add(route);
                	}
                }
                reader.Close();
            }
            return routes;
        }
    }
}
/*
                    CREATE TABLE [Ways] (
                     [id] long PRIMARY KEY NOT NULL,
                        , Name varchar(50) NULL
                        , Type NOT NULL DEFAULT 0
                     );
                    CREATE TABLE [Routes] (
                     [id] long PRIMARY KEY NOT NULL,
                     ref varchar(50) NOT NULL,
                      name varchar(50) NULL
                     , Operator varchar(50) NULL
                     , Network varchar(50) NULL
                      , Type NOT NULL DEFAULT 0
                     , masterRouteID long DEFAULT 0
                     );
                    CREATE TABLE [MasterRoutes] (
                     [id] long PRIMARY KEY NOT NULL,
                     ref varchar(50) NOT NULL,
                     , Operator varchar(50) NULL
                      name varchar(50) NULL
                     , Operator varchar(50) NULL
                     , Network varchar(50) NULL
                      , Type NOT NULL DEFAULT 0
                     );
                    CREATE TABLE [Platforms] (
                     [id] long PRIMARY KEY NOT NULL,
                     ref varchar(50) NOT NULL,
                      name varchar(50) NULL
                     , Operator varchar(50) NULL
                     , Network varchar(50) NULL
                      , Type NOT NULL DEFAULT 0
                     );
                    CREATE TABLE [RouteWays] (
                     [RouteId] long NOT NULL,
                     [WayId] long NOT NULL
                     );
                    CREATE TABLE [WaysNodes] (
                     [WayId] long NOT NULL,
                     [NodeId] long NOT NULL
                     );
                    
                    CREATE TABLE [PlatformNodes] (
                     [PlatformId] long NOT NULL,
                     [NodeId] long NOT NULL
                     );
                    CREATE TABLE [RouteNodes] (
                     [RouteId] long NOT NULL,
                     [NodeId] long NOT NULL
                     );
*/