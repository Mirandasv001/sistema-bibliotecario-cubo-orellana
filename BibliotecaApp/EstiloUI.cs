namespace BibliotecaApp
{
    /// <summary>Paleta y helpers visuales compartidos por toda la aplicación.</summary>
    public static class EstiloUI
    {
        // Paleta institucional
        public static readonly Color FondoOscuro = Color.FromArgb(27, 36, 55);      // menú lateral
        public static readonly Color HoverOscuro = Color.FromArgb(45, 60, 90);      // botón activo / hover
        public static readonly Color Acento = Color.FromArgb(79, 107, 237);         // botones primarios
        public static readonly Color AcentoHover = Color.FromArgb(99, 125, 245);
        public static readonly Color FondoClaro = Color.FromArgb(244, 246, 250);    // fondo del contenido
        public static readonly Color Blanco = Color.White;
        public static readonly Color TextoOscuro = Color.FromArgb(40, 46, 58);
        public static readonly Color GrisBorde = Color.FromArgb(210, 215, 224);
        public static readonly Color AlertaRojo = Color.FromArgb(255, 205, 205);    // filas vencidas

        public const string FuenteBase = "Segoe UI";

        public static Font TituloSeccion() => new(FuenteBase, 14F, FontStyle.Bold);
        public static Font Subtitulo() => new(FuenteBase, 8.5F, FontStyle.Regular);
        public static Font Etiqueta() => new(FuenteBase, 9F, FontStyle.Bold);
        public static Font BotonMenu() => new(FuenteBase, 10F, FontStyle.Bold);
        public static Font BotonPrincipal() => new(FuenteBase, 9.75F, FontStyle.Bold);

        /// <summary>Aplica el estilo estándar a un botón de acción primario.</summary>
        public static void EstilizarBotonPrimario(Button boton)
        {
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.BackColor = Acento;
            boton.ForeColor = Blanco;
            boton.Font = BotonPrincipal();
            boton.Cursor = Cursors.Hand;
            boton.Height = 38;

            boton.MouseEnter += (_, _) => boton.BackColor = AcentoHover;
            boton.MouseLeave += (_, _) => boton.BackColor = Acento;
        }

        /// <summary>Aplica el estilo estándar a un botón secundario.</summary>
        public static void EstilizarBotonSecundario(Button boton)
        {
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderColor = GrisBorde;
            boton.BackColor = Blanco;
            boton.ForeColor = TextoOscuro;
            boton.Cursor = Cursors.Hand;
            boton.Height = 38;
        }

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
    }
}
