using Microsoft.Data.Sqlite;

namespace BibliotecaApp
{
    /// <summary>
    /// Capa de acceso a datos (SQLite). Crea la base 'biblioteca.db', las tablas
    /// del sistema e importa el catálogo de libros desde el CSV al arrancar.
    /// </summary>
    public static class ConexionDB
    {
        private const string NombreBaseDatos = "biblioteca.db";

        public static string RutaBaseDatos =>
            Path.Combine(AppContext.BaseDirectory, NombreBaseDatos);

        public static string CadenaConexion =>
            new SqliteConnectionStringBuilder
            {
                DataSource = RutaBaseDatos,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

        /// <summary>Abre y devuelve una conexión SQLite ya abierta.</summary>
        public static SqliteConnection ObtenerConexion()
        {
            var conexion = new SqliteConnection(CadenaConexion);
            conexion.Open();
            return conexion;
        }

        /// <summary>Inicializa la base de datos: tablas + importación del catálogo.</summary>
        public static void Inicializar()
        {
            using var conexion = ObtenerConexion();
            CrearTablas(conexion);
            MigrarEsquemaControlSala(conexion);
            MigrarEsquemaPrestamos(conexion);
            ImportarCatalogoDesdeCsv(conexion);
            SeedUsuarios(conexion);
        }

        /// <summary>
        /// Inserta los usuarios por defecto si la tabla está vacía.
        /// </summary>
        private static void SeedUsuarios(SqliteConnection conexion)
        {
            using var check = conexion.CreateCommand();
            check.CommandText = "SELECT COUNT(*) FROM Usuarios;";
            long count = (long)(check.ExecuteScalar() ?? 0);
            if (count > 0) return;

            using var cmd = conexion.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO Usuarios (Nombre, Usuario, Contrasena, Rol) VALUES
                ('Usuario Operador', 'UserCubo', '1234$', 'Operador'),
                ('Administrador', 'AdminCubo', 'Admin123$', 'Administrador');";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Migra bases creadas con el esquema anterior: si la tabla
        /// ControlUsuariosSala aún tiene la columna 'Taller', la elimina
        /// preservando todas las filas. Además, añade la columna 'Estado' si no existe.
        /// </summary>
        private static void MigrarEsquemaControlSala(SqliteConnection conexion)
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

            // 1. Eliminar columna 'Taller' si existe (migración legacy)
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
                    // Plan B para versiones antiguas de SQLite: reconstruir la tabla.
                    using var transaccion = conexion.BeginTransaction();
                    try
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

                        transaccion.Commit();
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }

            // 2. Añadir columna 'Estado' si no existe
            if (!columnas.Contains("Estado"))
            {
                try
                {
                    using var agregar = conexion.CreateCommand();
                    agregar.CommandText = "ALTER TABLE ControlUsuariosSala ADD COLUMN Estado TEXT DEFAULT 'Pendiente';";
                    agregar.ExecuteNonQuery();

                    // Actualizar registros existentes: 'En lectura' -> 'Pendiente', resto -> 'Entregado'
                    using var actualizar = conexion.CreateCommand();
                    actualizar.CommandText = @"
                        UPDATE ControlUsuariosSala
                        SET Estado = CASE
                            WHEN HoraRecibido = 'En lectura' THEN 'Pendiente'
                            ELSE 'Entregado'
                        END
                        WHERE Estado IS NULL OR Estado = '';";
                    actualizar.ExecuteNonQuery();
                }
                catch (SqliteException)
                {
                    // Si falla ALTER TABLE (SQLite muy antiguo), reconstruir tabla completa
                    using var transaccion = conexion.BeginTransaction();
                    try
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

                        transaccion.Commit();
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Agrega campos de versiones nuevas sin perder préstamos existentes.
        /// Los préstamos antiguos permanecen sin código porque no hay forma
        /// segura de deducir qué copia física les correspondía.
        /// </summary>
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

            using var indice = conexion.CreateCommand();
            indice.CommandText = "CREATE INDEX IF NOT EXISTS IX_Prestamos_CodigoLibro ON PrestamosExternos(CodigoLibro);";
            indice.ExecuteNonQuery();
        }

        // ------------------------------------------------------------------
        //  Estructura
        // ------------------------------------------------------------------
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
                    FechaDevolucion  TEXT
                );";

            const string sqlUsuarios = @"
                CREATE TABLE IF NOT EXISTS Usuarios (
                    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nombre         TEXT NOT NULL,
                    Usuario        TEXT NOT NULL UNIQUE,
                    Contrasena     TEXT NOT NULL,
                    Rol            TEXT NOT NULL DEFAULT 'Operador'
                );";

            // Clave/valor para banderas de negocio persistentes.
            // Se usa, entre otras, para recordar que el admin vació el inventario
            // y evitar que la importación automática del CSV lo repueble al arrancar.
            const string sqlConfiguracion = @"
                CREATE TABLE IF NOT EXISTS Configuracion (
                    Clave TEXT PRIMARY KEY,
                    Valor TEXT
                );";

            using (var cmd = conexion.CreateCommand())
            {
                cmd.CommandText = sqlLibros + sqlSala + sqlPrestamos + sqlUsuarios + sqlConfiguracion;
                cmd.ExecuteNonQuery();
            }

            using var indice = conexion.CreateCommand();
            indice.CommandText = "CREATE INDEX IF NOT EXISTS IX_Libros_Titulo ON Libros(Titulo);";
            indice.ExecuteNonQuery();
        }

        // ------------------------------------------------------------------
        //  Bandera: inventario vaciado manualmente por el administrador
        // ------------------------------------------------------------------
        private const string ClaveInventarioVaciado = "InventarioVaciado";

        /// <summary>
        /// Marca (o desmarca) que el administrador vació el inventario a mano.
        /// Mientras la marca esté en '1', el arranque NO repuebla la tabla Libros
        /// desde el CSV, de modo que el vaciado persiste entre sesiones.
        /// </summary>
        public static void MarcarInventarioVaciado(bool vaciado)
        {
            using var conexion = ObtenerConexion();
            using var cmd = conexion.CreateCommand();
            cmd.CommandText = "INSERT OR REPLACE INTO Configuracion (Clave, Valor) VALUES (@clave, @valor);";
            cmd.Parameters.AddWithValue("@clave", ClaveInventarioVaciado);
            cmd.Parameters.AddWithValue("@valor", vaciado ? "1" : "0");
            cmd.ExecuteNonQuery();
        }

        /// <summary>Lee la bandera de forma atómica sobre la conexión ya abierta.</summary>
        private static bool InventarioVaciadoPorUsuario(SqliteConnection conexion)
        {
            using var cmd = conexion.CreateCommand();
            cmd.CommandText = "SELECT Valor FROM Configuracion WHERE Clave = @clave;";
            cmd.Parameters.AddWithValue("@clave", ClaveInventarioVaciado);

            object? valor = cmd.ExecuteScalar();
            return string.Equals(valor as string, "1", StringComparison.OrdinalIgnoreCase);
        }

        // ------------------------------------------------------------------
        //  Importación del catálogo CSV
        // ------------------------------------------------------------------
        private static void ImportarCatalogoDesdeCsv(SqliteConnection conexion)
        {
            // Si el administrador vació el inventario explícitamente, NO se
            // repuebla desde el CSV: el vaciado debe sobrevivir al reinicio.
            // Se restaura con la importación manual de un CSV desde el inventario.
            if (InventarioVaciadoPorUsuario(conexion)) return;

            string? rutaCsv = BuscarArchivoCsv();
            if (rutaCsv == null) return;

            var filas = LectorCsv.Leer(rutaCsv);
            if (filas.Count == 0) return;

            using var transaccion = conexion.BeginTransaction();
            try
            {
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

                transaccion.Commit();
                System.Diagnostics.Debug.WriteLine($"Catálogo CUBO: {insertados} libros insertados desde CSV.");
            }
            catch
            {
                transaccion.Rollback();
                throw;
            }
        }

        private static string? BuscarArchivoCsv()
        {
            // Busca el CSV en el directorio de salida y en las carpetas superiores
            // (solución/proyecto) para cubrir también la ejecución en desarrollo.
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (int i = 0; i < 6 && dir != null; i++, dir = dir.Parent!)
            {
                string? ruta = Directory
                    .EnumerateFiles(dir.FullName, "*.csv")
                    .FirstOrDefault(f => Path.GetFileName(f)
                        .Contains("INVENTARIO", StringComparison.OrdinalIgnoreCase))
                    ?? Directory.EnumerateFiles(dir.FullName, "*.csv").FirstOrDefault();

                if (ruta != null) return ruta;
            }
            return null;
        }
    }
}
