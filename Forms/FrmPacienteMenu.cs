using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ConsultorioDentalApp.Data;
using MySql.Data.MySqlClient;
using System.IO;
using System.Diagnostics;

namespace ConsultorioDentalApp.Forms
{
    public class FrmPacienteMenu : Form
    {
        private readonly int _pacienteId;

        private Panel pnlHeader;
        private Label lblNombre;
        private Label lblHistoria;
        private Label lblEdad;
        private Label lblRegistro;

        private FlowLayoutPanel flpAcciones;

        private string _nombrePaciente;
        private int? _edadPaciente;

        private Panel pnlContenido;
        private FlowLayoutPanel flpArchivos;

        public FrmPacienteMenu(int pacienteId)
        {
            _pacienteId = pacienteId;
            InitializeComponent();
            CargarDatosPaciente();
        }

        private void InitializeComponent()
        {
            Text = "Menú del paciente";
            BackColor = Color.FromArgb(20, 20, 24);
            StartPosition = FormStartPosition.CenterScreen;

            // permitir redimensionar y maximizar
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;

            // arrancar maximizado
            WindowState = FormWindowState.Maximized;

            Font = new Font("Segoe UI", 10f);


            // ======= PANEL ENCABEZADO =======
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110
            };
            pnlHeader.Paint += PnlHeader_Paint;
            Controls.Add(pnlHeader);

            lblNombre = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 214, 0),
                Left = 20,
                Top = 10,
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblNombre);

            lblHistoria = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Left = 22,
                Top = 50,
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblHistoria);

            lblEdad = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.White,
                Left = 22,
                Top = 70,
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblEdad);

            lblRegistro = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.Gainsboro,
                Left = 22,
                Top = 90,
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblRegistro);

            // ======= FILA DE ACCIONES =======
            flpAcciones = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 90,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(20, 10, 0, 10),
                BackColor = Color.Transparent,
                AutoScroll = true
            };
            Controls.Add(flpAcciones);

            // ====== PANEL CENTRAL PARA ARCHIVOS ======
            pnlContenido = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 30),
                Padding = new Padding(5)
            };
            Controls.Add(pnlContenido);
            pnlContenido.BringToFront();

            flpArchivos = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 24),
                AutoScroll = true
            };
            pnlContenido.Controls.Add(flpArchivos);


            // Botones de acción (puedes ajustar los textos que quieras)
            flpAcciones.Controls.Add(CrearAccionPaciente("Registro",
                Properties.Resources.informacion, (s, e) => MostrarEnConstruccion("Registro")));

            flpAcciones.Controls.Add(CrearAccionPaciente("Consultas",
    Properties.Resources.consultoria, BtnConsultas_Click));


            flpAcciones.Controls.Add(CrearAccionPaciente("Imágenes",
    Properties.Resources.agenda, BtnImagenes_Click));

            flpAcciones.Controls.Add(CrearAccionPaciente("Documentos",
                Properties.Resources.plantilla, BtnDocumentos_Click));


            flpAcciones.Controls.Add(CrearAccionPaciente("Proformas",
                Properties.Resources.facturas, (s, e) => MostrarEnConstruccion("Proformas")));

            flpAcciones.Controls.Add(CrearAccionPaciente("Odonto",
                Properties.Resources.Fichas, (s, e) => AbrirOdontograma()));

            flpAcciones.Controls.Add(CrearAccionPaciente("Eliminar",
                Properties.Resources.borrar, (s, e) => EliminarPaciente()));

            flpAcciones.Controls.Add(CrearAccionPaciente("Atrás",
                Properties.Resources.Apps, (s, e) => Close()));

            
        }

        // Encabezado con degradado oscuro
        private void PnlHeader_Paint(object sender, PaintEventArgs e)
        {
            var rect = pnlHeader.ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (var brush = new LinearGradientBrush(
                       rect,
                       Color.FromArgb(40, 40, 44),
                       Color.FromArgb(15, 15, 18),
                       LinearGradientMode.Horizontal))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        // Crea un "botón" redondo con icono + texto debajo
        private Panel CrearAccionPaciente(string texto, Image icono, EventHandler onClick)
        {
            var cont = new Panel
            {
                Width = 80,
                Height = 70,
                Margin = new Padding(8, 0, 8, 0),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };

            var circulo = new Panel
            {
                Width = 46,
                Height = 46,
                Left = (cont.Width - 46) / 2,
                Top = 0,
                BackColor = Color.Transparent,
                Tag = false
            };

            circulo.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                bool hovered = (bool)(circulo.Tag ?? false);
                Color baseColor = Color.FromArgb(0, 172, 237);   // celeste
                Color hoverColor = Color.FromArgb(0, 195, 255);
                var color = hovered ? hoverColor : baseColor;

                var rect = new Rectangle(0, 0, circulo.Width - 1, circulo.Height - 1);
                using (var b = new SolidBrush(color))
                using (var pen = new Pen(Color.White, 2f))
                {
                    g.FillEllipse(b, rect);
                    g.DrawEllipse(pen, rect);
                }
            };

            var pic = new PictureBox
            {
                Image = icono,
                Width = 24,
                Height = 24,
                SizeMode = PictureBoxSizeMode.Zoom,
                Left = (circulo.Width - 24) / 2,
                Top = (circulo.Height - 24) / 2,
                BackColor = Color.Transparent
            };
            circulo.Controls.Add(pic);

            var lbl = new Label
            {
                Text = texto,
                AutoSize = false,
                Width = cont.Width,
                Height = 20,
                Top = 48,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8f),
                BackColor = Color.Transparent
            };

            cont.Controls.Add(circulo);
            cont.Controls.Add(lbl);

            // Click en todo el conjunto
            void ClickHandler(object s, EventArgs e) => onClick(s, e);
            cont.Click += ClickHandler;
            circulo.Click += ClickHandler;
            pic.Click += ClickHandler;
            lbl.Click += ClickHandler;

            // Hover
            void HoverOn(object s, EventArgs e)
            {
                circulo.Tag = true;
                circulo.Invalidate();
            }

            void HoverOff(object s, EventArgs e)
            {
                circulo.Tag = false;
                circulo.Invalidate();
            }

            cont.MouseEnter += HoverOn;
            cont.MouseLeave += HoverOff;
            circulo.MouseEnter += HoverOn;
            circulo.MouseLeave += HoverOff;
            pic.MouseEnter += HoverOn;
            pic.MouseLeave += HoverOff;
            lbl.MouseEnter += HoverOn;
            lbl.MouseLeave += HoverOff;

            return cont;
        }

        // ========== LÓGICA ==========

        private void CargarDatosPaciente()
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();

                    string sql = @"
                        SELECT Nombre, Edad, FechaNacimiento
                        FROM Paciente
                        WHERE Id = @Id;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", _pacienteId);

                        using (var rd = cmd.ExecuteReader())
                        {
                            if (rd.Read())
                            {
                                _nombrePaciente = rd["Nombre"]?.ToString();
                                int edadBd;
                                _edadPaciente = int.TryParse(rd["Edad"]?.ToString(), out edadBd)
                                    ? edadBd
                                    : (int?)null;

                                DateTime? fechaNac = null;
                                if (rd["FechaNacimiento"] != DBNull.Value)
                                    fechaNac = Convert.ToDateTime(rd["FechaNacimiento"]);

                                // ===== Título principal =====
                                lblNombre.Text = _nombrePaciente ?? "Paciente sin nombre";

                                // Historia clínica: usar Id con ceros a la izquierda
                                string historia = _pacienteId.ToString("00000");
                                lblHistoria.Text = $"Historia Clínica: {historia}";

                                // Edad
                                if (_edadPaciente.HasValue)
                                {
                                    lblEdad.Text = $"Edad: {_edadPaciente.Value} años";
                                }
                                else if (fechaNac.HasValue)
                                {
                                    int edadCalc = CalcularEdad(fechaNac.Value);
                                    lblEdad.Text = $"Edad: {edadCalc} años";
                                }
                                else
                                {
                                    lblEdad.Text = "Edad: N/D";
                                }

                                // Registro: fecha actual (si no tienes columna de registro en BD)
                                lblRegistro.Text = $"Registro: {DateTime.Now:dd/MM/yyyy - HH:mm:ss}";
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
            CargarArchivosPaciente();

        }

        private int CalcularEdad(DateTime fechaNac)
        {
            var hoy = DateTime.Today;
            int edad = hoy.Year - fechaNac.Year;
            if (fechaNac.Date > hoy.AddYears(-edad)) edad--;
            return edad;
        }

        private void AbrirOdontograma()
        {
            using (var frm = new FrmFichaClinica(_pacienteId))
            {
                frm.StartPosition = FormStartPosition.CenterScreen;
                frm.ShowDialog(this);
            }
        }

        private void EliminarPaciente()
        {
            var r = MessageBox.Show(
                $"¿Seguro que deseas eliminar al paciente:\n{_nombrePaciente}?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (r != DialogResult.Yes) return;

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = "DELETE FROM Paciente WHERE Id = @Id;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", _pacienteId);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Paciente eliminado correctamente.",
                    "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar paciente:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MostrarEnConstruccion(string modulo)
        {
            MessageBox.Show($"{modulo} - módulo en construcción.",
                "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnImagenes_Click(object sender, EventArgs e)
        {
            AgregarArchivoPaciente(soloImagenes: true);
        }

        private void BtnDocumentos_Click(object sender, EventArgs e)
        {
            AgregarArchivoPaciente(soloImagenes: false);
        }

        private void AgregarArchivoPaciente(bool soloImagenes)
        {
            using (var ofd = new OpenFileDialog())
            {
                if (soloImagenes)
                {
                    ofd.Title = "Seleccionar imagen del paciente";
                    ofd.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                }
                else
                {
                    ofd.Title = "Seleccionar documento (PDF o imagen)";
                    ofd.Filter = "PDF e Imágenes|*.pdf;*.jpg;*.jpeg;*.png;*.bmp;*.gif|Todos|*.*";
                }

                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;

                string origen = ofd.FileName;
                string nombre = Path.GetFileName(origen);

                // Carpeta de archivos del paciente dentro de la app
                string carpetaBase = Path.Combine(Application.StartupPath, "ArchivosPacientes");
                string carpetaPaciente = Path.Combine(carpetaBase, _pacienteId.ToString());
                Directory.CreateDirectory(carpetaPaciente);

                // Evitar sobrescribir: si ya existe, agrega sufijo
                string destino = Path.Combine(carpetaPaciente, nombre);
                int contador = 1;
                while (File.Exists(destino))
                {
                    string nombreSinExt = Path.GetFileNameWithoutExtension(nombre);
                    string ext = Path.GetExtension(nombre);
                    destino = Path.Combine(carpetaPaciente, $"{nombreSinExt}_{contador}{ext}");
                    contador++;
                }

                File.Copy(origen, destino);

                // Guardar en BD
                GuardarArchivoEnBD(nombre, destino);

                // Refrescar lista
                CargarArchivosPaciente();
            }
        }

        private void GuardarArchivoEnBD(string nombreArchivo, string rutaArchivo)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                INSERT INTO PacienteArchivo
                    (PacienteId, NombreArchivo, RutaArchivo, TipoMime)
                VALUES
                    (@PacienteId, @NombreArchivo, @RutaArchivo, @TipoMime);";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@PacienteId", _pacienteId);
                        cmd.Parameters.AddWithValue("@NombreArchivo", nombreArchivo);
                        cmd.Parameters.AddWithValue("@RutaArchivo", rutaArchivo);
                        cmd.Parameters.AddWithValue("@TipoMime", ObtenerMimeDesdeExtension(rutaArchivo));
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar archivo:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ObtenerMimeDesdeExtension(string path)
        {
            string ext = Path.GetExtension(path);
            if (string.IsNullOrEmpty(ext))
                return "application/octet-stream";

            ext = ext.ToLowerInvariant();

            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";

                case ".png":
                    return "image/png";

                case ".bmp":
                    return "image/bmp";

                case ".gif":
                    return "image/gif";

                case ".pdf":
                    return "application/pdf";

                default:
                    return "application/octet-stream";
            }
        }


        private void CargarArchivosPaciente()
        {
            flpArchivos.Controls.Clear();

            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    string sql = @"
                SELECT Id, NombreArchivo, RutaArchivo, TipoMime, FechaRegistro
                FROM PacienteArchivo
                WHERE PacienteId = @PacienteId
                ORDER BY FechaRegistro DESC;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@PacienteId", _pacienteId);

                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                int id = rd.GetInt32("Id");
                                string nombre = rd.GetString("NombreArchivo");
                                string ruta = rd.GetString("RutaArchivo");
                                string mime = rd["TipoMime"]?.ToString();

                                flpArchivos.Controls.Add(
                                    CrearTarjetaArchivo(id, nombre, ruta, mime));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar archivos:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Control CrearTarjetaArchivo(int id, string nombre, string ruta, string mime)
        {
            bool esImagen = mime != null && mime.StartsWith("image");

            var cont = new Panel
            {
                Width = 220,
                Height = 150,
                Margin = new Padding(8),
                BackColor = Color.FromArgb(35, 35, 40),
                BorderStyle = BorderStyle.FixedSingle,
                Tag = ruta
            };

            Control preview;

            if (esImagen && File.Exists(ruta))
            {
                preview = new PictureBox
                {
                    ImageLocation = ruta,
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Dock = DockStyle.Top,
                    Height = 110,
                    BackColor = Color.Black
                };
            }
            else
            {
                // Ícono simple para PDF u otros (puedes usar un resource específico)
                preview = new Label
                {
                    Text = Path.GetExtension(ruta)?.ToUpperInvariant(),
                    Dock = DockStyle.Top,
                    Height = 110,
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                    BackColor = Color.Black
                };
            }

            var lblNombre = new Label
            {
                Text = nombre,
                Dock = DockStyle.Fill,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoEllipsis = true,
                Padding = new Padding(4, 2, 4, 2)
            };

            cont.Controls.Add(lblNombre);
            cont.Controls.Add(preview);

            // Click: abrir archivo con la app por defecto
            void Abrir(object s, EventArgs e)
            {
                try
                {
                    if (File.Exists(ruta))
                        Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
                    else
                        MessageBox.Show("El archivo no se encuentra:\n" + ruta,
                            "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo abrir el archivo:\n" + ex.Message,
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            cont.Click += Abrir;
            preview.Click += Abrir;
            lblNombre.Click += Abrir;

            return cont;
        }
        private void BtnConsultas_Click(object sender, EventArgs e)
        {
            using (var frm = new FrmConsultasPaciente(_pacienteId, _nombrePaciente))
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog(this);
            }
        }




    }
}
