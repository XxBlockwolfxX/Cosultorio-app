using System;
using System.Drawing;
using System.Windows.Forms;
using ConsultorioDentalApp.Services;

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

        // ===== Horario =====
        private CheckedListBox clbDias;
        private DateTimePicker dtpIni, dtpFin;
        private NumericUpDown nudIntervalo;
        private AgendaConfig agendaCfg;

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

            // ✅ aumentamos alto para que quepa el horario
            ClientSize = new Size(560, 520);
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
                Width = 340,
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

            // ===== CONTENEDOR HORARIO =====
            var pnlHorario = new Panel
            {
                Left = 20,
                Top = 270,
                Width = 520,
                Height = 190,
                BackColor = Color.FromArgb(45, 48, 58)
            };
            Controls.Add(pnlHorario);

            // ✅ construir UI del horario (esto antes no se llamaba)
            BuildHorarioUI(pnlHorario);

            // ===== BOTONES ABAJO =====
            btnGuardar = new Button
            {
                Text = "Guardar",
                Left = 350,
                Top = 475,
                Width = 90,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(39, 174, 96)
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            Controls.Add(btnGuardar);

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Left = 450,
                Top = 475,
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

            // ✅ validar y guardar horario
            try
            {
                GuardarHorario();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            NombreUsuario = nombre;
            DialogResult = DialogResult.OK;
            Close();
        }

        // ===================== HORARIO UI =====================
        private void BuildHorarioUI(Panel contenedor)
        {
            agendaCfg = AgendaConfigService.Cargar();

            var lbl = new Label
            {
                Text = "Horario laboral",
                ForeColor = Color.White,
                AutoSize = true,
                Top = 12,
                Left = 12,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold)
            };

            clbDias = new CheckedListBox
            {
                Left = 12,
                Top = 42,
                Width = 180,
                Height = 130,
                BackColor = Color.FromArgb(40, 40, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            string[] dias = { "Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo" };
            for (int i = 0; i < 7; i++)
            {
                bool marcado = agendaCfg.DiasLaborales.Contains(i + 1);
                clbDias.Items.Add(dias[i], marcado);
            }

            contenedor.Controls.Add(lbl);
            contenedor.Controls.Add(clbDias);

            // Hora inicio
            contenedor.Controls.Add(new Label
            {
                Text = "Inicio",
                ForeColor = Color.White,
                Left = 220,
                Top = 45,
                AutoSize = true
            });

            dtpIni = new DateTimePicker
            {
                Left = 220,
                Top = 65,
                Width = 140,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };

            // Hora fin
            contenedor.Controls.Add(new Label
            {
                Text = "Fin",
                ForeColor = Color.White,
                Left = 220,
                Top = 100,
                AutoSize = true
            });

            dtpFin = new DateTimePicker
            {
                Left = 220,
                Top = 120,
                Width = 140,
                Format = DateTimePickerFormat.Time,
                ShowUpDown = true
            };

            dtpIni.Value = DateTime.Today.Add(agendaCfg.HoraInicio);
            dtpFin.Value = DateTime.Today.Add(agendaCfg.HoraFin);

            contenedor.Controls.Add(dtpIni);
            contenedor.Controls.Add(dtpFin);

            // Intervalo
            contenedor.Controls.Add(new Label
            {
                Text = "Intervalo (min)",
                ForeColor = Color.White,
                Left = 380,
                Top = 45,
                AutoSize = true
            });

            nudIntervalo = new NumericUpDown
            {
                Left = 380,
                Top = 65,
                Width = 120,
                Minimum = 5,
                Maximum = 120,
                Value = agendaCfg.IntervaloMin,
                BackColor = Color.FromArgb(40, 40, 48),
                ForeColor = Color.White
            };

            contenedor.Controls.Add(nudIntervalo);
        }

        private void GuardarHorario()
        {
            var cfg = new AgendaConfig();
            cfg.DiasLaborales.Clear();

            for (int i = 0; i < clbDias.Items.Count; i++)
                if (clbDias.GetItemChecked(i))
                    cfg.DiasLaborales.Add(i + 1);

            cfg.HoraInicio = dtpIni.Value.TimeOfDay;
            cfg.HoraFin = dtpFin.Value.TimeOfDay;
            cfg.IntervaloMin = (int)nudIntervalo.Value;

            if (cfg.DiasLaborales.Count == 0)
                throw new Exception("Selecciona al menos un día laboral.");

            if (cfg.HoraFin <= cfg.HoraInicio)
                throw new Exception("La hora fin debe ser mayor que la hora inicio.");

            AgendaConfigService.Guardar(cfg);
        }
    }
}
