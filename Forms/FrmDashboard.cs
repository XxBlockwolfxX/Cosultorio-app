using System;
using System.Windows.Forms;
using System.Drawing;

namespace ConsultorioDentalApp.Forms
{
    public partial class FrmDashboard : Form
    {
        public FrmDashboard()
        {
            InitializeComponent();
            ConstruirInterfaz();
        }

        private void ConstruirInterfaz()
        {
            this.Text = "Consultorio Dental - Panel Principal";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.White;

            // === Menú lateral ===
            FlowLayoutPanel menu = new FlowLayoutPanel()
            {
                Dock = DockStyle.Left,
                Width = 220,
                FlowDirection = FlowDirection.TopDown,
                BackColor = Color.FromArgb(45, 60, 90),
                Padding = new Padding(10)
            };

            // === Botones ===
            Button btnPacientes = CrearBoton("Pacientes", () => new FrmPacientes().ShowDialog());
            Button btnFicha = CrearBoton("Ficha Clínica", () => new FrmFichaClinica().ShowDialog());
            Button btnProced = CrearBoton("Procedimientos", () => new FrmProcedimientos().ShowDialog());

            menu.Controls.AddRange(new Control[] { btnPacientes, btnFicha, btnProced });

            Label lblTitulo = new Label()
            {
                Text = "Panel Principal",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 60, 90),
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Height = 80
            };

            this.Controls.Add(menu);
            this.Controls.Add(lblTitulo);
        }

        private Button CrearBoton(string texto, Action accion)
        {
            Button btn = new Button()
            {
                Text = texto,
                Width = 200,
                Height = 50,
                Margin = new Padding(10),
                BackColor = Color.FromArgb(90, 120, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (s, e) => accion();
            return btn;
        }
    }
}
