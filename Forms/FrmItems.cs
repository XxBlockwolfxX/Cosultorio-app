using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ConsultorioDentalApp.Data;  

namespace ConsultorioDentalApp.Forms
{
    public partial class FrmItems : Form
    {
        private Panel pnlHeader;
        private Label lblTitulo;
        private Button btnNuevoItem;

        private Panel pnlGrid;
        private DataGridView dgvItems;

        public FrmItems()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            BackColor = Color.FromArgb(25, 25, 30);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9f);
            FormBorderStyle = FormBorderStyle.None;
            Dock = DockStyle.Fill;

            // ===== CABECERA =====
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(0, 122, 204)
            };
            Controls.Add(pnlHeader);

            lblTitulo = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Left,
                Width = 400,
                Text = "Items / Tratamientos",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 20f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(30, 0, 0, 0)
            };
            pnlHeader.Controls.Add(lblTitulo);

            btnNuevoItem = new Button
            {
                Text = "Nuevo item",
                Width = 140,
                Height = 36,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(0, 180, 120),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNuevoItem.FlatAppearance.BorderSize = 0;
            btnNuevoItem.Top = (pnlHeader.Height - btnNuevoItem.Height) / 2;
            btnNuevoItem.Left = pnlHeader.Width - btnNuevoItem.Width - 40;
            btnNuevoItem.Click += BtnNuevoItem_Click;
            pnlHeader.Controls.Add(btnNuevoItem);

            pnlHeader.Resize += (s, e) =>
            {
                btnNuevoItem.Top = (pnlHeader.Height - btnNuevoItem.Height) / 2;
                btnNuevoItem.Left = pnlHeader.Width - btnNuevoItem.Width - 40;
            };

            // ===== PANEL GRID =====
            pnlGrid = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 30),
                Padding = new Padding(40, 80, 40, 40)
            };

            Controls.Add(pnlGrid);

            // ===== DATAGRID =====
            dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(35, 35, 40),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                ReadOnly = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false
            };

            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(50, 50, 60);
            dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvItems.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgvItems.ColumnHeadersHeight = 32;

            dgvItems.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 55);
            dgvItems.DefaultCellStyle.ForeColor = Color.White;
            dgvItems.DefaultCellStyle.SelectionBackColor = Color.FromArgb(70, 70, 90);
            dgvItems.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvItems.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 50);

            pnlGrid.Controls.Add(dgvItems);

            ConfigurarColumnas();

            dgvItems.CellClick += DgvItems_CellClick;
            dgvItems.CellEndEdit += DgvItems_CellEndEdit;

            CargarItemsDeBase();
        }

        private void ConfigurarColumnas()
        {
            dgvItems.Columns.Clear();

            // ID (oculta, clave primaria)
            var colId = new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "Id",
                Visible = false
            };
            dgvItems.Columns.Add(colId);

            // Código
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Codigo",
                HeaderText = "Código Item",
                FillWeight = 80
            });

            // Categoría
            var colCategoria = new DataGridViewComboBoxColumn
            {
                Name = "Categoria",
                HeaderText = "Categoría",
                FillWeight = 80,
                FlatStyle = FlatStyle.Flat
            };
            colCategoria.Items.Add("Servicio");
            colCategoria.Items.Add("Producto");
            dgvItems.Columns.Add(colCategoria);

            // Descripción
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Descripcion",
                HeaderText = "Descripción",
                FillWeight = 200
            });

            // Proveedor
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Proveedor",
                HeaderText = "Proveedor",
                FillWeight = 120
            });

            // Familia
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Familia",
                HeaderText = "Familia de Item",
                FillWeight = 120
            });

            // Precio
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Precio",
                HeaderText = "Precio",
                FillWeight = 80,
                DefaultCellStyle = { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Stock
            dgvItems.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Stock",
                HeaderText = "Stock",
                FillWeight = 60,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            });

            // Botón +
            dgvItems.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Mas",
                HeaderText = "+",
                Text = "+",
                UseColumnTextForButtonValue = true,
                Width = 30,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
            });

            // Botón -
            dgvItems.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Menos",
                HeaderText = "-",
                Text = "-",
                UseColumnTextForButtonValue = true,
                Width = 30,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
            });

            // Botón eliminar
            dgvItems.Columns.Add(new DataGridViewButtonColumn
            {
                Name = "Eliminar",
                HeaderText = "X",
                Text = "X",
                UseColumnTextForButtonValue = true,
                Width = 30,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells
            });
        }

        // ================== CARGA DESDE BD ==================
        private void CargarItemsDeBase()
        {
            dgvItems.Rows.Clear();

            using (var cn = DatabaseHelper.GetConnection())
            {
                cn.Open();

                string sql = @"
            SELECT id, codigo, categoria, descripcion,
                   proveedor, familia, precio, stock
            FROM items
            WHERE activo = 1
            ORDER BY descripcion";

                using (var cmd = new MySqlCommand(sql, cn))
                using (var rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        dgvItems.Rows.Add(
                            rd.GetInt32("id"),
                            rd.GetString("codigo"),
                            rd.GetString("categoria"),
                            rd.GetString("descripcion"),
                            rd["proveedor"] == DBNull.Value ? "" : rd["proveedor"].ToString(),
                            rd["familia"] == DBNull.Value ? "" : rd["familia"].ToString(),
                            rd.GetDecimal("precio"),
                            rd.GetInt32("stock"));
                    }
                }
            }
        }


        // ================== NUEVO ITEM ==================
        private void BtnNuevoItem_Click(object sender, EventArgs e)
        {
            int rowIndex = dgvItems.Rows.Add(
                0,              
                "",             
                "Servicio",     
                "",
                "",
                "",
                0.00m,
                0);

            dgvItems.ClearSelection();
            dgvItems.CurrentCell = dgvItems.Rows[rowIndex].Cells["Codigo"];
            dgvItems.BeginEdit(true);
        }

        // ================== GUARDAR (INSERT/UPDATE) ==================
        private void DgvItems_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            GuardarFila(dgvItems.Rows[e.RowIndex]);
        }

        private void GuardarFila(DataGridViewRow row)
        {
            int id = ObtenerEntero(row.Cells["Id"].Value);
            string codigo = row.Cells["Codigo"].Value?.ToString() ?? "";
            string categoria = row.Cells["Categoria"].Value?.ToString() ?? "";
            string descripcion = row.Cells["Descripcion"].Value?.ToString() ?? "";
            string proveedor = row.Cells["Proveedor"].Value?.ToString() ?? "";
            string familia = row.Cells["Familia"].Value?.ToString() ?? "";
            decimal precio = ObtenerDecimal(row.Cells["Precio"].Value);
            int stock = ObtenerEntero(row.Cells["Stock"].Value);

            using (var cn = DatabaseHelper.GetConnection())
            {
                cn.Open();

                if (id == 0)
                {
                    // INSERT
                    string sql = @"
                INSERT INTO items (codigo, categoria, descripcion,
                                   proveedor, familia, precio, stock, activo)
                VALUES (@codigo, @categoria, @descripcion,
                        @proveedor, @familia, @precio, @stock, 1);
                SELECT LAST_INSERT_ID();";

                    using (var cmd = new MySqlCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@codigo", codigo);
                        cmd.Parameters.AddWithValue("@categoria", categoria);
                        cmd.Parameters.AddWithValue("@descripcion", descripcion);
                        cmd.Parameters.AddWithValue("@proveedor", proveedor);
                        cmd.Parameters.AddWithValue("@familia", familia);
                        cmd.Parameters.AddWithValue("@precio", precio);
                        cmd.Parameters.AddWithValue("@stock", stock);

                        object result = cmd.ExecuteScalar();
                        int newId;
                        if (result != null && int.TryParse(result.ToString(), out newId))
                        {
                            row.Cells["Id"].Value = newId;
                        }
                    }
                }
                else
                {
                    // UPDATE
                    string sql = @"
                UPDATE items SET
                    codigo      = @codigo,
                    categoria   = @categoria,
                    descripcion = @descripcion,
                    proveedor   = @proveedor,
                    familia     = @familia,
                    precio      = @precio,
                    stock       = @stock
                WHERE id = @id";

                    using (var cmd = new MySqlCommand(sql, cn))
                    {
                        cmd.Parameters.AddWithValue("@codigo", codigo);
                        cmd.Parameters.AddWithValue("@categoria", categoria);
                        cmd.Parameters.AddWithValue("@descripcion", descripcion);
                        cmd.Parameters.AddWithValue("@proveedor", proveedor);
                        cmd.Parameters.AddWithValue("@familia", familia);
                        cmd.Parameters.AddWithValue("@precio", precio);
                        cmd.Parameters.AddWithValue("@stock", stock);
                        cmd.Parameters.AddWithValue("@id", id);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }


        // ================== BOTONES + / - / X ==================
        private void DgvItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = dgvItems.Rows[e.RowIndex];
            var colName = dgvItems.Columns[e.ColumnIndex].Name;

            if (colName == "Mas")
            {
                int stock = ObtenerEntero(row.Cells["Stock"].Value);
                stock++;
                row.Cells["Stock"].Value = stock;
                GuardarFila(row); 
            }
            else if (colName == "Menos")
            {
                int stock = ObtenerEntero(row.Cells["Stock"].Value);
                if (stock > 0) stock--;
                row.Cells["Stock"].Value = stock;
                GuardarFila(row); 
            }
            else if (colName == "Eliminar")
            {
                int id = ObtenerEntero(row.Cells["Id"].Value);

                if (MessageBox.Show("¿Eliminar este item?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (id != 0)
                    {
                        using (var cn = DatabaseHelper.GetConnection())
                        {
                            cn.Open();

                            using (var cmd = new MySqlCommand(
                                "UPDATE items SET activo = 0 WHERE id = @id", cn))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    dgvItems.Rows.RemoveAt(e.RowIndex);
                }
            }

        }

        // ================== HELPERS ==================
        private int ObtenerEntero(object valor)
        {
            if (valor == null || valor == DBNull.Value) return 0;
            int n;
            return int.TryParse(valor.ToString(), out n) ? n : 0;
        }

        private decimal ObtenerDecimal(object valor)
        {
            if (valor == null || valor == DBNull.Value) return 0m;
            decimal d;
            return decimal.TryParse(valor.ToString(), out d) ? d : 0m;
        }
    }
}
