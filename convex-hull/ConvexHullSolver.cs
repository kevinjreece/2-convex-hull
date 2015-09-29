using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;

namespace _2_convex_hull
{
    class ConvexHullSolver
    {
        System.Drawing.Graphics g;
        System.Windows.Forms.PictureBox pictureBoxView;

        public ConvexHullSolver(System.Drawing.Graphics g, System.Windows.Forms.PictureBox pictureBoxView)
        {
            this.g = g;
            this.pictureBoxView = pictureBoxView;
        }

        public void Refresh()
        {
            // Use this especially for debugging and whenever you want to see what you have drawn so far
            pictureBoxView.Refresh();
        }

        public void Pause(int milliseconds)
        {
            // Use this especially for debugging and to animate your algorithm slowly
            pictureBoxView.Refresh();
            System.Threading.Thread.Sleep(milliseconds);
        }

        public void Solve(List<PointF> pointList)
        {
            pointList.Sort(new PointComparer());
            Polygon poly = getConvexHull(pointList);
            poly.draw(g);
        }

        public Polygon getConvexHull(List<PointF> pointList)
        {
            //Console.WriteLine("getConvexHull");
            Polygon poly;
            int count = pointList.Count;
            if (count > 3)
            {
                List<PointF> left = pointList.GetRange(0, count / 2);// count = 5, index = 0, num = 5/2 = 2 => 0, 1
                List<PointF> right = pointList.GetRange(count / 2, (count % 2 == 0 ? count / 2 : count / 2 + 1));// count = 5, index = 5/2 = 2, num = 5/2 = 2 => 2, 3
                poly = mergeConvexHulls(getConvexHull(left), getConvexHull(right));
            }
            else if (count == 3)
            {
                poly = create3PointPolygon(pointList);
            }
            else if (count == 2)
            {
                poly = create2PointPolygon(pointList);
            }
            else
            {
                poly = null;
            }
            //poly.draw(g);

            return poly;
        }

        public Polygon create2PointPolygon(List<PointF> pointlist)
        {
            //Console.WriteLine("create2PointPolygon");
            if (pointlist.Count == 2)
            {
                DirectedPoint left = new DirectedPoint(pointlist.ElementAt(0));
                DirectedPoint right = new DirectedPoint(pointlist.ElementAt(1));
                left.setNext(right);
                right.setNext(left);
                return new Polygon(left, right);
            }
            else
            {
                return null;
            }
        }
            
        public Polygon create3PointPolygon(List<PointF> pointlist)
        {
            //Console.WriteLine("create3PointPolygon");
            if (pointlist.Count == 3)
            {
                DirectedPoint left = new DirectedPoint(pointlist.ElementAt(0));
                DirectedPoint center = new DirectedPoint(pointlist.ElementAt(1));
                DirectedPoint right = new DirectedPoint(pointlist.ElementAt(2));
                double slopeLC = getSlope(left, center);
                double slopeCR = getSlope(center, right);
                if (slopeLC <= slopeCR)
                {
                    left.setNext(center);
                    center.setNext(right);
                    right.setNext(left);
                }
                else
                {
                    left.setNext(right);
                    center.setNext(left);
                    right.setNext(center);
                }
                return new Polygon(left, right);
            }
            else
            {
                return null;
            }
        }

        public double getSlope(DirectedPoint left, DirectedPoint right)
        {
            //Console.WriteLine("getSlope for (" + left.getPoint().X + ", " + left.getPoint().Y + ") and (" + right.getPoint().X + ", " + right.getPoint().Y + ")\n");
            double rise = right.getPoint().Y - left.getPoint().Y;
            double run = right.getPoint().X - left.getPoint().X;
            if (run == 0) { Console.WriteLine("Run == 0"); }
            return rise / run;
        }

        public Polygon mergeConvexHulls(Polygon left, Polygon right)
        {
            //Console.WriteLine("mergeConvexHulls");
            DirectedPoint topLeft = left.getRight();
            DirectedPoint bottomLeft = topLeft;
            DirectedPoint topRight = right.getLeft();
            DirectedPoint bottomRight = topRight;
            DirectedPoint temp;
            bool leftDone = false;
            bool rightDone = false;

            while (!leftDone || !rightDone)
            {
                while (!leftDone)
                {
                    temp = topLeft.getPrev();
                    if (getSlope(temp, topRight) > getSlope(topLeft, topRight))
                    {
                        topLeft = temp;
                        rightDone = false;
                    }
                    else
                    {
                        leftDone = true;
                    }
                }
                while (!rightDone)
                {
                    temp = topRight.getNext();
                    if (getSlope(topLeft, temp) < getSlope(topLeft, topRight))
                    {
                        topRight = temp;
                        leftDone = false;
                    }
                    else
                    {
                        rightDone = true;
                    }
                }
            }

            leftDone = false;
            rightDone = false;

            while (!leftDone || !rightDone)
            {
                while (!leftDone)
                {
                    temp = bottomLeft.getNext();
                    if (getSlope(temp, bottomRight) < getSlope(bottomLeft, bottomRight))
                    {
                        bottomLeft = temp;
                        rightDone = false;
                    }
                    else
                    {
                        leftDone = true;
                    }
                }
                while (!rightDone)
                {
                    temp = bottomRight.getPrev();
                    if (getSlope(bottomLeft, temp) > getSlope(bottomLeft, bottomRight))
                    {
                        bottomRight = temp;
                        leftDone = false;
                    }
                    else
                    {
                        rightDone = true;
                    }
                }
            }

            topLeft.setNext(topRight);
            bottomRight.setNext(bottomLeft);
            left.setRight(right.getRight());

            return left;
        }

        
    }

    class Polygon
    {
        private DirectedPoint left;
        private DirectedPoint right;

        public Polygon(DirectedPoint left, DirectedPoint right)
        {
            this.left = left;
            this.right = right;
        }

        public void draw(Graphics g)
        {
            Pen pen = new Pen(Color.Black);
            DirectedPoint start = this.getLeft();
            DirectedPoint curr = start;
            //int x = 0;
            do
            {
                //if (++x > 50) { pen = new Pen(Color.Red); }
                DirectedPoint next = curr.getNext();
                //Console.WriteLine("Draw (" + curr.getPoint().X + ", " + curr.getPoint().Y + ") to (" + next.getPoint().X + ", " + next.getPoint().Y + ")\n");
                g.DrawLine(pen, curr.getPoint(), next.getPoint());
                curr = next;
            } while (curr != start);
        }

        public void setLeft(DirectedPoint left)
        {
            this.left = left;
        }

        public void setRight(DirectedPoint right)
        {
            this.right = right;
        }

        public DirectedPoint getLeft()
        {
            return this.left;
        }

        public DirectedPoint getRight()
        {
            return this.right;
        }
    }

    class DirectedPoint
    {
        private PointF point;
        private DirectedPoint next;
        private DirectedPoint prev;

        public DirectedPoint(PointF point)
        {
            this.point = point;
        }

        public PointF getPoint()
        {
            return this.point;
        }

        public void setNext(DirectedPoint next)
        {
            this.next = next;
            if (this.next.getPrev() != this) { this.next.setPrev(this); }
        }

        public void setPrev(DirectedPoint prev)
        {
            this.prev = prev;
            if (this.prev.getNext() != this) { this.prev.setNext(this); }
        }

        public DirectedPoint getNext()
        {
            return this.next;
        }

        public DirectedPoint getPrev()
        {
            return this.prev;
        }
    }

    class PointComparer : IComparer<PointF>
    {
        public int Compare(PointF a, PointF b)
        {
            double XDiff = a.X - b.X;
            double YDiff = a.Y - b.Y;
            if (XDiff == 0)
            {
                if (YDiff > 0) { return 1; }
                else if (YDiff < 0) { return -1; }
                else { return 0; }
            }
            else {
                if (XDiff > 0) { return 1; }
                else if (XDiff < 0) { return -1; }
                else { return 0; }
            }
        }
    }
}
