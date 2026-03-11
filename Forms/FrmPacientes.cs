using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ConsultorioDentalApp.Data;
using System.Drawing.Drawing2D;
using System.Linq;



namespace ConsultorioDentalApp.Forms
{
    public partial class FrmPacientes : Form
    {
        // Evento que usará FrmPrincipal para saber qué paciente se eligió
        public event Action<int> PacienteSeleccionado;

        // Controles del formulario de alta / edición
        TextBox txtNombre, txtEdad, txtTelefono, txtWhatsapp, txtDireccion, txtCorreo, txtCiudad;
        ComboBox cmbSexo, cmbEstadoCivil;
        DateTimePicker dtpFechaNacimiento;
        DataGridView dgvPacientes;
        Button btnAgregar;
        Panel pnlFormulario;

        // Controles del modo listado tipo MedSys
        private Panel pnlHeader;
        private Panel pnlContent;
        private TextBox txtBuscar;
        private Button btnBuscar;
        private Label lblHeaderTitulo;
        private Label lblCantidad;
        // Iconos para las columnas de acción
        private Image iconInfo;
        private Image iconMsg;
        private Image iconDelete;
        private Image iconEdit;

        // Iconos decorados para la grill
        private Image iconInfoGrid;
        private Image iconEditGrid;
        private Image iconMsgGrid;
        private Image iconDeleteGrid;


        // ===================== CONSTRUCTORES =====================

        // Form completo (alta / edición)
        public FrmPacientes() : this(false)
        {
        }

        // Modo listado incrustado o completo
        public FrmPacientes(bool soloListado)
        {
            if (soloListado)
                InicializarUIListado();      // estilo MedSys
            else
                InicializarComponentesPersonalizados(); // estilo formulario completo

            CargarPacientes(); // carga inicial sin filtro
        }

        // ===================== UI COMPLETA (ALTA/EDICIÓN) =====================

        private void InicializarComponentesPersonalizados()
        {
            this.Text = "Gestión de Pacientes";
            this.BackColor = Color.WhiteSmoke;
            this.Font = new Font("Segoe UI", 10);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Width = 950;
            this.Height = 650;

            // === TÍTULO ===
            Label lblTitulo = new Label()
            {
                Text = "Pacientes Registrados",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(40, 75, 125),
                AutoSize = true,
                Location = new Point(30, 20)
            };
            this.Controls.Add(lblTitulo);

            // === PANEL DE FORMULARIO (ALTA / EDICIÓN) ===
            pnlFormulario = new Panel()
            {
                Left = 30,
                Top = 70,
                Width = 870,
                Height = 160,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(pnlFormulario);

            int xLabel = 20, xField = 120, yBase = 20, sepY = 35;

            // ======= NOMBRE =======
            pnlFormulario.Controls.Add(CrearLabel("Nombre:", xLabel, yBase));
            txtNombre = CrearTextBox(xField, yBase);

            // ======= EDAD =======
            pnlFormulario.Controls.Add(CrearLabel("Edad:", xLabel, yBase + sepY));
            txtEdad = CrearTextBox(xField, yBase + sepY, 80);

            // ======= SEXO =======
            pnlFormulario.Controls.Add(CrearLabel("Sexo:", 450, yBase));
            cmbSexo = new ComboBox()
            {
                Left = 520,
                Top = yBase - 3,
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbSexo.Items.AddRange(new string[] { "Masculino", "Femenino", "Otro" });
            pnlFormulario.Controls.Add(cmbSexo);

            // ======= FECHA DE NACIMIENTO =======
            pnlFormulario.Controls.Add(CrearLabel("F. Nacimiento:", 450, yBase + sepY));
            dtpFechaNacimiento = new DateTimePicker()
            {
                Left = 560,
                Top = yBase + sepY - 3,
                Width = 150,
                Format = DateTimePickerFormat.Short
            };
            pnlFormulario.Controls.Add(dtpFechaNacimiento);
            // No permitir fechas futuras
            dtpFechaNacimiento.MaxDate = DateTime.Today;

            // Edad se calcula sola (mejor que el usuario no la escriba)
            txtEdad.ReadOnly = true;
            txtEdad.BackColor = Color.Gainsboro;

            // Evento para recalcular edad
            dtpFechaNacimiento.ValueChanged += (s, e) => ActualizarEdadDesdeNacimiento();

            // calcula una vez al iniciar
            ActualizarEdadDesdeNacimiento();


            // ======= ESTADO CIVIL =======
            pnlFormulario.Controls.Add(CrearLabel("Estado Civil:", xLabel, yBase + sepY * 2));
            cmbEstadoCivil = new ComboBox()
            {
                Left = xField + 20,
                Top = yBase + sepY * 2 - 3,
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbEstadoCivil.Items.AddRange(new string[] { "Soltero", "Casado", "Divorciado", "Unión libre" });
            pnlFormulario.Controls.Add(cmbEstadoCivil);

            // ======= CORREO =======
            pnlFormulario.Controls.Add(CrearLabel("Correo:", 450, yBase + sepY * 2));
            txtCorreo = CrearTextBox(520, yBase + sepY * 2, 250);

            // ======= TELÉFONO MÓVIL =======
            pnlFormulario.Controls.Add(CrearLabel("Teléfono móvil:", xLabel, yBase + sepY * 3));
            txtTelefono = CrearTextBox(xField, yBase + sepY * 3);

            // ======= WHATSAPP =======
            pnlFormulario.Controls.Add(CrearLabel("Whatsapp:", 450, yBase + sepY * 3));
            txtWhatsapp = CrearTextBox(520, yBase + sepY * 3, 150);

            // ======= DIRECCIÓN =======
            pnlFormulario.Controls.Add(CrearLabel("Dirección:", xLabel, yBase + sepY * 4));
            txtDireccion = CrearTextBox(xField, yBase + sepY * 4, 430);

            // ======= CIUDAD =======
            pnlFormulario.Controls.Add(CrearLabel("Ciudad:", 450, yBase + sepY * 4));
            txtCiudad = CrearTextBox(520, yBase + sepY * 4, 180);

            // === BOTÓN AGREGAR ===
            btnAgregar = new Button()
            {
                Text = "Agregar Paciente",
                BackColor = Color.FromArgb(45, 150, 90),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Width = 180,
                Height = 35,
                Left = 350,
                Top = pnlFormulario.Bottom + 15
            };
            btnAgregar.FlatAppearance.BorderSize = 0;
            btnAgregar.Click += BtnAgregar_Click;
            this.Controls.Add(btnAgregar);

            // === DATAGRID ===
            dgvPacientes = new DataGridView()
            {
                Left = 30,
                Top = btnAgregar.Bottom + 20,
                Width = 870,
                Height = 300,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                BackgroundColor = Color.FromArgb(30, 30, 36),
                BorderStyle = BorderStyle.None,
                ScrollBars = ScrollBars.Both,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };
            dgvPacientes.CellDoubleClick += dgvPacientes_CellDoubleClick;
            dgvPacientes.CellContentClick += dgvPacientes_CellContentClick;
            this.Controls.Add(dgvPacientes);

            AplicarEstiloGrid();
        }

        // ===================== UI LISTADO (MODO MedSys) =====================

        private void InicializarUIListado()
        {
            SuspendLayout();

            BackColor = Color.FromArgb(18, 18, 24);
            Font = new Font("Segoe UI", 9.5f);
            FormBorderStyle = FormBorderStyle.None;

            // ===== PANEL CABECERA (DEGRADADO) =====
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 160,                    // un poco más alto
                BackColor = Color.Transparent
            };
            pnlHeader.Paint += PnlHeader_Paint;
            Controls.Add(pnlHeader);

            // Título
            lblHeaderTitulo = new Label
            {
                Text = "Listado de Pacientes",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                AutoSize = true,
                Left = 20,
                Top = 20,
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblHeaderTitulo);

            // Cantidad [ N / N ]
            lblCantidad = new Label
            {
                Text = "[ 0 / 0 ]",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                AutoSize = true,
                Left = 25,
                Top = 55,
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblCantidad);

            // Caja de búsqueda
            txtBuscar = new TextBox
            {
                Width = 420,
                Height = 28,
                Top = 90,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 48),
                BorderStyle = BorderStyle.None,
                Multiline = true,
                Text = ""
            };
            pnlHeader.Controls.Add(txtBuscar);

            // Borde del textbox
            txtBuscar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(70, 70, 82), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, txtBuscar.Width - 1, txtBuscar.Height - 1);
                }
            };

            // Botón Buscar
            btnBuscar = new Button
            {
                Text = "Buscar",
                Top = 90,
                Width = 100,
                Height = 28,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(41, 128, 185)
            };
            btnBuscar.FlatAppearance.BorderSize = 0;
            btnBuscar.Click += BtnBuscar_Click;
            pnlHeader.Controls.Add(btnBuscar);

            // Recolocar búsqueda centrada cuando cambie el tamaño
            pnlHeader.Resize += (s, e) => RecolocarBarraBusqueda();

            // ===== PANEL CONTENIDO =====
            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 36),
                Padding = new Padding(20, 10, 20, 20)   // margen alrededor del grid
            };
            Controls.Add(pnlContent);
            pnlContent.BringToFront();

            // DATAGRID
            dgvPacientes = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                BackgroundColor = Color.FromArgb(30, 30, 36),
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };
            dgvPacientes.CellDoubleClick += dgvPacientes_CellDoubleClick;
            dgvPacientes.CellContentClick += dgvPacientes_CellContentClick;
            pnlContent.Controls.Add(dgvPacientes);

            AplicarEstiloGrid();

            ResumeLayout();
        }

        private void RecolocarBarraBusqueda()
        {
            if (txtBuscar == null || btnBuscar == null || pnlHeader == null) return;

            int espacio = 8; // separación entre caja y botón
            int totalWidth = txtBuscar.Width + espacio + btnBuscar.Width;
            int left = (pnlHeader.Width - totalWidth) / 2;

            txtBuscar.Left = left;
            btnBuscar.Left = txtBuscar.Right + espacio;
        }


        private void PnlHeader_Paint(object sender, PaintEventArgs e)
        {
            var rect = pnlHeader.ClientRectangle;
            if (rect.Width == 0 || rect.Height == 0) return;

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Gradiente de celeste a azul
            using (var brush = new LinearGradientBrush(
                rect,
                Color.FromArgb(80, 180, 255),   // celeste
                Color.FromArgb(20, 50, 120),    // azul más oscuro
                LinearGradientMode.Horizontal))
            {
                g.FillRectangle(brush, rect);
            }

            // Decoración: algunos hexágonos semitransparentes
            DibujarHexagonoDecorativo(g, new PointF(rect.Width * 0.18f, rect.Height * 0.70f), 26f, 0.25f);
            DibujarHexagonoDecorativo(g, new PointF(rect.Width * 0.30f, rect.Height * 0.45f), 34f, 0.18f);
            DibujarHexagonoDecorativo(g, new PointF(rect.Width * 0.55f, rect.Height * 0.65f), 40f, 0.22f);
            DibujarHexagonoDecorativo(g, new PointF(rect.Width * 0.80f, rect.Height * 0.40f), 30f, 0.20f);
        }

        private void DibujarHexagonoDecorativo(Graphics g, PointF centro, float radio, float alpha)
        {
            // Colores semitransparentes
            int aBorde = (int)(255 * alpha);
            int aRelleno = (int)(255 * (alpha * 0.6f));

            Color colorBorde = Color.FromArgb(aBorde, 255, 255, 255);
            Color colorRelleno = Color.FromArgb(aRelleno, 255, 255, 255);

            // Calcular puntos del hexágono
            PointF[] puntos = new PointF[6];
            for (int i = 0; i < 6; i++)
            {
                double angulo = Math.PI / 3 * i - Math.PI / 2; // empezando hacia arriba
                float x = centro.X + radio * (float)Math.Cos(angulo);
                float y = centro.Y + radio * (float)Math.Sin(angulo);
                puntos[i] = new PointF(x, y);
            }

            using (var path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                path.AddPolygon(puntos);

                using (var b = new SolidBrush(colorRelleno))
                using (var p = new Pen(colorBorde, 2f))
                {
                    g.FillPath(b, path);
                    g.DrawPath(p, path);
                }
            }
        }



        private void AplicarEstiloGrid()
        {
            if (dgvPacientes == null) return;

            dgvPacientes.EnableHeadersVisualStyles = false;

            dgvPacientes.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(200, 80, 40);
            dgvPacientes.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPacientes.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            dgvPacientes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dgvPacientes.ColumnHeadersHeight = 34;

            dgvPacientes.DefaultCellStyle.BackColor = Color.FromArgb(40, 40, 48);
            dgvPacientes.DefaultCellStyle.ForeColor = Color.Gainsboro;
            dgvPacientes.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
            dgvPacientes.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvPacientes.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 58);

            dgvPacientes.RowTemplate.Height = 28;
        }

        // ===================== DOBLE CLICK EN PACIENTE =====================

        private void dgvPacientes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // clic en encabezado, lo ignoramos

            var fila = dgvPacientes.Rows[e.RowIndex];

            object valorId = fila.Cells["Id"].Value;
            if (valorId == null) return;

            if (!int.TryParse(valorId.ToString(), out int id))
                return;

            if (PacienteSeleccionado != null)
            {
                PacienteSeleccionado(id);
            }
            else
            {
                new FrmFichaClinica(id).ShowDialog();
            }
        }

        private void dgvPacientes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var grid = dgvPacientes;
            var colName = grid.Columns[e.ColumnIndex].Name;
            var row = grid.Rows[e.RowIndex];

            if (row.Cells["Id"].Value == null) return;
            if (!int.TryParse(row.Cells["Id"].Value.ToString(), out int id)) return;

            if (colName == "colInfo")
            {
                MostrarFichaPaciente(row);
            }
            else if (colName == "colEdit")              // <─ NUEVO
            {
                EditarPaciente(id);
            }
            else if (colName == "colMsg")
            {
                string numero = row.Cells["TelefonoMovil"]?.Value?.ToString();
                EnviarMensajeWhatsApp(numero);
            }
            else if (colName == "colDelete")
            {
                EliminarPaciente(id);
            }
        }




        // ===================== HELPERS DE UI =====================

        private Label CrearLabel(string texto, int x, int y)
        {
            return new Label()
            {
                Text = texto,
                Left = x,
                Top = y,
                AutoSize = true,
                ForeColor = Color.FromArgb(40, 75, 125)
            };
        }

        private TextBox CrearTextBox(int x, int y, int width = 200)
        {
            var txt = new TextBox()
            {
                Left = x,
                Top = y - 3,
                Width = width,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlFormulario.Controls.Add(txt);
            return txt;
        }

        // ===================== INSERTAR PACIENTE =====================

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    var cmd = new MySqlCommand(@"
    INSERT INTO Paciente 
    (Nombre, Edad, Sexo, FechaNacimiento, EstadoCivil, Correo, TelefonoMovil, Whatsapp, Direccion, Ciudad)
    VALUES (@Nombre, @Edad, @Sexo, @FechaNacimiento, @EstadoCivil, @Correo, @TelefonoMovil, @Whatsapp, @Direccion, @Ciudad);", conn);

                    cmd.Parameters.AddWithValue("@Nombre", txtNombre.Text);
                    int edad = CalcularEdad(dtpFechaNacimiento.Value.Date);
                    cmd.Parameters.AddWithValue("@Edad", edad);

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

                MessageBox.Show("Paciente agregado correctamente", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                CargarPacientes();
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al agregar paciente: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LimpiarCampos()
        {
            txtNombre?.Clear();
            txtEdad?.Clear();
            txtTelefono?.Clear();
            txtWhatsapp?.Clear();
            txtDireccion?.Clear();
            txtCorreo?.Clear();
            txtCiudad?.Clear();
            if (cmbSexo != null) cmbSexo.SelectedIndex = -1;
            if (cmbEstadoCivil != null) cmbEstadoCivil.SelectedIndex = -1;
            if (dtpFechaNacimiento != null) dtpFechaNacimiento.Value = DateTime.Today;
            ActualizarEdadDesdeNacimiento();

        }

        // ===================== CARGAR PACIENTES =====================

        void CargarPacientes(string filtro = null)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                var sql = @"
    SELECT 
        Id,
        Nombre,
        Edad,
        Sexo,
        FechaNacimiento,
        EstadoCivil,
        Direccion,
        Ciudad,
        TelefonoMovil,
        Whatsapp,
        Correo
    FROM Paciente";


                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    sql += " WHERE Nombre LIKE @filtro OR Correo LIKE @filtro OR TelefonoMovil LIKE @filtro";
                }

                sql += " ORDER BY Nombre;";

                var da = new MySqlDataAdapter(sql, conn);
                if (!string.IsNullOrWhiteSpace(filtro))
                    da.SelectCommand.Parameters.AddWithValue("@filtro", "%" + filtro + "%");

                var dt = new DataTable();
                da.Fill(dt);

                dgvPacientes.DataSource = dt;

                var grid = dgvPacientes;
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                grid.AllowUserToAddRows = false;

                // Ocultar Id
                if (grid.Columns["Id"] != null)
                    grid.Columns["Id"].Visible = false;

                // Encabezados amigables
                if (grid.Columns["FechaNacimiento"] != null)
                    grid.Columns["FechaNacimiento"].HeaderText = "Nacimiento";
                if (grid.Columns["TelefonoMovil"] != null)
                    grid.Columns["TelefonoMovil"].HeaderText = "Tel. Móvil";

                // Anchos personalizados
                if (grid.Columns["Nombre"] != null) grid.Columns["Nombre"].Width = 200;
                if (grid.Columns["Edad"] != null) grid.Columns["Edad"].Width = 50;
                if (grid.Columns["Sexo"] != null) grid.Columns["Sexo"].Width = 80;
                if (grid.Columns["FechaNacimiento"] != null) grid.Columns["FechaNacimiento"].Width = 90;
                if (grid.Columns["EstadoCivil"] != null) grid.Columns["EstadoCivil"].Width = 90;
                if (grid.Columns["Direccion"] != null) grid.Columns["Direccion"].Width = 160;   
                if (grid.Columns["Ciudad"] != null) grid.Columns["Ciudad"].Width = 90;       
                if (grid.Columns["TelefonoMovil"] != null) grid.Columns["TelefonoMovil"].Width = 110;
                if (grid.Columns["Whatsapp"] != null) grid.Columns["Whatsapp"].Width = 110;
                if (grid.Columns["Correo"] != null) grid.Columns["Correo"].Width = 140;       


                grid.RowTemplate.Height = 28;

                // Actualizar contador en modo listado
                if (lblCantidad != null)
                {
                    int total = dt.Rows.Count;
                    lblCantidad.Text = $"[ {total} / {total} ]";
                }
            }
            // Añadir columnas de acción (si aún no existen)
            AgregarColumnasAccion();
        }

        private void BtnBuscar_Click(object sender, EventArgs e)
        {
            var filtro = txtBuscar.Text?.Trim();
            CargarPacientes(filtro);
        }

        private void AgregarColumnasAccion()
        {
            if (dgvPacientes == null) return;

            CrearIconosAccionGrid();

            // Eliminar versiones anteriores
            string[] colsAccion = { "colInfo", "colEdit", "colMsg", "colDelete" };
            foreach (var name in colsAccion)
            {
                if (dgvPacientes.Columns.Contains(name))
                    dgvPacientes.Columns.Remove(name);
            }

            // Info
            var colInfo = new DataGridViewImageColumn
            {
                Name = "colInfo",
                HeaderText = "",
                Image = iconInfoGrid,
                Width = 28,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };
            dgvPacientes.Columns.Add(colInfo);

            // Editar (amarillo)
            var colEdit = new DataGridViewImageColumn
            {
                Name = "colEdit",
                HeaderText = "",
                Image = iconEditGrid,
                Width = 28,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };
            dgvPacientes.Columns.Add(colEdit);

            // Mensaje / WhatsApp (verde)
            var colMsg = new DataGridViewImageColumn
            {
                Name = "colMsg",
                HeaderText = "",
                Image = iconMsgGrid,
                Width = 28,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };
            dgvPacientes.Columns.Add(colMsg);

            // Eliminar (rojo)
            var colDelete = new DataGridViewImageColumn
            {
                Name = "colDelete",
                HeaderText = "",
                Image = iconDeleteGrid,
                Width = 28,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };
            dgvPacientes.Columns.Add(colDelete);

            dgvPacientes.RowTemplate.Height = 30;
        }






        private void MostrarFichaPaciente(DataGridViewRow row)
        {
            string nombre = row.Cells["Nombre"]?.Value?.ToString();
            string edad = row.Cells["Edad"]?.Value?.ToString();
            string nacimiento = row.Cells["FechaNacimiento"]?.Value?.ToString();
            string telefono = row.Cells["TelefonoMovil"]?.Value?.ToString();
            string whatsapp = row.Cells["Whatsapp"]?.Value?.ToString();
            string correo = row.Cells["Correo"]?.Value?.ToString();
            string direccion = row.Cells["Direccion"]?.Value?.ToString();
            string ciudad = row.Cells["Ciudad"]?.Value?.ToString();


            using (var frm = new FrmPacienteInfo(
    nombre,
    edad,
    nacimiento,
    telefono,
    whatsapp,
    correo,
    direccion,
    ciudad))
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog(this);
            }
        }

        private void EnviarMensajeWhatsApp(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero))
            {
                MessageBox.Show("El paciente no tiene teléfono móvil registrado.",
                    "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Dejamos solo dígitos
            string soloDigitos = new string(numero.Where(char.IsDigit).ToArray());
            if (string.IsNullOrWhiteSpace(soloDigitos))
            {
                MessageBox.Show("El número de teléfono no es válido.",
                    "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string url = "https://wa.me/" + soloDigitos;

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir WhatsApp Web.\n" + ex.Message,
                    "WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EliminarPaciente(int id)
        {
            if (MessageBox.Show("¿Deseas eliminar este paciente?",
                "Confirmar eliminación", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    var cmd = new MySqlCommand("DELETE FROM Paciente WHERE Id = @Id;", conn);
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }

                // Recargar manteniendo el filtro actual si existiera
                string filtro = txtBuscar != null ? txtBuscar.Text?.Trim() : null;
                CargarPacientes(filtro);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar paciente:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CrearIconosAccion()
        {
            if (iconInfo != null) return; // ya creados

            iconInfo = CrearIconoCircular(Color.FromArgb(52, 152, 219), "i");  // azul info
            iconMsg = CrearIconoCircular(Color.FromArgb(39, 174, 96), "W");  // verde WhatsApp
            iconDelete = CrearIconoCircular(Color.FromArgb(231, 76, 60), "X");  // rojo eliminar
            iconEdit = CrearIconoCircular(Color.FromArgb(241, 196, 15), "E"); // amarillo editar
        }



        private Image CrearIconoCircular(Color colorFondo, string texto)
        {
            int size = 24;
            var bmp = new Bitmap(size, size);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (var brush = new SolidBrush(colorFondo))
                using (var pen = new Pen(Color.White, 2f))
                {
                    var rect = new Rectangle(2, 2, size - 4, size - 4);
                    g.FillEllipse(brush, rect);
                    g.DrawEllipse(pen, rect);
                }

                using (var font = new Font("Segoe UI", 10f, FontStyle.Bold))
                using (var brushText = new SolidBrush(Color.White))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(texto, font, brushText, new RectangleF(0, 0, size, size), sf);
                }
            }
            return bmp;
        }

        private void EditarPaciente(int id)
        {
            using (var frm = new FrmPacienteEditar(id))
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    // recargar lista manteniendo filtro actual (si existe)
                    string filtro = txtBuscar != null ? txtBuscar.Text?.Trim() : null;
                    CargarPacientes(filtro);
                }
            }
        }
        private void CrearIconosAccionGrid()
        {
            if (iconInfoGrid != null) return; // ya creados

            // Usa aquí los recursos que ya cargaste en Properties.Resources
            iconInfoGrid = CrearIconoCircularConImagen(
                Color.FromArgb(52, 152, 219),   // azul info
                Properties.Resources.informacion2);

            iconEditGrid = CrearIconoCircularConImagen(
                Color.FromArgb(241, 196, 15),   // AMARILLO editar
                Properties.Resources.editar);

            iconMsgGrid = CrearIconoCircularConImagen(
                Color.FromArgb(39, 174, 96),    // VERDE WhatsApp
                Properties.Resources.whatsapp);

            iconDeleteGrid = CrearIconoCircularConImagen(
                Color.FromArgb(231, 76, 60),    // ROJO eliminar
                Properties.Resources.eliminarusuario);
        }

        private Image CrearIconoCircularConImagen(Color backColor, Image glyph)
        {
            int size = 24;
            var bmp = new Bitmap(size, size);

            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (var brush = new SolidBrush(backColor))
                using (var pen = new Pen(Color.White, 2f))
                {
                    var rect = new Rectangle(1, 1, size - 3, size - 3);
                    g.FillEllipse(brush, rect);
                    g.DrawEllipse(pen, rect);
                }

                if (glyph != null)
                {
                    int padding = 5;
                    var rGlyph = new Rectangle(
                        padding,
                        padding,
                        size - padding * 2,
                        size - padding * 2);

                    g.DrawImage(glyph, rGlyph);
                }
            }

            return bmp;
        }

        private void ActualizarEdadDesdeNacimiento()
        {
            int edad = CalcularEdad(dtpFechaNacimiento.Value.Date);
            txtEdad.Text = edad.ToString();
        }

        private int CalcularEdad(DateTime fechaNac)
        {
            var hoy = DateTime.Today;
            int edad = hoy.Year - fechaNac.Year;
            if (fechaNac > hoy.AddYears(-edad)) edad--;
            if (edad < 0) edad = 0;
            return edad;
        }





    }
}
