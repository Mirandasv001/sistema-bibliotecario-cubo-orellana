using System.ComponentModel;

namespace BibliotecaApp
{
    partial class UcGuiaUso
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        private void InitializeComponent()
        {
            this.rtbManual = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // rtbManual
            // 
            this.rtbManual.BackColor = System.Drawing.Color.White;
            this.rtbManual.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbManual.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbManual.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rtbManual.Location = new System.Drawing.Point(0, 0);
            this.rtbManual.Name = "rtbManual";
            this.rtbManual.ReadOnly = true;
            this.rtbManual.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbManual.Size = new System.Drawing.Size(1040, 702);
            this.rtbManual.TabIndex = 0;
            this.rtbManual.Text = "";
            // 
            // UcGuiaUso
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.AliceBlue;
            this.Controls.Add(this.rtbManual);
            this.Name = "UcGuiaUso";
            this.Size = new System.Drawing.Size(1040, 702);
            this.Load += new System.EventHandler(this.UcGuiaUso_Load);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbManual;
    }
}