using System;
using System.Drawing;
using System.Windows.Forms;

namespace ConsultorioDentalApp.Forms
{
    public partial class FrmConfiguracionUsuario : Form
    {
        private PictureBox picFoto;
        private TextBox txtNombre;
        private Button btnCambiarFoto;
        private Button btnGuardar;
        private Button btnCancelar;
        private Label lblTitulo;
        private Label lblNombre;
        private Panel panelFoto;

        public Image FotoSeleccionada { get; private set; }
        public string NombreUsuario { get; private set; }

        public FrmConfiguracionUsuario(string nombreActual, Image fotoActual)
        {
            FotoSeleccionada = fotoActual;
            NombreUsuario = nombreActual;

            InitializeComponent();

            txtNombre.Text = nombreActual ?? "";
            if (fotoActual != null)
                picFoto.Image = fotoActual;
        }

        private void InitializeComponent()
        {
            Text = "Configuración de usuario";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(520, 260);
            BackColor = Color.FromArgb(30, 30, 36);

            // ===== TÍTULO =====
            lblTitulo = new Label
            {
                Text = "Personaliza tu perfil",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Left = 20,
                Top = 15
            };
            Controls.Add(lblTitulo);

            // ===== PANEL FOTO =====
            panelFoto = new Panel
            {
                Left = 20,
                Top = 60,
                Width = 140,
                Height = 140,
                BackColor = Color.FromArgb(45, 48, 58)
            };

            picFoto = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black
            };

            panelFoto.Controls.Add(picFoto);
            Controls.Add(panelFoto);

            // ===== NOMBRE =====
            lblNombre = new Label
            {
                Text = "Nombre para mostrar",
                ForeColor = Color.Gainsboro,
                AutoSize = true,
                Left = 180,
                Top = 80
            };
            Controls.Add(lblNombre);

            txtNombre = new TextBox
            {
                Left = 180,
                Top = 110,
                Width = 300,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 48),
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(txtNombre);

            // ===== BOTÓN CAMBIAR FOTO =====
            btnCambiarFoto = new Button
            {
                Text = "Cambiar foto...",
                Left = 20,
                Top = 210,
                Width = 140,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(70, 70, 82)
            };
            btnCambiarFoto.FlatAppearance.BorderSize = 0;
            btnCambiarFoto.Click += BtnCambiarFoto_Click;
            Controls.Add(btnCambiarFoto);

            // ===== BOTÓN GUARDAR =====
            btnGuardar = new Button
            {
                Text = "Guardar",
                Left = 290,
                Top = 210,
                Width = 90,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(39, 174, 96)
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            Controls.Add(btnGuardar);

            // ===== BOTÓN CANCELAR =====
            btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = 390,
                Top = 210,
                Width = 90,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(100, 100, 110),
                DialogResult = DialogResult.Cancel
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            Controls.Add(btnCancelar);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }

        private void BtnCambiarFoto_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "Seleccionar foto de usuario";
                ofd.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        // Clonamos la imagen para no dejar bloqueado el archivo
                        using (var imgTemp = Image.FromFile(ofd.FileName))
                        {
                            FotoSeleccionada = new Bitmap(imgTemp);
                        }
                        picFoto.Image = FotoSeleccionada;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("No se pudo cargar la imagen:\n" + ex.Message,
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            var nombre = txtNombre.Text?.Trim();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show("Ingresa un nombre para mostrar.",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombre.Focus();
                return;
            }

            NombreUsuario = nombre;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
