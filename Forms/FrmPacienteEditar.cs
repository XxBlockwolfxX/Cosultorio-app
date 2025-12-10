using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ConsultorioDentalApp.Data;

namespace ConsultorioDentalApp.Forms
{
    public class FrmPacienteEditar : Form
    {
        private int pacienteId;

        TextBox txtNombre, txtEdad, txtTelefono, txtWhatsapp, txtDireccion, txtCorreo, txtCiudad;
        ComboBox cmbSexo, cmbEstadoCivil;
        DateTimePicker dtpFechaNacimiento;
        Button btnGuardar, btnCancelar;

        public FrmPacienteEditar(int id)
        {
            pacienteId = id;
            InitializeComponent();
            CargarPaciente();
        }

        private void InitializeComponent()
        {
            Text = "Editar paciente";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(620, 360);   // un poco más alto para Ciudad
            BackColor = Color.WhiteSmoke;
            Font = new Font("Segoe UI", 10f);

            int xLabel = 20, xField = 140, yBase = 20, sepY = 35;

            // Nombre
            Controls.Add(CrearLabel("Nombre:", xLabel, yBase));
            txtNombre = CrearTextBox(xField, yBase, 430);

            // Edad
            Controls.Add(CrearLabel("Edad:", xLabel, yBase + sepY));
            txtEdad = CrearTextBox(xField, yBase + sepY, 60);

            // Sexo
            Controls.Add(CrearLabel("Sexo:", 320, yBase + sepY));
            cmbSexo = new ComboBox
            {
                Left = 380,
                Top = yBase + sepY - 3,
                Width = 190,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSexo.Items.AddRange(new[] { "Masculino", "Femenino", "Otro" });
            Controls.Add(cmbSexo);

            // Fecha Nacimiento
            Controls.Add(CrearLabel("Fecha nacimiento:", xLabel, yBase + sepY * 2));
            dtpFechaNacimiento = new DateTimePicker
            {
                Left = xField,
                Top = yBase + sepY * 2 - 3,
                Width = 140,
                Format = DateTimePickerFormat.Short
            };
            Controls.Add(dtpFechaNacimiento);

            // Estado civil
            Controls.Add(CrearLabel("Estado civil:", 320, yBase + sepY * 2));
            cmbEstadoCivil = new ComboBox
            {
                Left = 410,
                Top = yBase + sepY * 2 - 3,
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEstadoCivil.Items.AddRange(new[] { "Soltero", "Casado", "Divorciado", "Unión libre" });
            Controls.Add(cmbEstadoCivil);

            // Teléfono
            Controls.Add(CrearLabel("Tel. móvil:", xLabel, yBase + sepY * 3));
            txtTelefono = CrearTextBox(xField, yBase + sepY * 3, 180);

            // Whatsapp
            Controls.Add(CrearLabel("Whatsapp:", 320, yBase + sepY * 3));
            txtWhatsapp = CrearTextBox(410, yBase + sepY * 3, 160);

            // Correo
            Controls.Add(CrearLabel("Correo:", xLabel, yBase + sepY * 4));
            txtCorreo = CrearTextBox(xField, yBase + sepY * 4, 430);

            // Dirección
            Controls.Add(CrearLabel("Dirección:", xLabel, yBase + sepY * 5));
            txtDireccion = CrearTextBox(xField, yBase + sepY * 5, 430);

            // Ciudad  ✅
            Controls.Add(CrearLabel("Ciudad:", xLabel, yBase + sepY * 6));
            txtCiudad = CrearTextBox(xField, yBase + sepY * 6, 250);

            // Botones
            btnGuardar = new Button
            {
                Text = "Guardar",
                Left = ClientSize.Width - 210,
                Top = ClientSize.Height - 50,
                Width = 90,
                Height = 32,
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
                Left = ClientSize.Width - 110,
                Top = ClientSize.Height - 50,
                Width = 90,
                Height = 32,
                BackColor = Color.FromArgb(120, 120, 130),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            Controls.Add(btnCancelar);

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }

        private Label CrearLabel(string texto, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Left = x,
                Top = y,
                AutoSize = true,
                ForeColor = Color.FromArgb(40, 75, 125)
            };
        }

        private TextBox CrearTextBox(int x, int y, int width)
        {
            var txt = new TextBox
            {
                Left = x,
                Top = y - 3,
                Width = width,
                BorderStyle = BorderStyle.FixedSingle
            };
            Controls.Add(txt);
            return txt;
        }

        // ===== Cargar datos del paciente =====
        private void CargarPaciente()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                        SELECT Nombre, Edad, Sexo, FechaNacimiento, EstadoCivil,
                               Correo, TelefonoMovil, Whatsapp, Direccion, Ciudad
                        FROM Paciente
                        WHERE Id = @Id;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", pacienteId);

                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                txtNombre.Text = rd["Nombre"]?.ToString();
                                txtEdad.Text = rd["Edad"]?.ToString();
                                cmbSexo.SelectedItem = rd["Sexo"]?.ToString();

                                if (rd["FechaNacimiento"] != DBNull.Value)
                                    dtpFechaNacimiento.Value = Convert.ToDateTime(rd["FechaNacimiento"]);

                                cmbEstadoCivil.SelectedItem = rd["EstadoCivil"]?.ToString();
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

        // ===== Guardar cambios =====
        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                        UPDATE Paciente SET
                            Nombre = @Nombre,
                            Edad = @Edad,
                            Sexo = @Sexo,
                            FechaNacimiento = @FechaNacimiento,
                            EstadoCivil = @EstadoCivil,
                            Correo = @Correo,
                            TelefonoMovil = @TelefonoMovil,
                            Whatsapp = @Whatsapp,
                            Direccion = @Direccion,
                            Ciudad = @Ciudad
                        WHERE Id = @Id;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", pacienteId);
                        cmd.Parameters.AddWithValue("@Nombre", txtNombre.Text);
                        cmd.Parameters.AddWithValue("@Edad",
                            string.IsNullOrWhiteSpace(txtEdad.Text) ? (object)DBNull.Value : txtEdad.Text);
                        cmd.Parameters.AddWithValue("@Sexo", cmbSexo.SelectedItem?.ToString() ?? "");
                        cmd.Parameters.AddWithValue("@FechaNacimiento", dtpFechaNacimiento.Value.Date);
                        cmd.Parameters.AddWithValue("@EstadoCivil", cmbEstadoCivil.SelectedItem?.ToString() ?? "");
                        cmd.Parameters.AddWithValue("@Correo", txtCorreo.Text);
                        cmd.Parameters.AddWithValue("@TelefonoMovil", txtTelefono.Text);
                        cmd.Parameters.AddWithValue("@Whatsapp", txtWhatsapp.Text);
                        cmd.Parameters.AddWithValue("@Direccion", txtDireccion.Text);
                        cmd.Parameters.AddWithValue("@Ciudad", txtCiudad.Text);

                        cmd.ExecuteNonQuery();
                    }
                }

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
