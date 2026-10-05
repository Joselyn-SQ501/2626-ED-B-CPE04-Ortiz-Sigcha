using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using SistemaVuelos.Models;

namespace SistemaVuelos.Services
{
    // Servicio central que administra el grafo dirigido de vuelos mediante listas de adyacencia
    public class GrafoVuelosServicio
    {
        // Lista de adyacencia donde la clave es el codigo del aeropuerto y el valor la lista de salidas directas
        private readonly Dictionary<string, List<Vuelo>> _adyacencia;

        // Conjunto para registrar todos los aeropuertos sin repeticiones
        private readonly HashSet<string> _aeropuertos;

        // Propiedad publica para consultar el total de aeropuertos
        public int TotalAeropuertos
        {
            get { return _aeropuertos.Count; }
        }

        // Constructor que inicializa las estructuras en memoria principal
        public GrafoVuelosServicio()
        {
            _adyacencia = new Dictionary<string, List<Vuelo>>(StringComparer.OrdinalIgnoreCase);
            _aeropuertos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        // Registra una conexion aerea dirigida en la lista de adyacencia
        public void AgregarVuelo(string origen, string destino, double precio, string aerolinea)
        {
            origen = (origen != null) ? origen.Trim().ToUpper() : string.Empty;
            destino = (destino != null) ? destino.Trim().ToUpper() : string.Empty;

            if (string.IsNullOrEmpty(origen) || string.IsNullOrEmpty(destino))
            {
                return;
            }

            // Agrega el vertice de origen si todavia no figuraba en la lista
            if (!_adyacencia.ContainsKey(origen))
            {
                _adyacencia[origen] = new List<Vuelo>();
            }

            // Agrega el vertice de destino si todavia no figuraba en la lista
            if (!_adyacencia.ContainsKey(destino))
            {
                _adyacencia[destino] = new List<Vuelo>();
            }

            // Inserta la arista con la informacion del vuelo
            Vuelo nuevoVuelo = new Vuelo(origen, destino, precio, aerolinea);
            _adyacencia[origen].Add(nuevoVuelo);

            // Registra ambos aeropuertos en el conjunto global
            _aeropuertos.Add(origen);
            _aeropuertos.Add(destino);
        }

        // Lee el archivo de texto plano linea por linea separando los datos por comas
        public bool CargarDesdeArchivo(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
            {
                return false;
            }

            _adyacencia.Clear();
            _aeropuertos.Clear();

            try
            {
                string[] lineas = File.ReadAllLines(rutaArchivo);
                foreach (string lineaRaw in lineas)
                {
                    string linea = lineaRaw.Trim();

                    // Salta registros vacios o lineas marcadas como comentarios
                    if (string.IsNullOrEmpty(linea) || linea.StartsWith("#"))
                    {
                        continue;
                    }

                    string[] partes = linea.Split(',');
                    if (partes.Length >= 4)
                    {
                        string orig = partes[0].Trim();
                        string dest = partes[1].Trim();
                        double precio;
                        
                        // Convierte el valor numerico del pasaje a formato decimal
                        if (!double.TryParse(partes[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out precio))
                        {
                            double.TryParse(partes[2].Trim(), out precio);
                        }

                        string aero = partes[3].Trim();
                        AgregarVuelo(orig, dest, precio, aero);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Retorna todos los aeropuertos registrados en orden alfabetico
        public List<string> ObtenerAeropuertos()
        {
            List<string> lista = new List<string>(_aeropuertos);
            lista.Sort();
            return lista;
        }

        // Obtiene las conexiones salientes de un aeropuerto determinado
        public List<Vuelo> ObtenerVuelosSalientes(string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                return new List<Vuelo>();
            }

            codigo = codigo.Trim().ToUpper();
            if (_adyacencia.ContainsKey(codigo))
            {
                return new List<Vuelo>(_adyacencia[codigo]);
            }
            return new List<Vuelo>();
        }

        // Retorna el total de conexiones directas registradas en el sistema
        public int ObtenerTotalConexiones()
        {
            int total = 0;
            foreach (var kvp in _adyacencia)
            {
                total += kvp.Value.Count;
            }
            return total;
        }

        // Algoritmo de Dijkstra respaldado en monticulo binario para hallar la tarifa minima
        public RutaResultado CalcularVueloMasBarato(string origen, string destino)
        {
            RutaResultado resultado = new RutaResultado();
            origen = (origen != null) ? origen.Trim().ToUpper() : string.Empty;
            destino = (destino != null) ? destino.Trim().ToUpper() : string.Empty;

            resultado.Origen = origen;
            resultado.Destino = destino;

            // Iniciamos el cronometro de alta resolucion para medir el tiempo exacto
            Stopwatch reloj = Stopwatch.StartNew();

            // Validamos que ambos aeropuertos formen parte de la red
            if (string.IsNullOrEmpty(origen) || string.IsNullOrEmpty(destino) ||
                !_aeropuertos.Contains(origen) || !_aeropuertos.Contains(destino))
            {
                reloj.Stop();
                resultado.TiempoEjecucionMs = reloj.Elapsed.TotalMilliseconds;
                resultado.ExisteRuta = false;
                return resultado;
            }

            // Estructura de cola de prioridad para extraer siempre el vertice con menor costo
            MonticuloMinimo cola = new MonticuloMinimo();

            // Diccionario con las distancias minimas conocidas hacia cada terminal
            Dictionary<string, double> distancias = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

            // Inicializamos las distancias en infinito para cada aeropuerto
            foreach (string aero in _aeropuertos)
            {
                distancias[aero] = double.PositiveInfinity;
            }

            // El origen inicia con un costo de cero dolares
            distancias[origen] = 0.0;

            // Diccionarios auxiliares para reconstruir el camino y guardar el boleto tomado
            Dictionary<string, string> predecesores = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, Vuelo> vuelosUsados = new Dictionary<string, Vuelo>(StringComparer.OrdinalIgnoreCase);

            // Conjunto de aeropuertos que ya fueron evaluados de forma definitiva
            HashSet<string> visitados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Encolamos el nodo inicial en la cola de prioridad
            cola.Encolar(origen, 0.0);

            // Bucle voraz mientras existan nodos pendientes por explorar
            while (cola.Cantidad > 0)
            {
                // Extraemos el aeropuerto mas prometedor con menor costo acumulado
                ElementoPrioridad actual = cola.Desencolar();
                string nodoActual = actual.Vertice;

                // Si ya fue evaluado continuamos con el siguiente vertice
                if (visitados.Contains(nodoActual))
                {
                    continue;
                }

                // Marcamos el nodo actual como visitado
                visitados.Add(nodoActual);

                // Si alcanzamos el destino interrumpimos la exploracion de aristas
                if (nodoActual.Equals(destino, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                if (!_adyacencia.ContainsKey(nodoActual))
                {
                    continue;
                }

                // Evaluamos cada vuelo directo saliente desde la terminal actual
                foreach (Vuelo vuelo in _adyacencia[nodoActual])
                {
                    string vecino = vuelo.Destino;
                    if (visitados.Contains(vecino))
                    {
                        continue;
                    }

                    // Calculamos el costo provisional sumando la tarifa del tramo
                    double nuevoCosto = distancias[nodoActual] + vuelo.Precio;

                    // Proceso de relajacion si encontramos un itinerario mas economico
                    if (nuevoCosto < distancias[vecino])
                    {
                        distancias[vecino] = nuevoCosto;
                        predecesores[vecino] = nodoActual;
                        vuelosUsados[vecino] = vuelo;
                        cola.Encolar(vecino, nuevoCosto);
                    }
                }
            }

            reloj.Stop();
            resultado.TiempoEjecucionMs = reloj.Elapsed.TotalMilliseconds;

            // Verificamos si logramos llegar al destino
            if (double.IsPositiveInfinity(distancias[destino]))
            {
                resultado.ExisteRuta = false;
                return resultado;
            }

            resultado.ExisteRuta = true;
            resultado.CostoTotal = distancias[destino];

            // Reconstruccion retrospectiva del itinerario optimo desde el destino hacia el origen
            List<string> caminoInvertido = new List<string>();
            List<Vuelo> vuelosInvertidos = new List<Vuelo>();

            string paso = destino;
            while (predecesores.ContainsKey(paso))
            {
                caminoInvertido.Add(paso);
                vuelosInvertidos.Add(vuelosUsados[paso]);
                paso = predecesores[paso];
            }
            caminoInvertido.Add(origen);

            // Invertimos la secuencia para mostrar el itinerario en orden cronologico de viaje
            caminoInvertido.Reverse();
            vuelosInvertidos.Reverse();

            resultado.Nodos = caminoInvertido;
            resultado.Vuelos = vuelosInvertidos;

            return resultado;
        }

        // Realiza multiples busquedas automaticas para medir el desempeno promedio
        public void EjecutarBenchmark(int repeticiones)
        {
            if (_aeropuertos.Count < 2)
            {
                Console.WriteLine("Se requieren al menos dos aeropuertos para la prueba.");
                return;
            }

            List<string> lista = ObtenerAeropuertos();
            string orig = lista[0];
            string dest = lista[lista.Count - 1];

            List<double> tiempos = new List<double>();
            for (int i = 0; i < repeticiones; i++)
            {
                RutaResultado r = CalcularVueloMasBarato(orig, dest);
                tiempos.Add(r.TiempoEjecucionMs);
            }

            double suma = 0;
            double minimo = double.MaxValue;
            double maximo = double.MinValue;

            foreach (double t in tiempos)
            {
                suma += t;
                if (t < minimo) minimo = t;
                if (t > maximo) maximo = t;
            }

            double promedio = suma / repeticiones;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n==================================================================");
            Console.WriteLine("          BENCHMARK DE RENDIMIENTO - ALGORITMO DIJKSTRA");
            Console.WriteLine("==================================================================");
            Console.ResetColor();
            Console.WriteLine("Ruta de prueba evaluada: {0} hacia {1}", orig, dest);
            Console.WriteLine("Numero de ejecuciones consecutivas: {0}", repeticiones);
            Console.WriteLine("Tiempo promedio por consulta: {0:F5} milisegundos", promedio);
            Console.WriteLine("Tiempo minimo registrado: {0:F5} milisegundos", minimo);
            Console.WriteLine("Tiempo maximo registrado: {0:F5} milisegundos", maximo);
            Console.WriteLine("Complejidad teorica del algoritmo: O lineal logaritmico");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("==================================================================\n");
            Console.ResetColor();
        }
    }
}
