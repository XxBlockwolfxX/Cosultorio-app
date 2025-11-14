using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ConsultorioDentalApp.Forms
{
    public class OdontogramaDendooControl : UserControl
    {
        private enum ToothRow
        {
            UpperMain,
            Occlusal,
            LowerMain
        }

        private class Tooth
        {
            public int Number;
            public RectangleF Bounds;
            public ToothRow Row;
        }

        private readonly List<Tooth> _teeth = new List<Tooth>();
        private readonly ToolTip _toolTip = new ToolTip();

        public OdontogramaDendooControl()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.BackColor = Color.White;
            RecalculateLayout();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecalculateLayout();
            Invalidate();
        }

        private void RecalculateLayout()
        {
            _teeth.Clear();

            if (Width < 400 || Height < 200) return;

            // Margenes y tamaños básicos
            float marginLeft = 40;
            float marginRight = 40;
            float usableWidth = Width - marginLeft - marginRight;
            int teethPerRow = 16;
            float gapX = usableWidth / teethPerRow;

            // Y-base para cada fila
            float upperY = 40;
            float occlusalY = 155;
            float lowerY = 235;

            // Tamaños de dientes
            float mainToothWidth = gapX * 0.8f;
            float mainToothHeight = 70;
            float occlusalSize = gapX * 0.5f;

            // Números aproximados (FDI) para mostrar debajo
            int[] upperNumbers = { 18, 17, 16, 15, 14, 13, 12, 11, 21, 22, 23, 24, 25, 26, 27, 28 };
            int[] lowerNumbers = { 48, 47, 46, 45, 44, 43, 42, 41, 31, 32, 33, 34, 35, 36, 37, 38 };

            // Fila superior principal
            for (int i = 0; i < teethPerRow; i++)
            {
                float centerX = marginLeft + gapX * (i + 0.5f);
                RectangleF r = new RectangleF(
                    centerX - mainToothWidth / 2f,
                    upperY,
                    mainToothWidth,
                    mainToothHeight);

                _teeth.Add(new Tooth
                {
                    Number = upperNumbers[i],
                    Bounds = r,
                    Row = ToothRow.UpperMain
                });
            }

            // Fila oclusal (pequeños)
            for (int i = 0; i < teethPerRow; i++)
            {
                float centerX = marginLeft + gapX * (i + 0.5f);
                RectangleF r = new RectangleF(
                    centerX - occlusalSize / 2f,
                    occlusalY,
                    occlusalSize,
                    occlusalSize);

                _teeth.Add(new Tooth
                {
                    Number = 0, // no mostramos número aquí
                    Bounds = r,
                    Row = ToothRow.Occlusal
                });
            }

            // Fila inferior principal
            for (int i = 0; i < teethPerRow; i++)
            {
                float centerX = marginLeft + gapX * (i + 0.5f);
                RectangleF r = new RectangleF(
                    centerX - mainToothWidth / 2f,
                    lowerY,
                    mainToothWidth,
                    mainToothHeight);

                _teeth.Add(new Tooth
                {
                    Number = lowerNumbers[i],
                    Bounds = r,
                    Row = ToothRow.LowerMain
                });
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            if (_teeth.Count == 0) return;

            DrawBackgroundLines(g);
            DrawTeeth(g);
            DrawNumbers(g);
        }

        private void DrawBackgroundLines(Graphics g)
        {
            // Líneas horizontales detrás de arcadas (gris claro)
            using (var pen = new Pen(Color.FromArgb(220, 220, 220), 1f))
            {
                float topLineY = 30;
                float bottomLineY = 210;
                for (int i = 0; i < 10; i++)
                {
                    float y = topLineY + i * 8;
                    g.DrawLine(pen, 20, y, Width - 20, y);
                }
                for (int i = 0; i < 10; i++)
                {
                    float y = bottomLineY + i * 8;
                    g.DrawLine(pen, 20, y, Width - 20, y);
                }
            }

            // Línea roja central
            using (var penRed = new Pen(Color.FromArgb(200, 60, 60), 2f))
            {
                g.DrawLine(penRed, 20, 110, Width - 20, 110);
            }
        }

        private void DrawTeeth(Graphics g)
        {
            foreach (var t in _teeth)
            {
                switch (t.Row)
                {
                    case ToothRow.UpperMain:
                        DrawMainTooth(g, t.Bounds, true);
                        break;
                    case ToothRow.LowerMain:
                        DrawMainTooth(g, t.Bounds, false);
                        break;
                    case ToothRow.Occlusal:
                        DrawOcclusalTooth(g, t.Bounds);
                        break;
                }
            }
        }

        private void DrawMainTooth(Graphics g, RectangleF r, bool upper)
        {
            using (var pen = new Pen(Color.FromArgb(90, 90, 90), 1.4f))
            {
                // corona
                float crownHeight = r.Height * 0.4f;
                RectangleF crown = new RectangleF(r.X, r.Y, r.Width, crownHeight);

                GraphicsPath path = new GraphicsPath();
                float radius = r.Width * 0.25f;

                // curva superior
                path.AddArc(crown.X, crown.Y, radius, crownHeight, 180, 90);
                path.AddArc(crown.Right - radius, crown.Y, radius, crownHeight, 270, 90);
                path.AddLine(crown.Right, crown.Bottom, crown.X, crown.Bottom);
                path.CloseFigure();

                g.DrawPath(pen, path);

                // separación de cúspides (opcional)
                g.DrawLine(pen, crown.X + crown.Width * 0.33f, crown.Y + crownHeight * 0.2f,
                               crown.X + crown.Width * 0.33f, crown.Bottom - crownHeight * 0.1f);
                g.DrawLine(pen, crown.X + crown.Width * 0.66f, crown.Y + crownHeight * 0.2f,
                               crown.X + crown.Width * 0.66f, crown.Bottom - crownHeight * 0.1f);

                // raíces
                float rootTop = crown.Bottom;
                float rootBottom = r.Bottom;
                float centerX = r.X + r.Width / 2f;
                float rootWidth = r.Width * 0.3f;

                if (upper)
                {
                    // dos raíces
                    RectangleF leftRoot = new RectangleF(centerX - rootWidth - 2, rootTop, rootWidth, rootBottom - rootTop);
                    RectangleF rightRoot = new RectangleF(centerX + 2, rootTop, rootWidth, rootBottom - rootTop);

                    DrawRoot(g, pen, leftRoot, true);
                    DrawRoot(g, pen, rightRoot, true);
                }
                else
                {
                    // raíz única más ancha
                    RectangleF root = new RectangleF(centerX - rootWidth / 1.5f, rootTop, rootWidth * 1.5f, rootBottom - rootTop);
                    DrawRoot(g, pen, root, false);
                }
            }
        }

        private void DrawRoot(Graphics g, Pen pen, RectangleF r, bool splitEnd)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddBezier(
                new PointF(r.X, r.Y),
                new PointF(r.X - r.Width * 0.2f, r.Y + r.Height * 0.5f),
                new PointF(r.X + r.Width * 0.1f, r.Bottom),
                new PointF(r.X + r.Width * 0.2f, r.Bottom));

            float endX = r.Right;
            path.AddBezier(
                new PointF(endX, r.Y),
                new PointF(endX + r.Width * 0.2f, r.Y + r.Height * 0.5f),
                new PointF(endX - r.Width * 0.1f, r.Bottom),
                new PointF(endX - r.Width * 0.2f, r.Bottom));

            g.DrawPath(pen, path);

            if (splitEnd)
            {
                // pequeña bifurcación
                g.DrawLine(pen, r.X + 2, r.Bottom, r.X - r.Width * 0.1f, r.Bottom + 4);
                g.DrawLine(pen, r.Right - 2, r.Bottom, r.Right + r.Width * 0.1f, r.Bottom + 4);
            }
        }

        private void DrawOcclusalTooth(Graphics g, RectangleF r)
        {
            using (var pen = new Pen(Color.FromArgb(90, 90, 90), 1.3f))
            {
                GraphicsPath p = new GraphicsPath();
                float w = r.Width;
                float h = r.Height;
                float cx = r.X + w / 2f;
                float cy = r.Y + h / 2f;

                // contorno romboidal
                p.AddPolygon(new[]
                {
                    new PointF(cx, r.Y),
                    new PointF(r.Right, cy),
                    new PointF(cx, r.Bottom),
                    new PointF(r.X, cy)
                });
                g.DrawPath(pen, p);

                // cruz oclusal
                g.DrawLine(pen, cx, r.Y + 3, cx, r.Bottom - 3);
                g.DrawLine(pen, r.X + 3, cy, r.Right - 3, cy);
            }
        }

        private void DrawNumbers(Graphics g)
        {
            using (var brush = new SolidBrush(Color.FromArgb(60, 60, 60)))
            using (var font = new Font("Segoe UI", 8f))
            {
                foreach (var t in _teeth)
                {
                    if (t.Row == ToothRow.UpperMain && t.Number != 0)
                    {
                        string text = t.Number.ToString();
                        SizeF size = g.MeasureString(text, font);
                        PointF pos = new PointF(
                            t.Bounds.X + (t.Bounds.Width - size.Width) / 2f,
                            t.Bounds.Bottom + 2);
                        g.DrawString(text, font, brush, pos);
                    }
                    else if (t.Row == ToothRow.LowerMain && t.Number != 0)
                    {
                        string text = t.Number.ToString();
                        SizeF size = g.MeasureString(text, font);
                        PointF pos = new PointF(
                            t.Bounds.X + (t.Bounds.Width - size.Width) / 2f,
                            t.Bounds.Y - size.Height - 2);
                        g.DrawString(text, font, brush, pos);
                    }
                }
            }
        }

        // Opcional: al hacer click muestra qué diente se seleccionó
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            foreach (var t in _teeth)
            {
                if (t.Bounds.Contains(e.Location))
                {
                    _toolTip.Show(
                        t.Number == 0 ? "Superficie oclusal" : $"Diente {t.Number}",
                        this,
                        e.Location,
                        1500);
                    break;
                }
            }
        }
    }
}
