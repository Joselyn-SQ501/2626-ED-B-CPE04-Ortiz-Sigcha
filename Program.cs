using System;
using System.Collections.Generic;
using System.IO;
using SistemaVuelos.Models;
using SistemaVuelos.Services;

namespace SistemaVuelos
{
    // Aplicacion de consola para gestion de vuelos y calculo de tarifas optimas con grafos
    // Autores: Marcelo Ortiz y Joselyn Sigcha
    // Universidad Estatal Amazonica - Estructura de Datos
    class Program
    {
        // Instancia unica del servicio de vuelos
        private static GrafoVuelosServicio _servicio;
        private static string _rutaArchivoDb = "vuelos.txt";

        // Limpia la terminal si el entorno lo permite
        static void LimpiarPantalla()
        {
            try
            {
                if (!Console.IsInputRedirected && !Console.IsOutputRedirected)
                {
                    Console.Clear();
                }
            }
            catch { }
        }

        // Realiza una pausa interactiva esperando una pulsacion de tecla
        static void Pausa()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\nPresione cualquier tecla para regresar al menu principal...");
                Console.ResetColor();
                if (!Console.IsInputRedirected)
                {
                    Console.ReadKey();
                }
                else
                {
                    Console.ReadLine();
                }
            }
            catch { }
        }

        // Punto de entrada principal de la aplicacion
        static void Main(string[] args)
        {
            Console.Title = "Sistema de Vuelos Baratos - Estructura de Datos UEA";
            _servicio = new GrafoVuelosServicio();

            // Intento de carga inicial del archivo plano
            if (!File.Exists(_rutaArchivoDb))
            {
                string rutaAlternativa = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vuelos.txt");
                if (File.Exists(rutaAlternativa))
                {
                    _rutaArchivoDb = rutaAlternativa;
                }
            }

            if (_servicio.CargarDesdeArchivo(_rutaArchivoDb))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("-> Base de datos '{0}' cargada con exito. Total de aeropuertos: {1}.", _rutaArchivoDb, _servicio.TotalAeropuertos);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Aviso: No se pudo cargar '{0}'. Podra cargarlo desde el menu.", _rutaArchivoDb);
                Console.ResetColor();
            }

            bool salir = false;
            while (!salir)
            {
                LimpiarPantalla();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("======================================================================");
                Console.WriteLine("   UNIVERSIDAD ESTATAL AMAZONICA - SISTEMA DE VUELOS BARATOS (CPE 04)");
                Console.WriteLine("   Autores: Marcelo Ortiz y Joselyn Sigcha | Tutor: Ing. Santiago Nogales");
                Console.WriteLine("======================================================================");
                Console.ResetColor();
                Console.WriteLine("1. Ver reporte general de la red de vuelos y aeropuertos");
                Console.WriteLine("2. Consultar vuelos salientes de un aeropuerto");
                Console.WriteLine("3. Buscar la ruta de vuelo mas economica con Dijkstra");
                Console.WriteLine("4. Ejecutar prueba de rendimiento y complejidad del algoritmo");
                Console.WriteLine("5. Recargar base de datos de vuelos desde archivo plano");
                Console.WriteLine("6. Salir");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("======================================================================");
                Console.ResetColor();
                Console.Write("Seleccione una opcion del menu: ");

                string entrada = Console.ReadLine();
                if (entrada != null)
                {
                    entrada = entrada.Trim();
                }

                switch (entrada)
                {
                    case "1":
                        MostrarReporteGeneral();
                        Pausa();
                        break;

                    case "2":
                        ConsultarAeropuerto();
                        Pausa();
                        break;

                    case "3":
                        BuscarRutaEconomica();
                        Pausa();
                        break;

                    case "4":
                        EjecutarBenchmark();
                        Pausa();
                        break;

                    case "5":
                        RecargarBaseDatos();
                        Pausa();
                        break;

                    case "6":
                        salir = true;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\nGracias por utilizar el sistema de optimizacion de vuelos. Hasta pronto.");
                        Console.ResetColor();
                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nOpcion no valida. Seleccione un numero del 1 al 6.");
                        Console.ResetColor();
                        Pausa();
                        break;
                }
            }
        }

        // Muestra en consola la totalidad de aeropuertos y sus vuelos directos
        static void MostrarReporteGeneral()
        {
            LimpiarPantalla();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("======================================================================");
            Console.WriteLine("          REPORTE GENERAL DE LA RED DE VUELOS COMERCIALES");
            Console.WriteLine("======================================================================");
            Console.ResetColor();

            List<string> aeropuertos = _servicio.ObtenerAeropuertos();
            Console.WriteLine("Total de aeropuertos registrados: {0}", aeropuertos.Count);
            Console.WriteLine("Total de conexiones directas activas: {0}", _servicio.ObtenerTotalConexiones());
            Console.WriteLine("----------------------------------------------------------------------");

            foreach (string aero in aeropuertos)
            {
                List<Vuelo> vuelos = _servicio.ObtenerVuelosSalientes(aero);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[Aeropuerto {0}] - Conexiones salientes: {1}", aero, vuelos.Count);
                Console.ResetColor();

                if (vuelos.Count == 0)
                {
                    Console.WriteLine("   └──> Sin conexiones salientes programadas.");
                }
                else
                {
                    foreach (Vuelo v in vuelos)
                    {
                        Console.WriteLine("   └──> Destino: {0,-5} | Tarifa: ${1,7:F2} USD | Aerolinea: {2}", v.Destino, v.Precio, v.Aerolinea);
                    }
                }
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n======================================================================");
            Console.ResetColor();
        }

        // Permite consultar los vuelos salientes desde una terminal concreta
        static void ConsultarAeropuerto()
        {
            LimpiarPantalla();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--- CONSULTA DE SALIDAS POR TERMINAL AEREA ---");
            Console.ResetColor();
            Console.Write("Ingrese el codigo del aeropuerto ej UIO GYE PTY: ");
            string codigo = Console.ReadLine();
            if (codigo != null)
            {
                codigo = codigo.Trim().ToUpper();
            }

            List<Vuelo> vuelos = _servicio.ObtenerVuelosSalientes(codigo);
            if (vuelos.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nNo se encontraron salidas registradas para el aeropuerto '{0}'.", codigo);
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nConexiones salientes directas desde {0}:", codigo);
            Console.ResetColor();
            foreach (Vuelo v in vuelos)
            {
                Console.WriteLine("-> Hacia {0} | Precio: ${1:F2} USD | Aerolinea: {2}", v.Destino, v.Precio, v.Aerolinea);
            }
        }

        // Ejecuta la busqueda voraz con Dijkstra y desglosa el itinerario economico
        static void BuscarRutaEconomica()
        {
            LimpiarPantalla();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("======================================================================");
            Console.WriteLine("       BUSQUEDA DE LA RUTA MAS ECONOMICA - ALGORITMO DE DIJKSTRA");
            Console.WriteLine("======================================================================");
            Console.ResetColor();
            Console.Write("Ingrese el codigo del aeropuerto de origen ej UIO: ");
            string origen = Console.ReadLine();
            if (origen != null)
            {
                origen = origen.Trim().ToUpper();
            }

            Console.Write("Ingrese el codigo del aeropuerto de destino ej MIA: ");
            string destino = Console.ReadLine();
            if (destino != null)
            {
                destino = destino.Trim().ToUpper();
            }

            RutaResultado resultado = _servicio.CalcularVueloMasBarato(origen, destino);

            Console.WriteLine("\n----------------------------------------------------------------------");
            if (resultado.ExisteRuta)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("¡Ruta economica calculada exitosamente!");
                Console.ResetColor();
                Console.WriteLine("Itinerario de viaje: {0}", string.Join(" -> ", resultado.Nodos.ToArray()));
                Console.WriteLine("Numero de escalas tecnicas: {0}", resultado.NumeroEscalas);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Tarifa total acumulada: ${0:F2} USD", resultado.CostoTotal);
                Console.ResetColor();
                Console.WriteLine("Tiempo de computo del algoritmo: {0:F4} ms", resultado.TiempoEjecucionMs);

                Console.WriteLine("\nDesglose pormenorizado de tramos:");
                for (int i = 0; i < resultado.Vuelos.Count; i++)
                {
                    Vuelo v = resultado.Vuelos[i];
                    Console.WriteLine("   Tramo {0}: {1} -> {2} | Tarifa: ${3:F2} USD | {4}", i + 1, v.Origen, v.Destino, v.Precio, v.Aerolinea);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No existe ninguna conexion aerea viable entre '{0}' y '{1}'.", origen, destino);
                Console.ResetColor();
            }
            Console.WriteLine("----------------------------------------------------------------------");
        }

        // Ejecuta el benchmark de velocidad del algoritmo
        static void EjecutarBenchmark()
        {
            LimpiarPantalla();
            Console.WriteLine("Iniciando medicion de rendimiento del algoritmo con 1000 iteraciones...");
            _servicio.EjecutarBenchmark(1000);
        }

        // Permite recargar el archivo plano de vuelos
        static void RecargarBaseDatos()
        {
            LimpiarPantalla();
            Console.Write("Ingrese el nombre del archivo de vuelos o Enter para usar '{0}': ", _rutaArchivoDb);
            string archivo = Console.ReadLine();
            if (archivo != null)
            {
                archivo = archivo.Trim();
            }
            if (string.IsNullOrEmpty(archivo))
            {
                archivo = _rutaArchivoDb;
            }

            if (_servicio.CargarDesdeArchivo(archivo))
            {
                _rutaArchivoDb = archivo;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n-> Base de datos '{0}' recargada exitosamente.", archivo);
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nError: No se pudo leer el archivo '{0}'. Verifique que exista.", archivo);
                Console.ResetColor();
            }
        }
    }
}
