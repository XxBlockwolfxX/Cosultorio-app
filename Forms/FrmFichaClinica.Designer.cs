using System.Windows.Forms;

namespace ConsultorioDentalApp.Forms
{
    partial class FrmFichaClinica
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        // NOTA IMPORTANTÍSIMA:
        // Este formulario se construye COMPLETAMENTE desde BuildUI()
        // Por lo tanto, el designer DEBE permanecer vacío.
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 700);
            this.Text = "Ficha Clínica";
        }

        #endregion
    }
}
