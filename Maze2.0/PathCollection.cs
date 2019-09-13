/*  ---------------------------------------------------------------------------------------------------------------------------------------
 *  (C) 2019, Dr Warren Creemers.
 *  This file is subject to the terms and conditions defined in the included file 'LICENSE.txt'
 *  ---------------------------------------------------------------------------------------------------------------------------------------
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WD_toolbox.Data.DataStructures;

namespace Maze2._0
{
    public class PathCollection : VList<Path>
    {
        Dictionary <Point, Path> existingPoints;
        public PathCollection() : base()
        {
            existingPoints = new Dictionary<Point,Path>();
        }

        public PathCollection(int capacity) : base(capacity)
        {
            existingPoints = new Dictionary<Point,Path>();
        }

        public PathCollection(IEnumerable<Path> collection) : base(collection)
        {
            existingPoints = new Dictionary<Point,Path>();
            rebuildExistingPointsTable();
        }

        public PathCollection(PathCollection other) : base(other)
        {
            existingPoints = new Dictionary<Point,Path>(other.existingPoints);
        }

        protected void rebuildExistingPointsTable()
        {
            existingPoints.Clear();
 	        foreach(Path path in this)
            {
                foreach(Point p in path)
                {
                    if(!existingPoints.ContainsKey(p))
                    {
                        existingPoints.Add(p, path);
                    }
                }
            }
        }

        public Path FindFirstIntersectingPath(Point p)
        {
            return existingPoints.ContainsKey(p) ? existingPoints[p] : null;
        }

        public override void Add(Path item)
        {
 	        base.Add(item);
            foreach(Point p in item)
            {
                if(!existingPoints.ContainsKey(p))
                {
                    existingPoints.Add(p, item);
                }
            }
        }

        public override void Clear()
        {
 	         base.Clear();
        }

        public override bool Remove(Path item)
        {
 	        bool r = base.Remove(item);
            rebuildExistingPointsTable();
            return r;
        }

        public override void RemoveAt(int index)
        {
 	        base.RemoveAt(index);
            rebuildExistingPointsTable();
        }

        public virtual bool Contains(Point point)
        {
            //temp code because something seems funny in the hashing function
            /*foreach (Point p in existingPoints.Keys)
            {
                if ((p.X == point.X) && (p.Y == point.Y))
                {
                    return true;
                }
            }

            return false;*/
            return existingPoints.ContainsKey(point);
        }
    }
}
