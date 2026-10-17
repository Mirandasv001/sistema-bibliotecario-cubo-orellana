# Sistema de Gestión Bibliotecaria — CUBO Ing. Rigoberto Orellana

Sistema de escritorio integral desarrollado en **C# (.NET 10 WinForms)** para la digitalización, control de inventario y trazabilidad operativa de préstamos en el Centro Urbano de Bienestar y Oportunidades (CUBO).

---

## 📌 Resumen del Proyecto

El sistema reemplaza el control manual en bitácoras físicas por una plataforma digital centralizada, optimizando los tiempos de atención, garantizando la consistencia de los datos y permitiendo la administración eficiente de más de **3,800 ejemplares**.

---

## 🚀 Características Principales

* **Inventario General en Tiempo Real:** Catálogo de más de 3,800 libros con búsqueda y filtrado instantáneo por título, código de ejemplar y disponibilidad.
* **Flujo Seguro de Préstamos Externos:** Transferencia automática de datos desde el inventario hacia el módulo de préstamos mediante interacción directa (doble clic), previniendo errores humanos de digitación.
* **Interfaz Dinámica (UI/UX Adaptativa):** Implementación de `SplitContainer` horizontal que permite expandir o contraer la vista de datos estilo terminal, con desplazamiento vertical nativo.
* **Control de Devoluciones y Renovaciones:** Registro granular de fechas, personal a cargo y actualización automática del estado del ejemplar.
* **Módulo de Alertas de Vencimiento:** Monitoreo y control de préstamos con fecha de entrega expirada, con cálculo de días de retraso según la hora local.
* **Guía de Uso Integrada:** Documentación de procesos accesible directamente desde la barra lateral del sistema.
* **Control de Acceso:** Autenticación de usuarios con perfiles de Administrador y Operador.
* **Instalador Autónomo:** Paquete de instalación para Windows que incluye el motor .NET 10, sin descargas adicionales.

---

## 🛠️ Stack Tecnológico

* **Lenguaje:** C# (.NET 10)
* **Interfaz Gráfica:** Windows Forms (WinForms) con diseño modular basado en `UserControls`
* **Persistencia de Datos:** SQLite (`Microsoft.Data.Sqlite`) — la base se crea automáticamente en `%APPDATA%\BibliotecaCUBO`
* **Empaquetado:** `dotnet publish` (self-contained) + Inno Setup 6
* **Control de Versiones:** Git / GitHub

---

## 📸 Capturas del Sistema

| Módulo de Inventario | Módulo de Préstamos Externos |
| :---: | :---: |
| <img width="620" alt="Inventario" src="<img width="1917" height="965" alt="image" src="https://github.com/user-attachments/assets/51cb6aac-7fbe-4fc3-836d-38bd248f060c" />
" /> | <img width="620" alt="Préstamos Externos" src="https://github.com/user-attachments/assets/5aa81144-af5c-4a9a-839c-6285c3a31d6e" /> |

| Login | Control de Sala |
| :---: | :---: |
| <img width="620" alt="Login" src="docs/capturas/01-login.png" /> | <img width="620" alt="Control de Sala" src="docs/capturas/03-control-sala.png" /> |

| Alertas de Vencidos | Estadísticas |
| :---: | :---: |
| <img width="620" alt="Alertas de Vencidos" src="docs/capturas/05-alertas-vencidos.png" /> | <img width="620" alt="Estadísticas" src="docs/capturas/06-estadisticas.png" /> |

| Guía de Uso |
| :---: |
| <img width="620" alt="Guía de Uso" src="docs/capturas/07-guia-de-uso.png" /> |

> 📂 **¿Dónde van las capturas?** En la carpeta **`docs/capturas/`** de este repositorio, con estos nombres exactos: `01-login.png`, `03-control-sala.png`, `05-alertas-vencidos.png`, `06-estadisticas.png` y `07-guia-de-uso.png`. Las dos primeras (Inventario y Préstamos) ya están alojadas en GitHub.

---

## ⚙️ Instalación y Ejecución

### Opción A — Usuario final (recomendada)

1. Descargar `BibliotecaCUBO-Instalador-1.0.exe` desde la sección **Releases** de GitHub.
2. Ejecutar el instalador y seguir el asistente (crea accesos directos en el menú de inicio y, opcionalmente, en el escritorio).
3. Listo: **no requiere instalar .NET ni ningún componente adicional**, el motor viene incluido.

> 💾 Los datos se guardan en `%APPDATA%\BibliotecaCUBO\biblioteca.db` (carpeta del usuario, sin permisos de administración).

### Opción B — Desarrolladores (desde el código fuente)

**Prerrequisitos:**
* Sistema Operativo: Windows 10 / 11 (x64)
* [.NET 10 SDK](https://dotnet.microsoft.com/download) o Visual Studio 2022+ con la carga de trabajo de escritorio .NET

**Pasos:**
1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/Mirandasv001/sistema-bibliotecario-cubo-orellana.git
   ```
2. **Abrir la solución:** abrir el archivo `BibliotecaApp.slnx` en Visual Studio.
3. **Compilar y ejecutar:** presionar `F5` o seleccionar *Compilar > Compilar Solución*.
4. **Generar el instalador** (opcional): ejecutar `dotnet publish -c Release -r win-x64 --self-contained true` y luego compilar `Instalar-BibliotecaCUBO.iss` con Inno Setup 6.

**Credenciales de prueba:** el sistema crea por defecto un usuario Administrador (`AdminCubo` / `Admin123$`) y un Operador (`UserCubo` / `1234$`).

---

## 👤 Autor

**Erick Miranda** — [@Mirandasv001](https://github.com/Mirandasv001)

Estudiante de Ingeniería en Desarrollo de Software
