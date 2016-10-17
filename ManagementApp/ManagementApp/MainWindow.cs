﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ManagementApp
{
    public partial class MainWindow : Form
    {
        private NodeType nType;
        private const int GAP = 10;
        private Bitmap containerPoints;
        private List<ContainerElement> elements = new List<ContainerElement>();
        private int clientNodesNumber;
        private int networkNodesNumber;
        private ContainerElement nodeFrom;
        private ContainerElement nodeFromTo;
        private bool isDrawing = false;
        private Point domainFrom;
        private Graphics myGraphics;

        public MainWindow()
        {
            InitializeComponent();
            clientNodesNumber = 0;
            networkNodesNumber = 0;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Cross;
            nType = NodeType.CLIENT_NODE;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Cross;
            nType = NodeType.NETWORK_NODE;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Cross;
            nType = NodeType.CONNECTION;
        }

        private void domain_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Cross;
            nType = NodeType.DOMAIN;
        }


        private void putToGrid(ref int x, ref int y)
        {
            x = GAP * (int)Math.Round((double)x / GAP);
            y = GAP * (int)Math.Round((double)y / GAP);
        }

        private void container_MouseClick(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            putToGrid(ref x, ref y);
            switch (nType)
            {
                case NodeType.CLIENT_NODE:
                    elements.Add(new ClientNode(x, y, "CN" + clientNodesNumber++));
                    textConsole.AppendText("Client Node added at: " + x + "," + y);
                    textConsole.AppendText(Environment.NewLine);
                    break;
                case NodeType.NETWORK_NODE:
                    elements.Add(new NetNode(x, y, "NN" + networkNodesNumber++));
                    textConsole.AppendText("Network Node added at: " + x + "," + y);
                    textConsole.AppendText(Environment.NewLine);
                    break;
            }
            container.Refresh();
        }

        private ContainerElement getNodeFrom(int x, int y)
        {
            return elements.Where(i => i.ContainedPoints.ElementAtOrDefault(0).X == x &&
               i.ContainedPoints.ElementAtOrDefault(0).Y == y).FirstOrDefault(); 
        }

        private void container_Paint_1(object sender, PaintEventArgs e)
        {
            Graphics panel = e.Graphics;
   
            Rectangle rect;
            foreach (var elem in elements.AsParallel().Where(i => i is NetNode))
            {
                rect = new Rectangle(elem.ContainedPoints.ElementAt(0).X - 5,
                    elem.ContainedPoints.ElementAt(0).Y - 5, 11, 11);
                panel.FillEllipse(Brushes.Bisque, rect);
                panel.DrawEllipse(Pens.Black, rect);
                panel.DrawString(elem.Name, new Font("Arial", 5), Brushes.Black, new Point(elem.ContainedPoints.ElementAt(0).X + 3,
                    elem.ContainedPoints.ElementAt(0).Y + 3));
            }
            foreach (var elem in elements.AsParallel().Where(i => i is ClientNode))
            {
                rect = new Rectangle(elem.ContainedPoints.ElementAt(0).X - 5,
                   elem.ContainedPoints.ElementAt(0).Y - 5, 11, 11);
                panel.FillEllipse(Brushes.AliceBlue, rect);
                panel.DrawEllipse(Pens.Black, rect);
                panel.DrawString(elem.Name, new Font("Arial", 5), Brushes.Black, new Point(elem.ContainedPoints.ElementAt(0).X + 3,
                    elem.ContainedPoints.ElementAt(0).Y + 3));
            }
            foreach (var elem in elements.AsParallel().Where(i => i is NodeConnection))
            {
                // Create pen.
                Pen blackPen = new Pen(Color.Black, 2);
                // Draw line to screen.
                Point from = elem.ContainedPoints.ElementAt(0);
                Point to = elem.ContainedPoints.ElementAt(1);
                panel.DrawLine(blackPen, from, to);
                panel.DrawString(elem.Name, new Font("Arial", 5), Brushes.Black, new Point((from.X + to.X)/2 + 3,
                   (from.Y + to.Y) / 2 + 3));
            }
            foreach (var elem in elements.AsParallel().Where(i => i is Domain))
            {
                Domain tmp = (Domain)elem;
                Point from = tmp.PointFrom;
                rect = new Rectangle(from.X, from.Y, tmp.Width, tmp.Height);
                panel.DrawRectangle(new Pen(Color.PaleVioletRed, 3),rect);
            }



                container.BackgroundImage = containerPoints;
        }

        private void container_MouseDown(object sender, MouseEventArgs e)
        {
            int x = e.X;
            int y = e.Y;
            putToGrid(ref x, ref y);
            if (nType == NodeType.CONNECTION)
            {
                nodeFrom = getNodeFrom(x, y);
                isDrawing = true;
            }
            if (nType == NodeType.DOMAIN)
            {
                domainFrom = new Point(x, y);
                isDrawing = true;
            }
                
           
        }

        private void container_MouseUp(object sender, MouseEventArgs e)
        {

            int x = e.X;
            int y = e.Y;
            isDrawing = false;
            putToGrid(ref x, ref y);
            if (nType == NodeType.CONNECTION && nodeFrom != null)
            {
                ContainerElement nodeTo = getNodeFrom(x, y);
                if(nodeTo != null)
                    bind(nodeFrom, nodeTo);
                else if(nodeFromTo != null)
                    bind(nodeFrom, nodeFromTo);
            }
            if(nType == NodeType.DOMAIN && domainFrom != null)
            {
                Point domainTo = new Point(x, y);
                elements.Add(new Domain(domainFrom,domainTo));
                textConsole.AppendText("Domain added");
                textConsole.AppendText(Environment.NewLine);

                if(e.X - domainFrom.X < 0 || e.Y - domainFrom.Y < 0)
                {
                    textConsole.AppendText("There are problems with negative sizes.");
                    textConsole.AppendText(Environment.NewLine);
                }
            }
            container.Refresh();
        }

        private void bind(ContainerElement nodeFrom, ContainerElement nodeTo)
        {
            elements.Add(new NodeConnection(nodeFrom, nodeTo,nodeFrom.Name + "-" + nodeTo.Name));
            textConsole.AppendText("Connection  added");
            textConsole.AppendText(Environment.NewLine);
        }

        private void Connection_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                this.Cursor = Cursors.Arrow;
                nType = NodeType.NOTHING;
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            containerPoints = new Bitmap(container.ClientSize.Width, container.ClientSize.Height);
            for (int x = 0; x < container.ClientSize.Width;
                x += GAP)
            {
                for (int y = 0; y < container.ClientSize.Height;
                    y += GAP)
                {
                    containerPoints.SetPixel(x, y, Color.Black);
                }
            }
            myGraphics = container.CreateGraphics();
        }

        private void container_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing && nodeFrom != null && nType == NodeType.CONNECTION)
            {
                container.Refresh();
                Pen blackPen = new Pen(Color.Black, 3);
                Point s = new Point(nodeFrom.ContainedPoints.ElementAt(0).X, nodeFrom.ContainedPoints.ElementAt(0).Y);
                Point p = new Point(e.X, e.Y);

                double distance = Double.PositiveInfinity;
                double d = Double.PositiveInfinity;

                foreach (var elem in elements.AsParallel().Where(i => i is NetNode))
                {
                    if (elem != nodeFrom)
                    {
                        if (elem != nodeFrom)
                        {
                            d = Math.Round(Math.Sqrt(Math.Pow(elem.ContainedPoints.ElementAt(0).X - e.X, 2) + Math.Pow(elem.ContainedPoints.ElementAt(0).Y - e.Y, 2)), 2);
                            if (d < distance)
                            {
                                distance = d;
                                nodeFromTo = elem;
                            }
                            d = Double.PositiveInfinity;
                        }
                    }
                }
                foreach (var elem in elements.AsParallel().Where(i => i is ClientNode))
                {
                    if (elem != nodeFrom)
                    {
                        d = Math.Round(Math.Sqrt(Math.Pow(elem.ContainedPoints.ElementAt(0).X - e.X, 2) + Math.Pow(elem.ContainedPoints.ElementAt(0).Y - e.Y, 2)), 2);
                        if (d < distance)
                        {
                            distance = d;
                            nodeFromTo = elem;
                        }
                        d = Double.PositiveInfinity;
                    }
                }

                if (distance > 100)
                    myGraphics.DrawLine(blackPen, s, p);
                else
                {
                    Point end = new Point(nodeFromTo.ContainedPoints.ElementAt(0).X, nodeFromTo.ContainedPoints.ElementAt(0).Y);
                    myGraphics.DrawLine(blackPen, s, end);
                }
                System.Threading.Thread.Sleep(10);
            } else if (isDrawing && nType == NodeType.DOMAIN)
            {
                container.Refresh();
                Pen pr = new Pen(Color.PaleVioletRed, 3);
                myGraphics.DrawRectangle(pr, domainFrom.X, domainFrom.Y, e.X - domainFrom.X, e.Y - domainFrom.Y);
                System.Threading.Thread.Sleep(10);
            }
        }
    }

    enum NodeType
    {
        CLIENT_NODE,
        NETWORK_NODE,
        CONNECTION,
        DOMAIN,
        NOTHING
    }
}
