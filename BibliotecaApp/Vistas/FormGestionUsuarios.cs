using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    /// <summary>
    /// Formulario para gestión de contraseñas de usuarios (Solo Administrador).
    /// Permite cambiar la contraseña de cualquier usuario del sistema.
    /// </summary>
    public class FormGestionUsuarios : Form
    {
        private ComboBox _cmbUsuarios;
        private TextBox _txtNuevaPassword;
        private TextBox _txtConfirmarPassword;
        private Button _btnGuardar;
        private Button _btnCancelar;

        public FormGestionUsuarios()
        {
            InitializeComponent();
            CargarUsuarios();
        }

        private void InitializeComponent()
        {
            this._cmbUsuarios = new ComboBox();
            this._txtNuevaPassword = new TextBox();
            this._txtConfirmarPassword = new TextBox();
            this._btnCancelar = new Button();
            this._btnGuardar = new Button();
            this.SuspendLayout();

            // 
            // _cmbUsuarios
            // 
            this._cmbUsuarios.DropDownHeight = 200;
            this._cmbUsuarios.DropDownStyle = ComboBoxStyle.DropDownList;
            this._cmbUsuarios.Font = new Font("Segoe UI", 10F);
            this._cmbUsuarios.Location = new Point(20, 95);
            this._cmbUsuarios.Name = "_cmbUsuarios";
            this._cmbUsuarios.Size = new Size(450, 28);
            this._cmbUsuarios.TabIndex = 0;
            // 
            // _txtNuevaPassword
            // 
            this._txtNuevaPassword.Font = new Font("Segoe UI", 10F);
            this._txtNuevaPassword.Location = new Point(20, 165);
            this._txtNuevaPassword.Name = "_txtNuevaPassword";
            this._txtNuevaPassword.PasswordChar = '•';
            this._txtNuevaPassword.Size = new Size(450, 27);
            this._txtNuevaPassword.TabIndex = 1;
            this._txtNuevaPassword.UseSystemPasswordChar = true;
            // 
            // _txtConfirmarPassword
            // 
            this._txtConfirmarPassword.Font = new Font("Segoe UI", 10F);
            this._txtConfirmarPassword.Location = new Point(20, 235);
            this._txtConfirmarPassword.Name = "_txtConfirmarPassword";
            this._txtConfirmarPassword.PasswordChar = '•';
            this._txtConfirmarPassword.Size = new Size(450, 27);
            this._txtConfirmarPassword.TabIndex = 2;
            this._txtConfirmarPassword.UseSystemPasswordChar = true;
            // 
            // _btnCancelar
            // 
            this._btnCancelar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this._btnCancelar.DialogResult = DialogResult.Cancel;
            this._btnCancelar.Location = new Point(250, 390);
            this._btnCancelar.Name = "_btnCancelar";
            this._btnCancelar.Size = new Size(110, 38);
            this._btnCancelar.TabIndex = 4;
            this._btnCancelar.Text = "Cancelar";
            this._btnCancelar.UseVisualStyleBackColor = true;
            // 
            // _btnGuardar
            // 
            this._btnGuardar.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this._btnGuardar.Location = new Point(370, 390);
            this._btnGuardar.Name = "_btnGuardar";
            this._btnGuardar.Size = new Size(110, 38);
            this._btnGuardar.TabIndex = 3;
            this._btnGuardar.Text = "Guardar";
            this._btnGuardar.UseVisualStyleBackColor = true;
            this._btnGuardar.Click += new EventHandler(this.BtnGuardar_Click);
            // 
            // FormGestionUsuarios
            // 
            this.AcceptButton = this._btnGuardar;
            this.BackColor = Color.FromArgb(244, 246, 249);
            this.CancelButton = this._btnCancelar;
            this.ClientSize = new Size(520, 460);
            this.Controls.Add(this._btnGuardar);
            this.Controls.Add(this._btnCancelar);
            this.Controls.Add(this._txtConfirmarPassword);
            this.Controls.Add(this._txtNuevaPassword);
            this.Controls.Add(this._cmbUsuarios);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new Size(520, 460);
            this.Name = "FormGestionUsuarios";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Gestión de Usuarios";
            this.KeyDown += new KeyEventHandler(this.FormGestionUsuarios_KeyDown);
            this.Load += new EventHandler(this.FormGestionUsuarios_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void FormGestionUsuarios_Load(object sender, EventArgs e)
        {
            CargarUsuarios();
            
            // Título
            var lblTitulo = EstiloUI.CrearEtiqueta("Cambiar Contraseña de Usuario");
            lblTitulo.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitulo.ForeColor = Color.FromArgb(43, 45, 66);
            lblTitulo.Location = new Point(20, 20);
            lblTitulo.AutoSize = true;
            this.Controls.Add(lblTitulo);

            // Usuario
            var lblUsuario = EstiloUI.CrearEtiqueta("Usuario:");
            lblUsuario.Location = new Point(20, 70);
            lblUsuario.AutoSize = true;
            this.Controls.Add(lblUsuario);

            // Nueva contraseña
            var lblNuevaPass = EstiloUI.CrearEtiqueta("Nueva Contraseña:");
            lblNuevaPass.Location = new Point(20, 140);
            lblNuevaPass.AutoSize = true;
            this.Controls.Add(lblNuevaPass);

            // Confirmar contraseña
            var lblConfirmarPass = EstiloUI.CrearEtiqueta("Confirmar Contraseña:");
            lblConfirmarPass.Location = new Point(20, 210);
            lblConfirmarPass.AutoSize = true;
            this.Controls.Add(lblConfirmarPass);

            // Estilizar controles
            EstiloUI.EstilizarEntrada(_cmbUsuarios);
            EstiloUI.EstilizarEntrada(_txtNuevaPassword);
            EstiloUI.EstilizarEntrada(_txtConfirmarPassword);
            EstiloUI.EstilizarBotonPrimario(_btnGuardar);
            EstiloUI.EstilizarBotonSecundario(_btnCancelar);
        }

        private void CargarUsuarios()
        {
            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var cmd = conexion.CreateCommand();
                cmd.CommandText = "SELECT Id, Nombre, Usuario, Rol FROM Usuarios ORDER BY Nombre;";

                var tabla = new DataTable();
                using (var lector = cmd.ExecuteReader())
                {
                    tabla.Load(lector);
                }

                // Crear columna de visualización combinada
                tabla.Columns.Add("Display", typeof(string));
                foreach (DataRow row in tabla.Rows)
                {
                    row["Display"] = $"{row["Nombre"]} ({row["Usuario"]}) - {row["Rol"]}";
                }

                _cmbUsuarios.DataSource = tabla;
                _cmbUsuarios.DisplayMember = "Display";
                _cmbUsuarios.ValueMember = "Id";

                if (_cmbUsuarios.Items.Count > 0)
                    _cmbUsuarios.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar usuarios: {ex.Message}",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGuardar_Click(object? sender, EventArgs e)
        {
            // Validar selección de usuario (manejo robusto de SelectedValue)
            object selectedValue = _cmbUsuarios.SelectedValue;
            if (selectedValue == null || selectedValue == DBNull.Value)
            {
                MessageBox.Show("Seleccione un usuario.", "Biblioteca CUBO",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Extracción robusta del ID (maneja DataRowView, int, long, etc.)
            int idUsuario;
            try
            {
                if (selectedValue is DataRowView drv)
                    idUsuario = Convert.ToInt32(drv["Id"]);
                else
                    idUsuario = Convert.ToInt32(selectedValue);
            }
            catch
            {
                MessageBox.Show("Error al obtener el ID del usuario seleccionado.", "Biblioteca CUBO",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string nuevaPass = _txtNuevaPassword.Text;
            string confirmarPass = _txtConfirmarPassword.Text;

            // Validaciones
            if (string.IsNullOrWhiteSpace(nuevaPass))
            {
                MessageBox.Show("Ingrese la nueva contraseña.", "Biblioteca CUBO",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtNuevaPassword.Focus();
                return;
            }

            if (nuevaPass.Length < 6)
            {
                MessageBox.Show("La contraseña debe tener al menos 6 caracteres.", "Biblioteca CUBO",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtNuevaPassword.Focus();
                return;
            }

            if (nuevaPass != confirmarPass)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Biblioteca CUBO",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtConfirmarPassword.Focus();
                return;
            }

            try
            {
                using var conexion = ConexionDB.ObtenerConexion();
                using var cmd = conexion.CreateCommand();

                // Hash seguro de la nueva contraseña
                string hashedPass = PasswordHasher.Hash(nuevaPass);
                cmd.CommandText = "UPDATE Usuarios SET Contrasena = @pass WHERE Id = @id;";
                cmd.Parameters.AddWithValue("@pass", hashedPass);
                cmd.Parameters.AddWithValue("@id", idUsuario);

                int filasAfectadas = cmd.ExecuteNonQuery();

                if (filasAfectadas > 0)
                {
                    MessageBox.Show("La contraseña del usuario seleccionado ha sido actualizada correctamente.",
                        "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Error: No se encontró el usuario en la base de datos (ID: " + idUsuario + ").",
                        "Error de Actualización", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (SqliteException ex)
            {
                MessageBox.Show($"Error de base de datos al actualizar: {ex.Message}",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al actualizar: {ex.Message}",
                    "Biblioteca CUBO", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormGestionUsuarios_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }
    }
}