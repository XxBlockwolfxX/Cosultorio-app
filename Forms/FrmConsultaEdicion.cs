using System;
using System.Drawing;
using System.Windows.Forms;
using ConsultorioDentalApp.Data;
using MySql.Data.MySqlClient;

namespace ConsultorioDentalApp.Forms
{
    public class FrmConsultaEdicion : Form
    {
        private readonly int _pacienteId;
        private readonly string _nombrePaciente;
        private readonly int? _consultaId;

        // Controles principales
        private TextBox txtMotivo;
        private ComboBox cmbProcedimiento;
        private TextBox txtAnamnesis;
        private TextBox txtExamenAparato;
        private TextBox txtDiagnostico;
        private TextBox txtCieCodigo;
        private Label lblCieDescripcion;
        private Button btnBuscarCie;

        // Medicamentos
        private DataGridView dgvMedicamentos;
        private Button btnAgregarMed;
        private Button btnQuitarMed;
        private ComboBox cmbMedNombre;
        private ComboBox cmbMedPresentacion;
        private NumericUpDown nudMedCantidad;
        private ComboBox cmbMedPrescripcion;

        // Botones generales
        private Button btnGuardar;
        private Button btnCancelar;

        public FrmConsultaEdicion(int pacienteId, string nombrePaciente, int? consultaId = null)
        {
            _pacienteId = pacienteId;
            _nombrePaciente = nombrePaciente;
            _consultaId = consultaId;

            InitializeComponent();

            if (_consultaId.HasValue)
                CargarConsulta();
        }

        private void InitializeComponent()
        {
            Text = "Consulta";
            BackColor = Color.FromArgb(60, 60, 65);
            Font = new Font("Segoe UI", 10f);
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(1100, 650);

            // ========== ENCABEZADO ==========
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(45, 45, 50)
            };

            var lblNombre = new Label
            {
                Text = _nombrePaciente,
                AutoSize = true,
                Left = 80,
                Top = 8,
                ForeColor = Color.FromArgb(255, 214, 0),
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            pnlTop.Controls.Add(lblNombre);

            var lblDatos = new Label
            {
                Text = "Historia Clínica: " + _pacienteId.ToString("00000") +
                       "   -   Fecha Consulta: " + DateTime.Now.ToString("dddd, dd 'de' MMMM 'de' yyyy"),
                AutoSize = true,
                Left = 80,
                Top = 40,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            pnlTop.Controls.Add(lblDatos);

            // ========== PANEL IZQUIERDO (DATOS CLÍNICOS) ==========
            var pnlLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 520,
                BackColor = Color.FromArgb(55, 55, 60),
                Padding = new Padding(10)
            };

            int y = 10;
            int sep = 6;
            int hBox = 28;

            // Motivo
            pnlLeft.Controls.Add(CrearLabel("Motivo de la consulta", 10, y));
            y += 20;
            txtMotivo = new TextBox
            {
                Left = 10,
                Top = y,
                Width = 480,
                Height = 60,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(40, 40, 45),
                ForeColor = Color.White
            };
            pnlLeft.Controls.Add(txtMotivo);
            y += 60 + sep;

            // Procedimiento
            pnlLeft.Controls.Add(CrearLabel("Procedimiento", 10, y));
            y += 20;
            cmbProcedimiento = new ComboBox
            {
                Left = 10,
                Top = y,
                Width = 480,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(40, 40, 45),
                ForeColor = Color.White
            };
            cmbProcedimiento.Items.AddRange(new object[]
            {
                "Control",
                "Primera consulta",
                "Procedimiento odontológico",
                "Emergencia",
                "Otro"
            });
            pnlLeft.Controls.Add(cmbProcedimiento);
            y += hBox + sep;

            // Anamnesis
            pnlLeft.Controls.Add(CrearLabel("Anamnesis", 10, y));
            y += 20;
            txtAnamnesis = new TextBox
            {
                Left = 10,
                Top = y,
                Width = 480,
                Height = 60,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(40, 40, 45),
                ForeColor = Color.White
            };
            pnlLeft.Controls.Add(txtAnamnesis);
            y += 60 + sep;

            // Examen aparato afecto
            pnlLeft.Controls.Add(CrearLabel("Examen aparato afecto", 10, y));
            y += 20;
            txtExamenAparato = new TextBox
            {
                Left = 10,
                Top = y,
                Width = 480,
                Height = 60,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(40, 40, 45),
                ForeColor = Color.White
            };
            pnlLeft.Controls.Add(txtExamenAparato);
            y += 60 + sep;

            // Diagnóstico
            pnlLeft.Controls.Add(CrearLabel("Diagnóstico", 10, y));
            y += 20;
            txtDiagnostico = new TextBox
            {
                Left = 10,
                Top = y,
                Width = 480,
                Height = 60,
                Multiline = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(40, 40, 45),
                ForeColor = Color.White
            };
            pnlLeft.Controls.Add(txtDiagnostico);
            y += 60 + sep;

            // Código CIE
            pnlLeft.Controls.Add(CrearLabel("Código CIE", 10, y));
            y += 20;

            txtCieCodigo = new TextBox
            {
                Left = 10,
                Top = y,
                Width = 80,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(40, 40, 45),
                ForeColor = Color.White,
                ReadOnly = true
            };
            pnlLeft.Controls.Add(txtCieCodigo);

            btnBuscarCie = new Button
            {
                Text = "...",
                Left = 95,
                Top = y - 1,
                Width = 35,
                Height = hBox + 2
            };
            btnBuscarCie.Click += BtnBuscarCie_Click;
            pnlLeft.Controls.Add(btnBuscarCie);

            lblCieDescripcion = new Label
            {
                Left = 140,
                Top = y + 4,
                Width = 350,
                Height = 22,
                ForeColor = Color.Gainsboro,
                BackColor = Color.Transparent
            };
            pnlLeft.Controls.Add(lblCieDescripcion);

            // ========== PANEL DERECHO (RECETA / MEDICAMENTOS) ==========
            var pnlRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(50, 50, 55),
                Padding = new Padding(10)
            };

            // Título de la sección
            var lblReceta = new Label
            {
                Text = "RECETA / MEDICAMENTOS",
                Dock = DockStyle.Top,
                Height = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold)
            };

            // Panel superior para los campos de receta
            var pnlRecetaTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90
            };

            // --- fila 1: Medicamento / Presentación / Cantidad ---
            var lblMed = new Label
            {
                Text = "Medicamento:",
                Left = 0,
                Top = 8,
                AutoSize = true,
                ForeColor = Color.Gainsboro
            };
            pnlRecetaTop.Controls.Add(lblMed);

            cmbMedNombre = new ComboBox
            {
                Left = 110,
                Top = 5,
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };
            cmbMedNombre.Items.AddRange(new object[]
            {
                "AMOXICILINA",
                "PARACETAMOL",
                "IBUPROFENO",
                "DICLOFENACO",
                "KETOROLACO",
                "METRONIDAZOL",
                "NAPROXENO",
                "Edición..."
            });
            cmbMedNombre.SelectedIndexChanged += CmbMedNombre_SelectedIndexChanged;
            pnlRecetaTop.Controls.Add(cmbMedNombre);

            var lblPres = new Label
            {
                Text = "Presentación:",
                Left = 300,
                Top = 8,
                AutoSize = true,
                ForeColor = Color.Gainsboro
            };
            pnlRecetaTop.Controls.Add(lblPres);

            cmbMedPresentacion = new ComboBox
            {
                Left = 390,
                Top = 5,
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbMedPresentacion.Items.AddRange(new object[]
            {
                "Tabletas",
                "Cápsulas",
                "Gotas",
                "Jarabe",
                "Suspensión",
                "Pasta",
                "Pomada",
                "Gel",
                "Inyectable",
                "Sobres",
                "Colutorio",
                "Spray",
                "Edición..."
            });
            cmbMedPresentacion.SelectedIndexChanged += CmbMedPresentacion_SelectedIndexChanged;
            pnlRecetaTop.Controls.Add(cmbMedPresentacion);

            var lblCant = new Label
            {
                Text = "Cant.:",
                Left = 560,
                Top = 8,
                AutoSize = true,
                ForeColor = Color.Gainsboro
            };
            pnlRecetaTop.Controls.Add(lblCant);

            nudMedCantidad = new NumericUpDown
            {
                Left = 600,
                Top = 5,
                Width = 60,
                Minimum = 1,
                Maximum = 999,
                Value = 1
            };
            pnlRecetaTop.Controls.Add(nudMedCantidad);

            // --- fila 2: Prescripción + botones ---
            var lblPresc = new Label
            {
                Text = "Prescripción:",
                Left = 0,
                Top = 38,
                AutoSize = true,
                ForeColor = Color.Gainsboro
            };
            pnlRecetaTop.Controls.Add(lblPresc);

            cmbMedPrescripcion = new ComboBox
            {
                Left = 110,
                Top = 35,
                Width = 340,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbMedPrescripcion.Items.AddRange(new object[]
            {
                "Cada 2 horas",
                "Cada 4 horas",
                "Cada 6 horas",
                "Cada 8 horas",
                "Cada 12 horas",
                "Cada 24 horas",
                "Cada semana",
                "Cada mes",
                "Edición..."
            });
            cmbMedPrescripcion.SelectedIndexChanged += CmbMedPrescripcion_SelectedIndexChanged;
            pnlRecetaTop.Controls.Add(cmbMedPrescripcion);

            btnAgregarMed = new Button
            {
                Text = "Agregar",
                Width = 90,
                Height = 28,
                Left = 460,
                Top = 40,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAgregarMed.FlatAppearance.BorderSize = 0;
            btnAgregarMed.Click += BtnAgregarMed_Click;
            pnlRecetaTop.Controls.Add(btnAgregarMed);

            btnQuitarMed = new Button
            {
                Text = "Quitar",
                Width = 90,
                Height = 28,
                Left = 560,
                Top = 40,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnQuitarMed.FlatAppearance.BorderSize = 0;
            btnQuitarMed.Click += BtnQuitarMed_Click;
            pnlRecetaTop.Controls.Add(btnQuitarMed);

            // Grid de medicamentos
            dgvMedicamentos = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToResizeRows = false,
                BackgroundColor = Color.FromArgb(45, 45, 50),
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                GridColor = Color.DimGray
            };
            dgvMedicamentos.EnableHeadersVisualStyles = false;
            dgvMedicamentos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgvMedicamentos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvMedicamentos.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvMedicamentos.DefaultCellStyle.BackColor = Color.FromArgb(50, 50, 55);
            dgvMedicamentos.DefaultCellStyle.ForeColor = Color.White;
            dgvMedicamentos.DefaultCellStyle.SelectionBackColor = Color.DodgerBlue;
            dgvMedicamentos.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvMedicamentos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(60, 60, 65);

            dgvMedicamentos.Columns.Add("Medicamento", "Medicamento");
            dgvMedicamentos.Columns.Add("Presentacion", "Presentación");
            dgvMedicamentos.Columns.Add("Cantidad", "Cant.");
            dgvMedicamentos.Columns.Add("Prescripcion", "Prescripción");
            dgvMedicamentos.Columns["Cantidad"].Width = 60;

            // ¡OJO! Orden correcto de adición para que el Dock funcione bien
            pnlRight.Controls.Add(dgvMedicamentos); // Fill
            pnlRight.Controls.Add(pnlRecetaTop);    // Top
            pnlRight.Controls.Add(lblReceta);       // Top encima

            // ========== BARRA INFERIOR (GUARDAR / CANCELAR) ==========
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(45, 45, 50)
            };

            btnGuardar = new Button
            {
                Text = "Guardar",
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnGuardar.FlatAppearance.BorderSize = 0;

            btnGuardar.Click += BtnGuardar_Click;

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom
            };
            btnCancelar.FlatAppearance.BorderSize = 0;
            btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

            // Posicionar dentro del panel inferior
            pnlBottom.Controls.Add(btnGuardar);
            pnlBottom.Controls.Add(btnCancelar);
            pnlBottom.Resize += (s, e) =>
            {
                btnCancelar.Left = pnlBottom.ClientSize.Width - btnCancelar.Width - 10;
                btnGuardar.Left = btnCancelar.Left - btnGuardar.Width - 10;
                btnGuardar.Top = btnCancelar.Top = (pnlBottom.ClientSize.Height - btnGuardar.Height) / 2;
            };

            // Orden de dock en el formulario
            Controls.Add(pnlRight);
            Controls.Add(pnlLeft);
            Controls.Add(pnlBottom);
            Controls.Add(pnlTop);
        }

        private Label CrearLabel(string texto, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Left = x,
                Top = y,
                AutoSize = true,
                ForeColor = Color.White
            };
        }

        // ========= CARGAR / GUARDAR CONSULTA =========

        private void CargarConsulta()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                SELECT Motivo, Procedimiento, Anamnesis,
                       ExamenAparato, Diagnostico, CodigoCie
                FROM Consulta
                WHERE Id = @Id;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", _consultaId.Value);

                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                txtMotivo.Text = rd["Motivo"] as string;
                                cmbProcedimiento.Text = rd["Procedimiento"] as string;
                                txtAnamnesis.Text = rd["Anamnesis"] as string;
                                txtExamenAparato.Text = rd["ExamenAparato"] as string;
                                txtDiagnostico.Text = rd["Diagnostico"] as string;
                                txtCieCodigo.Text = rd["CodigoCie"] as string;

                                if (!string.IsNullOrEmpty(txtCieCodigo.Text))
                                    CargarDescripcionCie(txtCieCodigo.Text);
                            }
                        }
                    }

                    // Cargar también los medicamentos
                    CargarMedicamentos(_consultaId.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar consulta:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CargarMedicamentos(int consultaId)
        {
            dgvMedicamentos.Rows.Clear();

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                SELECT Medicamento, Presentacion, Cantidad, Prescripcion
                FROM ConsultaMedicamento
                WHERE ConsultaId = @Id
                ORDER BY Id;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", consultaId);

                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                dgvMedicamentos.Rows.Add(
                                    rd["Medicamento"]?.ToString(),
                                    rd["Presentacion"]?.ToString(),
                                    rd["Cantidad"] != DBNull.Value ? Convert.ToInt32(rd["Cantidad"]) : 0,
                                    rd["Prescripcion"]?.ToString()
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar medicamentos:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    using (var tx = conn.BeginTransaction())
                    {
                        long consultaIdLong;

                        // 1) INSERT o UPDATE de la consulta
                        string sql;
                        if (_consultaId.HasValue)
                        {
                            sql = @"
                        UPDATE Consulta SET
                            Motivo = @Motivo,
                            Procedimiento = @Procedimiento,
                            Anamnesis = @Anamnesis,
                            ExamenAparato = @ExamenAparato,
                            Diagnostico = @Diagnostico,
                            CodigoCie = @CodigoCie
                        WHERE Id = @Id;";
                        }
                        else
                        {
                            sql = @"
                        INSERT INTO Consulta
                            (PacienteId, Motivo, Procedimiento,
                             Anamnesis, ExamenAparato,
                             Diagnostico, CodigoCie, FechaConsulta)
                        VALUES
                            (@Pac, @Motivo, @Procedimiento,
                             @Anamnesis, @ExamenAparato,
                             @Diagnostico, @CodigoCie, NOW());";
                        }

                        using (var cmd = new MySqlCommand(sql, conn, tx))
                        {
                            if (_consultaId.HasValue)
                                cmd.Parameters.AddWithValue("@Id", _consultaId.Value);
                            else
                                cmd.Parameters.AddWithValue("@Pac", _pacienteId);

                            cmd.Parameters.AddWithValue("@Motivo", txtMotivo.Text);
                            cmd.Parameters.AddWithValue("@Procedimiento", cmbProcedimiento.Text);
                            cmd.Parameters.AddWithValue("@Anamnesis", txtAnamnesis.Text);
                            cmd.Parameters.AddWithValue("@ExamenAparato", txtExamenAparato.Text);
                            cmd.Parameters.AddWithValue("@Diagnostico", txtDiagnostico.Text);
                            cmd.Parameters.AddWithValue("@CodigoCie", txtCieCodigo.Text);

                            cmd.ExecuteNonQuery();

                            consultaIdLong = _consultaId.HasValue
                                ? _consultaId.Value
                                : cmd.LastInsertedId;
                        }

                        int consultaId = (int)consultaIdLong;

                        // 2) Borrar medicamentos anteriores de esa consulta
                        using (var cmdDel = new MySqlCommand(
                            "DELETE FROM ConsultaMedicamento WHERE ConsultaId = @ConsultaId;",
                            conn, tx))
                        {
                            cmdDel.Parameters.AddWithValue("@ConsultaId", consultaId);
                            cmdDel.ExecuteNonQuery();
                        }

                        // 3) Insertar los medicamentos del grid
                        string sqlMed = @"
                    INSERT INTO ConsultaMedicamento
                        (ConsultaId, Medicamento, Presentacion, Cantidad, Prescripcion)
                    VALUES
                        (@ConsultaId, @Med, @Pres, @Cant, @Presc);";

                        using (var cmdMed = new MySqlCommand(sqlMed, conn, tx))
                        {
                            cmdMed.Parameters.Add("@ConsultaId", MySqlDbType.Int32);
                            cmdMed.Parameters.Add("@Med", MySqlDbType.VarChar);
                            cmdMed.Parameters.Add("@Pres", MySqlDbType.VarChar);
                            cmdMed.Parameters.Add("@Cant", MySqlDbType.Int32);
                            cmdMed.Parameters.Add("@Presc", MySqlDbType.Text);

                            foreach (DataGridViewRow row in dgvMedicamentos.Rows)
                            {
                                if (row.IsNewRow) continue;

                                var med = row.Cells["Medicamento"].Value?.ToString();
                                if (string.IsNullOrWhiteSpace(med)) continue;

                                cmdMed.Parameters["@ConsultaId"].Value = consultaId;
                                cmdMed.Parameters["@Med"].Value = med;
                                cmdMed.Parameters["@Pres"].Value =
                                    row.Cells["Presentacion"].Value?.ToString();
                                cmdMed.Parameters["@Cant"].Value =
                                    Convert.ToInt32(row.Cells["Cantidad"].Value ?? 0);
                                cmdMed.Parameters["@Presc"].Value =
                                    row.Cells["Prescripcion"].Value?.ToString();

                                cmdMed.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar consulta/medicamentos:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // ========= CIE =========

        private void BtnBuscarCie_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmCieSelector())
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    txtCieCodigo.Text = frm.CodigoSeleccionado;
                    lblCieDescripcion.Text = frm.DescripcionSeleccionada;
                }
            }
        }

        private void CargarDescripcionCie(string codigo)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT Descripcion FROM Cie10 WHERE Codigo = @Cod LIMIT 1;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Cod", codigo);
                        object desc = cmd.ExecuteScalar();
                        lblCieDescripcion.Text = desc != null ? desc.ToString() : "";
                    }
                }
            }
            catch
            {
                // silencioso
            }
        }

        // ========= MEDICAMENTOS =========

        private void BtnAgregarMed_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cmbMedNombre.Text))
                {
                    MessageBox.Show("Ingrese o seleccione el nombre del medicamento.", "Atención",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbMedNombre.Focus();
                    return;
                }

                string presentacion = cmbMedPresentacion.SelectedItem != null
                    ? cmbMedPresentacion.SelectedItem.ToString()
                    : string.Empty;

                string prescripcion = cmbMedPrescripcion.SelectedItem != null
                    ? cmbMedPrescripcion.SelectedItem.ToString()
                    : string.Empty;

                int index = dgvMedicamentos.Rows.Add(
                    cmbMedNombre.Text.Trim(),
                    presentacion,
                    (int)nudMedCantidad.Value,
                    prescripcion
                );

                dgvMedicamentos.ClearSelection();
                dgvMedicamentos.Rows[index].Selected = true;
                if (dgvMedicamentos.Rows.Count > 0)
                    dgvMedicamentos.FirstDisplayedScrollingRowIndex = index;

                cmbMedPresentacion.SelectedIndex = -1;
                cmbMedPrescripcion.SelectedIndex = -1;
                nudMedCantidad.Value = 1;
                cmbMedNombre.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar medicamento:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnQuitarMed_Click(object sender, EventArgs e)
        {
            if (dgvMedicamentos.CurrentRow != null)
            {
                dgvMedicamentos.Rows.Remove(dgvMedicamentos.CurrentRow);
            }
        }

        private string PedirTexto(string titulo, string etiqueta, string valorInicial = "")
        {
            using (var form = new Form())
            using (var lbl = new Label())
            using (var txt = new TextBox())
            using (var btnOk = new Button())
            using (var btnCancel = new Button())
            {
                form.Text = titulo;
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MinimizeBox = false;
                form.MaximizeBox = false;
                form.ClientSize = new Size(380, 130);
                form.BackColor = Color.White;

                lbl.Text = etiqueta;
                lbl.Left = 10;
                lbl.Top = 10;
                lbl.AutoSize = true;

                txt.Left = 10;
                txt.Top = 35;
                txt.Width = 360;
                txt.Text = valorInicial ?? "";

                btnOk.Text = "Aceptar";
                btnOk.DialogResult = DialogResult.OK;
                btnOk.Left = 190;
                btnOk.Top = 80;
                btnOk.Width = 80;

                btnCancel.Text = "Cancelar";
                btnCancel.DialogResult = DialogResult.Cancel;
                btnCancel.Left = 290;
                btnCancel.Top = 80;
                btnCancel.Width = 80;

                form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                return form.ShowDialog(this) == DialogResult.OK
                    ? txt.Text
                    : null;
            }
        }

        private void CmbMedNombre_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMedNombre.SelectedItem != null &&
                cmbMedNombre.SelectedItem.ToString() == "Edición...")
            {
                var nuevo = PedirTexto("Medicamento", "Ingrese el nombre del medicamento:");
                if (!string.IsNullOrWhiteSpace(nuevo))
                {
                    int indexEdicion = cmbMedNombre.Items.Count - 1;
                    cmbMedNombre.Items.Insert(indexEdicion, nuevo);
                    cmbMedNombre.SelectedIndex = indexEdicion;
                }
                else
                {
                    cmbMedNombre.SelectedIndex = -1;
                }
            }
        }

        private void CmbMedPresentacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMedPresentacion.SelectedItem != null &&
                cmbMedPresentacion.SelectedItem.ToString() == "Edición...")
            {
                var nuevo = PedirTexto("Presentación", "Ingrese la presentación del medicamento:");
                if (!string.IsNullOrWhiteSpace(nuevo))
                {
                    int indexEdicion = cmbMedPresentacion.Items.Count - 1;
                    cmbMedPresentacion.Items.Insert(indexEdicion, nuevo);
                    cmbMedPresentacion.SelectedIndex = indexEdicion;
                }
                else
                {
                    cmbMedPresentacion.SelectedIndex = -1;
                }
            }
        }

        private void CmbMedPrescripcion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMedPrescripcion.SelectedItem != null &&
                cmbMedPrescripcion.SelectedItem.ToString() == "Edición...")
            {
                var nuevo = PedirTexto("Prescripción", "Ingrese la prescripción (ej: Cada 8 horas por 5 días):");
                if (!string.IsNullOrWhiteSpace(nuevo))
                {
                    int indexEdicion = cmbMedPrescripcion.Items.Count - 1;
                    cmbMedPrescripcion.Items.Insert(indexEdicion, nuevo);
                    cmbMedPrescripcion.SelectedIndex = indexEdicion;
                }
                else
                {
                    cmbMedPrescripcion.SelectedIndex = -1;
                }
            }
        }
    }
}
