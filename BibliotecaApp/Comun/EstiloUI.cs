using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace BibliotecaApp
{
    public static class EstiloUI
    {
        public static readonly Color FondoOscuro = Color.FromArgb(0x1B, 0x2B, 0x42);       
        public static readonly Color HoverOscuro = Color.FromArgb(0x3A, 0x50, 0x6B);        
        public static readonly Color Acento = Color.FromArgb(0x3A, 0x50, 0x6B);             
        public static readonly Color AcentoHover = Color.FromArgb(0x4A, 0x60, 0x7B);         
        public static readonly Color FondoClaro = Color.FromArgb(0xF4, 0xF6, 0xF9);          
        public static readonly Color FondoPergamino = Color.AliceBlue;

        public static readonly Color Blanco = Color.White;

        public static readonly Color TextoOscuro = Color.FromArgb(0x2B, 0x2D, 0x42);        
        public static readonly Color GrisBorde = Color.FromArgb(0xCF, 0xD8, 0xE3);           
        public static readonly Color HoverSecundario = Color.FromArgb(0xED, 0xF0, 0xF4);     
        public static readonly Color BordeSecundarioHover = Color.FromArgb(0xB4, 0xB9, 0xC2); 
        public static readonly Color AlertaRojo = Color.FromArgb(0xFF, 0xCD, 0xCD);          
        public const string FuenteBase = "Segoe UI";

        public static readonly Font FuenteNormal = new Font(FuenteBase, 10f, FontStyle.Regular, GraphicsUnit.Point);

        public static readonly Font FuenteTitulo = new Font(FuenteBase, 14f, FontStyle.Bold, GraphicsUnit.Point);

        public static readonly Font FuenteSubtitulo = new Font(FuenteBase, 11f, FontStyle.Bold, GraphicsUnit.Point);

        public static readonly Font FuenteEtiqueta = new Font(FuenteBase, 9.5f, FontStyle.Regular, GraphicsUnit.Point);

        public static readonly Font FuenteBoton = new Font(FuenteBase, 10f, FontStyle.Bold, GraphicsUnit.Point);

        public static readonly Font FuenteTituloTarjeta = new Font(FuenteBase, 9f, FontStyle.Bold, GraphicsUnit.Point);

        public static readonly Font FuenteCifraGrande = new Font(FuenteBase, 28f, FontStyle.Bold, GraphicsUnit.Point);

        public static readonly Font FuenteGraficaValor = new Font(FuenteBase, 11f, FontStyle.Bold, GraphicsUnit.Point);

        public static readonly Font FuenteGraficaCategoria = new Font(FuenteBase, 10f, FontStyle.Bold, GraphicsUnit.Point);

        public static readonly Font FuenteGraficaTitulo = new Font(FuenteBase, 12f, FontStyle.Bold, GraphicsUnit.Point);

        public static readonly Font FuenteSinDatos = new Font(FuenteBase, 11f, FontStyle.Italic, GraphicsUnit.Point);

        public static readonly Font FuenteVacia = new Font(FuenteBase, 14f, FontStyle.Italic, GraphicsUnit.Point);

        public static Font TituloSeccion() => FuenteTitulo;

        public static Font Subtitulo() => FuenteSubtitulo;

        public static Font Etiqueta() => FuenteEtiqueta;

        public static Font BotonMenu() => FuenteBoton;

        public static Font BotonPrincipal() => FuenteBoton;

        public static void EstilizarBotonPrimario(Button boton)
        {
            if (boton == null) return;

            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = Acento;
            boton.FlatAppearance.MouseDownBackColor = FondoOscuro;

            boton.BackColor = FondoOscuro;      // #1B2B42
            boton.ForeColor = Blanco;
            boton.Font = BotonPrincipal();       // 10 pt, Bold, Segoe UI
            boton.Cursor = Cursors.Hand;
            boton.Height = 40;
            boton.UseVisualStyleBackColor = false;

            boton.MouseEnter -= BotonPrimario_MouseEnter;
            boton.MouseLeave -= BotonPrimario_MouseLeave;
            boton.MouseEnter += BotonPrimario_MouseEnter;
            boton.MouseLeave += BotonPrimario_MouseLeave;
        }

        private static void BotonPrimario_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is Button btn) btn.BackColor = Acento;
        }

        private static void BotonPrimario_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is Button btn) btn.BackColor = FondoOscuro;
        }

        public static void EstilizarBotonSecundario(Button boton)
        {
            if (boton == null) return;

            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = HoverSecundario;
            boton.FlatAppearance.MouseDownBackColor = GrisBorde;

            boton.BackColor = Color.FromArgb(0xE2, 0xEA, 0xFC); // #E2EAFC - gris azulado claro
            boton.ForeColor = FondoOscuro;                       // Texto azul oscuro
            boton.Font = BotonPrincipal();                        // 10 pt, Bold, Segoe UI
            boton.Cursor = Cursors.Hand;
            boton.Height = 40;
            boton.UseVisualStyleBackColor = false;

            boton.MouseEnter -= BotonSecundario_MouseEnter;
            boton.MouseLeave -= BotonSecundario_MouseLeave;
            boton.MouseEnter += BotonSecundario_MouseEnter;
            boton.MouseLeave += BotonSecundario_MouseLeave;
        }

        private static void BotonSecundario_MouseEnter(object? sender, EventArgs e)
        {
            if (sender is Button btn) btn.BackColor = Color.FromArgb(0xD0, 0xDC, 0xF0);
        }

        private static void BotonSecundario_MouseLeave(object? sender, EventArgs e)
        {
            if (sender is Button btn) btn.BackColor = Color.FromArgb(0xE2, 0xEA, 0xFC);
        }

        public static Label CrearEtiqueta(string texto)
        {
            return new Label
            {
                Text = texto,
                AutoSize = true,
                Font = Etiqueta(),           // 9.5 pt, Regular, Segoe UI
                ForeColor = TextoOscuro,     // #2B2D42
                Margin = new Padding(3, 8, 3, 2)
            };
        }

        public static void EstilizarEntrada(Control control)
        {
            if (control == null) return;

            control.Font = FuenteNormal; 
            control.ForeColor = TextoOscuro;                                                 // #2B2D42
            control.BackColor = Blanco;
            control.Margin = new Padding(3, 0, 12, 6);

            if (control is TextBoxBase tb)
            {
                tb.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (control is ComboBox cb)
            {
                cb.DropDownStyle = ComboBoxStyle.DropDownList;
                cb.FlatStyle = FlatStyle.Flat; // Look moderno
            }
        }

        public static void EstablecerPlaceholder(TextBox textBox, string texto)
        {
            NativeMethods.SendMessage(
                textBox.Handle,
                NativeMethods.EM_SETCUEBANNER,
                IntPtr.Zero,
                texto);
        }

        public static string RemoverTildes(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto;

            string normalized = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(normalized.Length);

            foreach (char c in normalized)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }

    internal static class NativeMethods
    {
        public const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SendMessage(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            string lParam);
    }
}