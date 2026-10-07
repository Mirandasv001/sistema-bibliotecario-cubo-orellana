using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BibliotecaApp
{
    partial class UcHistorialPrestamos
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            dgvHistorial = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvHistorial).BeginInit();
            SuspendLayout();

            // dgvHistorial
            dgvHistorial.AllowUserToAddRows = false;
            dgvHistorial.AllowUserToDeleteRows = false;
            dgvHistorial.AllowUserToResizeRows = false;
            dgvHistorial.BackgroundColor = System.Drawing.Color.White;
            dgvHistorial.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvHistorial.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvHistorial.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dgvHistorial.DefaultCellStyle = dataGridViewCellStyle2;
            dgvHistorial.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvHistorial.EnableHeadersVisualStyles = false;
            dgvHistorial.GridColor = System.Drawing.Color.FromArgb(200, 200, 200);
            dgvHistorial.Location = new System.Drawing.Point(0, 0);
            dgvHistorial.Margin = new System.Windows.Forms.Padding(0);
            dgvHistorial.MultiSelect = false;
            dgvHistorial.Name = "dgvHistorial";
            dgvHistorial.ReadOnly = true;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvHistorial.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            dgvHistorial.RowHeadersVisible = false;
            dgvHistorial.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvHistorial.Size = new System.Drawing.Size(1040, 657);
            dgvHistorial.TabIndex = 0;
            dgvHistorial.CellFormatting += dgvHistorial_CellFormatting;

            // pnlPaginacion
            pnlPaginacion = new System.Windows.Forms.Panel();
            btnAnterior = new System.Windows.Forms.Button();
            btnSiguiente = new System.Windows.Forms.Button();
            lblPagina = new System.Windows.Forms.Label();
            pnlPaginacion.SuspendLayout();
            SuspendLayout();
            // 
            // pnlPaginacion
            // 
            pnlPaginacion.BackColor = System.Drawing.Color.White;
            pnlPaginacion.Controls.Add(btnAnterior);
            pnlPaginacion.Controls.Add(btnSiguiente);
            pnlPaginacion.Controls.Add(lblPagina);
            pnlPaginacion.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlPaginacion.Height = 45;
            pnlPaginacion.Location = new System.Drawing.Point(0, 657);
            pnlPaginacion.Name = "pnlPaginacion";
            pnlPaginacion.Size = new System.Drawing.Size(1040, 45);
            pnlPaginacion.TabIndex = 1;
            // 
            // btnAnterior
            // 
            btnAnterior.Anchor = System.Windows.Forms.AnchorStyles.Left;
            btnAnterior.Location = new System.Drawing.Point(20, 10);
            btnAnterior.Name = "btnAnterior";
            btnAnterior.Size = new System.Drawing.Size(90, 25);
            btnAnterior.TabIndex = 0;
            btnAnterior.Text = "< Anterior";
            btnAnterior.UseVisualStyleBackColor = true;
            btnAnterior.Click += btnAnterior_Click;
            // 
            // btnSiguiente
            // 
            btnSiguiente.Anchor = System.Windows.Forms.AnchorStyles.Right;
            btnSiguiente.Location = new System.Drawing.Point(930, 10);
            btnSiguiente.Name = "btnSiguiente";
            btnSiguiente.Size = new System.Drawing.Size(90, 25);
            btnSiguiente.TabIndex = 1;
            btnSiguiente.Text = "Siguiente >";
            btnSiguiente.UseVisualStyleBackColor = true;
            btnSiguiente.Click += btnSiguiente_Click;
            // 
            // lblPagina
            // 
            lblPagina.Anchor = System.Windows.Forms.AnchorStyles.None;
            lblPagina.AutoSize = true;
            lblPagina.Location = new System.Drawing.Point(490, 14);
            lblPagina.Name = "lblPagina";
            lblPagina.Size = new System.Drawing.Size(60, 15);
            lblPagina.TabIndex = 2;
            lblPagina.Text = "Página 1 de 1";
            lblPagina.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UcHistorialPrestamos
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            Padding = new System.Windows.Forms.Padding(0);
            Margin = new System.Windows.Forms.Padding(0);
            Controls.Add(dgvHistorial);
            Controls.Add(pnlPaginacion);
            Name = "UcHistorialPrestamos";
            Size = new System.Drawing.Size(1040, 702);
            Load += UcHistorialPrestamos_Load;
            ((System.ComponentModel.ISupportInitialize)dgvHistorial).EndInit();
            pnlPaginacion.ResumeLayout(false);
            pnlPaginacion.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.DataGridView dgvHistorial;
        private System.Windows.Forms.Panel pnlPaginacion;
        private System.Windows.Forms.Button btnAnterior;
        private System.Windows.Forms.Button btnSiguiente;
        private System.Windows.Forms.Label lblPagina;
    }
}