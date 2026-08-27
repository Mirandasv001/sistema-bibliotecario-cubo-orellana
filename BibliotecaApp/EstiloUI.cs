using System.Runtime.InteropServices;

namespace BibliotecaApp
{
    /// <summary>Paleta y helpers visuales compartidos por toda la aplicación.</summary>
    public static class EstiloUI
    {
        // ── Paleta institucional ──────────────────────────────────────────
        public static readonly Color FondoOscuro = Color.FromArgb(27, 36, 55);
        public static readonly Color HoverOscuro = Color.FromArgb(45, 60, 90);
        public static readonly Color Acento = Color.FromArgb(79, 107, 237);
        public static readonly Color AcentoHover = Color.FromArgb(99, 125, 245);
        public static readonly Color FondoClaro = Color.FromArgb(244, 246, 250);
        public static readonly Color FondoPergamino = Color.FromArgb(250, 247, 239);
        public static readonly Color Blanco = Color.White;
        public static readonly Color TextoOscuro = Color.FromArgb(40, 46, 58);
        public static readonly Color GrisBorde = Color.FromArgb(210, 215, 224);
        public static readonly Color HoverSecundario = Color.FromArgb(237, 240, 244);
        public static readonly Color BordeSecundarioHover = Color.FromArgb(180, 185, 194);
        public static readonly Color AlertaRojo = Color.FromArgb(255, 205, 205);

        // ── Fuentes ──────────────────────────────────────────────────────
        public const string FuenteBase = "Segoe UI";

        public static Font TituloSeccion() => new(FuenteBase, 14F, FontStyle.Bold);
        public static Font Subtitulo() => new(FuenteBase, 8.5F, FontStyle.Regular);
        public static Font Etiqueta() => new(FuenteBase, 9F, FontStyle.Bold);
        public static Font BotonMenu() => new(FuenteBase, 10F, FontStyle.Bold);
        public static Font BotonPrincipal() => new(FuenteBase, 9.75F, FontStyle.Bold);

        // ── Botones ──────────────────────────────────────────────────────

        /// <summary>Botón primario: fondo azul institucional + hover suave.</summary>
        public static void EstilizarBotonPrimario(Button boton)
        {
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.BackColor = Acento;
            boton.ForeColor = Blanco;
            boton.Font = BotonPrincipal();
            boton.Cursor = Cursors.Hand;
            boton.Height = 38;

            boton.MouseEnter += (_, _) =>
            {
                boton.BackColor = AcentoHover;
                boton.FlatAppearance.BorderColor = AcentoHover;
            };
            boton.MouseLeave += (_, _) =>
            {
                boton.BackColor = Acento;
                boton.FlatAppearance.BorderColor = Acento;
            };
        }

        /// <summary>Botón secundario: borde sutil + hover con elevación visual.</summary>
        public static void EstilizarBotonSecundario(Button boton)
        {
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderColor = GrisBorde;
            boton.BackColor = Blanco;
            boton.ForeColor = TextoOscuro;
            boton.Font = BotonPrincipal();
            boton.Cursor = Cursors.Hand;
            boton.Height = 38;

            boton.MouseEnter += (_, _) =>
            {
                boton.BackColor = HoverSecundario;
                boton.FlatAppearance.BorderColor = BordeSecundarioHover;
            };
            boton.MouseLeave += (_, _) =>
            {
                boton.BackColor = Blanco;
                boton.FlatAppearance.BorderColor = GrisBorde;
            };
        }

        // ── Etiquetas y entradas ─────────────────────────────────────────

        /// <summary>Crea una etiqueta de campo con el estilo estándar.</summary>
        public static Label CrearEtiqueta(string texto)
        {
            return new Label
            {
                Text = texto,
                AutoSize = true,
                Font = Etiqueta(),
                ForeColor = TextoOscuro,
                Margin = new Padding(3, 8, 3, 2)
            };
        }

        /// <summary>Configura una entrada de datos con apariencia uniforme.</summary>
        public static void EstilizarEntrada(Control control)
        {
            control.Font = new Font(FuenteBase, 10F);
            control.Margin = new Padding(3, 0, 12, 6);

            if (control is TextBoxBase tb)
            {
                tb.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        // ── Placeholders nativos (Win32) ─────────────────────────────────

        /// <summary>
        /// Aplica un placeholder nativo (EM_SETCUEBANNER) a un TextBox.
        /// El texto se muestra en gris claro y desaparece al recibir foco.
        /// </summary>
        public static void EstablecerPlaceholder(TextBox textBox, string texto)
        {
            NativeMethods.SendMessage(
                textBox.Handle,
                NativeMethods.EM_SETCUEBANNER,
                IntPtr.Zero,
                texto);
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    //  P/Invoke Win32 para Placeholders nativos en TextBox
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Declara las funciones nativas de user32.dll necesarias para
    /// aplicar el cue banner (placeholder) en controles TextBox.
    /// EM_SETCUEBANNER (0x1501) es soportado desde Windows Vista.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// Mensaje EM_SETCUEBANNER: establece el texto de placeholder.
        /// wParam = 1 → permanece visible al recibir foco; 0 → desaparece.
        /// </summary>
        public const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr SendMessage(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            string lParam);
    }
}
