/*  ---------------------------------------------------------------------------------------------------------------------------------------
 *  (C) 2019, Dr Warren Creemers.
 *  This file is subject to the terms and conditions defined in the included file 'LICENSE.txt'
 *  ---------------------------------------------------------------------------------------------------------------------------------------
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;
using QuickGraph.Data;
using WD_toolbox.AplicationFramework;
using WD_toolbox.Rendering;
using WD_toolbox;
using QuickGraph.Algorithms.ShortestPath;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WD_toolbox.Maths.Units;

namespace Maze2._0
{
    public class Maze : IStatusProvider, INotifyPropertyChanged
    {
        //--------------------------------------------------------------------------------------------------------------------
        // internal classes and enums
        //--------------------------------------------------------------------------------------------------------------------
        public enum NodeType
        {
            Floor, Start, End
        };

        public enum Alignment{Left, Right, Above, Below, None};

        public enum Corners
        {
            TopLeft, TopRight, BottomLeft, BottomRight
        };

        public class MazeNode : IEquatable<MazeNode>, IComparable<MazeNode>
        {
            public static volatile int lastID=0;
            public static volatile object _lock = new object();

            public int ID {get; protected set;}
            public NodeType Type { get; set; }
            public MazeEdge TopEdge { get; internal set; }
            public MazeEdge BottomEdge { get; internal set; }
            public MazeEdge LeftEdge { get; internal set; }
            public MazeEdge RightEdge { get; internal set; }

            public bool blockedAbove { get { return (TopEdge != null) ? TopEdge.Wall : true; } }
            public bool blockedBelow { get { return (BottomEdge != null) ? BottomEdge.Wall : true; } }
            public bool blockedLeft { get { return (LeftEdge != null) ? LeftEdge.Wall : true; } }
            public bool blockedRight { get { return (RightEdge != null) ? RightEdge.Wall : true; } }

            public MazeNode(int id, NodeType type) 
            {
                ID = id; 
                Type = type;
                TopEdge = BottomEdge = LeftEdge = RightEdge = null;
            }
            public static MazeNode CreateNew(NodeType type) 
            {
                lock (_lock)
                {
                    lastID++;
                    return new MazeNode(lastID, type);
                }
            }

            public bool Equals(MazeNode other) { return this.ID == other.ID; }
            public int CompareTo(MazeNode other) { return this.ID.CompareTo(other.ID); }
            public override int GetHashCode() { return ID.GetHashCode(); }

            public bool IsDeadEnd()
            {
                return EdgeCount() >= 3;
            }

            public int EdgeCount()
            {
                return tallyEdgeCount(TopEdge) + tallyEdgeCount(BottomEdge) + tallyEdgeCount(LeftEdge) + tallyEdgeCount(RightEdge);
            }

            private static int tallyEdgeCount(MazeEdge edge)
            {
                if (edge != null)
                {
                    return edge.Wall ? 1 : 0;
                }

                return 0;
            }

        }

        public class MazeEdge : Edge<MazeNode> {
            public bool Wall {get; set;}

            public MazeEdge (MazeNode source, MazeNode target) : base(source, target) 
            {
                Wall = true;
            }
        }

        public class MazeGraph : BidirectionalGraph<MazeNode, MazeEdge> 
        {
            public MazeGraph() : base() { init(); }
            public MazeGraph(bool allowParallelEdges)
                : base(allowParallelEdges) { }
            public MazeGraph(bool allowParallelEdges, int vertexCapacity)
                : base(allowParallelEdges, vertexCapacity) { init(); }
            public MazeGraph(bool allowParallelEdges, int capacity, Func<int, QuickGraph.Collections.IVertexEdgeDictionary<MazeNode, MazeEdge>> vertexEdgesDictionaryFactory)
                : base(allowParallelEdges, capacity, vertexEdgesDictionaryFactory) { init(); }
            public MazeGraph(bool allowParallelEdges, int vertexCapacity, int edgeCapacity)
                : base(allowParallelEdges, vertexCapacity, edgeCapacity) { init(); }
            public MazeGraph(bool allowParallelEdges, int vertexCapacity, int edgeCapacity, IEqualityComparer<MazeNode> vertexComparer)
                : base(allowParallelEdges, vertexCapacity, edgeCapacity, vertexComparer) { init(); }

            void init()
            {
            }
        }

        

        /*Ipublic class MazeGraphLayout : ContextualGraphLayout<MazeNode, MazeEdge, MazeGraph> {
            public MyGraphLayout () : base() { }

            public MyGraphLayout (bool allowParallelEdges)
                : base(allowParallelEdges) { }

            public MyGraphLayout (bool allowParallelEdges, int vertexCapacity)
                : base(allowParallelEdges, vertexCapacity) { }
        }*/

        //--------------------------------------------------------------------------------------------------------------------
        // Transient Data
        //--------------------------------------------------------------------------------------------------------------------
        [NonSerialized]
        INotifyPropertyChangedHelper notifyHelper = null; //this is made solid (and correctly setup) when needed, things break if this is not null here.


        //--------------------------------------------------------------------------------------------------------------------
        // Instance Data
        //--------------------------------------------------------------------------------------------------------------------
        int width;
        [Category("Size")]
        public int Width
        {
            get { return width; }
            set { width = value; propertyChanged(true); }
        }

        int height;
        [Category("Size")]
        public int Height
        {
            get { return height; }
            set { height = value; propertyChanged(true); }
        }

        int seed;
        [Category("Structure")]
        public int Seed
        {
            get { return seed; }
            set { seed = value; propertyChanged(true); }
        }

        int lineWidth;
        [Category("Apperance")]
        public int LineWidth
        {
            get { return lineWidth; }
            set { lineWidth = value; propertyChanged(); }
        }

        Color lineColor;
        [Category("Apperance")]
        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; propertyChanged(); }
        }

        Color fillColor;
        [Category("Apperance")]
        public Color FillColor
        {
            get { return fillColor; }
            set { fillColor = value; propertyChanged(); }
        }

        Color pathColor;
        [Category("Apperance")]
        public Color PathColor
        {
            get { return pathColor; }
            set { pathColor = value; propertyChanged(); }
        }

        Boolean roundEdges;
        [Category("Apperance")]
        public Boolean RoundEdges
        {
            get { return roundEdges; }
            set { roundEdges = value; propertyChanged(); }
        }

        int blockSizeInPixels;
        [Category("Size")]
        public int BlockSizeInPixels
        {
            get { return blockSizeInPixels; }
            set { blockSizeInPixels = value; propertyChanged(); }
        }


        Distance blockOutputSize;
        [Category("Output")]
        public Distance BlockOutputSize
        {
            get { return blockOutputSize; }
            set { blockOutputSize = value; propertyChanged(); }
        }

        [Category("Output")]
        public double DPI
        {
            get { return Resolution.fromMesurement(blockSizeInPixels, blockOutputSize).DPI; }
            set { blockOutputSize = Distance.FromMilliMetres(blockSizeInPixels / (double)Resolution.fromDPI(value).DotsPerMilliMetre); propertyChanged(); }
        }

        public Size TotalSizeInPixels { get { return new Size(blockSizeInPixels*width, blockSizeInPixels*height); } }


        //AdjacencyGraph<MazeNode, TaggedEdge<MazeNode, bool>> graph;
        MazeGraph graph;

        MazeNode[,] matrix;

        public string GenrationAlgorithemName { get { return "Recursive backtracker";} }

        public string Description { get { return string.Format("{0}x{1}(#{2})", Width, Height, Seed); } }

        //--------------------------------------------------------------------------------------------------------------------
        // Constructors / Factory methods
        //--------------------------------------------------------------------------------------------------------------------
        public Maze(int width, int height)
        {
            this.width = width;
            this.height = height;

            lineWidth = 8;
            blockSizeInPixels = 16;
            fillColor = Color.White;
            lineColor = Color.Red;
            pathColor = Color.Green;
            roundEdges = true;

            blockOutputSize = Distance.FromMilliMetres(blockSizeInPixels / (double)Resolution.fromDPI(300).DotsPerMilliMetre);

            init();
        }

        //-----------------------------------------------------------------------------------
        // Generation
        //-----------------------------------------------------------------------------------
        protected void propertyChanged(bool resultsInNewMaze = false, [CallerMemberName] string propertyName = null)
        {
            if (resultsInNewMaze)
            {
                Generate(Seed);
            }
            if (notifyHelper == null)
            {
                notifyHelper = new INotifyPropertyChangedHelper();
                notifyHelper.PropertyChanged += (S, E) => {if(this.PropertyChanged != null) { this.PropertyChanged(S, E);}};
            }
            notifyHelper.Raise(propertyName);
        }

        public void reset() { init(); }

        private void init()
        {
            //this.UpdateStatus(Status.Starting());
            int total = Width * Height;
            graph = new  MazeGraph(false, total, 4);

            matrix = new MazeNode[Width, Height];

            //create the nodes
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    matrix[x, y] = new MazeNode(x + (y * Width), NodeType.Floor);
                    graph.AddVertex(matrix[x, y]);
                }
                //this.UpdateStatus(Status.UpdateLoopMessage("Creating nodes", x, Width));
            }

            //populate the edges
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    /*
                    if (x == 0)
                    {
                        matrix[x, y].LeftEdge = new MazeEdge(matrix[x, y], null);
                    }
                    if (y == 0)
                    {
                        matrix[x, y].TopEdge = new MazeEdge(matrix[x, y], null);
                    }
                    if (x == (Width - 1))
                    {
                        matrix[x, y].RightEdge = new MazeEdge(matrix[x, y], null);
                    }
                    if (y == (Height - 1))
                    {
                        matrix[x, y].BottomEdge = new MazeEdge(matrix[x, y], null);
                    }*/


                    if(x > 0) {
                        //matrix[x, y].LeftEdge = new MazeEdge(matrix[x, y], matrix[x - 1, y]);
                        //graph.AddEdge(matrix[x, y].LeftEdge);

                        matrix[x, y].LeftEdge = matrix[x - 1, y].RightEdge;
                    }
                    if(y > 0) {
                        //matrix[x, y].TopEdge = new MazeEdge(matrix[x, y], matrix[x, y - 1]);
                        //graph.AddEdge(matrix[x, y].TopEdge);

                        matrix[x, y].TopEdge = matrix[x, y - 1].BottomEdge;
                    }
                    if(x < (Width-1)) {
                        matrix[x, y].RightEdge = new MazeEdge(matrix[x, y], matrix[x + 1, y]);
                        graph.AddEdge(matrix[x, y].RightEdge);
                    }
                    if(y < (Height-1)) {
                        matrix[x, y].BottomEdge = new MazeEdge(matrix[x, y], matrix[x, y + 1]);
                        graph.AddEdge(matrix[x, y].BottomEdge);
                    }
                }
                //this.UpdateStatus(Status.UpdateLoopMessage("Populating edges", x, Width));
            }

            //this.UpdateStatus(Status.Done());
        }

        public void GenerateWithNewSeed()
        {
            seed = (int)DateTime.Now.Ticks;
            Random rand = new Random(seed);
            reset();
            Generate(rand);
        }

        public void Generate(int seed)
        {
            Random rand = new Random(seed);
            reset();
            Generate(rand);
        }

        public virtual void Generate(Random rand)
        {
            if ((Width == 0) || (Height == 0))
            {
                return;
            }
            //init
            bool[,] visited = new bool[Width, Height];
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    visited[x, y] = false;
                }
            }
            int total = Width * Height;
            Stack<Point> stack = new Stack<Point>();

            //starting position
            int xPos = rand.Next(Width);
            int yPos = rand.Next(Height);

            //Make the initial cell the current cell and mark it as visited
            visited[xPos, yPos] = true;

            int visitCount = 1;
            //While there are unvisited cells
            while (visitCount < total)
            {
                List<Point> points = getUnvisitedNeighbours(visited, xPos, yPos);
                //If the current cell has any neighbours which have not been visited
                if (points.Count > 0)
                {
                    //Choose randomly one of the unvisited neighbours
                    Point p = points.GetRandomItem(rand);
                    //Push the current cell to the stack
                    stack.Push(new Point(xPos, yPos));
                    //Remove the wall between the current cell and the chosen cell
                    destroyWall(xPos, yPos, p);

                    //Make the chosen cell the current cell and mark it as visited
                    xPos = p.X;
                    yPos = p.Y;
                    visited[xPos, yPos] = true;
                    visitCount++;
                }
                else if (stack.Count > 0)
                {
                    //Else if stack is not empty
                    //Pop a cell from the stack
                    Point p = stack.Pop();
                    //Make it the current cell
                    xPos = p.X;
                    yPos = p.Y;
                }
                else
                {
                    //Pick a random unvisited cell, make it the current cell and mark it as visited
                    xPos = rand.Next(Width);
                    yPos = rand.Next(Height);
                    visited[xPos, yPos] = true;
                    visitCount++;
                }
            }
        }

        private void destroyWall(int xPos, int yPos, Point p)
        {
            int _x = p.X - xPos;
            switch (_x)
            {
                case 0:
                    int _y = p.Y - yPos;
                    switch (_y)
                    {
                        case -1:
                            matrix[xPos, yPos].TopEdge.Wall = false;
                            break;
                        case 1:
                            matrix[xPos, yPos].BottomEdge.Wall = false;
                            break;
                    }
                    break;
                case -1:
                    matrix[xPos, yPos].LeftEdge.Wall = false;
                    break;
                case 1:
                    matrix[xPos, yPos].RightEdge.Wall = false;
                    break;
            }
        }

        private List<Point> getUnvisitedNeighbours(bool[,] visited, int xPos, int yPos)
        {
            List<Point> points = new List<Point>();
            if (xPos > 0)
            {
                if (!visited[xPos - 1, yPos])
                {
                    points.Add(new Point(xPos - 1, yPos));
                }
            }
            if (yPos > 0)
            {
                if (!visited[xPos, yPos - 1])
                {
                    points.Add(new Point(xPos, yPos - 1));
                }
            }
            if (xPos < (Width-1))
            {
                if (!visited[xPos + 1, yPos])
                {
                    points.Add(new Point(xPos + 1, yPos));
                }
            }
            if (yPos < (Height - 1))
            {
                if (!visited[xPos, yPos + 1])
                {
                    points.Add(new Point(xPos, yPos + 1));
                }
            }

            return points;
        }

        //-----------------------------------------------------------------------------------
        // Rendering
        //-----------------------------------------------------------------------------------
        public void Render(IRenderer r, Rectangle bounds)//, int lineWidth = 2)
        {
            Render(r, bounds, this.LineColor, this.LineWidth);
        }

        protected void Render(IRenderer r, Rectangle bounds, Color col, int lineWidth=2)
        {
            Font mazeFont = new Font("Ariel", 16);

            r.FillRectangle(Color.White, bounds);

            double xStep = (bounds.Width / (double)Width);
            double yStep = (bounds.Height / (double)Height);
             //populate the edges
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    int x1 = (int)(bounds.Left + xStep * x);
                    int x2 = (int)(bounds.Left + xStep * (x+1));
                    int y1 = (int)(bounds.Top + yStep * y);
                    int y2 = (int)(bounds.Top + yStep * (y + 1));
                    MazeNode n = matrix[x, y];

                    switch (n.Type)
                    {
                        case NodeType.Floor:
                            break;
                        case NodeType.Start:
                            r.FillRectangle(Color.LightPink, x1 + 1, x2 + 1, (int)(xStep - 2), (int)(yStep - 2));
                            r.DrawString(Color.Red, "S", mazeFont, x1, y2);
                            break;
                        case NodeType.End:
                            r.FillRectangle(Color.LightPink, x1 + 1, x2 + 1, (int)(xStep - 2), (int)(yStep - 2));
                            r.DrawString(Color.Red, "F", mazeFont, x1, y2);
                            break;
                        default:
                            break;
                    }

                    if (needToDrawWall(n.LeftEdge)) {
                        drawWall(r, col, lineWidth, x1, y1, x1, y2);
                    }
                    if (needToDrawWall(n.TopEdge)) {
                        drawWall(r, col, lineWidth, x1, y1, x2, y1);
                    }
                    if (needToDrawWall(n.RightEdge)) {
                        drawWall(r, col, lineWidth, x2, y1, x2, y2);
                    }
                    if (needToDrawWall(n.BottomEdge)) {
                        drawWall(r, col, lineWidth, x1, y2, x2, y2);
                    }

                    if (RoundEdges)
                    {
                        r.AngleType = AngleTypes.Degrees;
                        Color cornerFill = Color.Green;
                        Color cornerWall = Color.Blue;

                        drawCornerIfNeeded(r, n, Corners.TopLeft, x1, y1, x2, y2, xStep, yStep);
                        drawCornerIfNeeded(r, n, Corners.TopRight, x1, y1, x2, y2, xStep, yStep);
                        drawCornerIfNeeded(r, n, Corners.BottomRight, x1, y1, x2, y2, xStep, yStep);
                        drawCornerIfNeeded(r, n, Corners.BottomLeft, x1, y1, x2, y2, xStep, yStep);
                    }

                }
            }

            //finally the start finish, just fudge for now
            int start = lineWidth/2;
            int end = blockSizeInPixels - start;
            r.DrawLine(FillColor, lineWidth*3, bounds.Left, bounds.Top + start, bounds.Left, bounds.Top + end);
            r.DrawLine(FillColor, lineWidth*3, bounds.Right, bounds.Bottom - start, bounds.Right, bounds.Bottom - end);
        }

        private void drawCornerIfNeeded(IRenderer r, MazeNode node,
                                        Corners corner,
                                        int x1, int y1, int x2, int y2, 
                                        double xStep, double yStep)
        {
            if (needToDrawCorner(node, corner))
            {
                int halfLinewidth = -(LineWidth / 2);

                Rectangle cornerRec = normilisedRectangle(
                    (int)((x1 + x2) / 2),
                    (int)((y1 + y2) / 2),
                    ((int)(xStep / 2) * HorizComponent(corner)) -halfLinewidth,
                    ((int)(yStep / 2) * VertComponent(corner)) -halfLinewidth);

                r.SetHighQuality(false);
                r.FillRectangle(this.LineColor, cornerRec);
                r.SetHighQuality(true);
                r.FillPie(this.fillColor, 
                            x1-halfLinewidth, y1-halfLinewidth, 
                            (int)xStep-lineWidth, (int)yStep-lineWidth, 
                            SwwepStart(corner), 90);
            }
        }

        private static int SwwepStart(Corners corner)
        {
            switch (corner)
            {
                case Corners.TopLeft:
                    return 180;
                case Corners.TopRight:
                    return 270;
                case Corners.BottomRight:
                    return 0;
                case Corners.BottomLeft:
                    return 90;
                default:
                    return 0;
            }
        }

        private static int HorizComponent(Corners corner)
        {
            switch (corner)
            {
                case Corners.TopLeft:
                    return -1;
                case Corners.BottomLeft:
                    return -1;
                default:
                    return 1;
            }
        }

        private static int VertComponent(Corners corner)
        {
            switch (corner)
            {
                case Corners.TopLeft:
                    return -1;
                case Corners.TopRight:
                    return -1;
                default:
                    return 1;
            }
        }

        private bool needToDrawCorner(MazeNode n, Corners corner)
        {
            switch (corner)
            {
                case Corners.TopLeft:
                    return needToDrawCorner(n.LeftEdge, n.TopEdge);
                case Corners.TopRight:
                    return needToDrawCorner(n.RightEdge, n.TopEdge);
                case Corners.BottomLeft:
                    return needToDrawCorner(n.LeftEdge, n.BottomEdge);
                case Corners.BottomRight:
                    return needToDrawCorner(n.RightEdge, n.BottomEdge);
                default:
                    return false;
            }
        }

        private bool needToDrawCorner(MazeEdge edge1, MazeEdge edge2)
        {
            return needToDrawWall(edge1) && needToDrawWall(edge2);
        }

        private void drawWall(IRenderer r, Color col, int width, int x1, int y1, int x2, int y2)
        {
            r.SetHighQuality(false);
            r.DrawLine(col, width, x1, y1, x2, y2);
            if (RoundEdges)
            {
                r.SetHighQuality(true);
                r.FillCircle(col, x1, y1, width / 2);
                r.FillCircle(col, x2, y2, width / 2);
            }
            else
            {
                r.FillRectangle(col, squareCenteredAt(x1, y1, width));
                r.FillRectangle(col, squareCenteredAt(x2, y2, width));
            }
        }

        static Rectangle squareCenteredAt(int x, int y, int size)
        {
            int h = size/2;
            return new Rectangle(x - h, y - h, size, size);
        }

        static Rectangle normilisedRectangle(int x, int y, int width, int height)
        {
            if (width < 0)
            {
                width = -width;
                x -= width;
            }
            if (height < 0)
            {
                height = -height;
                y -= height;
            }
            return new Rectangle(x, y, width, height);
        }


        public void renderPath(IRenderer r, Rectangle bounds, Path path)
        {
            int xOff = bounds.X;
            int yOff = bounds.Y;
            double xStep = (bounds.Width / (double)Width);
            double yStep = (bounds.Height / (double)Height);

            
            for(int i=0; i<(path.Count-1); i++)
            {
                Point thisPos = path[i];
                Point nextPos = path[i+1];

                Point middleThis = new Point(xOff + (int)((thisPos.X + 0.5) * xStep), yOff + (int)((thisPos.Y + 0.5) * yStep));
                Point middleNext = new Point(xOff + (int)((nextPos.X + 0.5) * xStep), yOff + (int)((nextPos.Y + 0.5) * yStep));
                
                r.DrawLine(Color.Blue, 2, middleThis, middleNext);

                /*Alignment alignment = getAlignment(thisPos, nextPos);
                switch (alignment)
	            {
		            case Alignment.Left:
                     break;
                    case Alignment.Right:
                     break;
                    case Alignment.Above:
                     break;
                    case Alignment.Below:
                     break;
                    case Alignment.None:
                     break;
                    default:
                     break;
	            }*/
            }
        }


        public void renderMask(IRenderer r, Rectangle bounds, bool[,] mask)
        {
            double xStep = (bounds.Width / (double)Width);
            double yStep = (bounds.Height / (double)Height);

            Color maskColor = Color.FromArgb(64, Color.Green);
             //populate the edges
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if(mask[x,y])
                    {
                        int x1 = (int)(bounds.Left + xStep * x);
                        int y1 = (int)(bounds.Top + yStep * y);
                        
                        r.FillRectangle(maskColor, x1, y1, (int)xStep, (int)yStep);
                    }
                }
            }
        }

        private bool needToDrawWall(MazeEdge mazeEdge)
        {
            if (mazeEdge == null) {
                return true;
            }
            return mazeEdge.Wall;
        }

        //-----------------------------------------------------------------------------------
        // Accessors
        //-----------------------------------------------------------------------------------
        public MazeNode this[Point p] { get { return matrix[p.X, p.Y]; } }

        public MazeNode this[int x, int y] { get { return matrix[x, y]; } }

        public List<Point> findAll(NodeType type)
        {
            List<Point> points = new List<Point>();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (matrix[x, y].Type == type)
                    {
                        points.Add(new Point(x, y));
                    }
                }
            }
            return points;
        }

        public bool isValidPos(Point pos)
        {
            return isValidPos(pos.X, pos.Y);
        }

        public bool isValidPos(int x, int y)
        {
            return (x >= 0) && (y >= 0) && (x < Width) && (y < Height);
        }

        //-----------------------------------------------------------------------------------
        // Misc functions
        //-----------------------------------------------------------------------------------
        public static Alignment getAlignment(Point from, Point to)
        {
            return getAlignment(from.X, from.Y, to.X, to.Y);
        }

        public static Alignment getAlignment(int xFrom, int yFrom, int xTo, int yTo)
        {
             int _x = xTo - xFrom;
            switch (_x)
            {
                case 0:
                    int _y = yTo - yFrom;
                    switch (_y)
                    {
                        case -1:
                            return Alignment.Above;
                        case 1:
                            return Alignment.Below;
                    }
                    break;
                case -1:
                    return Alignment.Left;
                case 1:
                    return Alignment.Right;
            }

            return Alignment.None;
        }

        //-----------------------------------------------------------------------------------
        // Path tracing
        //-----------------------------------------------------------------------------------
        public Path findShortestPath(Point from, Point to)
        {
            return findShortestPath(from.X, from.Y, to.X, to.Y);
        }

        public Path findShortestPath(int xFrom, int yFrom, int xTo, int yTo)
        {
            List<Point> starts = new List<Point>() { new Point(xFrom, yFrom) };
            List<Point> ends = new List<Point>() { new Point(xTo, yTo) };
            List<Path> paths = Path.solve(this, starts, ends).OrderBy(S => S.Count).ToList();
            return (paths.Count > 0) ? paths[0] : null;
        }

        public List<Path> solve()
        {
            List<Path> solves = Path.solve(this);
            return solves.OrderBy(S => S.Count).ToList();
        }

        public Path findLongestStartAndFinish()
        {
            int helped = 0, notHelped = 0;
            Path max = new Path(new Point(0, 0));
            int total = Width * Height;
            this.UpdateStatus(Status.Starting("Starting optimising start / end"));


            PathCollection[] fromPointCol= new PathCollection[total];
            for (int t = 0; t < total; t++)
            {
                fromPointCol[t] = new PathCollection();
            }

            for (int t = 0; t < (total - 1); t++)
            {
                Point to = new Point(t % Width, t / Width);
                PathCollection toPointCol = new PathCollection();

                for (int f = total-1; f >= (t+1); f--)
                {
                    Point from = new Point(f % Width, f / Width);

                    //if we are starting on a previous path, then we know a longer version exists
                    if (fromPointCol[f].Contains(to))
                    {
                        helped+=f;
                        break;
                    }

                    if (!toPointCol.Contains(from))
                    {
                        Path path = findShortestPath(from, to);
                        if (path != null)
                        {
                            if (path.Count > max.Count)
                            {
                                max = path;
                            }
                            toPointCol.Add(path);
                            fromPointCol[f].Add(path);
                        }
                        notHelped++;
                    }
                    else
                    {
                        helped++;
                    }
                }
                //this.UpdateStatus(Status.UpdateLoop(i, total));
                this.UpdateStatus(Status.UpdateLoopMessage(""+(helped/(double)(helped+notHelped))+"effective ", t, total));
            }

            this.UpdateStatus(Status.Done());
            return max;
        }

        public bool canIDoThisMove(int xFrom, int yFrom, int xTo, int yTo)
        {
            int _x = xTo - xFrom;
            switch (_x)
            {
                case 0:
                    int _y = yTo - yFrom;
                    switch (_y)
                    {
                        case -1:
                            return !matrix[xFrom, yFrom].TopEdge.Wall;
                        case 1:
                            return !matrix[xFrom, yFrom].BottomEdge.Wall;
                    }
                    break;
                case -1:
                    return !matrix[xFrom, yFrom].LeftEdge.Wall;
                case 1:
                    return !matrix[xFrom, yFrom].RightEdge.Wall;
            }

            return false;
        }

        //-----------------------------------------------------------------------------------
        // Masks
        //-----------------------------------------------------------------------------------
        private bool[,] generateMask(bool value)
        {
            bool[,] mask = new bool[Width, Height];
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    mask[x, y] = value;
                }
            }

            return mask;
        }

        public bool[,] findDeadEnds()
        {
             bool[,] mask = generateMask(false);
             List<Point> points = new List<Point>();
             for (int x = 0; x < Width; x++)
             {
                 for (int y = 0; y < Height; y++)
                 {
                    if(matrix[x, y].IsDeadEnd())
                    {
                        mask[x, y] = true;
                        //return mask;
                    }
                 }
             }

             return mask;
        }

        
        public bool[,] findDeadEndPassages()
        {
            //HashSet<Point> sae();
            var sae = findAll(NodeType.End);
            sae.AddRange(findAll(NodeType.Start));
            //startsAndends.
            return findDeadEndPassages(sae.ToHashSet());
        }

        public bool[,] findDeadEndPassages(HashSet<Point> startsAndends)
        {
            bool[,] mask = findDeadEnds();
            foreach (Point p in startsAndends)
            {
                mask[p.X, p.Y] = false;
            }

            bool workDone = true;
            while (workDone)
            {
                workDone = false;
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        MazeNode n = this[x, y];
                        if (!mask[x, y])// || (n.EdgeCount() < 2))
                        {
                            continue;
                        }

                        if (!n.blockedAbove && isUnfoundDeadEndPassege(x, y - 1, mask, startsAndends))
                        {
                            mask[x, y - 1] = true;
                            workDone = true;
                        }
                        if (!n.blockedBelow && isUnfoundDeadEndPassege(x, y + 1, mask, startsAndends))
                        {
                            mask[x, y + 1] = true;
                            workDone = true;
                        }
                        if (!n.blockedLeft && isUnfoundDeadEndPassege(x - 1, y, mask, startsAndends))
                        {
                            mask[x - 1, y] = true;
                            workDone = true;
                        }
                        if (!n.blockedRight && isUnfoundDeadEndPassege(x + 1, y, mask, startsAndends))
                        {
                            mask[x + 1, y] = true;
                            workDone = true;
                        }
                    }
                }
            }

            return mask;
        }

        //X and Y musk be valid
        private bool isUnfoundDeadEndPassege(int x, int y, bool[,] mask, HashSet<Point> startsAndends)
        {
            if (mask[x, y]) {
                return false;
            }
            if(startsAndends.Contains(new Point (x, y)))
            {
                return false;
            }
            if (this[x, y].EdgeCount() >= 2)
            {
                return true;
            }

            return false;
        }

        //-----------------------------------------------------------------------------------
        // IStatusProvider
        //-----------------------------------------------------------------------------------
        OnStatusChangeDelegate IStatusProvider.OnStatusChange { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


    }
}
