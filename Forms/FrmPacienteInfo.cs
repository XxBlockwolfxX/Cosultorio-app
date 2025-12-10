using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ConsultorioDentalApp.Forms
{
    public class FrmPacienteInfo : Form
    {
        public FrmPacienteInfo(
            string nombre,
            string edad,
            string nacimiento,
            string telefono,
            string whatsapp,
            string correo,
            string direccion,
            string ciudad)
        {
            InitializeComponent();

            lblNombre.Text = nombre;
            lblEdad.Text = $"Edad: {edad} años";
            lblNacimiento.Text = $"Nacimiento: {nacimiento}";
            lblTelefono.Text = $"Tel. móvil: {telefono}";
            lblWhatsapp.Text = $"WhatsApp: {whatsapp}";
            lblCorreo.Text = $"Email: {correo}";
            lblDireccion.Text = $"Dirección: {direccion}";
        }

        private Label lblNombre;
        private Label lblEdad;
        private Label lblNacimiento;
        private Label lblTelefono;
        private Label lblWhatsapp;
        private Label lblCorreo;
        private Label lblDireccion;
        private Panel avatarPanel;

        private void InitializeComponent()
        {
            Text = "Detalle del paciente";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 220);
            BackColor = Color.FromArgb(60, 60, 65);

            // Panel avatar circular
            avatarPanel = new Panel
            {
                Left = 20,
                Top = 30,
                Width = 90,
                Height = 90,
                BackColor = Color.Transparent
            };
            avatarPanel.Paint += AvatarPanel_Paint;
            Controls.Add(avatarPanel);

            // Nombre grande
            lblNombre = new Label
            {
                Left = 130,
                Top = 25,
                Width = 360,
                AutoSize = false,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold)
            };
            Controls.Add(lblNombre);

            lblEdad = CrearLabelDetalle(130, 60);
            lblNacimiento = CrearLabelDetalle(130, 80);
            lblTelefono = CrearLabelDetalle(130, 100);
            lblWhatsapp = CrearLabelDetalle(130, 120);
            lblCorreo = CrearLabelDetalle(130, 140);
            lblDireccion = CrearLabelDetalle(130, 160);
        }

        private Label CrearLabelDetalle(int x, int y)
        {
            var lbl = new Label
            {
                Left = x,
                Top = y,
                Width = 360,
                AutoSize = false,
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular)
            };
            Controls.Add(lbl);
            return lbl;
        }

        private void AvatarPanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(2, 2, avatarPanel.Width - 4, avatarPanel.Height - 4);

            using (var pen = new Pen(Color.White, 3))
            {
                g.DrawEllipse(pen, rect);
            }

            // Silueta simple
            using (var brush = new SolidBrush(Color.FromArgb(120, 120, 130)))
            {
                // cabeza
                g.FillEllipse(brush,
                    rect.Left + rect.Width * 0.27f,
                    rect.Top + rect.Height * 0.15f,
                    rect.Width * 0.46f,
                    rect.Height * 0.40f);

                // cuerpo
                g.FillEllipse(brush,
                    rect.Left + rect.Width * 0.18f,
                    rect.Top + rect.Height * 0.40f,
                    rect.Width * 0.64f,
                    rect.Height * 0.50f);
            }
        }
    }
}
