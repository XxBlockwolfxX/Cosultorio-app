using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ConsultorioDentalApp.Data;

namespace ConsultorioDentalApp.Forms
{
    public partial class FrmPacienteRegistro : Form
    {
        private readonly int _pacienteId;
        TextBox txtNombre, txtEdad, txtCorreo, txtTelefono, txtWhatsapp,
                txtDireccion, txtCiudad;
        ComboBox cmbSexo, cmbEstadoCivil;
        DateTimePicker dtpNacimiento;
        Button btnGuardar, btnCancelar;

        public FrmPacienteRegistro(int pacienteId)
        {
            _pacienteId = pacienteId;

            InitializeComponent();
            BuildUI();
            CargarPaciente();
        }

        private void BuildUI()
        {
            Text = "Registro del paciente";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(780, 460);
            BackColor = Color.FromArgb(18, 18, 24);
            Font = new Font("Segoe UI", 10);

            // ===== TÍTULO =====
            var lblTitulo = new Label
            {
                Text = "Datos del paciente",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Left = 20,
                Top = 15,
                BackColor = Color.Transparent
            };
            Controls.Add(lblTitulo);

            // ===== PANEL CAMPOS =====
            var pnl = new Panel
            {
                Left = 20,
                Top = 55,
                Width = ClientSize.Width - 40,
                Height = ClientSize.Height - 120,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = Color.FromArgb(28, 28, 34)
            };
            Controls.Add(pnl);

            int x1 = 20, x2 = 380;
            int y = 20, dy = 35;

            // Nombre
            pnl.Controls.Add(CrearLabel("Nombre completo:", x1, y));
            txtNombre = CrearTextBox(pnl, x1 + 140, y, 260);

            // Edad
            y += dy;
            pnl.Controls.Add(CrearLabel("Edad:", x1, y));
            txtEdad = CrearTextBox(pnl, x1 + 140, y, 80);

            // Sexo
            y += dy;
            pnl.Controls.Add(CrearLabel("Sexo:", x1, y));
            cmbSexo = new ComboBox
            {
                Left = x1 + 140,
                Top = y - 3,
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSexo.Items.AddRange(new[] { "Masculino", "Femenino", "Otro" });
            pnl.Controls.Add(cmbSexo);

            // Fecha nacimiento
            y += dy;
            pnl.Controls.Add(CrearLabel("Fecha nacimiento:", x1, y));
            dtpNacimiento = new DateTimePicker
            {
                Left = x1 + 140,
                Top = y - 3,
                Width = 140,
                Format = DateTimePickerFormat.Short
            };
            dtpNacimiento.ValueChanged += DtpNacimiento_ValueChanged;
            pnl.Controls.Add(dtpNacimiento);

            // Estado civil
            y += dy;
            pnl.Controls.Add(CrearLabel("Estado civil:", x1, y));
            cmbEstadoCivil = new ComboBox
            {
                Left = x1 + 140,
                Top = y - 3,
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEstadoCivil.Items.AddRange(new[] { "Soltero", "Casado", "Divorciado", "Unión libre" });
            pnl.Controls.Add(cmbEstadoCivil);

            // ===== Columna derecha =====
            int y2 = 20;

            pnl.Controls.Add(CrearLabel("Correo:", x2, y2));
            txtCorreo = CrearTextBox(pnl, x2 + 110, y2, 230);

            y2 += dy;
            pnl.Controls.Add(CrearLabel("Teléfono móvil:", x2, y2));
            txtTelefono = CrearTextBox(pnl, x2 + 110, y2, 180);

            y2 += dy;
            pnl.Controls.Add(CrearLabel("Whatsapp:", x2, y2));
            txtWhatsapp = CrearTextBox(pnl, x2 + 110, y2, 180);

            y2 += dy;
            pnl.Controls.Add(CrearLabel("Dirección:", x2, y2));
            txtDireccion = CrearTextBox(pnl, x2 + 110, y2, 230);

            y2 += dy;
            pnl.Controls.Add(CrearLabel("Ciudad:", x2, y2));
            txtCiudad = CrearTextBox(pnl, x2 + 110, y2, 180);

            // ===== BOTONES =====
            btnGuardar = new Button
            {
                Text = "Guardar cambios",
                Width = 160,
                Height = 34,
                Left = Width - 360,
                Top = ClientSize.Height - 55,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            Controls.Add(btnGuardar);

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Width = 120,
                Height = 34,
                Left = Width - 190,
                Top = ClientSize.Height - 55,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                BackColor = Color.FromArgb(64, 64, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) => Close();
            Controls.Add(btnCancelar);
        }

        private Label CrearLabel(string texto, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Left = x,
                Top = y,
                AutoSize = true,
                ForeColor = Color.Gainsboro,
                BackColor = Color.Transparent
            };
        }

        private TextBox CrearTextBox(Control parent, int x, int y, int w)
        {
            var txt = new TextBox
            {
                Left = x,
                Top = y - 3,
                Width = w,
                BorderStyle = BorderStyle.FixedSingle
            };
            parent.Controls.Add(txt);
            return txt;
        }

        // === Cargar datos del paciente desde la BD ===
        private void CargarPaciente()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"
                        SELECT Nombre, Edad, Sexo, FechaNacimiento,
                               EstadoCivil, Correo, TelefonoMovil,
                               Whatsapp, Direccion, Ciudad
                        FROM Paciente
                        WHERE Id = @Id;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", _pacienteId);

                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                txtNombre.Text = rd["Nombre"]?.ToString();
                                txtEdad.Text = rd["Edad"]?.ToString();
                                cmbSexo.Text = rd["Sexo"]?.ToString();

                                if (!rd.IsDBNull(rd.GetOrdinal("FechaNacimiento")))
                                    dtpNacimiento.Value = rd.GetDateTime(rd.GetOrdinal("FechaNacimiento"));

                                cmbEstadoCivil.Text = rd["EstadoCivil"]?.ToString();
                                txtCorreo.Text = rd["Correo"]?.ToString();
                                txtTelefono.Text = rd["TelefonoMovil"]?.ToString();
                                txtWhatsapp.Text = rd["Whatsapp"]?.ToString();
                                txtDireccion.Text = rd["Direccion"]?.ToString();
                                txtCiudad.Text = rd["Ciudad"]?.ToString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar datos del paciente:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Recalcula edad cuando cambias la fecha de nacimiento
        private void DtpNacimiento_ValueChanged(object sender, EventArgs e)
        {
            var hoy = DateTime.Today;
            int edad = hoy.Year - dtpNacimiento.Value.Year;
            if (dtpNacimiento.Value.Date > hoy.AddYears(-edad)) edad--;
            txtEdad.Text = edad.ToString();
        }

        // === Guardar cambios ===
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"
                        UPDATE Paciente SET
                            Nombre         = @Nombre,
                            Edad           = @Edad,
                            Sexo           = @Sexo,
                            FechaNacimiento= @FechaNacimiento,
                            EstadoCivil    = @EstadoCivil,
                            Correo         = @Correo,
                            TelefonoMovil  = @TelefonoMovil,
                            Whatsapp       = @Whatsapp,
                            Direccion      = @Direccion,
                            Ciudad         = @Ciudad
                        WHERE Id = @Id;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        int edad = 0;
                        int.TryParse(txtEdad.Text, out edad);

                        cmd.Parameters.AddWithValue("@Nombre", txtNombre.Text.Trim());
                        cmd.Parameters.AddWithValue("@Edad", edad);
                        cmd.Parameters.AddWithValue("@Sexo", cmbSexo.SelectedItem?.ToString() ?? "");
                        cmd.Parameters.AddWithValue("@FechaNacimiento", dtpNacimiento.Value.Date);
                        cmd.Parameters.AddWithValue("@EstadoCivil", cmbEstadoCivil.SelectedItem?.ToString() ?? "");
                        cmd.Parameters.AddWithValue("@Correo", txtCorreo.Text.Trim());
                        cmd.Parameters.AddWithValue("@TelefonoMovil", txtTelefono.Text.Trim());
                        cmd.Parameters.AddWithValue("@Whatsapp", txtWhatsapp.Text.Trim());
                        cmd.Parameters.AddWithValue("@Direccion", txtDireccion.Text.Trim());
                        cmd.Parameters.AddWithValue("@Ciudad", txtCiudad.Text.Trim());
                        cmd.Parameters.AddWithValue("@Id", _pacienteId);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Datos del paciente actualizados correctamente.",
                    "Registro", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar cambios:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
