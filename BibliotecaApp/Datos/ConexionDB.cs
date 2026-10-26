using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    public static class ConexionDB
    {
        private const string NombreBaseDatos = "biblioteca.db";

        private static readonly string CarpetaDatos =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "BibliotecaCUBO");

        // Carpeta alternativa de datos (portable / pruebas). Si no se indica,
        // la BD vive siempre en %APPDATA%\BibliotecaCUBO.
        public static string? CarpetaDatosPersonalizada { get; set; }

        public static string RutaBaseDatos
        {
            get
            {
                string carpeta = CarpetaDatosPersonalizada ?? CarpetaDatos;
                Directory.CreateDirectory(carpeta);
                return Path.Combine(carpeta, NombreBaseDatos);
            }
        }

        public static string CadenaConexion =>
            new SqliteConnectionStringBuilder
            {
                DataSource = RutaBaseDatos,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

        public static string? CatalogoCsvPath { get; set; } = null;

        public static SqliteConnection ObtenerConexion()
        {
            var conexion = new SqliteConnection(CadenaConexion);
            conexion.Open();
            return conexion;
        }

        public static void Inicializar()
        {
            using var conexion = ObtenerConexion();
            using var transaccion = conexion.BeginTransaction();
            try
            {
                CrearTablas(conexion);
                MigrarEsquemaLibros(conexion);
                MigrarEsquemaControlSala(conexion, transaccion);
                MigrarEsquemaPrestamos(conexion);
                MigrarIdentidadEjemplar(conexion);
                SeedUsuarios(conexion);
                transaccion.Commit();
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }

            // El catálogo CSV se importa en una transacción propia: un archivo
            // corrupto no debe impedir que la aplicación arranque.
            try
            {
                using var conexionCsv = ObtenerConexion();
                using var transaccionCsv = conexionCsv.BeginTransaction();
                try
                {
                    ImportarCatalogoDesdeCsv(conexionCsv, transaccionCsv);
                    transaccionCsv.Commit();
                }
                catch
                {
                    transaccionCsv.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Catálogo CSV omitido: {ex.Message}");
            }
        }

        // BD antiguas no traían Disponibilidad: sin este ALTER rompen inventario,
        // préstamos y sala. La columna nueva arranca como 'Disponible'.
        private static void MigrarEsquemaLibros(SqliteConnection conexion)
        {
            var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var info = conexion.CreateCommand())
            {
                info.CommandText = "PRAGMA table_info(Libros);";
                using var lector = info.ExecuteReader();
                while (lector.Read())
                    columnas.Add(lector.GetString(1));
            }

            if (columnas.Contains("Disponibilidad")) return;

            using var agregar = conexion.CreateCommand();
            agregar.CommandText =
                "ALTER TABLE Libros ADD COLUMN Disponibilidad TEXT DEFAULT 'Disponible';";
            agregar.ExecuteNonQuery();
        }

        private static void SeedUsuarios(SqliteConnection conexion)
        {
            using var check = conexion.CreateCommand();
            check.CommandText = "SELECT COUNT(*) FROM Usuarios;";
            long count = (long)(check.ExecuteScalar() ?? 0);
            if (count > 0) return;

            using var cmd = conexion.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Usuarios (Nombre, Usuario, Contrasena, Rol) VALUES
                (@nombre1, @usuario1, @pass1, @rol1),
                (@nombre2, @usuario2, @pass2, @rol2);";
            cmd.Parameters.AddWithValue("@nombre1", "Usuario Operador");
            cmd.Parameters.AddWithValue("@usuario1", "UserCubo");
            cmd.Parameters.AddWithValue("@pass1", PasswordHasher.Hash("1234$"));
            cmd.Parameters.AddWithValue("@rol1", "Operador");
            cmd.Parameters.AddWithValue("@nombre2", "Administrador");
            cmd.Parameters.AddWithValue("@usuario2", "AdminCubo");
            cmd.Parameters.AddWithValue("@pass2", PasswordHasher.Hash("Admin123$"));
            cmd.Parameters.AddWithValue("@rol2", "Administrador");
            cmd.ExecuteNonQuery();
        }

        private static void MigrarEsquemaControlSala(SqliteConnection conexion, SqliteTransaction transaccion)
        {
            var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var info = conexion.CreateCommand())
            {
                info.CommandText = "PRAGMA table_info(ControlUsuariosSala);";
                using var lector = info.ExecuteReader();
                while (lector.Read())
                {
                    columnas.Add(lector.GetString(1));
                }
            }

            if (columnas.Contains("Taller"))
            {
                try
                {
                    using var eliminar = conexion.CreateCommand();
                    eliminar.CommandText = "ALTER TABLE ControlUsuariosSala DROP COLUMN Taller;";
                    eliminar.ExecuteNonQuery();
                }
                catch (SqliteException)
                {
                    const string nuevaTabla = @"
                        CREATE TABLE ControlUsuariosSala_Nueva (
                            ID             INTEGER PRIMARY KEY AUTOINCREMENT,
                            Fecha          TEXT,
                            NombreUsuario  TEXT,
                            Genero         TEXT,
                            Edad           INTEGER,
                            TituloLibro    TEXT,
                            HoraEntrega    TEXT,
                            HoraRecibido   TEXT,
                            PersonalTurno  TEXT,
                            Estado         TEXT DEFAULT 'Pendiente'
                        );";

                    using (var crear = conexion.CreateCommand())
                    {
                        crear.Transaction = transaccion;
                        crear.CommandText = nuevaTabla;
                        crear.ExecuteNonQuery();
                    }

                    using (var copiar = conexion.CreateCommand())
                    {
                        copiar.Transaction = transaccion;
                        copiar.CommandText = @"
                            INSERT INTO ControlUsuariosSala_Nueva
                                (ID, Fecha, NombreUsuario, Genero, Edad,
                                 TituloLibro, HoraEntrega, HoraRecibido, PersonalTurno, Estado)
                            SELECT ID, Fecha, NombreUsuario, Genero, Edad,
                                   TituloLibro, HoraEntrega, HoraRecibido, PersonalTurno,
                                   CASE WHEN HoraRecibido = 'En lectura' THEN 'Pendiente' ELSE 'Entregado' END
                            FROM ControlUsuariosSala;";
                        copiar.ExecuteNonQuery();
                    }

                    using (var borrarVieja = conexion.CreateCommand())
                    {
                        borrarVieja.Transaction = transaccion;
                        borrarVieja.CommandText = "DROP TABLE ControlUsuariosSala;";
                        borrarVieja.ExecuteNonQuery();
                    }

                    using (var renombrar = conexion.CreateCommand())
                    {
                        renombrar.Transaction = transaccion;
                        renombrar.CommandText =
                            "ALTER TABLE ControlUsuariosSala_Nueva RENAME TO ControlUsuariosSala;";
                        renombrar.ExecuteNonQuery();
                    }
                }
            }

            if (!columnas.Contains("Estado"))
            {
                try
                {
                    using var agregar = conexion.CreateCommand();
                    agregar.CommandText = "ALTER TABLE ControlUsuariosSala ADD COLUMN Estado TEXT DEFAULT 'Pendiente';";
                    agregar.ExecuteNonQuery();
                }
                catch (SqliteException)
                {
                    const string nuevaTabla = @"
                        CREATE TABLE ControlUsuariosSala_Nueva (
                            ID             INTEGER PRIMARY KEY AUTOINCREMENT,
                            Fecha          TEXT,
                            NombreUsuario  TEXT,
                            Genero         TEXT,
                            Edad           INTEGER,
                            TituloLibro    TEXT,
                            HoraEntrega    TEXT,
                            HoraRecibido   TEXT,
                            PersonalTurno  TEXT,
                            Estado         TEXT DEFAULT 'Pendiente'
                        );";

                    using (var crear = conexion.CreateCommand())
                    {
                        crear.Transaction = transaccion;
                        crear.CommandText = nuevaTabla;
                        crear.ExecuteNonQuery();
                    }

                    using (var copiar = conexion.CreateCommand())
                    {
                        copiar.Transaction = transaccion;
                        copiar.CommandText = @"
                            INSERT INTO ControlUsuariosSala_Nueva
                                (ID, Fecha, NombreUsuario, Genero, Edad,
                                 TituloLibro, HoraEntrega, HoraRecibido, PersonalTurno, Estado)
                            SELECT ID, Fecha, NombreUsuario, Genero, Edad,
                                   TituloLibro, HoraEntrega, HoraRecibido, PersonalTurno,
                                   CASE WHEN HoraRecibido = 'En lectura' THEN 'Pendiente' ELSE 'Entregado' END
                            FROM ControlUsuariosSala;";
                        copiar.ExecuteNonQuery();
                    }

                    using (var borrarVieja = conexion.CreateCommand())
                    {
                        borrarVieja.Transaction = transaccion;
                        borrarVieja.CommandText = "DROP TABLE ControlUsuariosSala;";
                        borrarVieja.ExecuteNonQuery();
                    }

                    using (var renombrar = conexion.CreateCommand())
                    {
                        renombrar.Transaction = transaccion;
                        renombrar.CommandText =
                            "ALTER TABLE ControlUsuariosSala_Nueva RENAME TO ControlUsuariosSala;";
                        renombrar.ExecuteNonQuery();
                    }
                }
            }

            // Backfill idempotente: se ejecuta en cada arranque, no solo la primera vez.
            // Tras el ALTER, las filas antiguas LEEN el DEFAULT ('Pendiente') y no NULL,
            // por eso hay que reparar también las marcadas 'Pendiente' sin estar en lectura.
            using (var completar = conexion.CreateCommand())
            {
                completar.Transaction = transaccion;
                completar.CommandText = @"
                    UPDATE ControlUsuariosSala
                    SET Estado = CASE
                        WHEN UPPER(IFNULL(HoraRecibido, '')) = 'EN LECTURA' THEN 'Pendiente'
                        ELSE 'Entregado'
                    END
                    WHERE IFNULL(Estado, '') = ''
                       OR (Estado = 'Pendiente'
                           AND UPPER(IFNULL(HoraRecibido, '')) <> 'EN LECTURA');";
                completar.ExecuteNonQuery();
            }
        }

        private static void MigrarEsquemaPrestamos(SqliteConnection conexion)
        {
            var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var info = conexion.CreateCommand())
            {
                info.CommandText = "PRAGMA table_info(PrestamosExternos);";
                using var lector = info.ExecuteReader();
                while (lector.Read())
                    columnas.Add(lector.GetString(1));
            }

            if (!columnas.Contains("CodigoLibro"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN CodigoLibro TEXT;";
                agregar.ExecuteNonQuery();
            }

            if (!columnas.Contains("FechaDevolucion"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN FechaDevolucion TEXT;";
                agregar.ExecuteNonQuery();
            }

            // Trazabilidad de quién ejecutó cada renovación / devolución
            if (!columnas.Contains("PersonalRenovo"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN PersonalRenovo TEXT;";
                agregar.ExecuteNonQuery();
            }

            if (!columnas.Contains("PersonalDevolvio"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN PersonalDevolvio TEXT;";
                agregar.ExecuteNonQuery();
            }

            if (!columnas.Contains("Genero"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN Genero TEXT DEFAULT '';";
                agregar.ExecuteNonQuery();
            }

            if (!columnas.Contains("CorreoBienvenidaEnviado"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN CorreoBienvenidaEnviado INTEGER DEFAULT 0;";
                agregar.ExecuteNonQuery();
            }

            if (!columnas.Contains("CorreoAlertaEnviado"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE PrestamosExternos ADD COLUMN CorreoAlertaEnviado INTEGER DEFAULT 0;";
                agregar.ExecuteNonQuery();
            }

            using var indice = conexion.CreateCommand();
            indice.CommandText = "CREATE INDEX IF NOT EXISTS IX_Prestamos_CodigoLibro ON PrestamosExternos(CodigoLibro);";
            indice.ExecuteNonQuery();
        }

        // Añade CodigoLibro a ControlUsuariosSala y completa el código en filas antiguas
        // solo cuando el título identifica un único ejemplar (sin elegir al azar).
        private static void MigrarIdentidadEjemplar(SqliteConnection conexion)
        {
            var columnas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var info = conexion.CreateCommand())
            {
                info.CommandText = "PRAGMA table_info(ControlUsuariosSala);";
                using var lector = info.ExecuteReader();
                while (lector.Read())
                    columnas.Add(lector.GetString(1));
            }

            if (!columnas.Contains("CodigoLibro"))
            {
                using var agregar = conexion.CreateCommand();
                agregar.CommandText = "ALTER TABLE ControlUsuariosSala ADD COLUMN CodigoLibro TEXT DEFAULT '';";
                agregar.ExecuteNonQuery();
            }

            using (var completarSala = conexion.CreateCommand())
            {
                completarSala.CommandText = @"
                    UPDATE ControlUsuariosSala
                    SET CodigoLibro = (
                        SELECT L.Codigo FROM Libros L
                        WHERE L.Titulo = ControlUsuariosSala.TituloLibro)
                    WHERE IFNULL(CodigoLibro, '') = ''
                      AND (SELECT COUNT(*) FROM Libros L
                           WHERE L.Titulo = ControlUsuariosSala.TituloLibro) = 1;";
                completarSala.ExecuteNonQuery();
            }

            using (var completarPrestamos = conexion.CreateCommand())
            {
                completarPrestamos.CommandText = @"
                    UPDATE PrestamosExternos
                    SET CodigoLibro = (
                        SELECT L.Codigo FROM Libros L
                        WHERE L.Titulo = PrestamosExternos.TituloLibro)
                    WHERE IFNULL(CodigoLibro, '') = ''
                      AND (SELECT COUNT(*) FROM Libros L
                           WHERE L.Titulo = PrestamosExternos.TituloLibro) = 1;";
                completarPrestamos.ExecuteNonQuery();
            }
        }

        private static void CrearTablas(SqliteConnection conexion)
        {
            const string sqlLibros = @"
                CREATE TABLE IF NOT EXISTS Libros (
                    Codigo         TEXT PRIMARY KEY,
                    Titulo         TEXT NOT NULL,
                    Autor          TEXT,
                    Editorial      TEXT,
                    Estado         TEXT,
                    Ubicacion      TEXT,
                    Disponibilidad TEXT DEFAULT 'Disponible'
                );";

            const string sqlSala = @"
                CREATE TABLE IF NOT EXISTS ControlUsuariosSala (
                    ID             INTEGER PRIMARY KEY AUTOINCREMENT,
                    Fecha          TEXT,
                    NombreUsuario  TEXT,
                    Genero         TEXT,
                    Edad           INTEGER,
                    TituloLibro    TEXT,
                    HoraEntrega    TEXT,
                    HoraRecibido   TEXT,
                    PersonalTurno  TEXT
                );";

            const string sqlPrestamos = @"
                CREATE TABLE IF NOT EXISTS PrestamosExternos (
                    ID               INTEGER PRIMARY KEY AUTOINCREMENT,
                    NombreUsuario    TEXT,
                    Correo           TEXT,
                    DUI              TEXT,
                    Telefono         TEXT,
                    Direccion        TEXT,
                    TituloLibro      TEXT,
                    FechaPrestamo    TEXT,
                    PersonalPresto   TEXT,
                    FechaRenovacion  TEXT,
                    PersonalRenovo   TEXT,
                    FechaEntrega     TEXT,
                    PersonalRecibio  TEXT,
                    EstadoLibro      TEXT DEFAULT 'Pendiente',
                    CodigoLibro      TEXT,
                    FechaDevolucion  TEXT,
                    PersonalDevolvio TEXT,
                    Genero          TEXT DEFAULT '',
                    CorreoBienvenidaEnviado INTEGER DEFAULT 0,
                    CorreoAlertaEnviado     INTEGER DEFAULT 0
                );";

            const string sqlUsuarios = @"
                CREATE TABLE IF NOT EXISTS Usuarios (
                    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre         TEXT NOT NULL,
                    Usuario        TEXT NOT NULL UNIQUE,
                    Contrasena     TEXT NOT NULL,
                    Rol            TEXT NOT NULL DEFAULT 'Operador'
                );";

            const string sqlConfiguracion = @"
                CREATE TABLE IF NOT EXISTS Configuracion (
                    Clave TEXT PRIMARY KEY,
                    Valor TEXT
                );";

            const string sqlUbicacionesConfig = @"
                CREATE TABLE IF NOT EXISTS UbicacionesConfig (
                    NombreUbicacion TEXT PRIMARY KEY
                );";

            using (var cmd = conexion.CreateCommand())
            {
                cmd.CommandText = sqlLibros + sqlSala + sqlPrestamos + sqlUsuarios
                    + sqlConfiguracion + sqlUbicacionesConfig;
                cmd.ExecuteNonQuery();
            }

            using var indice = conexion.CreateCommand();
            indice.CommandText = "CREATE INDEX IF NOT EXISTS IX_Libros_Titulo ON Libros(Titulo);";
            indice.ExecuteNonQuery();

            using (var cmdConteo = conexion.CreateCommand())
            {
                cmdConteo.CommandText = "SELECT COUNT(*) FROM UbicacionesConfig;";
                long total = Convert.ToInt64(cmdConteo.ExecuteScalar());

                if (total == 0)
                {
                    foreach (string modulo in new[] { "M1", "M2", "M3", "M4", "M5" })
                    {
                        using var cmdSemilla = conexion.CreateCommand();
                        cmdSemilla.CommandText =
                            "INSERT INTO UbicacionesConfig (NombreUbicacion) VALUES (@nombre);";
                        cmdSemilla.Parameters.AddWithValue("@nombre", modulo);
                        cmdSemilla.ExecuteNonQuery();
                    }
                }
            }
        }

        private const string ClaveInventarioVaciado = "InventarioVaciado";

        public static void MarcarInventarioVaciado(bool vaciado)
        {
            using var conexion = ObtenerConexion();
            MarcarInventarioVaciado(conexion, null, vaciado);
        }

        // Sobrecarga transaccional: el flag se marca con la misma transacción del borrado
        public static void MarcarInventarioVaciado(
            SqliteConnection conexion, SqliteTransaction? transaccion, bool vaciado)
        {
            using var cmd = conexion.CreateCommand();
            cmd.Transaction = transaccion;
            cmd.CommandText = "INSERT OR REPLACE INTO Configuracion (Clave, Valor) VALUES (@clave, @valor);";
            cmd.Parameters.AddWithValue("@clave", ClaveInventarioVaciado);
            cmd.Parameters.AddWithValue("@valor", vaciado ? "1" : "0");
            cmd.ExecuteNonQuery();
        }

        private static bool InventarioVaciadoPorUsuario(SqliteConnection conexion)
        {
            using var cmd = conexion.CreateCommand();
            cmd.CommandText = "SELECT Valor FROM Configuracion WHERE Clave = @clave;";
            cmd.Parameters.AddWithValue("@clave", ClaveInventarioVaciado);

            object? valor = cmd.ExecuteScalar();
            return string.Equals(valor as string, "1", StringComparison.OrdinalIgnoreCase);
        }

        private static void ImportarCatalogoDesdeCsv(SqliteConnection conexion, SqliteTransaction transaccion)
        {
            if (InventarioVaciadoPorUsuario(conexion)) return;

            string? rutaCsv = BuscarArchivoCsv();
            if (rutaCsv == null) return;

            var filas = LectorCsv.Leer(rutaCsv);
            if (filas.Count == 0) return;

            using var comando = conexion.CreateCommand();
            comando.Transaction = transaccion;
            comando.CommandText = @"
                INSERT OR IGNORE INTO Libros (Codigo, Titulo, Autor, Editorial, Estado, Ubicacion)
                VALUES ($codigo, $titulo, $autor, $editorial, $estado, $ubicacion);";

            var pCodigo = comando.Parameters.Add("$codigo", SqliteType.Text);
            var pTitulo = comando.Parameters.Add("$titulo", SqliteType.Text);
            var pAutor = comando.Parameters.Add("$autor", SqliteType.Text);
            var pEditorial = comando.Parameters.Add("$editorial", SqliteType.Text);
            var pEstado = comando.Parameters.Add("$estado", SqliteType.Text);
            var pUbicacion = comando.Parameters.Add("$ubicacion", SqliteType.Text);

            int insertados = 0;
            foreach (var fila in filas)
            {
                // Omite encabezado y filas incompletas o sin código.
                if (fila.Length < 6) continue;
                if (fila[0].Equals("CODIGO", StringComparison.OrdinalIgnoreCase)) continue;

                string codigo = fila[0].Trim();
                if (codigo.Length == 0) continue;

                pCodigo.Value = codigo;
                pTitulo.Value = fila[1];
                pAutor.Value = fila[2];
                pEditorial.Value = fila[3];
                pEstado.Value = fila[4];
                pUbicacion.Value = fila[5];

                insertados += comando.ExecuteNonQuery();
            }

            System.Diagnostics.Debug.WriteLine($"Catálogo CUBO: {insertados} libros insertados desde CSV.");
        }

        private const string NombreCatalogoCsv =
            "CATÁLOGO BIBLIOTECA CUBO SITIO DEL NIÑO - ORIGINAL-2026 .xlsx - INVENTARIO TOTAL (1).csv";

        // Nombre sin acentos ni ñ: los instaladores suelen descomprimir con otro codepage
        // y el nombre largo puede no coincidir en la PC de destino.
        private const string NombreCatalogoAscii = "catalogo_cubo.csv";

        private static string? BuscarArchivoCsv()
        {
            if (!string.IsNullOrWhiteSpace(CatalogoCsvPath) && File.Exists(CatalogoCsvPath))
                return CatalogoCsvPath;

            // Ruta fija junto al .exe, sin búsquedas ascendentes
            string carpeta = AppContext.BaseDirectory;

            foreach (string nombre in new[] { NombreCatalogoAscii, NombreCatalogoCsv })
            {
                string ruta = Path.Combine(carpeta, nombre);
                if (File.Exists(ruta)) return ruta;
            }

            // Último recurso: si el instalador renombró el fichero, se toma el único CSV
            // que haya junto al .exe. Con más de uno no se elige ninguno al azar.
            try
            {
                string[] candidatos = Directory.GetFiles(carpeta, "*.csv", SearchOption.TopDirectoryOnly);
                if (candidatos.Length == 1) return candidatos[0];
            }
            catch
            {
                // Sin acceso a la carpeta: se omite la importación, la app sigue funcionando.
            }

            return null;
        }
    }
}
