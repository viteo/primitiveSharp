﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.Primitives;
using SixLabors.Shapes;
using System.Linq;

namespace primitive
{
    public class RectangleStraight : IShape
    {
        public Worker Worker { get; set; }
        public int X1 { get; set; }
        public int X2 { get; set; }
        public int Y1 { get; set; }
        public int Y2 { get; set; }

        public RectangleStraight(Worker worker)
        {
            var rnd = worker.Rnd;
            X1 = rnd.Next(worker.W);
            Y1 = rnd.Next(worker.H);
            X2 = Util.ClampInt(X1 + rnd.Next(32) + 1, 0, worker.W - 1);
            Y2 = Util.ClampInt(Y1 + rnd.Next(32) + 1, 0, worker.H - 1);
            Worker = worker;
        }

        public RectangleStraight(Worker worker, int x1, int y1, int x2, int y2)
        {
            Worker = worker;
            X1 = x1; Y1 = y1;
            X2 = x2; Y2 = y2;
        }

        private void CheckBounds()
        {
            int x1 = X1, y1 = Y1;
            int x2 = X2, y2 = Y2;
            if (x1 > x2) { X1 = x2; X2 = x1; }
            else { X1 = x1; X2 = x2; }
            if (y1 > y2) { Y1 = y2; Y2 = y1; }
            else { Y1 = y1; Y2 = y2; }
        }

        public void Draw(Image<Rgba32> image, Rgba32 color, double scale)
        {
            CheckBounds();
            PathBuilder pb = new PathBuilder();
            pb.AddLine(X1, Y1, X1, Y2);
            pb.AddLine(X1, Y2, X2, Y2);
            pb.AddLine(X2, Y2, X2, Y1);
            image.Mutate(im => im
            .Fill(color, pb.Build().Transform(Matrix3x2.CreateScale((float)scale))));

            //image.Mutate(im => im
            //.Fill(color, new RectangleF(X1 * s, Y1 * s, (X2 - X1 + 1) * s, (Y2 - Y1 + 1) * s))
            //);
        }

        public string SVG(string attrs)
        {
            CheckBounds();
            var w = X2 - X1 + 1;
            var h = Y2 - Y1 + 1;
            return $"<rect {attrs} x=\"{X1}\" y=\"{Y1}\" width=\"{w}\" height=\"{h}\" />";
        }

        public IShape Copy()
        {
            return new RectangleStraight(Worker, X1, Y1, X2, Y2);
        }

        public void Mutate()
        {
            var w = Worker.W;
            var h = Worker.H;
            var rnd = Worker.Rnd;
            switch (rnd.Next(2))
            {
                case 0:
                    X1 = Util.ClampInt(X1 + (int)(rnd.NextGaussian() * 16), 0, w - 1);
                    Y1 = Util.ClampInt(Y1 + (int)(rnd.NextGaussian() * 16), 0, h - 1); break;
                case 1:
                    X2 = Util.ClampInt(X2 + (int)(rnd.NextGaussian() * 16), 0, w - 1);
                    Y2 = Util.ClampInt(Y2 + (int)(rnd.NextGaussian() * 16), 0, h - 1); break;
            }
        }

        public List<Scanline> Rasterize()
        {
            CheckBounds();
            var lines = new List<Scanline>();
            for (int y = Y1; y <= Y2; y++)
                lines.Add(new Scanline() { Y = y, X1 = this.X1, X2 = this.X2, Alpha = 0xffff });
            return lines;
        }
    }

    public class RectangleRotated : IShape
    {
        public Worker Worker { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Sx { get; set; }
        public int Sy { get; set; }
        public int Angle { get; set; }

        public RectangleRotated(Worker worker)
        {
            var rnd = worker.Rnd;
            X = rnd.Next(worker.W);
            Y = rnd.Next(worker.H);
            Sx = rnd.Next(32) + 1;
            Sy = rnd.Next(32) + 1;
            Angle = rnd.Next(360);
            Worker = worker;
            Mutate();
        }

        public RectangleRotated(Worker worker, int x, int y, int sx, int sy, int angle)
        {
            Worker = worker;
            X = x; Y = y;
            Sx = sx; Sy = sy;
            Angle = angle;
        }

        public void Draw(Image<Rgba32> image, Rgba32 color, double scale)
        {
            PathBuilder pb = new PathBuilder();
            var x1 = X - Sx / 2;
            var x2 = X + Sx / 2;
            var y1 = Y - Sy / 2;
            var y2 = Y + Sy / 2;
            pb.AddLine(x1, y1, x1, y2);
            pb.AddLine(x1, y2, x2, y2);
            pb.AddLine(x2, y2, x2, y1);
            image.Mutate(im => im
                .Fill(color, pb.Build().Transform(Matrix3x2.CreateScale((float)scale)).RotateDegree(Angle)));
        }

        public string SVG(string attrs)
        {
            return String.Format("<g transform=\"translate({0} {1}) rotate({2}) scale({3} {4})\"><rect {5} x=\"-0.5\" y=\"-0.5\" width=\"1\" height=\"1\" /></g>",
                X, Y, Angle, Sx, Sy, attrs);
        }

        public IShape Copy()
        {
            return new RectangleRotated(Worker, X, Y, Sx, Sy, Angle);
        }

        public void Mutate()
        {
            var w = Worker.W;
            var h = Worker.H;
            var rnd = Worker.Rnd;

            switch (rnd.Next(3))
            {
                case 0:
                    X = Util.ClampInt(X + (int)(rnd.NextGaussian() * 16), 0, w - 1);
                    Y = Util.ClampInt(Y + (int)(rnd.NextGaussian() * 16), 0, h - 1); break;
                case 1:
                    Sx = Util.ClampInt(Sx + (int)(rnd.NextGaussian() * 16), 0, w - 1);
                    Sy = Util.ClampInt(Sy + (int)(rnd.NextGaussian() * 16), 0, h - 1); break;
                case 3:
                    Angle = Angle + (int)(rnd.NextGaussian() * 32); break;
            }
            //while (!Valid())
            //{
            //    Sx = Util.ClampInt(Sx + (int)(rnd.NextGaussian() * 16), 0, w - 1);
            //    Sy = Util.ClampInt(Sy + (int)(rnd.NextGaussian() * 16), 0, h - 1);
            //}
        }

        private bool Valid()
        {
            int a = Sx, b = Sy;
            if (a < b) { a = Sy; b = Sx; }
            var aspect = (double)a / (double)b;
            return aspect <= 5;
        }

        public List<Scanline> Rasterize()
        {
            var w = Worker.W;
            var h = Worker.H;

            PathBuilder pb = new PathBuilder();
            var x1 = X - Sx / 2;
            var x2 = X + Sx / 2;
            var y1 = Y - Sy / 2;
            var y2 = Y + Sy / 2;
            pb.AddLine(x1, y1, x1, y2);
            pb.AddLine(x1, y2, x2, y2);
            pb.AddLine(x2, y2, x2, y1);
            pb.AddLine(x2, y1, x1, y1);
            var rect = pb.Build().RotateDegree(Angle);

            List<Scanline> lines = new List<Scanline>();


            var bounds = rect.Bounds;
            var bot = Util.ClampInt((int)bounds.Bottom, 0, h - 1);
            var top = Util.ClampInt((int)bounds.Top, 0, h - 1);
            for (int i = bot; i >= top; i--)
            {
                var interscertions = rect.FindIntersections(new PointF(bounds.Left, i), new PointF(bounds.Right, i)).ToList<PointF>();
                if (interscertions.Count != 0 && interscertions.Count % 2 == 0)
                {
                    x1 = Util.ClampInt((int)interscertions[0].X, 0, w - 1);
                    x2 = Util.ClampInt((int)interscertions[1].X, 0, w - 1);
                    lines.Add(new Scanline { Alpha = 0xffff, X1 = Math.Min(x1, x2), X2 = Math.Max(x1, x2), Y = i });
                }
                //else if (interscertions.Count == 1)
                //{
                //    Console.WriteLine(interscertions.Count);
                //    using (Image<Rgba32> image = new Image<Rgba32>(w, h))
                //    {
                //        image.Mutate(im => im
                //        .Fill(Rgba32.White).Fill(Rgba32.Black, rect));
                //        image.Save(@"c:\programs\rect.png");
                //    }
                //}
            }




            //var angle = Util.Radians(Angle);
            //var (rx1, ry1) = Util.Rotate(-Sx / 2, -Sy / 2, angle);
            //var (rx2, ry2) = Util.Rotate(Sx / 2, -Sy / 2, angle);
            //var (rx3, ry3) = Util.Rotate(Sx / 2, Sy / 2, angle);
            //var (rx4, ry4) = Util.Rotate(-Sx / 2, Sy / 2, angle);
            //int x1 = (int)rx1 + X, y1 = (int)ry1 + Y;
            //int x2 = (int)rx2 + X, y2 = (int)ry2 + Y;
            //int x3 = (int)rx3 + X, y3 = (int)ry3 + Y;
            //int x4 = (int)rx4 + X, y4 = (int)ry4 + Y;
            //var miny = Math.Min(y1, Math.Min(y2, Math.Min(y3, y4)));
            //var maxy = Math.Max(y1, Math.Max(y2, Math.Max(y3, y4)));
            //var n = maxy - miny + 1;
            //int[] min = new int[n];
            //int[] max = new int[n];
            //for (int i = 0; i < min.Length; i++)
            //    min[i] = w;
            //var xs = new int[] { x1, x2, x3, x4, x1 };
            //var ys = new int[] { y1, y2, y3, y4, y1 };
            //// TODO: this could be better probably
            //for (int i = 0; i < 4; i++)
            //{
            //    double x = xs[i], y = ys[i];
            //    double dx = xs[i + 1] - xs[i], dy = ys[i + 1] - ys[i];
            //    int count = (int)(Math.Sqrt(dx * dx + dy * dy)) * 2;
            //    for (int j = 0; j < count; j++)
            //    {
            //        double t = j / (double)(count - 1);
            //        int xi = (int)(x + dx * t);
            //        int yi = (int)(y + dy * t) - miny;
            //        min[yi] = Math.Min(min[yi], xi);
            //        max[yi] = Math.Max(max[yi], xi);
            //    }
            //}
            //var lines = new List<Scanline>();
            //for (int i = 0; i < n; i++)
            //{
            //    var y = miny + i;
            //    if (y < 0 || y >= h)
            //        continue;
            //    var a = Math.Max(min[i], 0);
            //    var b = Math.Min(max[i], w - 1);
            //    if (b >= a)
            //        lines.Add(new Scanline() { Y = y, X1 = a, X2 = b, Alpha = 0xffff });
            //}
            return lines;
        }







    }

}
