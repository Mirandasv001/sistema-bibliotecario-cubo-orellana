using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace BibliotecaApp
{
    /// <summary>Paleta y helpers visuales compartidos por toda la aplicación.</summary>
    public static class EstiloUI
    {
        // ═══════════════════════════════════════════════════════════════════
        // PALETA INSTITUCIONAL (Nombres originales intactos, valores modernizados)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>Azul institucional oscuro y elegante — #1B2B42.</summary>
        public static readonly Color FondoOscuro = Color.FromArgb(0x1B, 0x2B, 0x42);       // Antes: 27,36,55

        /// <summary>Hover sobre azul oscuro — tono ligeramente más claro.</summary>
        public static readonly Color HoverOscuro = Color.FromArgb(0x3A, 0x50, 0x6B);        // Antes: 45,60,90

        /// <summary>Azul de acento (botones primarios) — #3A506B.</summary>
        public static readonly Color Acento = Color.FromArgb(0x3A, 0x50, 0x6B);             // Antes: 79,107,237

        /// <summary>Hover de acento — tono más brillante.</summary>
        public static readonly Color AcentoHover = Color.FromArgb(0x4A, 0x60, 0x7B);         // Antes: 99,125,245

        /// <summary>Gris ultra suave para fondo de paneles — #F4F6F9.</summary>
        public static readonly Color FondoClaro = Color.FromArgb(0xF4, 0xF6, 0xF9);          // Sin cambios

        /// <summary>Fondo estilo pergamino (se mantiene por compatibilidad).</summary>
        public static readonly Color FondoPergamino = Color.FromArgb(0xFA, 0xF7, 0xEF);      // Antes: 250,247,239

        /// <summary>Blanco puro para tarjetas e inputs.</summary>
        public static readonly Color Blanco = Color.White;

        /// <summary>Gris casi negro para texto principal — #2B2D42 (mejor legibilidad).</summary>
        public static readonly Color TextoOscuro = Color.FromArgb(0x2B, 0x2D, 0x42);        // Antes: 40,46,58

        /// <summary>Gris medio para bordes sutiles.</summary>
        public static readonly Color GrisBorde = Color.FromArgb(0xCF, 0xD8, 0xE3);           // Antes: 210,215,224

        /// <summary>Hover botón secundario — gris muy suave.</summary>
        public static readonly Color HoverSecundario = Color.FromArgb(0xED, 0xF0, 0xF4);     // Antes: 237,240,244

        /// <summary>Borde en hover botón secundario.</summary>
        public static readonly Color BordeSecundarioHover = Color.FromArgb(0xB4, 0xB9, 0xC2); // Antes: 180,185,194

        /// <summary>Rojo suave para alertas (se mantiene).</summary>
        public static readonly Color AlertaRojo = Color.FromArgb(0xFF, 0xCD, 0xCD);          // Sin cambios


        // ═══════════════════════════════════════════════════════════════════
        // TIPOGRAFÍA (Nombres y firmas originales intactos, valores modernizados)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>Familia base: Segoe UI (requerido por diseño corporativo).</summary>
        public const string FuenteBase = "Segoe UI";

        /// <summary>Título de sección — 14 pt, Bold, Segoe UI.</summary>
        public static Font TituloSeccion() => new Font(FuenteBase, 14f, FontStyle.Bold, GraphicsUnit.Point);

        /// <summary>Subtítulo — 11 pt, SemiBold (Bold en WinForms), Segoe UI.</summary>
        public static Font Subtitulo() => new Font(FuenteBase, 11f, FontStyle.Bold, GraphicsUnit.Point);

        /// <summary>Etiquetas de campo — 9.5 pt, Regular, Segoe UI (más legible).</summary>
        public static Font Etiqueta() => new Font(FuenteBase, 9.5f, FontStyle.Regular, GraphicsUnit.Point);

        /// <summary>Botones de menú lateral — 10 pt, Bold, Segoe UI.</summary>
        public static Font BotonMenu() => new Font(FuenteBase, 10f, FontStyle.Bold, GraphicsUnit.Point);

        /// <summary>Botón principal — 10 pt, Bold, Segoe UI.</summary>
        public static Font BotonPrincipal() => new Font(FuenteBase, 10f, FontStyle.Bold, GraphicsUnit.Point);


        // ═══════════════════════════════════════════════════════════════════
        // BOTONES (FlatDesign + Hover via MouseEnter/MouseLeave)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>Botón primario: fondo azul institucional, texto blanco, Flat, sin bordes, hover a Acento.</summary>
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

            // Hover suave via eventos (además de FlatAppearance)
            boton.MouseEnter += (_, _) => boton.BackColor = Acento;
            boton.MouseLeave += (_, _) => boton.BackColor = FondoOscuro;
        }

        /// <summary>Botón secundario: fondo gris claro, texto azul oscuro, Flat, sin bordes, hover sutil.</summary>
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

            // Hover suave via eventos
            Color hoverColor = Color.FromArgb(0xD0, 0xDC, 0xF0);
            boton.MouseEnter += (_, _) => boton.BackColor = hoverColor;
            boton.MouseLeave += (_, _) => boton.BackColor = Color.FromArgb(0xE2, 0xEA, 0xFC);
        }


        // ═══════════════════════════════════════════════════════════════════
        // ETIQUETAS Y ENTRADAS (API original intacta)
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>Crea una etiqueta de campo con el estilo estándar.</summary>
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

        /// <summary>Configura una entrada de datos (TextBox/ComboBox) con apariencia uniforme y moderna.</summary>
        public static void EstilizarEntrada(Control control)
        {
            if (control == null) return;

            control.Font = new Font(FuenteBase, 10f, FontStyle.Regular, GraphicsUnit.Point); // 10 pt, Segoe UI
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


        // ═══════════════════════════════════════════════════════════════════
        // PLACEHOLDERS NATIVOS (Win32) — API original intacta
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>Aplica un placeholder nativo (EM_SETCUEBANNER) a un TextBox.</summary>
        public static void EstablecerPlaceholder(TextBox textBox, string texto)
        {
            NativeMethods.SendMessage(
                textBox.Handle,
                NativeMethods.EM_SETCUEBANNER,
                IntPtr.Zero,
                texto);
        }

        /// <summary>Elimina tildes y diacríticos de un texto.</summary>
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

    // ═══════════════════════════════════════════════════════════════════
    // P/Invoke Win32 para Placeholders nativos en TextBox
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>Funciones nativas de user32.dll para cue banner (placeholder).</summary>
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