using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ConsultorioDentalApp.Data;


namespace ConsultorioDentalApp.Forms
{
    public class FrmProformaEditar : Form
    {
        private readonly int _proformaId;
        private readonly int _pacienteId;
        private readonly string _nombrePaciente;

        private Label lblTitulo;
        private TabControl tab;
        private TabPage tabDetalle;
        private TabPage tabPagos;

        private DataGridView dgvDetalle;
        private DataGridView dgvPagos;

        private Panel panelRight;
        private Label lblTotalFactura;
        private Label lblTotalPagos;
        private Label lblSaldoPorPagar;
        private Label lblTotalFacturaValor;
        private Label lblTotalPagosValor;
        private Label lblSaldoPorPagarValor;
        private Button btnGuardar;
        private Button btnCerrar;
        private DateTimePicker _dtpPago;


        private DataTable _dtDetalle;
        private DataTable _dtPagos;

        public FrmProformaEditar(int proformaId, int pacienteId, string nombrePaciente)
        {
            _proformaId = proformaId;
            _pacienteId = pacienteId;
            _nombrePaciente = nombrePaciente;

            InitializeComponent();
            CargarDatos();
        }

        private void InitializeComponent()
        {
            Text = "Proforma";
            BackColor = Color.FromArgb(20, 20, 24);
            Font = new Font("Segoe UI", 10f);
            WindowState = FormWindowState.Maximized;

            // ===== TÍTULO =====
            lblTitulo = new Label
            {
                Text = $"Proforma {_proformaId:0000000} - {_nombrePaciente}",
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
                BackColor = Color.FromArgb(35, 35, 40)
            };
            Controls.Add(lblTitulo);

            // ===== PANEL DERECHO (TOTALES + BOTONES) =====
            panelRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = 260,
                BackColor = Color.FromArgb(30, 30, 34)
            };
            Controls.Add(panelRight);

            lblTotalFactura = new Label
            {
                Text = "Valor Total:",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Left = 20,
                Top = 30,
                AutoSize = true
            };
            panelRight.Controls.Add(lblTotalFactura);

            lblTotalFacturaValor = new Label
            {
                Text = "0,00",
                ForeColor = Color.LimeGreen,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Left = 20,
                Top = 50,
                Width = panelRight.Width - 40,
                BackColor = Color.Black
            };
            panelRight.Controls.Add(lblTotalFacturaValor);

            lblTotalPagos = new Label
            {
                Text = "Pagos realizados a la fecha:",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Left = 20,
                Top = 100,
                AutoSize = true
            };
            panelRight.Controls.Add(lblTotalPagos);

            lblTotalPagosValor = new Label
            {
                Text = "0,00",
                ForeColor = Color.DeepSkyBlue,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Left = 20,
                Top = 120,
                Width = panelRight.Width - 40,
                BackColor = Color.Black
            };
            panelRight.Controls.Add(lblTotalPagosValor);

            lblSaldoPorPagar = new Label
            {
                Text = "Saldo por pagar:",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                Left = 20,
                Top = 170,
                AutoSize = true
            };
            panelRight.Controls.Add(lblSaldoPorPagar);

            lblSaldoPorPagarValor = new Label
            {
                Text = "0,00",
                ForeColor = Color.OrangeRed,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Left = 20,
                Top = 190,
                Width = panelRight.Width - 40,
                BackColor = Color.Black
            };
            panelRight.Controls.Add(lblSaldoPorPagarValor);

            btnGuardar = new Button
            {
                Text = "Guardar",
                Width = panelRight.Width - 40,
                Height = 36,
                Left = 20,
                Top = 260,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;
            panelRight.Controls.Add(btnGuardar);

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Width = panelRight.Width - 40,
                Height = 36,
                Left = 20,
                Top = 310,
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCerrar.FlatAppearance.BorderSize = 0;
            btnCerrar.Click += (s, e) => Close();
            panelRight.Controls.Add(btnCerrar);

            // ===== TABCONTROL: DETALLE / PAGOS =====
            tab = new TabControl
            {
                Dock = DockStyle.Fill,
                Alignment = TabAlignment.Top
            };
            Controls.Add(tab);
            Controls.SetChildIndex(tab, 0);

            tabDetalle = new TabPage("Detalle") { BackColor = Color.FromArgb(20, 20, 24) };
            tabPagos = new TabPage("Pagos") { BackColor = Color.FromArgb(20, 20, 24) };
            tab.Controls.Add(tabDetalle);
            tab.Controls.Add(tabPagos);

            // ===== GRID DETALLE =====
            dgvDetalle = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(30, 30, 34),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoGenerateColumns = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect
            };
            dgvDetalle.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 50);
            dgvDetalle.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvDetalle.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDetalle.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 50);
            dgvDetalle.DefaultCellStyle.ForeColor = Color.White;
            dgvDetalle.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(55, 55, 60);

            dgvDetalle.CellEndEdit += (s, e) => RecalcularTotales();
            dgvDetalle.UserDeletedRow += (s, e) => RecalcularTotales();
            dgvDetalle.EditingControlShowing += DgvDetalle_EditingControlShowing;

            tabDetalle.Controls.Add(dgvDetalle);

            // ===== GRID PAGOS =====
            dgvPagos = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(30, 30, 34),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoGenerateColumns = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.CellSelect
            };
            dgvPagos.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 50);
            dgvPagos.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvPagos.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvPagos.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 50);
            dgvPagos.DefaultCellStyle.ForeColor = Color.White;
            dgvPagos.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(55, 55, 60);

            dgvPagos.CellEndEdit += (s, e) => RecalcularTotales();
            dgvPagos.UserDeletedRow += (s, e) => RecalcularTotales();

            // 👉 NUEVO: eventos para fecha por defecto y DateTimePicker
            dgvPagos.DefaultValuesNeeded += DgvPagos_DefaultValuesNeeded;
            dgvPagos.CellBeginEdit += DgvPagos_CellBeginEdit;
            dgvPagos.Scroll += (s, e) => OcultarDatePickerPago();
            dgvPagos.ColumnWidthChanged += (s, e) => OcultarDatePickerPago();
            dgvPagos.SizeChanged += (s, e) => OcultarDatePickerPago();

            // 👉 NUEVO: DateTimePicker embebido en el grid de pagos
            _dtpPago = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy HH:mm",  // si solo quieres fecha: "dd/MM/yyyy"
                Visible = false
            };
            dgvPagos.Controls.Add(_dtpPago);
            _dtpPago.ValueChanged += DtpPago_ValueChanged;
            _dtpPago.CloseUp += (s, e) => OcultarDatePickerPago();

            tabPagos.Controls.Add(dgvPagos);
        }

        // ==================== CARGA DE DATOS ====================

        private void CargarDatos()
        {
            CargarDetalle();
            CargarPagos();
            RecalcularTotales();
        }

        private void CargarDetalle()
        {
            _dtDetalle = new DataTable();
            _dtDetalle.Columns.Add("Id", typeof(int));
            _dtDetalle.Columns.Add("ItemId", typeof(int));
            _dtDetalle.Columns.Add("CodigoArticulo", typeof(string));
            _dtDetalle.Columns.Add("Detalle", typeof(string));
            _dtDetalle.Columns.Add("Clase", typeof(string));
            _dtDetalle.Columns.Add("PrecioUnitario", typeof(decimal));
            _dtDetalle.Columns.Add("Cantidad", typeof(decimal));
            _dtDetalle.Columns.Add("Importe", typeof(decimal));
            

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                // 1) Datos de la proforma (para totales iniciales si hace falta)
                string sqlCab = "SELECT Total, TotalPagado, SaldoPendiente FROM Proforma WHERE Id = @Id;";
                using (var cmd = new MySqlCommand(sqlCab, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", _proformaId);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            decimal total = rd.IsDBNull(0) ? 0 : rd.GetDecimal(0);
                            decimal totalPagos = rd.IsDBNull(1) ? 0 : rd.GetDecimal(1);
                            decimal saldo = rd.IsDBNull(2) ? 0 : rd.GetDecimal(2);

                            lblTotalFacturaValor.Text = total.ToString("N2");
                            lblTotalPagosValor.Text = totalPagos.ToString("N2");
                            lblSaldoPorPagarValor.Text = saldo.ToString("N2");
                        }
                    }
                }

                // 2) Detalle de la proforma
                string sqlDet = @"
    SELECT 
        d.Id,
        d.ProformaId,
        d.ItemId,
        d.CodigoArticulo,
        d.Detalle,
        d.Clase,
        d.Cantidad,
        d.PrecioUnitario,
        d.Importe,
        i.codigo      AS CodigoItem,
        i.descripcion AS DetalleItem,
        i.categoria   AS ClaseItem,
        i.precio      AS PrecioItem,
        i.stock       AS StockItem
    FROM ProformaDetalle d
    LEFT JOIN items i ON d.ItemId = i.id
    WHERE d.ProformaId = @Id
    ORDER BY d.Id;";


                using (var da = new MySqlDataAdapter(sqlDet, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@Id", _proformaId);
                    da.Fill(_dtDetalle);
                }
            }

            ConfigurarColumnasDetalle();
            dgvDetalle.DataSource = _dtDetalle;
        }

        private void ConfigurarColumnasDetalle()
        {
            dgvDetalle.Columns.Clear();

            // Combo de código de artículo
            var colCodigo = new DataGridViewComboBoxColumn
            {
                Name = "CodigoArticulo",
                HeaderText = "Cod. Artículo",
                DataPropertyName = "CodigoArticulo",
                DropDownWidth = 120,
                Width = 120,
                FlatStyle = FlatStyle.Flat
            };

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                var dtItems = new DataTable();
                using (var da = new MySqlDataAdapter(
                    @"SELECT 
              codigo     AS Codigo,
              descripcion AS Nombre,
              categoria   AS Clase,
              precio      AS Precio
          FROM items
          WHERE activo = 1
          ORDER BY codigo;", conn))
                {
                    da.Fill(dtItems);
                }

                colCodigo.DataSource = dtItems;
                colCodigo.DisplayMember = "Codigo";
                colCodigo.ValueMember = "Codigo";

                colCodigo.Tag = dtItems;
            }


            dgvDetalle.Columns.Add(colCodigo);

            // Detalle (texto)
            dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Detalle",
                HeaderText = "Detalle",
                DataPropertyName = "Detalle",
                Width = 260
            });

            // Clase (texto, podríamos hacerla combo si quieres)
            dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Clase",
                HeaderText = "Clase",
                DataPropertyName = "Clase",
                Width = 120
            });

            dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PrecioUnitario",
                HeaderText = "Precio Unit.",
                DataPropertyName = "PrecioUnitario",
                Width = 90,
                DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Cantidad",
                HeaderText = "Cant.",
                DataPropertyName = "Cantidad",
                Width = 70,
                DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            dgvDetalle.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Importe",
                HeaderText = "Importe",
                DataPropertyName = "Importe",
                Width = 100,
                DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Columnas ocultas
            var colId = new DataGridViewTextBoxColumn
            {
                Name = "Id",
                DataPropertyName = "Id",
                Visible = false
            };
            dgvDetalle.Columns.Add(colId);

            var colItemId = new DataGridViewTextBoxColumn
            {
                Name = "ItemId",
                DataPropertyName = "ItemId",
                Visible = false
            };
            dgvDetalle.Columns.Add(colItemId);
        }

        private void DgvDetalle_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (dgvDetalle.CurrentCell == null) return;

            if (dgvDetalle.Columns[dgvDetalle.CurrentCell.ColumnIndex].Name == "CodigoArticulo" &&
                e.Control is ComboBox combo)
            {
                combo.SelectedIndexChanged -= ComboCodigo_SelectedIndexChanged;
                combo.SelectedIndexChanged += ComboCodigo_SelectedIndexChanged;
            }
        }

        private void ComboCodigo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!(sender is ComboBox combo)) return;
            if (dgvDetalle.CurrentRow == null) return;

            var col = dgvDetalle.Columns["CodigoArticulo"] as DataGridViewComboBoxColumn;

            // 🔁 Reemplaza estas dos líneas:
            // if (col?.Tag is not DataTable dtItems) return;

            // ✅ Por estas:
            var dtItems = col != null ? col.Tag as DataTable : null;
            if (dtItems == null) return;

            string codigo = combo.Text;
            var rows = dtItems.Select($"Codigo = '{codigo.Replace("'", "''")}'");
            if (rows.Length == 0) return;

            var r = rows[0];

            dgvDetalle.CurrentRow.Cells["Detalle"].Value = r["Nombre"].ToString();
            dgvDetalle.CurrentRow.Cells["Clase"].Value = r["Clase"].ToString();
            dgvDetalle.CurrentRow.Cells["PrecioUnitario"].Value = Convert.ToDecimal(r["Precio"]);

            if (dgvDetalle.CurrentRow.Cells["Cantidad"].Value == null ||
                string.IsNullOrWhiteSpace(dgvDetalle.CurrentRow.Cells["Cantidad"].Value.ToString()))
            {
                dgvDetalle.CurrentRow.Cells["Cantidad"].Value = 1m;
            }

            RecalcularImporteFila(dgvDetalle.CurrentRow);
            RecalcularTotales();
        }


        private void RecalcularImporteFila(DataGridViewRow row)
        {
            if (row == null) return;

            decimal precio = 0, cantidad = 0;

            if (row.Cells["PrecioUnitario"].Value != null)
                decimal.TryParse(row.Cells["PrecioUnitario"].Value.ToString(), out precio);

            if (row.Cells["Cantidad"].Value != null)
                decimal.TryParse(row.Cells["Cantidad"].Value.ToString(), out cantidad);

            row.Cells["Importe"].Value = precio * cantidad;
        }

        private void CargarPagos()
        {
            _dtPagos = new DataTable();
            _dtPagos.Columns.Add("Id", typeof(int));
            _dtPagos.Columns.Add("TipoPago", typeof(string));
            _dtPagos.Columns.Add("EstadoPago", typeof(string));
            _dtPagos.Columns.Add("Fecha", typeof(DateTime));
            _dtPagos.Columns.Add("Monto", typeof(decimal));

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();

                string sql = @"
                    SELECT Id, TipoPago, EstadoPago, Fecha, Monto
                    FROM ProformaPago
                    WHERE ProformaId = @Id
                    ORDER BY Fecha;";

                using (var da = new MySqlDataAdapter(sql, conn))
                {
                    da.SelectCommand.Parameters.AddWithValue("@Id", _proformaId);
                    da.Fill(_dtPagos);
                }
            }

            ConfigurarColumnasPagos();
            dgvPagos.DataSource = _dtPagos;
        }

        private void ConfigurarColumnasPagos()
        {
            dgvPagos.Columns.Clear();

            // Tipo de pago (combo)
            var colTipo = new DataGridViewComboBoxColumn
            {
                Name = "TipoPago",
                HeaderText = "Tipo de pago",
                DataPropertyName = "TipoPago",
                Width = 140,
                FlatStyle = FlatStyle.Flat,
                DataSource = new[] { "Efectivo", "Tarjeta", "Transferencia", "Cheque", "Otro" }
            };
            dgvPagos.Columns.Add(colTipo);

            // Estado (combo)
            var colEstado = new DataGridViewComboBoxColumn
            {
                Name = "EstadoPago",
                HeaderText = "Estado",
                DataPropertyName = "EstadoPago",
                Width = 120,
                FlatStyle = FlatStyle.Flat,
                DataSource = new[] { "Pendiente", "Recibido", "Anulado" }
            };
            dgvPagos.Columns.Add(colEstado);

            // Fecha
            dgvPagos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Fecha",
                HeaderText = "Fecha",
                DataPropertyName = "Fecha",
                Width = 140,
                DefaultCellStyle = { Format = "dd/MM/yyyy HH:mm" }
            });

            // Monto
            dgvPagos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Monto",
                HeaderText = "Abono",
                DataPropertyName = "Monto",
                Width = 120,
                DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Id oculto
            dgvPagos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id",
                DataPropertyName = "Id",
                Visible = false
            });
        }

        // ==================== PAGOS: FECHA AUTOMÁTICA Y CALENDARIO ====================

        // Se llama cuando se crea una nueva fila en el grid
        private void DgvPagos_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["TipoPago"].Value = "Efectivo";    // opcional
            e.Row.Cells["EstadoPago"].Value = "Recibido";  // opcional
            e.Row.Cells["Fecha"].Value = DateTime.Now;     // fecha automática
        }

        // Mostrar el DateTimePicker al empezar a editar la columna Fecha
        private void DgvPagos_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (dgvPagos.Columns[e.ColumnIndex].Name != "Fecha")
            {
                OcultarDatePickerPago();
                return;
            }

            Rectangle rect = dgvPagos.GetCellDisplayRectangle(e.ColumnIndex, e.RowIndex, true);

            _dtpPago.Bounds = rect;
            _dtpPago.Visible = true;

            object valorActual = dgvPagos.Rows[e.RowIndex].Cells["Fecha"].Value;
            if (valorActual != null && valorActual != DBNull.Value)
                _dtpPago.Value = Convert.ToDateTime(valorActual);
            else
                _dtpPago.Value = DateTime.Now;
        }

        // Cuando el usuario cambia la fecha, la escribimos en la celda
        private void DtpPago_ValueChanged(object sender, EventArgs e)
        {
            if (dgvPagos.CurrentCell == null) return;
            if (dgvPagos.Columns[dgvPagos.CurrentCell.ColumnIndex].Name != "Fecha") return;

            dgvPagos.CurrentCell.Value = _dtpPago.Value;
        }

        // Oculta el DateTimePicker (al salir de la celda, scroll, etc.)
        private void OcultarDatePickerPago()
        {
            if (_dtpPago != null)
                _dtpPago.Visible = false;
        }


        // ==================== TOTALES ====================

        private void RecalcularTotales()
        {
            // 1) Total de la factura (suma de importes del detalle)
            decimal totalFactura = 0m;
            if (_dtDetalle != null)
            {
                foreach (DataRow row in _dtDetalle.Rows)
                {
                    if (row.RowState == DataRowState.Deleted) continue;
                    if (row["Importe"] == DBNull.Value) continue;

                    totalFactura += Convert.ToDecimal(row["Importe"]);
                }
            }

            // 2) Total de pagos y saldo restante
            decimal totalPagos = 0m;
            decimal saldoRestante = totalFactura;

            if (_dtPagos != null)
            {
                foreach (DataRow row in _dtPagos.Rows)
                {
                    if (row.RowState == DataRowState.Deleted) continue;
                    if (row["Monto"] == DBNull.Value) continue;

                    // solo pagos efectivamente recibidos
                    string estado = row["EstadoPago"]?.ToString() ?? "";
                    if (!estado.Equals("Recibido", StringComparison.OrdinalIgnoreCase))
                        continue;

                    decimal montoPago = Convert.ToDecimal(row["Monto"]);

                    totalPagos += montoPago;
                    saldoRestante -= montoPago;
                }
            }

            // 3) No permitir que el saldo baje de 0
            if (saldoRestante < 0)
                saldoRestante = 0;

            // 4) Mostrar totales
            lblTotalFacturaValor.Text = totalFactura.ToString("N2");
            lblTotalPagosValor.Text = totalPagos.ToString("N2");
            lblSaldoPorPagarValor.Text = saldoRestante.ToString("N2");
        }


        // ==================== GUARDAR ====================

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    {
                        // 1) Recalcular totales para guardar
                        RecalcularTotales();

                        decimal totalFactura = decimal.Parse(lblTotalFacturaValor.Text);
                        decimal totalPagos = decimal.Parse(lblTotalPagosValor.Text);
                        decimal saldo = decimal.Parse(lblSaldoPorPagarValor.Text);

                        // 2) Actualizar cabecera
                        string sqlCab = @"
                            UPDATE Proforma
                            SET Total = @Total,
                                TotalPagado = @TotalPagado,
                                SaldoPendiente = @SaldoPendiente
                            WHERE Id = @Id;";

                        using (var cmdCab = new MySqlCommand(sqlCab, conn, tx))
                        {
                            cmdCab.Parameters.AddWithValue("@Total", totalFactura);
                            cmdCab.Parameters.AddWithValue("@TotalPagado", totalPagos);
                            cmdCab.Parameters.AddWithValue("@SaldoPendiente", saldo);
                            cmdCab.Parameters.AddWithValue("@Id", _proformaId);
                            cmdCab.ExecuteNonQuery();
                        }

                        // 3) Borrar detalle y volver a insertar
                        using (var cmdDelDet = new MySqlCommand("DELETE FROM ProformaDetalle WHERE ProformaId = @Id;", conn, tx))
                        {
                            cmdDelDet.Parameters.AddWithValue("@Id", _proformaId);
                            cmdDelDet.ExecuteNonQuery();
                        }

                        string sqlInsDet = @"
    INSERT INTO ProformaDetalle
    (ProformaId, ItemId, CodigoArticulo, Detalle, Clase, PrecioUnitario, Cantidad, Importe)
    VALUES (@ProformaId, @ItemId, @CodigoArticulo, @Detalle, @Clase, @PrecioUnitario, @Cantidad, @Importe);";


                        using (var cmdInsDet = new MySqlCommand(sqlInsDet, conn, tx))
                        {
                            cmdInsDet.Parameters.Add("@ProformaId", MySqlDbType.Int32);
                            cmdInsDet.Parameters.Add("@ItemId", MySqlDbType.Int32);
                            cmdInsDet.Parameters.Add("@CodigoArticulo", MySqlDbType.VarChar);
                            cmdInsDet.Parameters.Add("@Detalle", MySqlDbType.VarChar);
                            cmdInsDet.Parameters.Add("@Clase", MySqlDbType.VarChar);
                            cmdInsDet.Parameters.Add("@PrecioUnitario", MySqlDbType.Decimal);
                            cmdInsDet.Parameters.Add("@Cantidad", MySqlDbType.Decimal);
                            cmdInsDet.Parameters.Add("@Importe", MySqlDbType.Decimal);

                            foreach (DataRow row in _dtDetalle.Rows)
                            {
                                if (row.RowState == DataRowState.Deleted) continue;

                                var codigo = row["CodigoArticulo"]?.ToString();
                                if (string.IsNullOrWhiteSpace(codigo))
                                    continue; // fila vacía, no la guardamos

                                int itemId = ObtenerItemIdPorCodigo(codigo, conn, tx);

                                cmdInsDet.Parameters["@ProformaId"].Value = _proformaId;
                                cmdInsDet.Parameters["@ItemId"].Value = itemId;
                                cmdInsDet.Parameters["@CodigoArticulo"].Value = codigo;
                                cmdInsDet.Parameters["@Detalle"].Value = row["Detalle"] ?? "";
                                cmdInsDet.Parameters["@Clase"].Value = row["Clase"] ?? "";
                                cmdInsDet.Parameters["@PrecioUnitario"].Value =
                                    row["PrecioUnitario"] == DBNull.Value ? 0 : row["PrecioUnitario"];
                                cmdInsDet.Parameters["@Cantidad"].Value =
                                    row["Cantidad"] == DBNull.Value ? 0 : row["Cantidad"];
                                cmdInsDet.Parameters["@Importe"].Value =
                                    row["Importe"] == DBNull.Value ? 0 : row["Importe"];

                                cmdInsDet.ExecuteNonQuery();
                            }
                        }


                        // 4) Borrar pagos y volver a insertarlos
                        using (var cmdDelPag = new MySqlCommand("DELETE FROM ProformaPago WHERE ProformaId = @Id;", conn, tx))
                        {
                            cmdDelPag.Parameters.AddWithValue("@Id", _proformaId);
                            cmdDelPag.ExecuteNonQuery();
                        }

                        string sqlInsPag = @"
                            INSERT INTO ProformaPago
                            (ProformaId, TipoPago, EstadoPago, Fecha, Monto)
                            VALUES (@ProformaId, @TipoPago, @EstadoPago, @Fecha, @Monto);";

                        using (var cmdInsPag = new MySqlCommand(sqlInsPag, conn, tx))
                        {
                            cmdInsPag.Parameters.Add("@ProformaId", MySqlDbType.Int32);
                            cmdInsPag.Parameters.Add("@TipoPago", MySqlDbType.VarChar);
                            cmdInsPag.Parameters.Add("@EstadoPago", MySqlDbType.VarChar);
                            cmdInsPag.Parameters.Add("@Fecha", MySqlDbType.DateTime);
                            cmdInsPag.Parameters.Add("@Monto", MySqlDbType.Decimal);

                            foreach (DataRow row in _dtPagos.Rows)
                            {
                                if (row.RowState == DataRowState.Deleted) continue;
                                if (row["Monto"] == DBNull.Value) continue;

                                cmdInsPag.Parameters["@ProformaId"].Value = _proformaId;
                                cmdInsPag.Parameters["@TipoPago"].Value = row["TipoPago"] ?? "";
                                cmdInsPag.Parameters["@EstadoPago"].Value = row["EstadoPago"] ?? "Recibido";
                                cmdInsPag.Parameters["@Fecha"].Value =
                                    row["Fecha"] == DBNull.Value ? DateTime.Now : row["Fecha"];
                                cmdInsPag.Parameters["@Monto"].Value = row["Monto"];

                                cmdInsPag.ExecuteNonQuery();
                            }
                        }

                        tx.Commit();
                    }
                }

                MessageBox.Show("Proforma guardada correctamente.", "OK",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la proforma:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int ObtenerItemIdPorCodigo(string codigo, MySqlConnection conn, MySqlTransaction tx)
        {
            string sql = "SELECT Id FROM Items WHERE Codigo = @Codigo LIMIT 1;";
            using (var cmd = new MySqlCommand(sql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@Codigo", codigo);
                var result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }
    }
}
