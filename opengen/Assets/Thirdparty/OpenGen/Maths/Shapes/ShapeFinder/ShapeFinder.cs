using System;
using System.Collections.Generic;
using opengen;
using opengen.maths;
using opengen.maths.shapes;
using opengen.types;
using UnityEngine;
using Clockwise = opengen.maths.shapes.Clockwise;



namespace opengen.maths.shapes.shapefinder
{
    public class ShapeFinder
    {
        private enum States
        {
            OuterPlot,
            Start,
            FindStart,
            FindPlots,
        }

        private States state = States.Start;
        private ShapeData data;
        private List<Node[]> thisNextStart = new();//list of start points to find a plot from
        private bool complete = false;

        public bool Complete { get { return complete; } }

        public void Execute(ShapeData input)
        {
            data = input;
            state = States.Start;
            complete = false;

            switch (state)
            {
                case States.Start:
                    FindOuterNodes();
                    state = States.OuterPlot;
                    break;

                case States.OuterPlot:
                    FindOuterNodes();
                    state = States.FindStart;
                    break;

                case States.FindStart:
                    FindStartPoint();
                    state = States.FindPlots;
                    break;

                case States.FindPlots:
                    FindPlot();
                    break;
            }
        }

        private void FindOuterNodes()
        {
            int nodeCount = data.nodes.Count;
            Vector2Fixed[] points = new Vector2Fixed[nodeCount];
            for(int p = 0; p < nodeCount; p++)
            {
                Node node = data.nodes[p];
                node.OrderConnections();//build connection node order lists
                points[p] = node.position;

//                Vector3 np0 = node.position.vector3XZ;
//                for(int cp = 0; cp < node.connections.Count; cp++)
//                {
//                    Vector3 cp0 = node.connections[cp].position.vector3XZ;
//                    Debug.DrawLine(np0 + Vector3.up*0.02f, cp0 + Vector3.up * 0.01f, Color.red);
//                }

            }
            Vector2Fixed[] hull = Convex.MakeHull(points);

            for(int i = 0; i < hull.Length; i++)
            {
                int ib = i < hull.Length - 1 ? i + 1 : 0;
                Vector3 p0 = hull[i].vector3XZ;
                Vector3 p1 = hull[ib].vector3XZ;
//                Debug.DrawLine(p0,p1+Vector3.up*0.01f,Color.magenta,30);
//                Debug.DrawLine(p1,p1+Vector3.up*0.01f,Color.magenta,30);
            }

            if(!Clockwise.Check(hull))
            {
                Array.Reverse(hull);
            }

            List<NodeLine> hullLines = new();

//            Node outerNodeCurrent = null;
//            Node outerNodeNext = null;
            for (int hp = 0; hp < hull.Length; hp++)
            {
                Vector2Fixed hp0 = hull[hp];
                bool found = false;
                for (int np = 0; np < nodeCount; np++)
                {
                    Node node = data.nodes[np];
                    Vector2Fixed np0 = node.position;
                    if (hp0 == np0)
                    {
                        int hpb = hp < hull.Length - 1 ? hp + 1 : 0;
                        
                        Vector2Fixed hp1 = hull[hpb];
                        Vector2 hullLineP0 = hp0.vector2;
                        Vector2 hullLineP1 = hp1.vector2;
                        for (int cp = 0; cp < node.connections.Count; cp++)
                        {
                            Node conn = node.connections[cp];
                            Vector2 connectionPoint = conn.position.vector2;

                            if(Segments.PointOnSegment(connectionPoint, hullLineP0, hullLineP1))
                            {
                                NodeLine hullLine = new(node, conn);
//                                Debug.DrawLine(node.position.vector3XZ, conn.position.vector3XZ, Color.cyan, 30);
                                hullLines.Add(hullLine);
                                found = true;
//                                outerNodeCurrent = node;
//                                outerNodeNext = conn;
                                break;
                            }
                        }
                    }

                    if(found)
                    {
                        break;
                    }

                    //                    if (outerNodeCurrent != null)
//                        break;
                }
//                if (outerNodeCurrent != null)
//                    break;
            }
            
            if(hullLines.Count == 0)
            {
//                Debug.Log("FindOuterNodes - NOT FOUND");
                data.Clear();
//                Debug.Log(nodeCount);
//                Debug.Log("hull: "+hull.Length);
//                Debug.Break();
                return;
            }

            int ita = 0;
            Node outerNodeCurrent = null;
            Node outerNodeNext = null;
            NodePlot plot = null;
            while (hullLines.Count > 0 || outerNodeCurrent != null)//find the second furthest outside node connecting to the furthest. From here we can walk the outside of the link nodes
            {
                if(outerNodeCurrent == null)
                {
//                    Debug.Log("FindOuterNodes new plot shape (hull lines)"+hullLines.Count);
                    outerNodeCurrent = hullLines[0].a;
                    outerNodeNext = hullLines[0].b;
                    hullLines.RemoveAt(0);
                    plot = new NodePlot();
                }

                plot.nodes.Add(outerNodeCurrent);
//                outerNodeCurrent.Plot(outerNodeNext);//mark these outer nodes as plotted so they can't be used
                Node next = outerNodeNext.LastNode(outerNodeCurrent);
                if(outerNodeNext.connections.Count < 2 || next == null)//this shape has lead to a dead end line
                {
//                    Debug.Log("FindOuterNodes ended on single connection issue");
//                    Debug.Log("outerNodeNext.connections "+outerNodeNext.connections.Count);
//                    Debug.Log("next is null: "+(next==null));
//                    Debug.Log("plot "+plot.nodes.Count);
                    
                    Vector3 np0 = outerNodeNext.position.vector3XZ;
                    for(int cp = 0; cp < outerNodeNext.connections.Count; cp++)
                    {
                        Vector3 cp0 = outerNodeNext.connections[cp].position.vector3XZ;
                        //Debug.DrawLine(np0 + Vector3.up * 0.5f, cp0 + Vector3.up * 0.5f, Color.red);
                    }

                    //reset the process - find new hull point to start from
                    outerNodeCurrent = null;
                    outerNodeNext = null;
                    plot = null;
                    continue;
                }

                NodeLine newLine = new(outerNodeNext, next);
                
//                                Debug.DrawLine(outerNodeNext.position.vector3XZ, next.position.vector3XZ, Color.cyan, 30);
                for(int l = 0; l < hullLines.Count; l++)
                {
                    if(hullLines[l] == newLine)
                    {
                        hullLines.RemoveAt(l);
                        break;
                    }
                }

                if(next == plot[0])
                {
                    if(plot.nodes.Count > 2)
                    {
                        plot.nodes.Add(outerNodeNext);
                        data.outPlots.Add(plot);
//                        plot.DrawDebugOutline(Color.yellow);
                    }
//                    Debug.Log("FindOuterNodes plot size "+plot.nodes.Count);
//                    Debug.Log("hull line count "+hullLines.Count);
                    outerNodeCurrent = null;
                    outerNodeNext = null;
                    plot = null;
                    continue;
                }
                outerNodeCurrent = outerNodeNext;
                outerNodeNext = next;

                ita++;
                if(ita > 7500)
                {
//                    Debug.Log("FindOuterNodes Outer plot >> too many iterations");
                    data.Clear();
                    return;
                }
            }

            if(plot != null)
            {
//                Debug.Log("Hull line drop out time");
                data.outPlots.Add(plot);
            }

            if(data.outPlots.Count == 0)
            {
//                Debug.Log("FindOuterNodes no outer plot found!");
//                Debug.Break();
            }
            

//            Debug.Log("FindOuterNodes shape count "+data.outPlots.Count);
        }

        private void FindStartPoint()
        {
            throw new NotImplementedException();
//            int startIndex = 0;
//            Debug.Log("find start point");
//            while (data.outPlots.Contains(data.nodes[startIndex]))
//            {
//                startIndex++;
//                if (startIndex > data.nodes.Count) break;
//            }
//            int startCount = 1;//data.nodes[startIndex].connections.Count;
//            //            Debug.Log(startCount);
//            for (int i = 0; i < startCount; i++)//add the first four link nodes that originate from the start of the generation
//                if (!data.nodes[0].IsPlotted(i))
//                    thisNextStart.Add(new[] { data.nodes[startIndex], data.nodes[startIndex].connections[i] });
        }

        private void FindPlot()
        {
            if (thisNextStart.Count == 0)
            {
                complete = true;
                return;
            }
            Node[] thisNext = thisNextStart[0];
            thisNextStart.RemoveAt(0);
            Node nCurrent = thisNext[0];
            Node nNext = thisNext[1];
            List<Node> plotPoints = new();//  
            int it = 0;

            while (it < 1000 && nCurrent != null)
            {
                plotPoints.Add(nCurrent);
                nCurrent.Plot(nNext);

                if (nNext == plotPoints[0] || plotPoints.Count > 100)//if we find ourselves back at the beginning
                {
                    nCurrent = null;
                    NodePlot newPlot = new(new List<Node>(plotPoints));//drop the data into the main list
                    data.plots.Add(newPlot);
                }
                else
                {
                    Node mnMove = nNext.NextNode(nCurrent);//move to next node from this link node
                    nCurrent = nNext;
                    nNext = mnMove;
                    //Debug.DrawLine(nCurrent.position.vector3XZ, nNext.position.vector3XZ, Color.red);
                }

                it++;
            }


            FindNextPlot();

            //            Debug.Log("find plots");
            //            while (!(thisNextStart.Count == 0 && nCurrent == null))//while we have link nodes to investigate
            //            {
            //                if (nCurrent == null)//get the next link node to investigate
            //                {
            //                    Node[] thisNext = thisNextStart[0];
            //                    nCurrent = 
            //                    nNext = thisNext[1];
            //                    plotPoints.Clear();//restart the current plot node list
            //                }
            //
            //                //                if (outnodes.Contains(nCurrent))
            //                //                    continue;
            //
            //                plotPoints.Add(nCurrent);
            //                nCurrent.Plot(nNext);
            //                if (nNext == plotPoints[0] || plotPoints.Count > 100)//if we find ourselves back at the beginning
            //                {
            //                    nCurrent = null;
            //                    LandPlot newPlot = new LandPlot(new List<Node>(plotPoints));//drop the data into the main list
            //                    data.plots.Add(newPlot);
            //                }
            //                else
            //                {
            //                    Node mnMove = nNext.NextNode(nCurrent);//move to next node from this link node
            //                    nCurrent = nNext;
            //                    nNext = mnMove;
            //                    Debug.DrawLine(nCurrent.positionV3, nNext.positionV3, Color.red);
            //                    Debug.Break();
            //                }
            //
            //                if (thisNextStart.Count == 0)//if we run out of things to look at - find something else
            //                {
            //                    int linkNodeCount = data.nodes.Count;
            //                    for (int i = 0; i < linkNodeCount; i++)
            //                    {
            //                        Node node = data.nodes[i];
            //                        if (node.FullyPlotted() || outnodes.Contains(node))
            //                            continue;
            //
            //                        Node newNext = node.NextUnplotted();
            //                        thisNextStart.Add(new[] { node, newNext });
            //
            //                        //                        Vector3 p0 = ToV3(node.position);
            //                        //                        Vector3 p1 = ToV3(newNext.position);
            //                        //                        Debug.DrawLine(p0,p1,Color.red,50f);
            //
            //                        break;
            //                    }
            //                }
            //
            //                it++;
            //                if (it > 10000)
            //                {
            //                    Debug.Log("ARRGGH!");
            //                    break;
            //                }
            //            }
            //
            //            int plotSize = data.plots.Count;
            //            Debug.Log(plotSize);
        }

        private void FindNextPlot()
        {
            throw new NotImplementedException();
//            if (thisNextStart.Count == 0)//if we run out of things to look at - find something else
//            {
//                int linkNodeCount = data.nodes.Count;
//                for (int i = 0; i < linkNodeCount; i++)
//                {
//                    Node node = data.nodes[i];
//                    if (node.FullyPlotted() || data.outnodes.Contains(node))
//                        continue;
//
//                    Node newNext = node.NextUnplotted();
//                    thisNextStart.Add(new[] { node, newNext });
//
//                    //                        Vector3 p0 = ToV3(node.position);
//                    //                        Vector3 p1 = ToV3(newNext.position);
//                    //                        Debug.DrawLine(p0,p1,Color.red,50f);
//
//                    break;
//                }
//            }
//
//            if (thisNextStart.Count == 0)
//                complete = true;
        }

        //        public static void BuildPlots(CityVO data)
        //        {
        //
        //
        //        }
    }
}
