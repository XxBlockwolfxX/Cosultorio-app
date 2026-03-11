using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;  // requiere Microsoft.Office.Interop.Word

namespace ConsultorioDentalApp.Forms
{
    public partial class FrmPlantillas : Form
    {
        private Panel pnlLeft;
        private Panel pnlTop;
        private Panel pnlCentro;

        private ListBox lstPlantillas;
        private Button btnCargar;
        private Button btnEliminar;
        private Button btnImprimir;
        private Button btnGuardarCambios;
        private Button btnVerEditar;
        private Button btnAbrirWord;

        // Barra de herramientas tipo Word
        private ToolStrip tsEditor;
        private ToolStripComboBox cboFuente;
        private ToolStripComboBox cboTamano;
        private ToolStripButton btnNegrita;
        private ToolStripButton btnCursiva;
        private ToolStripButton btnSubrayado;
        private ToolStripButton btnColor;
        private ToolStripButton btnViñetas;
        private ToolStripButton btnAlineaIzq;
        private ToolStripButton btnAlineaCentro;
        private ToolStripButton btnAlineaDer;

        private RichTextBox editor;

        private readonly string carpetaPlantillas = Path.Combine(Application.StartupPath, "Plantillas");
        private string archivoActual;          // archivo original (pdf, docx, rtf, etc.)
        private string archivoEditableActual;  // normalmente un .rtf o .txt que se muestra en el editor

        public FrmPlantillas()
        {
            InitializeComponent();
            BuildUI();
        }

        // ===================== UI =====================
        private void BuildUI()
        {
            BackColor = Color.FromArgb(25, 25, 30);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9f);

            // ===== Panel izquierdo (lista) =====
            pnlLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = Color.FromArgb(35, 35, 42)
            };
            Controls.Add(pnlLeft);

            lstPlantillas = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            lstPlantillas.SelectedIndexChanged += LstPlantillas_SelectedIndexChanged;
            pnlLeft.Controls.Add(lstPlantillas);

            // ===== Panel superior (botones) =====
            pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.FromArgb(30, 30, 36)
            };
            Controls.Add(pnlTop);

            // ===== Panel central (editor + barra tipo Word) =====
            pnlCentro = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(280, 50, 5, 5) // margen interno (izq, arriba, der, abajo)
            };
            Controls.Add(pnlCentro);

            // --- Barra de herramientas ---
            tsEditor = new ToolStrip
            {
                Dock = DockStyle.Top,
                GripStyle = ToolStripGripStyle.Hidden,
                RenderMode = ToolStripRenderMode.Professional,
                BackColor = Color.FromArgb(45, 45, 55),   // fondo oscuro
                ForeColor = Color.White                   // texto claro
            };

            cboFuente = new ToolStripComboBox { Width = 180 };
            // Cargar fuentes instaladas (solo las comunes para que no sea eterno)
            foreach (var fam in FontFamily.Families
                         .Where(f => !f.Name.StartsWith("@"))
                         .OrderBy(f => f.Name))
            {
                cboFuente.Items.Add(fam.Name);
            }

            cboTamano = new ToolStripComboBox { Width = 60 };
            int[] tamanos = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 28, 32 };
            foreach (int t in tamanos)
                cboTamano.Items.Add(t.ToString());

            btnNegrita = new ToolStripButton("N") { CheckOnClick = true, Font = new Font(Font, FontStyle.Bold) };
            btnCursiva = new ToolStripButton("K") { CheckOnClick = true, Font = new Font(Font, FontStyle.Italic) };
            btnSubrayado = new ToolStripButton("S") { CheckOnClick = true, Font = new Font(Font, FontStyle.Underline) };

            btnColor = new ToolStripButton("A");
            btnViñetas = new ToolStripButton("•");

            btnAlineaIzq = new ToolStripButton("Izq");
            btnAlineaCentro = new ToolStripButton("Centro");
            btnAlineaDer = new ToolStripButton("Der");

            tsEditor.Items.Add(new ToolStripLabel("Fuente:"));
            tsEditor.Items.Add(cboFuente);
            tsEditor.Items.Add(new ToolStripLabel(" Tamaño:"));
            tsEditor.Items.Add(cboTamano);
            tsEditor.Items.Add(new ToolStripSeparator());
            tsEditor.Items.Add(btnNegrita);
            tsEditor.Items.Add(btnCursiva);
            tsEditor.Items.Add(btnSubrayado);
            tsEditor.Items.Add(new ToolStripSeparator());
            tsEditor.Items.Add(btnColor);
            tsEditor.Items.Add(btnViñetas);
            tsEditor.Items.Add(new ToolStripSeparator());
            tsEditor.Items.Add(btnAlineaIzq);
            tsEditor.Items.Add(btnAlineaCentro);
            tsEditor.Items.Add(btnAlineaDer);

            // Ajustar colores de combos
            cboFuente.ComboBox.BackColor = Color.FromArgb(35, 35, 45);
            cboFuente.ComboBox.ForeColor = Color.White;

            cboTamano.ComboBox.BackColor = Color.FromArgb(35, 35, 45);
            cboTamano.ComboBox.ForeColor = Color.White;

            // Botones con texto blanco
            btnNegrita.ForeColor = Color.White;
            btnCursiva.ForeColor = Color.White;
            btnSubrayado.ForeColor = Color.White;
            btnColor.ForeColor = Color.White;
            btnViñetas.ForeColor = Color.White;
            btnAlineaIzq.ForeColor = Color.White;
            btnAlineaCentro.ForeColor = Color.White;
            btnAlineaDer.ForeColor = Color.White;

            pnlCentro.Controls.Add(tsEditor);

            // --- Editor ---
            editor = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.None
            };
            pnlCentro.Controls.Add(editor);
            editor.BringToFront();

            // Valores iniciales de la barra
            cboFuente.Text = editor.Font.FontFamily.Name;
            cboTamano.Text = ((int)editor.Font.Size).ToString();

            // Eventos de la barra
            cboFuente.SelectedIndexChanged += CboFuente_SelectedIndexChanged;
            cboTamano.SelectedIndexChanged += CboTamano_SelectedIndexChanged;
            btnNegrita.Click += (s, e) => ToggleFontStyle(FontStyle.Bold);
            btnCursiva.Click += (s, e) => ToggleFontStyle(FontStyle.Italic);
            btnSubrayado.Click += (s, e) => ToggleFontStyle(FontStyle.Underline);
            btnColor.Click += BtnColor_Click;
            btnViñetas.Click += (s, e) => editor.SelectionBullet = !editor.SelectionBullet;
            btnAlineaIzq.Click += (s, e) => editor.SelectionAlignment = HorizontalAlignment.Left;
            btnAlineaCentro.Click += (s, e) => editor.SelectionAlignment = HorizontalAlignment.Center;
            btnAlineaDer.Click += (s, e) => editor.SelectionAlignment = HorizontalAlignment.Right;

            // ===== Botones superiores =====
            btnCargar = new Button { Text = "Cargar", Left = 10, Top = 10, Width = 80 };
            btnCargar.Click += BtnCargar_Click;

            btnEliminar = new Button { Text = "Eliminar", Left = 100, Top = 10, Width = 80 };
            btnEliminar.Click += BtnEliminar_Click;

            btnImprimir = new Button { Text = "Imprimir", Left = 190, Top = 10, Width = 80 };
            btnImprimir.Click += BtnImprimir_Click;

            btnGuardarCambios = new Button { Text = "Guardar", Left = 280, Top = 10, Width = 80 };
            btnGuardarCambios.Click += BtnGuardarCambios_Click;

            btnVerEditar = new Button { Text = "Ver / Editar", Left = 370, Top = 10, Width = 90 };
            btnVerEditar.Click += BtnVerEditar_Click;

            btnAbrirWord = new Button { Text = "Abrir Word", Left = 470, Top = 10, Width = 90 };
            btnAbrirWord.Click += BtnAbrirWord_Click;

            pnlTop.Controls.AddRange(new Control[]
            {
                btnCargar, btnEliminar, btnImprimir, btnGuardarCambios, btnVerEditar, btnAbrirWord
            });

            // ===== Cargar archivos existentes =====
            Directory.CreateDirectory(carpetaPlantillas);
            CargarLista();
        }

        // ===================== Barra de herramientas =====================
        private void CboFuente_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboFuente.Text)) return;
            ApplyFontChange(fontName: cboFuente.Text, size: null);
        }

        private void CboTamano_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!float.TryParse(cboTamano.Text, out float size)) return;
            ApplyFontChange(fontName: null, size: size);
        }

        private void ApplyFontChange(string fontName, float? size)
        {
            Font current = editor.SelectionFont ?? editor.Font;

            string name = fontName ?? current.FontFamily.Name;
            float newSize = size ?? current.Size;

            editor.SelectionFont = new Font(name, newSize, current.Style);
            editor.Focus();
        }

        private void ToggleFontStyle(FontStyle style)
        {
            Font current = editor.SelectionFont ?? editor.Font;
            FontStyle newStyle = current.Style;

            if (current.Style.HasFlag(style))
                newStyle &= ~style;  // quitar estilo
            else
                newStyle |= style;   // agregar estilo

            editor.SelectionFont = new Font(current.FontFamily, current.Size, newStyle);
            editor.Focus();
        }

        private void BtnColor_Click(object sender, EventArgs e)
        {
            using (var cd = new ColorDialog())
            {
                cd.Color = editor.SelectionColor;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    editor.SelectionColor = cd.Color;
                    editor.Focus();
                }
            }
        }

        // ===================== LÓGICA PLANTILLAS =====================
        private void CargarLista()
        {
            lstPlantillas.Items.Clear();

            if (Directory.Exists(carpetaPlantillas))
            {
                foreach (var file in Directory.GetFiles(carpetaPlantillas))
                {
                    lstPlantillas.Items.Add(Path.GetFileName(file));
                }
            }

            archivoActual = null;
            archivoEditableActual = null;
            editor.Clear();
        }

        private void LstPlantillas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstPlantillas.SelectedItem == null)
                return;

            archivoActual = Path.Combine(carpetaPlantillas, lstPlantillas.SelectedItem.ToString());
            archivoEditableActual = null;
            editor.Clear();
        }

        private void BtnCargar_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Documentos|*.pdf;*.doc;*.docx;*.rtf;*.txt";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string destino = Path.Combine(carpetaPlantillas,
                                                  Path.GetFileName(ofd.FileName));
                    File.Copy(ofd.FileName, destino, true);
                    CargarLista();
                }
            }
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (lstPlantillas.SelectedItem == null) return;

            string nombre = lstPlantillas.SelectedItem.ToString();
            string ruta = Path.Combine(carpetaPlantillas, nombre);

            if (File.Exists(ruta))
            {
                if (MessageBox.Show("¿Eliminar la plantilla seleccionada?",
                    "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    File.Delete(ruta);
                    CargarLista();
                }
            }
        }

        /// <summary>
        /// Convierte un DOC/DOCX a RTF en la carpeta de plantillas y devuelve la ruta del RTF.
        /// </summary>
        private string ConvertirWordARtf(string rutaDocx)
        {
            string nombre = Path.GetFileNameWithoutExtension(rutaDocx);
            string rtfDestino = Path.Combine(carpetaPlantillas, nombre + ".rtf");

            var wordApp = new Word.Application();
            Word.Document doc = null;

            try
            {
                wordApp.Visible = false;
                doc = wordApp.Documents.Open(rutaDocx, ReadOnly: true, Visible: false);
                doc.SaveAs2(rtfDestino, Word.WdSaveFormat.wdFormatRTF);
            }
            finally
            {
                if (doc != null)
                {
                    doc.Close(false);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
                }

                wordApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
            }

            return rtfDestino;
        }

        /// <summary>
        /// Convierte un PDF a RTF usando Word (si la versión de Word soporta PDF).
        /// </summary>
        private string ConvertirPdfARtf(string rutaPdf)
        {
            string nombre = Path.GetFileNameWithoutExtension(rutaPdf);
            // para no pisar otros rtf, le añadimos sufijo _pdf
            string rtfDestino = Path.Combine(carpetaPlantillas, nombre + "_pdf.rtf");

            var wordApp = new Word.Application();
            Word.Document doc = null;

            try
            {
                wordApp.Visible = false;
                // Word abre el PDF y lo convierte internamente a documento editable
                doc = wordApp.Documents.Open(rutaPdf, ReadOnly: true, Visible: false);
                doc.SaveAs2(rtfDestino, Word.WdSaveFormat.wdFormatRTF);
            }
            finally
            {
                if (doc != null)
                {
                    doc.Close(false);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(doc);
                }

                wordApp.Quit();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
            }

            return rtfDestino;
        }

        // ===== Botón VER / EDITAR =====
        private void BtnVerEditar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(archivoActual))
            {
                MessageBox.Show("Seleccione una plantilla en la lista primero.");
                return;
            }

            string ext = Path.GetExtension(archivoActual).ToLower();

            try
            {
                if (ext == ".rtf")
                {
                    archivoEditableActual = archivoActual;
                    editor.LoadFile(archivoEditableActual, RichTextBoxStreamType.RichText);
                }
                else if (ext == ".txt")
                {
                    archivoEditableActual = archivoActual;
                    editor.Text = File.ReadAllText(archivoEditableActual);
                }
                else if (ext == ".doc" || ext == ".docx")
                {
                    try
                    {
                        archivoEditableActual = ConvertirWordARtf(archivoActual);
                        editor.LoadFile(archivoEditableActual, RichTextBoxStreamType.RichText);
                        MessageBox.Show("Se creó una versión RTF editable de la plantilla.\n" +
                                        "Archivo: " + Path.GetFileName(archivoEditableActual),
                                        "Plantillas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception exConv)
                    {
                        archivoEditableActual = null;
                        MessageBox.Show("No se pudo convertir el archivo de Word a RTF.\n" +
                                        "Ábrelo directamente con el visor externo.\n\n" +
                                        exConv.Message,
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (ext == ".pdf")
                {
                    // NUEVO: PDF → RTF → editor
                    try
                    {
                        archivoEditableActual = ConvertirPdfARtf(archivoActual);
                        editor.LoadFile(archivoEditableActual, RichTextBoxStreamType.RichText);
                        MessageBox.Show("Se creó una versión RTF editable del PDF.\n" +
                                        "Archivo: " + Path.GetFileName(archivoEditableActual),
                                        "Plantillas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception exConv)
                    {
                        archivoEditableActual = null;
                        MessageBox.Show("No se pudo convertir el PDF a RTF.\n" +
                                        "Ábrelo con el visor de PDF externo.\n\n" +
                                        exConv.Message,
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    archivoEditableActual = null;
                    editor.Clear();
                    MessageBox.Show("Formato no compatible para edición interna.\n" +
                                    "Use el visor externo para verlo.",
                                    "Plantillas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al abrir la plantilla:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGuardarCambios_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(archivoEditableActual))
            {
                MessageBox.Show("No hay un documento editable cargado.\n" +
                                "Use primero el botón 'Ver / Editar'.");
                return;
            }

            string ext = Path.GetExtension(archivoEditableActual).ToLower();

            try
            {
                if (ext == ".rtf")
                {
                    editor.SaveFile(archivoEditableActual, RichTextBoxStreamType.RichText);
                }
                else if (ext == ".txt")
                {
                    File.WriteAllText(archivoEditableActual, editor.Text);
                }
                else
                {
                    MessageBox.Show("Solo se pueden guardar cambios de RTF o TXT.",
                                    "Plantillas", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                MessageBox.Show("Cambios guardados correctamente.",
                                "Plantillas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la plantilla:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnImprimir_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(archivoActual))
            {
                MessageBox.Show("Seleccione una plantilla primero.");
                return;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = archivoActual,
                    Verb = "Print",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo enviar a impresión:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAbrirWord_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(archivoActual))
            {
                MessageBox.Show("Seleccione una plantilla primero.");
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = archivoActual,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir el archivo con la aplicación predeterminada:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
