/*  ---------------------------------------------------------------------------------------------------------------------------------------
 *  (C) 2019, Dr Warren Creemers.
 *  This file is subject to the terms and conditions defined in the included file 'LICENSE.txt'
 *  ---------------------------------------------------------------------------------------------------------------------------------------
 */
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WD_toolbox;
using WD_toolbox.Maths.Range;
using WD_toolbox.Data.Text;
using System;

namespace Maze2._0
{
    public class Path : List<Point>
    {
        public Path(Point start) : base()
        {
            this.Add(start);
        }

        public Path(Path oldPath, Point nextPoint) : base (oldPath)
        {
            this.Add(nextPoint);
        }

        public Path() : base()
        {
        }

        public Point CurrentPos { get { return this[this.Count - 1]; } }

        public List<Path> findNextPossiblePaths(Maze maze)
        {
            List<Path> result = new List<Path>();
            addToResultIfValidPath(maze, CurrentPos.getPointAbove(), result);
            addToResultIfValidPath(maze, CurrentPos.getPointBelow(), result);
            addToResultIfValidPath(maze, CurrentPos.getPointLeft(), result);
            addToResultIfValidPath(maze, CurrentPos.getPointRight(), result);

            return result;
        }

        private void addToResultIfValidPath(Maze maze, Point next, List<Path> result)
        {
            if (maze.isValidPos(next) && 
                maze.canIDoThisMove(CurrentPos.X, CurrentPos.Y, next.X, next.Y) && 
                !this.Contains(next))
            {
                result.Add(new Path(this, next));
            }
        }

        public bool hasSolvedMaze(Maze maze)
        {
            return maze[CurrentPos].Type == Maze.NodeType.End;
        }

        public static List<Path> solve(Maze maze, int maxSolves = 1000)
        {
            List<Point> starts = maze.findAll(Maze.NodeType.Start);
            List<Point> ends = maze.findAll(Maze.NodeType.End);
            return solve(maze, starts, ends, maxSolves);
        }

        public static List<Path> solve(Maze maze,
                                List<Point> starts,
                                List<Point> ends,
                                int maxSolves = 1000)
        {
             List<Path> paths = new List<Path>();
            List<Path> solutions = new List<Path>();
            foreach(Point start in starts)
            {
                paths.Add(new Path(start));
            }

            bool moreSearchingNeeded = true;
            while (moreSearchingNeeded && (solutions.Count < maxSolves))
            {
                List<Path> nextPaths = new List<Path>();
                foreach (Path path in paths)
                {
                    List<Path> continuences = path.findNextPossiblePaths(maze);
                    nextPaths.AddRange(continuences);
                }
                paths = nextPaths;

                moreSearchingNeeded = false;
                foreach (Path path in paths)
                {
                    if (!ends.Contains(path.CurrentPos))//!path.hasSolvedMaze(maze))
                    {
                        moreSearchingNeeded = true;
                    }
                    else
                    {
                        solutions.Add(path);
                        //paths.Remove(path);
                    }
                }
            }

            return solutions;
        }

        public static List<Path> findShortest(Maze maze, 
                                List<Point> starts, 
                                List<Point> ends,
                                PathCollection preSolves,
                                int maxSolves = 1000)
        {
            List<Path> paths = new List<Path>();
            List<Path> solutions = new List<Path>();
            foreach(Point start in starts)
            {
                paths.Add(new Path(start));
            }

            bool moreSearchingNeeded = true;
            while (moreSearchingNeeded && (solutions.Count < maxSolves))
            {
                List<Path> nextPaths = new List<Path>();
                foreach (Path path in paths)
                {
                    List<Path> continuences = path.findNextPossiblePaths(maze);
                    nextPaths.AddRange(continuences);
                }
                paths = nextPaths;

                moreSearchingNeeded = false;
                foreach (Path path in paths)
                {
                    if (!ends.Contains(path.CurrentPos))//!path.hasSolvedMaze(maze))
                    {
                        moreSearchingNeeded = true;
                    }
                    else
                    {
                        solutions.Add(path);
                        //paths.Remove(path);
                    }
                }
            }

            return solutions;
        }

        public bool Contains(int x, int y)
        {
            foreach(Point p in this)
            {
                if((p.X == x) && (p.Y == y)) {
                    return true;
                }
            }

            return false;
        }

        public int IndexOf(int x, int y)
        {
            return this.IndexOf(new Point(x, y));
        }

        private enum Movement {none=0, up, down, left, right};


        private static Dictionary<Tuple<Movement, Movement>, char> ArrowLut = new Dictionary<Tuple<Movement, Movement>, char>() 
        {
            {new Tuple<Movement, Movement>(Movement.left, Movement.left), Unicode.ArrowLeft},
            {new Tuple<Movement, Movement>(Movement.left, Movement.right), Unicode.ArrowClockWiseTurn},
            {new Tuple<Movement, Movement>(Movement.left, Movement.down), Unicode.ArrowDiagDownLeft},
            {new Tuple<Movement, Movement>(Movement.left, Movement.up), Unicode.ArrowDiagUpLeft},
            {new Tuple<Movement, Movement>(Movement.left, Movement.none), Unicode.ArrowLeft},

            {new Tuple<Movement, Movement>(Movement.right, Movement.left), Unicode.ArrowClockWiseTurn},
            {new Tuple<Movement, Movement>(Movement.right, Movement.right), Unicode.ArrowRight},
            {new Tuple<Movement, Movement>(Movement.right, Movement.down), Unicode.ArrowDiagDownRight},
            {new Tuple<Movement, Movement>(Movement.right, Movement.up), Unicode.ArrowDiagUpRight},
            {new Tuple<Movement, Movement>(Movement.right, Movement.none), Unicode.ArrowRight},

            {new Tuple<Movement, Movement>(Movement.down, Movement.left), Unicode.ArrowDiagDownLeft},
            {new Tuple<Movement, Movement>(Movement.down, Movement.right), Unicode.ArrowDiagDownRight},
            {new Tuple<Movement, Movement>(Movement.down, Movement.down), Unicode.ArrowDown},
            {new Tuple<Movement, Movement>(Movement.down, Movement.up), Unicode.ArrowClockWiseTurn},
            {new Tuple<Movement, Movement>(Movement.down, Movement.none), Unicode.ArrowDown},

            {new Tuple<Movement, Movement>(Movement.up, Movement.left), Unicode.ArrowDiagUpLeft},
            {new Tuple<Movement, Movement>(Movement.up, Movement.right), Unicode.ArrowDiagUpRight},
            {new Tuple<Movement, Movement>(Movement.up, Movement.down), Unicode.ArrowClockWiseTurn},
            {new Tuple<Movement, Movement>(Movement.up, Movement.up), Unicode.ArrowUp},
            {new Tuple<Movement, Movement>(Movement.up, Movement.none), Unicode.ArrowUp},

            {new Tuple<Movement, Movement>(Movement.none, Movement.left), Unicode.ArrowLeft},
            {new Tuple<Movement, Movement>(Movement.none, Movement.right), Unicode.ArrowRight},
            {new Tuple<Movement, Movement>(Movement.none, Movement.down), Unicode.ArrowDown},
            {new Tuple<Movement, Movement>(Movement.none, Movement.up), Unicode.ArrowUp},
            {new Tuple<Movement, Movement>(Movement.none, Movement.none), '!'},

        };

        public char getArrowSymbolForPos(int pos, bool allowDiagonalArrows)
        {
            if (allowDiagonalArrows)
            {
                pos = Range.clamp(pos, 0, Count - 1);
                Movement inDir = (pos != 0) ? findMovement(this[pos - 1], this[pos]) : Movement.none;
                Movement outDir = (pos != (this.Count - 1)) ? findMovement(this[pos], this[pos + 1]) : Movement.none;

                return ArrowLut[new Tuple<Movement, Movement>(inDir, outDir)];
            }
            else
            {
                Movement dir = (pos != (this.Count - 1)) ? findMovement(this[pos], this[pos + 1]) :
                    ((pos != 0) ? findMovement(this[pos - 1], this[pos]) : Movement.none);

                return (new char[] {' ', Unicode.ArrowUp, Unicode.ArrowDown, Unicode.ArrowLeft, Unicode.ArrowRight})[(int)dir];
            }
        }

        private Movement findMovement(Point from, Point to)
        {
            if (from == to.getPointBelow()) //moving up
            {
                return Movement.up;
            }
            else if (from == to.getPointAbove()) //moving down
            {
                return Movement.down;
            }
            else if (from == to.getPointLeft()) //moving right
            {
                return Movement.right;
            }
            else if (from == to.getPointRight()) // moving left
            {
                return Movement.left;
            }

            return Movement.none;
        }

        public void addPointsAfterIntersection(Path other)
        {
            int pos = other.IndexOf(CurrentPos);
            for(int i=pos+1; i<other.Count; i++)
            {
                Add(other[i]);
            }
        }

        /*internal static List<Point> findAllExplorablePoints(Maze maze)
        {
            List<Point> res = new List<Point>();
            List<Point> toExplore = maze.findAll(Maze.NodeType.Start);

            while (toExplore.Count > 0)
            {
                List<Point> nextExplore = new List<Point>();
                foreach (Point p in toExplore)
                {
                    nextExplore.Add(p.getPointRight());
                    nextExplore.Add(p.getPointLeft());
                    nextExplore.Add(p.getPointAbove());
                    nextExplore.Add(p.getPointBelow());

                    //add all floor points inside the maze that have not been seen yet
                    if (maze.pointInInnerPartOfMaze(p) && (maze[p] == MazeBlock.floor) && (!res.Contains(p)))
                    {
                        res.Add(p);
                    }
                }
                toExplore = nextExplore;
            }

            return res;
        }*/
    }
}
