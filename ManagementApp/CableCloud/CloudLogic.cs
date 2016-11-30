﻿using ClientNode;
using ManagementApp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace CableCloud
{
    class CloudLogic
    {
        private DataTable table;
        private Dictionary<int, NodeConnectionThread> portToThreadMap;
        public CloudLogic()
        {
            table = new DataTable("Connections");
            table.Columns.Add("fromPort", typeof(int)).AllowDBNull = false;
            table.Columns.Add("virtualFromPort", typeof(int)).AllowDBNull = false;
            table.Columns.Add("toPort", typeof(int)).AllowDBNull = false;
            table.Columns.Add("virtualToPort", typeof(int)).AllowDBNull = false;
        }

        public void connectToWindowApplication(int port)
        {
            TcpClient connection = new TcpClient("localhost", port);
            Thread clientThread = new Thread(new ParameterizedThreadStart(windowConnectionThread));
            clientThread.Start(connection);
        }

        private void windowConnectionThread(Object connection)
        {
            TcpClient clienttmp = (TcpClient)connection;
            BinaryReader reader = new BinaryReader(clienttmp.GetStream());
            string received_data = reader.ReadString();
            JMessage received_object = JMessage.Deserialize(received_data);
            if (received_object.Type == typeof(NodeConnection))
            {
                NodeConnection received_connection = received_object.Value.ToObject<NodeConnection>();
                connectToNodes( received_connection.LocalPortFrom, received_connection.VirtualPortFrom,
                   received_connection.LocalPortTo, received_connection.VirtualPortTo);
            }
            else
            {
                Console.WriteLine("\n Connection with Window Application: WRONG DATA");
            }

            reader.Close();
        }

        public void connectToNodes(int fromPort, int virtualFromPort,
                                    int toPort, int virtualToPort)
        {
            TcpClient connectionFrom = new TcpClient("localhost", fromPort);
            NodeConnectionThread fromArg = new NodeConnectionThread(ref connectionFrom, virtualToPort, table);

            TcpClient connectionTo = new TcpClient("localhost", toPort);
            NodeConnectionThread toArg = new NodeConnectionThread(ref connectionFrom, virtualFromPort, table);

            addNewCable(fromPort, virtualFromPort, toPort, virtualToPort);
        }

        

        private void  addNewCable(int fromPort, int virtualFromPort, int toPort, int virtualToPort)
        {
            table.Rows.Add(fromPort, virtualFromPort, toPort, virtualToPort);
        }

        private void deleteCable(int fromPort, int virtualFromPort, int  toPort, int  virtualToPort)
        {
            for (int i = table.Rows.Count - 1; i >= 0; i--)
            {
                DataRow dr = table.Rows[i];
                if (dr["fromPort"].Equals(fromPort) && dr["virtualFromPort"].Equals(virtualFromPort)
                    && dr["toPort"].Equals(toPort) && dr["virtualToPort"].Equals(virtualToPort))
                    table.Rows.Remove(dr);
            }
        }

        private class NodeConnectionThread
        {
            private Thread thread;

            private TcpClient connection;
            private int virtualToPort;
            private DataTable table;

            public NodeConnectionThread(ref TcpClient connection, int virtualToPort, DataTable table)
            {
                this.virtualToPort = virtualToPort;
                this.connection = connection;
                this.table = table;

                thread = new Thread(nodeConnectionThread);
                thread.Start();
            }

            private void nodeConnectionThread()
            {
                BinaryReader reader = new BinaryReader(connection.GetStream());
                string received_data = reader.ReadString();
                JMessage received_object = JMessage.Deserialize(received_data);
                // TODO Poki co string , potem bedzie tu klasa kontenera , przekazywanie dalej wiadomosci
                if (received_object.Type == typeof(int))
                {
                    int virtualFromPort = received_object.Value.ToObject<int>();
                    var fromPort = ((IPEndPoint)connection.Client.RemoteEndPoint).Port;
                    int virtualToPort;
                    int toPort;
                    for (int i = table.Rows.Count - 1; i >= 0; i--)
                    {
                        DataRow dr = table.Rows[i];
                        if (dr["fromPort"].Equals(fromPort) && dr["virtualFromPort"].Equals(virtualFromPort))
                        {
                            virtualToPort = (int) dr["toPort"];
                            toPort = (int) dr["virtualToPort"];
                        }
                    }
                }
                else
                {
                    Console.WriteLine("\n Connection with Node: WRONG DATA");
                }

                reader.Close();
            }
        
        }
    }
}