using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ConsultorioDentalApp.Forms
{
    public partial class FrmFichaClinica : Form
    {
        public FrmFichaClinica()
        {
            InitializeComponent();
            BuildUI();
        }

        private enum ToothState { Sano, Caries, Restaurado, Extraido }

        private Image LoadEmbedded(string fileName)
        {
            var asm = Assembly.GetExecutingAssembly();
            var resourcePath = $"ConsultorioDentalApp.Resources.{fileName}";
            using (var s = asm.GetManifestResourceStream(resourcePath))
            {
                return s != null ? Image.FromStream(s) : null;
            }
        }

        private void BuildUI()
        {
            // ================== CONFIG FORM ===================
            Text = "Ficha Clínica";
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10f);
            WindowState = FormWindowState.Maximized;

            // ================= PANEL PRINCIPAL =================
            var main = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            Controls.Add(main);

            // ================= PANEL IZQUIERDO =================
            var pnlPaciente = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.FromArgb(247, 249, 252)
            };
            main.Controls.Add(pnlPaciente);

            var lblPaciente = new Label
            {
                Text = "Paciente",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 70, 120),
                Location = new Point(20, 25)
            };
            pnlPaciente.Controls.Add(lblPaciente);

            int y = 90;
            void AddRow(string label, string val)
            {
                pnlPaciente.Controls.Add(new Label
                {
                    Text = label + ":",
                    Font = new Font("Segoe UI Semibold", 10f),
                    Location = new Point(20, y),
                    AutoSize = true
                });

                pnlPaciente.Controls.Add(new Label
                {
                    Text = val,
                    Font = new Font("Segoe UI", 10f),
                    Location = new Point(110, y),
                    AutoSize = true
                });

                y += 30;
            }

            AddRow("Nombre", "Juan Pérez");
            AddRow("Edad", "30");
            AddRow("Sexo", "M");
            AddRow("Teléfono", "123456789");
            AddRow("Correo", "demo@demo.net");

            // ================= PANEL DERECHO =================
            var pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            main.Controls.Add(pnlRight);

            // ============= WRAPPER PARA TABS =================
            var contentWrapper = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(300, 0, 0, 0),
                BackColor = Color.White
            };
            pnlRight.Controls.Add(contentWrapper);

            // =============== TABCONTROL ======================
            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Alignment = TabAlignment.Top,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new Size(260, 38),
                SizeMode = TabSizeMode.Fixed,
                Multiline = false
            };

            tabs.DrawItem += (s, e) =>
            {
                var page = tabs.TabPages[e.Index];
                bool selected = tabs.SelectedIndex == e.Index;

                var bg = selected ? Color.White : Color.FromArgb(242, 243, 245);
                var fg = selected ? Color.FromArgb(45, 90, 150) : Color.FromArgb(80, 80, 80);

                using (var b = new SolidBrush(bg))
                    e.Graphics.FillRectangle(b, e.Bounds);

                TextRenderer.DrawText(
                    e.Graphics,
                    page.Text,
                    tabs.Font,
                    e.Bounds,
                    fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };

            // ====== LOS TABS (ORDEN CORRECTO) ======
            var tpFicha = new TabPage("Ficha terapéutica") { BackColor = Color.White };
            var tpHistoria = new TabPage("Historia ortodóntica del paciente") { BackColor = Color.White };
            var tpTerapia = new TabPage("Terapia miofuncional") { BackColor = Color.White };

            tabs.TabPages.Add(tpFicha);
            tabs.TabPages.Add(tpHistoria);
            tabs.TabPages.Add(tpTerapia);

            contentWrapper.Controls.Add(tabs);

            // ================= HEADER FICHA =================
            var headerFicha = new Panel
            {
                Dock = DockStyle.Top,
                Height = 86,
                BackColor = Color.White
            };
            tpFicha.Controls.Add(headerFicha);

            headerFicha.Controls.Add(new Label
            {
                Text = "Fecha del último guardado (Obligatorio)",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(95, 95, 95),
                Location = new Point(24, 16)
            });

            headerFicha.Controls.Add(new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Width = 180,
                Location = new Point(24, 40)
            });

            // ================= PANEL CENTRAL =================
            var bodyFicha = new Panel { Dock = DockStyle.Fill };
            tpFicha.Controls.Add(bodyFicha);
            bodyFicha.BringToFront();

            // ================= ODONTOGRAMA ==================
            var pnlOdonto = new Panel
            {
                Dock = DockStyle.Top,
                Height = 340,
                BackColor = Color.White
            };
            bodyFicha.Controls.Add(pnlOdonto);


            var odontograma = new OdontogramaControl
            {
                Dock = DockStyle.Fill
            };

            odontograma.CaraSeleccionada += (pieza, cara) =>
            {
                MessageBox.Show($"Diente {pieza}, cara {cara}");
            };

            pnlOdonto.Controls.Add(odontograma);

            // ================= SECTION BOTTOM ===============
            var bottomFicha = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(249, 250, 251)
            };
            bodyFicha.Controls.Add(bottomFicha);

            bottomFicha.Controls.Add(new Label
            {
                Text = "Detección de cáncer bucal y de tejidos blandos",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 90, 150),
                Location = new Point(24, 16)
            });
        }
    }
}
