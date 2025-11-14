using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ConsultorioDentalApp.Forms   // o .Controls si prefieres
{
    public partial class OdontogramaControl : UserControl
    {
        // ==========================
        //  ESTADO INTERNO
        // ==========================
        private Panel pnlOdontograma;

        private readonly List<(string Tipo, int Inicio, int Fin, string Estado)> protesisLista
            = new List<(string, int, int, string)>();

        private class PiezaTag
        {
            public int NumeroDiente { get; set; }
            public FaceState Estado { get; set; } = new FaceState();
        }

        private class FaceState
        {
            public Color FillColor = Color.White;
            public Color BorderColor = Color.Gray;
            public string Overlay = "None";
        }

        // Evento opcional para que el formulario sepa qué pieza / cara se tocó
        public event Action<int, string> CaraSeleccionada;

        public OdontogramaControl()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            DoubleBuffered = true;

            pnlOdontograma = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true
            };
            Controls.Add(pnlOdontograma);

            GenerarOdontograma();

            // Pintar prótesis sobre todo el odontograma
            pnlOdontograma.Paint += (s, e) => DibujarProtesis(e.Graphics);
        }

        // =====================================================
        //  API PÚBLICA PARA PRÓTESIS (USADA DESDE EL FORM)
        // =====================================================
        public void AplicarProtesis(string tipo, int inicio, int fin, string estado)
        {
            // Removible Parcial por rango
            if (tipo == "Removible Parcial")
            {
                bool yaExisteMismoRango = protesisLista.Any(p =>
                    p.Tipo == tipo && p.Inicio == inicio && p.Fin == fin);

                if (yaExisteMismoRango)
                {
                    int idx = protesisLista.FindIndex(p =>
                        p.Tipo == tipo && p.Inicio == inicio && p.Fin == fin);

                    protesisLista[idx] = (tipo, inicio, fin, estado);
                }
                else
                {
                    protesisLista.Add((tipo, inicio, fin, estado));
                }

                pnlOdontograma.Refresh();
                return;
            }

            // Prótesis total (Superior / Inferior)
            var existente = protesisLista.FirstOrDefault(p => p.Tipo == tipo);
            if (!string.IsNullOrEmpty(existente.Tipo))
            {
                int index = protesisLista.FindIndex(p => p.Tipo == tipo);
                protesisLista[index] = (tipo, inicio, fin, estado);
            }
            else
            {
                protesisLista.Add((tipo, inicio, fin, estado));
            }

            pnlOdontograma.Refresh();
        }

        // Por si quieres limpiar todo desde el formulario
        public void LimpiarTodo()
        {
            foreach (var pieza in pnlOdontograma.Controls.OfType<Panel>())
            {
                if (pieza.Tag is PiezaTag info)
                    info.Estado = new FaceState();

                foreach (var btn in pieza.Controls.OfType<Button>())
                {
                    if (btn.Tag is FaceState st)
                    {
                        st.FillColor = Color.White;
                        st.Overlay = "None";
                        btn.Invalidate();
                    }
                }
                pieza.Invalidate();
            }

            protesisLista.Clear();
            pnlOdontograma.Refresh();
        }

        // =====================================================
        //  GENERACIÓN DEL ODONTOGRAMA
        // =====================================================
        private void GenerarOdontograma()
        {
            pnlOdontograma.Controls.Clear();
            pnlOdontograma.SuspendLayout();

            int anchoPanel = pnlOdontograma.Width;
            if (anchoPanel <= 0) anchoPanel = 1000;

            // === CENTRO DEL PANEL ===
            int centroX = anchoPanel / 2;

            // === CONFIG ===
            int espacioEntreDientes = 58;
            int topInicial = 40;
            int alturaFila = 80;
            int espacioEntreArcos = 150;

            // ======================================
            // GRUPOS DE DIENTES (F.D.I)
            // ======================================

            // Adultos
            int[] supIzq = { 18, 17, 16, 15, 14, 13, 12, 11 };
            int[] supDer = { 21, 22, 23, 24, 25, 26, 27, 28 };
            int[] infIzq = { 48, 47, 46, 45, 44, 43, 42, 41 };
            int[] infDer = { 31, 32, 33, 34, 35, 36, 37, 38 };

            // Temporales
            int[] supTempIzq = { 55, 54, 53, 52, 51 };
            int[] supTempDer = { 61, 62, 63, 64, 65 };
            int[] infTempIzq = { 85, 84, 83, 82, 81 };
            int[] infTempDer = { 71, 72, 73, 74, 75 };

            // ======================================
            // CÁLCULO DE POSICIONES CENTRADAS
            // ======================================
            int anchoArcoAdulto = supIzq.Length * espacioEntreDientes;
            int gap = 40;

            // Ajuste para mover la parte derecha
            int desplazamientoDerecha = 25; // Aumenta si quieres mover más

            int inicioIzquierda = centroX - (anchoArcoAdulto + gap);
            int inicioDerecha = centroX + gap + desplazamientoDerecha;

            // Evitar que se salga si la pantalla es pequeña
            if (inicioIzquierda < 10)
            {
                inicioIzquierda = 10;
                inicioDerecha = inicioIzquierda + anchoArcoAdulto + (gap * 2) + desplazamientoDerecha;
            }

            int offsetY = topInicial;

            // ======================================
            // CREAR ARCADAS ADULTAS
            // ======================================
            CrearFilaArco(supIzq, inicioIzquierda, offsetY);
            CrearFilaArco(supDer, inicioDerecha, offsetY);

            // ======================================
            // CREAR ARCADAS TEMPORALES SUPERIOR
            // ======================================
            CrearFilaArco(supTempIzq, inicioIzquierda + 110, offsetY + alturaFila);
            CrearFilaArco(supTempDer, inicioDerecha + 45, offsetY + alturaFila);

            // ======================================
            // CREAR ARCADAS ADULTAS INFERIOR
            // ======================================
            CrearFilaArco(infIzq, inicioIzquierda, offsetY + espacioEntreArcos + alturaFila);
            CrearFilaArco(infDer, inicioDerecha, offsetY + espacioEntreArcos + alturaFila);

            // ======================================
            // CREAR ARCADAS TEMPORALES INFERIOR
            // ======================================
            CrearFilaArco(infTempIzq, inicioIzquierda + 110, offsetY + espacioEntreArcos + (alturaFila * 2));
            CrearFilaArco(infTempDer, inicioDerecha + 45, offsetY + espacioEntreArcos + (alturaFila * 2));

            // ======================================
            // LINEAS DE CUADRANTES
            // ======================================
            pnlOdontograma.Paint += (s, e) =>
            {
                using (Pen pen = new Pen(Color.LightGray, 1.5f))
                {
                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                    int midX = pnlOdontograma.Width / 2;
                    int midY = offsetY + espacioEntreArcos + 50;

                    // Línea vertical central
                    e.Graphics.DrawLine(pen, midX, 0, midX, pnlOdontograma.Height);

                    // Línea horizontal central
                    e.Graphics.DrawLine(pen, 0, midY, pnlOdontograma.Width, midY);
                }
            };

            pnlOdontograma.ResumeLayout();
        }


        private void CrearFilaArco(int[] dientes, int startX, int startY)
        {
            int x = startX;

            foreach (int numero in dientes)
            {
                Panel pieza = new Panel
                {
                    Width = 48,
                    Height = 48,
                    Left = x,
                    Top = startY,
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Tag = new PiezaTag { NumeroDiente = numero }
                };

                pieza.Paint += (s, e) =>
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    using (Pen pen = new Pen(Color.FromArgb(100, 100, 100), 1))
                    {
                        // Marco exterior
                        Rectangle rectExterior = new Rectangle(0, 0, pieza.Width - 1, pieza.Height - 1);
                        e.Graphics.DrawRectangle(pen, rectExterior);

                        // Cuadro interior
                        int margenInterior = (int)(pieza.Width * 0.33);
                        Rectangle rectInterior = new Rectangle(
                            margenInterior,
                            margenInterior,
                            pieza.Width - 2 * margenInterior,
                            pieza.Height - 2 * margenInterior);
                        e.Graphics.DrawRectangle(pen, rectInterior);

                        // Diagonales
                        e.Graphics.DrawLine(pen, rectExterior.Left, rectExterior.Top,
                                                 rectInterior.Left, rectInterior.Top);
                        e.Graphics.DrawLine(pen, rectExterior.Right, rectExterior.Top,
                                                 rectInterior.Right, rectInterior.Top);
                        e.Graphics.DrawLine(pen, rectExterior.Left, rectExterior.Bottom,
                                                 rectInterior.Left, rectInterior.Bottom);
                        e.Graphics.DrawLine(pen, rectExterior.Right, rectExterior.Bottom,
                                                 rectInterior.Right, rectInterior.Bottom);
                    }

                    if (pieza.Tag is PiezaTag info && info.Estado.Overlay != "None")
                        DibujarOverlay(e.Graphics, pieza, info.Estado.Overlay);
                };

                Rectangle inner = new Rectangle(
                    (int)(pieza.Width * 0.33),
                    (int)(pieza.Height * 0.33),
                    (int)(pieza.Width * 0.34),
                    (int)(pieza.Height * 0.34));

                foreach (var cara in new[] { "V", "O", "M", "D", "L" })
                {
                    Button btn = CrearBotonCaraPoligonal(cara, inner, pieza.Width, pieza.Height);
                    pieza.Controls.Add(btn);
                }

                // Menú general de pieza al hacer clic derecho fuera de las caras
                pieza.MouseUp += (s, e) =>
                {
                    if (e.Button != MouseButtons.Right) return;
                    if (pieza.GetChildAtPoint(e.Location) != null) return;

                    pieza.ContextMenuStrip = CrearMenuContextual(pieza);
                    pieza.ContextMenuStrip.Show(pieza, e.Location);
                };

                pnlOdontograma.Controls.Add(pieza);

                Label lblNum = new Label
                {
                    Text = numero.ToString(),
                    AutoSize = false,
                    Width = 30,
                    Left = pieza.Left + (pieza.Width - 30) / 2,
                    Top = pieza.Bottom + 2,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 8, FontStyle.Bold),
                    ForeColor = Color.FromArgb(20, 60, 130)
                };
                pnlOdontograma.Controls.Add(lblNum);

                x += 58;
            }
        }

        private Button CrearBotonCaraPoligonal(string nombre, Rectangle inner, int w, int h)
        {
            Point[] puntos;

            switch (nombre)
            {
                case "V": // vestibular / superior
                    puntos = new[]
                    {
                        new Point(0, 0),
                        new Point(w, 0),
                        new Point(inner.Right, inner.Top),
                        new Point(inner.Left,  inner.Top)
                    };
                    break;

                case "O": // oclusal / inferior
                    puntos = new[]
                    {
                        new Point(inner.Left,  inner.Bottom),
                        new Point(inner.Right, inner.Bottom),
                        new Point(w, h),
                        new Point(0, h)
                    };
                    break;

                case "M": // mesial
                    puntos = new[]
                    {
                        new Point(0, 0),
                        new Point(inner.Left, inner.Top),
                        new Point(inner.Left, inner.Bottom),
                        new Point(0, h)
                    };
                    break;

                case "D": // distal
                    puntos = new[]
                    {
                        new Point(inner.Right, inner.Top),
                        new Point(w, 0),
                        new Point(w, h),
                        new Point(inner.Right, inner.Bottom)
                    };
                    break;

                default: // lingual / palatino
                    puntos = new[]
                    {
                        new Point(inner.Left,  inner.Top),
                        new Point(inner.Right, inner.Top),
                        new Point(inner.Right, inner.Bottom),
                        new Point(inner.Left,  inner.Bottom)
                    };
                    break;
            }

            int minX = puntos.Min(p => p.X);
            int minY = puntos.Min(p => p.Y);
            int maxX = puntos.Max(p => p.X);
            int maxY = puntos.Max(p => p.Y);

            Rectangle bounds = new Rectangle(minX, minY, maxX - minX, maxY - minY);
            Point[] local = puntos.Select(p => new Point(p.X - minX, p.Y - minY)).ToArray();

            Button btn = new Button
            {
                Bounds = bounds,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Tag = new FaceState(),
                Text = nombre,          // ← importante para guardar por cara
                Cursor = Cursors.Hand
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btn.FlatAppearance.MouseDownBackColor = Color.Transparent;

            GraphicsPath path = new GraphicsPath();
            path.AddPolygon(local);
            btn.Region = new Region(path);

            // Menú contextual de cara
            btn.ContextMenuStrip = CrearMenuContextual(btn);

            // Dibujo del color / overlay
            btn.Paint += (s, e) =>
            {
                var st = (FaceState)btn.Tag;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (SolidBrush b = new SolidBrush(st.FillColor == Color.White ? Color.Transparent : st.FillColor))
                    e.Graphics.FillPolygon(b, local);

                if (st.Overlay != "None")
                    DibujarOverlayCara(e.Graphics, btn.ClientRectangle, st.Overlay);
            };

            btn.Click += (s, e) =>
            {
                if (btn.Parent is Panel p && p.Tag is PiezaTag info)
                    CaraSeleccionada?.Invoke(info.NumeroDiente, nombre);
            };

            btn.BringToFront();
            return btn;
        }

        // =====================================================
        //  MENÚS CONTEXTUALES
        // =====================================================
        private ContextMenuStrip CrearMenuContextual(Button btn)
        {
            var st = (FaceState)btn.Tag;
            ContextMenuStrip menu = new ContextMenuStrip();

            menu.Items.Add("Restaurado (Azul)", null, (s, e) =>
            {
                st.FillColor = Color.SkyBlue;
                st.Overlay = "None";
                btn.Invalidate();
                btn.Parent.Invalidate();
            });

            menu.Items.Add("Por Restaurar (Rojo)", null, (s, e) =>
            {
                st.FillColor = Color.IndianRed;
                st.Overlay = "None";
                btn.Invalidate();
                btn.Parent.Invalidate();
            });

            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add("Con Corona (Azul)", null, (s, e) =>
            {
                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                {
                    info.Estado.Overlay = "CoronaAzul";
                    pieza.Invalidate();
                }
            });

            menu.Items.Add("Requiere Corona (Rojo)", null, (s, e) =>
            {
                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                {
                    info.Estado.Overlay = "CoronaRojo";
                    pieza.Invalidate();
                }
            });

            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add("Pieza Extraída (Azul)", null, (s, e) =>
            {
                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                {
                    info.Estado.Overlay = "XBlue";
                    pieza.Invalidate();
                }
            });

            menu.Items.Add("Por Extraer (Rojo)", null, (s, e) =>
            {
                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                {
                    info.Estado.Overlay = "XRed";
                    pieza.Invalidate();
                }
            });

            menu.Items.Add("Endodoncia Realizada (Azul)", null, (s, e) =>
            {
                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                {
                    info.Estado.Overlay = "TriBlue";
                    pieza.Invalidate();
                }
            });

            menu.Items.Add("Endodoncia por Realizar (Rojo)", null, (s, e) =>
            {
                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                {
                    info.Estado.Overlay = "TriRed";
                    pieza.Invalidate();
                }
            });

            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add("Recidiva de Caries", null, (s, e) =>
            {
                st.FillColor = Color.Transparent;
                st.Overlay = "RecidivaCaries";
                btn.Invalidate();
            });

            menu.Items.Add(new ToolStripSeparator());

            menu.Items.Add("Limpiar", null, (s, e) =>
            {
                st.FillColor = Color.White;
                st.Overlay = "None";

                if (btn.Parent is Panel pieza && pieza.Tag is PiezaTag info)
                    info.Estado.Overlay = "None";

                btn.Invalidate();
                btn.Parent.Invalidate();
            });

            return menu;
        }

        private ContextMenuStrip CrearMenuContextual(Panel pieza)
        {
            PiezaTag tag = pieza.Tag as PiezaTag;
            ContextMenuStrip menu = new ContextMenuStrip();

            menu.Items.Add("Con Corona (Azul)", null, (s, e) =>
            {
                tag.Estado.Overlay = "CoronaAzul";
                pieza.Invalidate();
            });

            menu.Items.Add("Requiere Corona (Rojo)", null, (s, e) =>
            {
                tag.Estado.Overlay = "CoronaRojo";
                pieza.Invalidate();
            });

            menu.Items.Add("Pieza Extraída (Azul)", null, (s, e) =>
            {
                tag.Estado.Overlay = "XBlue";
                pieza.Invalidate();
            });

            menu.Items.Add("Por Extraer (Rojo)", null, (s, e) =>
            {
                tag.Estado.Overlay = "XRed";
                pieza.Invalidate();
            });

            menu.Items.Add("Endodoncia Realizada (Azul)", null, (s, e) =>
            {
                tag.Estado.Overlay = "TriBlue";
                pieza.Invalidate();
            });

            menu.Items.Add("Endodoncia por Realizar (Rojo)", null, (s, e) =>
            {
                tag.Estado.Overlay = "TriRed";
                pieza.Invalidate();
            });

            menu.Items.Add("Limpiar", null, (s, e) =>
            {
                tag.Estado.Overlay = "None";
                pieza.Invalidate();
            });

            return menu;
        }

        // =====================================================
        //  DIBUJO DE OVERLAYS / PRÓTESIS
        // =====================================================
        private void DibujarOverlay(Graphics g, Panel pieza, string tipo)
        {
            int w = pieza.Width;
            int h = pieza.Height;

            if (tipo.StartsWith("X"))
            {
                using (Pen p = new Pen(tipo == "XBlue" ? Color.Blue : Color.Red, 2))
                {
                    g.DrawLine(p, 5, 5, w - 5, h - 5);
                    g.DrawLine(p, w - 5, 5, 5, h - 5);
                }
            }
            else if (tipo.StartsWith("Tri"))
            {
                using (SolidBrush br = new SolidBrush(tipo == "TriBlue" ? Color.Blue : Color.Red))
                {
                    Point[] pts =
                    {
                        new Point(w / 2, 8),
                        new Point(6,       h - 8),
                        new Point(w - 6,   h - 8)
                    };
                    g.FillPolygon(br, pts);
                }
            }
            else if (tipo == "CoronaAzul" || tipo == "CoronaRojo")
            {
                using (Pen p = new Pen(tipo == "CoronaAzul" ? Color.Blue : Color.Red, 6))
                {
                    p.Alignment = PenAlignment.Inset;
                    g.DrawRectangle(p, 3, 3, w - 6, h - 6);
                }
            }
        }

        private void DibujarOverlayCara(Graphics g, Rectangle bounds, string tipo)
        {
            if (tipo == "XBlue" || tipo == "XRed")
            {
                using (Pen p = new Pen(tipo == "XBlue" ? Color.Blue : Color.Red, 2))
                {
                    g.DrawLine(p, bounds.Left, bounds.Top,
                                  bounds.Right, bounds.Bottom);
                    g.DrawLine(p, bounds.Right, bounds.Top,
                                  bounds.Left, bounds.Bottom);
                }
            }
            else if (tipo == "TriBlue" || tipo == "TriRed")
            {
                using (SolidBrush br = new SolidBrush(tipo == "TriBlue" ? Color.Blue : Color.Red))
                {
                    Point mid = new Point(bounds.Left + bounds.Width / 2, bounds.Top + 4);
                    Point left = new Point(bounds.Left + 4, bounds.Bottom - 4);
                    Point right = new Point(bounds.Right - 4, bounds.Bottom - 4);
                    g.FillPolygon(br, new[] { mid, left, right });
                }
            }
            else if (tipo == "RecidivaCaries")
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddRectangle(bounds);
                    using (PathGradientBrush brush = new PathGradientBrush(path))
                    {
                        brush.CenterColor = Color.FromArgb(200, 100, 170, 255);
                        brush.SurroundColors = new[] { Color.FromArgb(120, 255, 0, 0) };
                        brush.CenterPoint = new PointF(bounds.Left + bounds.Width / 2f,
                                                         bounds.Top + bounds.Height / 2f);
                        g.FillRectangle(brush, bounds);
                    }
                }

                using (Pen borde = new Pen(Color.FromArgb(180, 255, 60, 60), 1.2f))
                    g.DrawRectangle(borde, bounds);
            }
        }

        private void DibujarProtesis(Graphics g)
        {
            if (protesisLista.Count == 0) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            foreach (var p in protesisLista)
            {
                Color colorBase = (p.Estado == "Por Realizar")
                    ? Color.Red
                    : Color.DodgerBlue;

                if (p.Tipo == "Superior Total")
                    DibujarLineaProtesis(g, colorBase, 65);
                else if (p.Tipo == "Inferior Total")
                    DibujarLineaProtesis(g, colorBase, 295);
                else if (p.Tipo == "Removible Parcial" && p.Inicio > 0)
                    DibujarRemovible(g, p.Inicio, p.Fin, colorBase);
            }
        }

        private void DibujarLineaProtesis(Graphics g, Color color, int yCentro)
        {
            int separacion = 6;
            using (Pen p = new Pen(color, 3))
            {
                p.StartCap = LineCap.Round;
                p.EndCap = LineCap.Round;
                g.DrawLine(p, 20, yCentro - separacion, pnlOdontograma.Width - 20, yCentro - separacion);
                g.DrawLine(p, 20, yCentro + separacion, pnlOdontograma.Width - 20, yCentro + separacion);
            }
        }

        private void DibujarRemovible(Graphics g, int inicio, int fin, Color color)
        {
            var piezas = pnlOdontograma.Controls.OfType<Panel>()
                .Where(p => p.Tag is PiezaTag tag &&
                            tag.NumeroDiente >= inicio &&
                            tag.NumeroDiente <= fin)
                .OrderBy(p => ((PiezaTag)p.Tag).NumeroDiente)
                .ToList();

            if (piezas.Count < 2) return;

            var primera = piezas.First();
            var ultima = piezas.Last();
            int y = (primera.Top + ultima.Bottom) / 2;

            using (Pen pen = new Pen(color, 5))
            {
                pen.DashStyle = DashStyle.DashDot;
                g.DrawLine(pen, primera.Left, y, ultima.Right, y);
            }
        }
    }
}
