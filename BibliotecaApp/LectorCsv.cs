using System.Text;

namespace BibliotecaApp
{
    /// <summary>
    /// Lector de archivos CSV compatible con RFC-4180 (campos entre comillas,
    /// comillas escapadas duplicadas y saltos de línea dentro del campo).
    /// </summary>
    public static class LectorCsv
    {
        public static List<string[]> Leer(string ruta)
        {
            Encoding codificacion = DetectarCodificacion(ruta);
            var filas = new List<string[]>();

            using var reader = new StreamReader(ruta, codificacion);
            var campoActual = new StringBuilder();
            var filaActual = new List<string>();
            bool dentroComillas = false;
            bool filaConDatos = false;

            int c;
            while ((c = reader.Read()) != -1)
            {
                char ch = (char)c;

                if (dentroComillas)
                {
                    if (ch == '"')
                    {
                        int siguiente = reader.Peek();
                        if (siguiente == '"')
                        {
                            campoActual.Append('"');
                            reader.Read();
                        }
                        else
                        {
                            dentroComillas = false;
                        }
                    }
                    else
                    {
                        campoActual.Append(ch);
                    }
                    continue;
                }

                switch (ch)
                {
                    case '"':
                        dentroComillas = true;
                        filaConDatos = true;
                        break;

                    case ',':
                        filaActual.Add(campoActual.ToString().Trim());
                        campoActual.Clear();
                        filaConDatos = true;
                        break;

                    case '\r':
                        break;

                    case '\n':
                        CerrarFila(filas, filaActual, campoActual, filaConDatos);
                        filaActual = new List<string>();
                        campoActual.Clear();
                        filaConDatos = false;
                        break;

                    default:
                        campoActual.Append(ch);
                        filaConDatos = true;
                        break;
                }
            }

            CerrarFila(filas, filaActual, campoActual, filaConDatos);
            return filas;
        }

        /// <summary>
        /// Garantiza la lectura correcta de ñ y tildes: si el archivo es UTF-8 válido
        /// (con o sin BOM) se lee como UTF-8; si contiene bytes inválidos, cae a
        /// Windows-1252 (ANSI latino, típico de exportaciones antiguas de Excel).
        /// Nunca se usa Encoding.Default, que depende del idioma del Windows.
        /// </summary>
        public static Encoding DetectarCodificacion(string ruta)
        {
            byte[] bytes = File.ReadAllBytes(ruta);

            // BOM UTF-8 (EF BB BF)
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return new UTF8Encoding(true);

            try
            {
                // Validación estricta: lanza DecoderFallbackException si hay
                // secuencias que no corresponden a UTF-8.
                new UTF8Encoding(false, throwOnInvalidBytes: true).GetString(bytes);
                return new UTF8Encoding(false);
            }
            catch (DecoderFallbackException)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding(1252);
            }
        }

        private static void CerrarFila(List<string[]> filas, List<string> fila, StringBuilder campo, bool conDatos)
        {
            if (!conDatos && fila.Count == 0) return;
            fila.Add(campo.ToString().Trim());
            filas.Add(fila.ToArray());
        }
    }
}
