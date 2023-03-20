using System;
using System.Collections.Generic;
using opengen.maths;
using opengen.maths.shapes.shapefinder;
using opengen.types;
using UnityEngine;


namespace opengen.mesh
{
    /// <summary>
    /// Define a mesh which we map out it's data as lines
    /// Then define a plane which we use to slice the mesh and derive an outline hull shape from it
    /// </summary>
    /// TODO clean up code when we're happy this is stable
    public class MeshCrossSection
    {
        public Vector2[][] shapes = new Vector2[0][];

        private int vertCount;
        List<MeshLine> lines = new();
        List<MeshTriangle> triangles = new();
        Dictionary<Vector3Fixed, MeshPoint> pointDic = new();
        List<MeshPoint> points = new();

        public MeshCrossSection(Vector3[] meshVerts, int[] meshTris)
        {
            vertCount = meshVerts.Length;
            
            lines = new List<MeshLine>();//.Clear();
            triangles = new List<MeshTriangle>();//.Clear();
            pointDic = new Dictionary<Vector3Fixed, MeshPoint>();//.Clear();
            points = new List<MeshPoint>();//.Clear();
            
            Vector3Fixed[] verts = new Vector3Fixed[vertCount];
//            Vector3Fixed[] norms = new Vector3Fixed[vertCount];
            //convert verts and normals to world space
            for (int v = 0; v < vertCount; v++)
            {
                Vector3Fixed vert = new(meshVerts[v]);
                verts[v] = vert;
                if (!pointDic.ContainsKey(vert))
                {
                    MeshPoint newPoint = new(vert);
                    points.Add(newPoint);
                    pointDic.Add(vert, newPoint);
                }
            }
            
            //Iterate over the mesh triangles to generate mesh lines and mesh triangles
            int triCount = meshTris.Length;
            for (int t = 0; t < triCount; t += 3)
            {
                int index0 = meshTris[t];
                int index1 = meshTris[t + 1];
                int index2 = meshTris[t + 2];

                Vector3Fixed vert0 = verts[index0];
                Vector3Fixed vert1 = verts[index1];
                Vector3Fixed vert2 = verts[index2];

                MeshPoint point0 = pointDic[vert0];
                MeshPoint point1 = pointDic[vert1];
                MeshPoint point2 = pointDic[vert2];


                bool line0Exists = false;
                bool line1Exists = false;
                bool line2Exists = false;

                MeshLine line0 = point0.Connect(point1, out line0Exists);
                MeshLine line1 = point1.Connect(point2, out line1Exists);
                MeshLine line2 = point2.Connect(point0, out line2Exists);

                if (!line0Exists)
                {
                    lines.Add(line0);
                }

                if (!line1Exists)
                {
                    lines.Add(line1);
                }

                if (!line2Exists)
                {
                    lines.Add(line2);
                }

                MeshTriangle triangle = new(vert0, vert1, vert2, line0, line1, line2);
                triangles.Add(triangle);

                line0.triangles.Add(triangle);
                line1.triangles.Add(triangle);
                line2.triangles.Add(triangle);
            }
        }
        
        public void Update(float elevation)
        {
            //CALCULATE the intersections across the defined y elevation
            int lineCount = lines.Count;
            for(int l = 0; l < lineCount; l++)
            {
                MeshLine line = lines[l];
                line.intersection = false;
                line.planeIntesection = Vector3Fixed.zero;

                Vector3 p0 = line.a.point.vector3;
                Vector3 p1 = line.b.point.vector3;

                bool testA = p0.y < elevation && p1.y > elevation;//check that line goes through cross section elevation
                bool testB = p1.y < elevation && p0.y > elevation;

                if(testA || testB)
                {
                    bool flipped = p0.y > p1.y;
                    float min = Mathf.Min(p0.y, p1.y);
                    float max = Mathf.Max(p0.y, p1.y);
                    float diff = max - min;
                    float elevationLerp = elevation - min;
                    float lerpValue = elevationLerp / diff;
                    if(flipped)
                    {
                        lerpValue = 1 - lerpValue;
                    }

                    Vector3 lerpPoint = Vector3.Lerp(p0, p1, lerpValue);
                    line.intersection = true;
                    line.planeIntesection = new Vector3Fixed(lerpPoint);
//                    Debug.DrawLine(line.planeIntesection.vector3, Vector3.zero, Color.red, 15);

//                    Vector3 px0 = new Vector3(p0.x, elevation, p0.z);
//                    Vector3 px1 = new Vector3(p1.x, elevation, p1.z);
//                    Debug.DrawLine(px0, px1, Color.red, 15);
                }
            }
//            Profiler.EndSample();

            UpdateCalculations();
        }

        public void Update(Vector3 point, Vector3 normal)
        {
//            Debug.Log("Update ==========================");
//            Profiler.BeginSample("UPDATE Execute C plane");

            //CALCULATE the intersections across mesh lines
            Plane plane = new(normal, point);
            Plane inversePlane = new(-normal, point);//calculate both directions
            int lineCount = lines.Count;
//            Color bcol = new Color(0,0,1,0.25f);
            for(int l = 0; l < lineCount; l++)
            {
                MeshLine line = lines[l];
                line.intersection = false;
                line.planeIntesection = Vector3Fixed.zero;

//                line.DebugDrawLine(bcol);

                float rayCastHitDistance;
                if(plane.Raycast(line.ray, out rayCastHitDistance))
                {
                    if(line.distance >= rayCastHitDistance)
                    {
                        line.intersection = true;
                        line.planeIntesection = new Vector3Fixed(line.ray.GetPoint(rayCastHitDistance));
//                        Debug.DrawLine(line.planeIntesection.vector3, Vector3.zero, Color.red, 15);
                    }
                }
                else if(inversePlane.Raycast(line.ray, out rayCastHitDistance))
                {
                    if(line.distance >= rayCastHitDistance)
                    {
                        line.intersection = true;
                        line.planeIntesection = new Vector3Fixed(line.ray.GetPoint(rayCastHitDistance));
//                        Debug.DrawLine(line.planeIntesection.vector3, Vector3.zero, Color.red, 15);
                    }
                }
            }
//            Profiler.EndSample();

            UpdateCalculations();
        }

        private void UpdateCalculations()
        {

//            Profiler.BeginSample("Execute D");
            //generate a node map of all the intersections
            List<Node> nodes = new();
            Dictionary<Vector2Fixed, Node> nodeDic = new();
            List<NodeLine> nodeLines = new();
//            Color tcol = new Color(0,0,1,0.2f);
//            Color icol = new Color(1,0,1,0.5f);
            foreach (MeshTriangle triangle in triangles)
            {
//                triangle.DebugDrawLine(tcol);
//                Debug.Break();
                if (triangle.IntersectionCount() == 2)
                {
                    Vector3Fixed[] intersections = triangle.GetIntesections();
                    Vector2Fixed p0 = intersections[0].vector2FixedXz;
                    Vector2Fixed p1 = intersections[1].vector2FixedXz;

                    Vector3 dp0 = intersections[0].vector3;
                    Vector3 dp1 = intersections[1].vector3;
//                    Debug.DrawLine(dp0,dp1,Color.green,15);

                    if (p0 != p1)
                    {
                        if (!nodeDic.ContainsKey(p0))
                        {
                            Node newNode = new(p0);
                            nodes.Add(newNode);
                            nodeDic.Add(p0, newNode);
                        }
                        if (!nodeDic.ContainsKey(p1))
                        {
                            Node newNode = new(p1);
                            nodes.Add(newNode);
                            nodeDic.Add(p1, newNode);
                        }

                        Node nodeP0 = nodeDic[p0];
                        Node nodeP1 = nodeDic[p1];

                        if (nodeP0.Connect(nodeP1))
                        {
                            nodeLines.Add(new NodeLine(nodeP0, nodeP1));
                        }
                    }
                    //else no need to do anything - nodes are space based so further connections will link to this node
                }
            }

            //There might be a case there the mesh has a hole in it - we'll brute force a solution
            //Any nodes with only a single connection will be connected to one another
            int nodeCount = nodes.Count;
            List<Node> singleConnectionNodes = new();
            for(int n = 0; n < nodeCount; n++)
            {
                if(nodes[n].connections.Count == 1)
                {
                    singleConnectionNodes.Add(nodes[n]);
                }
            }

            int singleConnectionNodeCount = singleConnectionNodes.Count;
            for(int sx = 0; sx < singleConnectionNodeCount; sx++)
            {
                Node nodeSx = singleConnectionNodes[sx];
                for(int sy = 0; sy < singleConnectionNodeCount; sy++)
                {
                    if(sx==sy)
                    {
                        continue;
                    }

                    Node nodeSy = singleConnectionNodes[sy];
                    if (nodeSx.Connect(nodeSy))
                    {
                        nodeLines.Add(new NodeLine(nodeSx, nodeSy));
                    }
                }
            }

//            Profiler.EndSample();

//            Profiler.BeginSample("Execute E");//1
            //Geometry might not be 100% clean for this kind of work
            //We need to find any geometry that interescts and solve it
            //note - we never remove nodelines from our list as that's very expensive - we flag them
            int nodeLineCount = nodeLines.Count;
            int aMax = nodeLineCount * 2;
            for (int a = 0; a < nodeLineCount; a++)
            {
                NodeLine lineA = nodeLines[a];
//                lineA.bounds.DrawDebug(Color.red);
                if(lineA.ignore)
                {
                    continue;
                }

                int bMax = nodeLineCount * 2;
                for (int b = a + 1; b < nodeLineCount; b++)
                {
                    bMax--;
                    if (bMax < 0)
                    {
                        break;
                    }

                    NodeLine lineB = nodeLines[b];
                    if(lineB.ignore)
                    {
                        continue;
                    }

                    if(!lineA.OverlapBounds(lineB))
                    {
                        continue;
                    }

                    bool aConn = false;
                    bool bConn = false;
                    if (lineB.Contains(lineA))
                    {
                        aConn = lineB.Contains(lineA.a);
                        bConn = lineB.Contains(lineA.b);
                    }
                    Vector2 ax = lineA.a.position.vector2;
                    Vector2 ay = lineA.b.position.vector2;
                    Vector2 bx = lineB.a.position.vector2;
                    Vector2 by = lineB.b.position.vector2;
                    if (!aConn && Segments.PointOnSegment(ax, bx, by))
                    {
                        lineB.ignore = true;
                        nodeLineCount--;
                        
                        if (lineB.Disconnect())
                        {
                            if (lineB.a.Connect(lineA.a))
                            {
                                NodeLine newLine = new(lineB.a, lineA.a);
                                nodeLines.Add(newLine);
                                nodeLineCount++;
                            }
                            
                            if (lineB.b.Connect(lineA.a))
                            {
                                NodeLine newLine = new(lineB.b, lineA.a);
                                nodeLines.Add(newLine);
                                nodeLineCount++;
                            }
                        }
                        continue;
                    }
                    if (!bConn && Segments.PointOnSegment(ay, bx, by))
                    {
                        lineB.ignore = true;
                        nodeLineCount--;

                        if (lineB.Disconnect())
                        {
                            if (lineB.a.Connect(lineA.b))
                            {
                                NodeLine newLine = new(lineB.a, lineA.b);
                                nodeLines.Add(newLine);
                                nodeLineCount++;
                            }

                            if (lineB.b.Connect(lineA.b))
                            {
                                NodeLine newLine = new(lineB.b, lineA.b);
                                nodeLines.Add(newLine);
                                nodeLineCount++;
                            }

                        }
                        continue;
                    }
                    if (!aConn && !bConn && Segments.FastIntersection(ax, ay, bx, by))
                    {
                        Vector2 lineX = ay - ax;
                        Vector2 lineY = by - bx;
                        Vector2 intersectionPoint = Segments.FindIntersection(lineX, ax, lineY, bx);

                        lineA.Disconnect();
                        lineA.ignore = true;
                        lineB.Disconnect();
                        lineB.ignore = true;
                        nodeLineCount += -2;
                        
                        Vector2Fixed intersectionPointF = new(intersectionPoint);
                        if (!nodeDic.ContainsKey(intersectionPointF))
                        {
                            Node newNode = new(intersectionPointF);
                            nodes.Add(newNode);
                            nodeDic.Add(intersectionPointF, newNode);
                        }
                        Node intersectionNode = nodeDic[intersectionPointF];
                        
                        if (intersectionNode.Connect(lineA.a))
                        {
                            nodeLines.Add(new NodeLine(lineA.a, intersectionNode));
                            nodeLineCount++;
                        }

                        if (intersectionNode.Connect(lineA.b))
                        {
                            nodeLines.Add(new NodeLine(lineA.b, intersectionNode));
                            nodeLineCount++;
                        }

                        if (intersectionNode.Connect(lineB.a))
                        {
                            nodeLines.Add(new NodeLine(lineB.a, intersectionNode));
                            nodeLineCount++;
                        }

                        if (intersectionNode.Connect(lineB.b))
                        {
                            nodeLines.Add(new NodeLine(lineB.b, intersectionNode));
                            nodeLineCount++;
                        }
                    }
                }
                aMax--;
                if (aMax < 0)
                {
                    //Debug.Log("aMax");
                    break;
                }
            }

//            for(int n = 0; n < nodes.Count; n++)
//            {
//                Node node = nodes[n];
//                Vector3 up = Vector3.up * n * 0.01f;
//                foreach(Node other in node.connections)
//                {
//                    Debug.DrawLine(node.position.vector3XZ + up, other.position.vector3XZ + up, Color.blue, 30);
//                }
//            }

            //finally we'll use the shape finder to get an external hull shape from the node map
            ShapeData shapeData = new(nodes);
            maths.shapes.shapefinder.ShapeFinder shapeFinder = new();
            shapeFinder.Execute(shapeData);
            NodePlot[] plots = shapeData.outPlots.ToArray();
            int plotCount = plots.Length;
            Vector2[][] output = new Vector2[plotCount][];
            for (int p = 0; p < plotCount; p++)
            {
                output[p] = plots[p].GetShape();
            }

            shapes = output;
        }

        private class MeshPoint : INode
        {
            public readonly Vector3Fixed point;
//            public readonly Vector3Fixed normal;
            private readonly List<MeshLine> _lines;
//            public bool exists;

            public MeshPoint(Vector3Fixed point)
            {
                this.point = point;
//                this.normal = normal;
                _lines = new List<MeshLine>(4);
//                exists = true;
            }

            public MeshLine Connect(MeshPoint other, out bool exists)
            {
                int linesCount = _lines.Count;
                for (int l = 0; l < linesCount; l++)
                {
                    MeshLine line = _lines[l];
                    if (line.Contains(other))
                    {
                        exists = true;
                        return line;
                    }
                }
                MeshLine newLine = new(this, other);
                _lines.Add(newLine);
                other._lines.Add(newLine);
                exists = false;
                return newLine;
            }

            public bool Equals(MeshPoint p)
            {
                return (Mathf.Abs(point.x - p.point.x) < Numbers.Epsilon) && (Mathf.Abs(point.y - p.point.y) < Numbers.Epsilon) && (Mathf.Abs(point.z - p.point.z) < Numbers.Epsilon);
            }

            public override int GetHashCode()
            {
                return Numbers.RoundToInt(point.x * point.y * point.z);
            }

            public override bool Equals(object a)
            {
                return base.Equals(a);// (Dot(this, a) > 0.999f);
            }

            public static bool operator ==(MeshPoint a, MeshPoint b)
            {
                return a.point == b.point;
            }

            public static bool operator !=(MeshPoint a, MeshPoint b)
            {
                return a.point != b.point;
            }

            public Vector2Fixed position
            {
                get { return point.vector2FixedXz; }
            }

            public INode[] connections
            {
                get
                {
                    int connectionCount = _lines.Count;
                    INode[] output = new INode[connectionCount];
                    for (int c = 0; c < connectionCount; c++)
                    {
                        output[c] = _lines[c].GetOther(this);
                    }

                    return output;
                }
            }
        }

        private class MeshLine
        {
            private readonly MeshPoint _a;
            private readonly MeshPoint _b;
            public bool intersection;
            public Vector3Fixed planeIntesection;
            public Ray ray = new();
            public readonly float distance;
            public readonly List<MeshTriangle> triangles;

            public MeshLine(MeshPoint a, MeshPoint b)
            {
                _a = a;
                _b = b;
                ray = new Ray(a.point.vector3, (b.point.vector3 - a.point.vector3).normalized);
                distance = Vector3Fixed.DistanceWorld(a.point, b.point);
                intersection = false;
                planeIntesection = Vector3Fixed.zero;
                triangles = new List<MeshTriangle>(2);
            }

            public MeshPoint a { get { return _a; } }
            public MeshPoint b { get { return _b; } }

            public bool Contains(MeshPoint x)
            {
                return _a == x || _b == x;
            }

            public MeshPoint GetOther(MeshPoint current)
            {
                if (current == _a)
                {
                    return _b;
                }

                if (current == _b)
                {
                    return _a;
                }

                return null;
            }

            public MeshTriangle GetOther(MeshTriangle current)
            {
                if (triangles.Count == 2)
                {
                    if (triangles[0] == current) { return triangles[1]; }
                    if (triangles[1] == current) { return triangles[0]; }
                }
                else
                {
                    //Debug.LogError("Multiple options available");
                    for (int i = 0; i < triangles.Count; i++)
                    {
                        if (triangles[i] != current)
                        {
                            return triangles[i];
                        }
                    }
                }
                return null;
            }

            public static bool operator ==(MeshLine a, MeshLine b)
            {
                if (object.ReferenceEquals(a, null))
                {
                    return object.ReferenceEquals(b, null);
                }

                if (object.ReferenceEquals(b, null))
                {
                    return false;
                }

                return a._a == b._a && a._b == b._b || a._a == b._b && a._b == b._a;
            }

            public static bool operator !=(MeshLine a, MeshLine b)
            {
                return !(a == b);
            }

            // public bool Equals(UnityEngine.Object a)
            // {
            //     return base.Equals(a);// (Dot(this, a) > 0.999f);
            // }

            public override bool Equals(object a)
            {
                return base.Equals(a);// (Dot(this, a) > 0.999f);
            }

            public override int GetHashCode()
            {
                return Numbers.RoundToInt(_a.GetHashCode() ^ _b.GetHashCode());
            }

//             public void DebugDrawLine(Color col)
//             {
//                 Debug.DrawLine(_a.point.vector3, _b.point.vector3, col, 3);
// //                Vector3 center = Vector3.Lerp(_a.point.vector3, _b.point.vector3, 0.5f);
// //                Vector3 triNormal = Vector3.zero;
// //                foreach (MeshTriangle triangle in triangles)
// //                {
// //                    Debug.DrawLine(center, triangle.centre.vector3, Color.red);
// //                    triNormal += triangle.normal.vector3;
// //                }
// //                Debug.DrawLine(_a.point.vector3, _b.point.vector3 + triNormal.normalized * 2, col);
// //                Debug.DrawLine(_b.point.vector3, _b.point.vector3 + triNormal.normalized * 2, col);
//             }
        }

        private class MeshTriangle
        {
            private readonly Vector3Fixed _p0;
            private readonly Vector3Fixed _p1;
            private readonly Vector3Fixed _p2;
            private readonly MeshLine _line0;
            private readonly MeshLine _line1;
            private readonly MeshLine _line2;
            private readonly Vector3Fixed _normal;
            private readonly Vector3Fixed _centre;

            public MeshTriangle(Vector3Fixed p0, Vector3Fixed p1, Vector3Fixed p2, MeshLine l0, MeshLine l1, MeshLine l2)
            {
                _p0 = p0;
                _p1 = p1;
                _p2 = p2;
                _line0 = l0;
                _line1 = l1;
                _line2 = l2;
                _normal = new Vector3Fixed(Vector3.Cross((_p1.vector3 - _p0.vector3).normalized, (_p2.vector3 - _p0.vector3).normalized).normalized);
                _centre = new Vector3Fixed((p0.vector3 + p1.vector3 + p2.vector3) / 3f);
            }

            public Vector3Fixed this[int index]
            {
                get
                {
                    switch (index)
                    {
                        case 0: return _p0;
                        case 1: return _p1;
                        case 2: return _p2;
                    }

                    //Debug.LogError("Index out of range");
                    return _p0;
                }
            }

            public MeshLine Line(int index)
            {
                switch (index)
                {
                    case 0: return _line0;
                    case 1: return _line1;
                    case 2: return _line2;
                }

                //Debug.LogError("Index out of range");
                return _line0;
            }

            public Vector3Fixed normal { get { return _normal; } }
            public Vector3Fixed centre { get { return _centre; } }

            public int IntersectionCount()
            {
                int output = 0;
                if (_line0.intersection)
                {
                    output++;
                }

                if (_line1.intersection)
                {
                    output++;
                }

                if (_line2.intersection)
                {
                    output++;
                }

                return output;
            }

            public Vector3Fixed[] GetIntesections()
            {
                Vector3Fixed[] output = new Vector3Fixed[IntersectionCount()];
                int index = 0;
                for (int i = 0; i < 3; i++)
                {
                    MeshLine line = Line(i);
                    if (line.intersection)
                    {
                        output[index] = line.planeIntesection;
                        index++;
                    }
                }
                return output;
            }

            public int GetOtherIntersectionIndex(MeshLine line)
            {
                if (!line.intersection)
                {
                    //                    Debug.LogWarning("Line not an intesection");
                    return -1;
                }
                if (IntersectionCount() != 2)
                {
                    //                    Debug.LogWarning("Invalid number of triangle intersections: " + IntersectionCount());
                    return -1;
                }
                for (int i = 0; i < 3; i++)
                {
                    if (!Line(i).intersection)
                    {
                        continue;
                    }

                    if (Line(i) == line)
                    {
                        continue;
                    }

                    return i;
                }

                //                Debug.LogWarning("Line not in triangle");
                return -1;
            }

//             public void DebugDrawLine(Color col)
//             {
//                 Vector3 normVector = _normal.vector3 * 0.1f;
//                 for (int i = 0; i < 3; i++)
//                 {
//                     Debug.DrawLine(centre.vector3, this[i].vector3, col);
//                     int ib = i < 2 ? i + 1 : 0;
//                     Debug.DrawLine(this[i].vector3, this[ib].vector3, col);
//                     //                    Debug.DrawLine(centre.vector3, centre.vector3 + normVector, Color.red);
//                 }
//
//                 int intCount = IntersectionCount();
//                 Vector3Fixed[] inter = GetIntesections();
//                 Color inCol = Color.red;
//                 if(intCount==2)inCol = Color.green;
//                 if(intCount>2)inCol = Color.magenta;
//                 for(int i = 0; i < inter.Length; i++)
//                 {
//                     Vector3 cdir = (centre.vector3 - inter[i].vector3).normalized * 0.1f;
//                     Debug.DrawLine(inter[i].vector3, inter[i].vector3 + normVector + cdir, inCol, 15);
//                 }
//
//
//                 //                for(int i = 0; i < intCount; i++)
//                 //                    Debug.DrawLine(centre.vector3 + normVector * i, centre.vector3 + normVector * (i + .5f), Color.magenta);
//                 if (intCount == 2)
//                 {
//                     Vector3 ip0 = _line0.intersection ? _line0.planeIntesection.vector3 : _line1.planeIntesection.vector3;
//                     Vector3 ip1 = _line0.intersection && _line1.intersection ? _line1.planeIntesection.vector3 : _line2.planeIntesection.vector3;
//                     Debug.DrawLine(ip0, ip1, Color.magenta, 15);
//                 }
// //                if (intCount == 1)
// //                {
// //                    for (int i = 0; i < 3; i++)
// //                    {
// //                        MeshLine line = Line(i);
// //                        if (line.intersection) Debug.DrawLine(centre.vector3, line.a.point.vector3, Color.red);
// //                        if (line.intersection) Debug.DrawLine(centre.vector3, line.b.point.vector3, Color.red);
// //                    }
// //                }
//             }
        }
    }
}