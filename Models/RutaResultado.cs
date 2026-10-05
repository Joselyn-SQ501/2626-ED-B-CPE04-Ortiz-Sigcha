using System.Collections.Generic;

namespace SistemaVuelos.Models
{
    // Estructura que almacena los resultados calculados por el algoritmo de Dijkstra
    public class RutaResultado
    {
        // Codigo del aeropuerto donde inicia el itinerario
        public string Origen { get; set; }

        // Codigo del aeropuerto donde finaliza el viaje
        public string Destino { get; set; }

        // Costo monetario minimo acumulado sumando todos los tramos
        public double CostoTotal { get; set; }

        // Secuencia ordenada de todos los aeropuertos visitados durante el vuelo
        public List<string> Nodos { get; set; }

        // Coleccion con el detalle especifico de cada vuelo tomado
        public List<Vuelo> Vuelos { get; set; }

        // Tiempo total de calculo medido en milisegundos con alta precision
        public double TiempoEjecucionMs { get; set; }

        // Indica si fue posible conectar ambos puntos mediante la red
        public bool ExisteRuta { get; set; }

        // Cantidad de paradas intermedias que el pasajero debe realizar
        public int NumeroEscalas
        {
            get
            {
                if (Nodos == null || Nodos.Count <= 2)
                {
                    return 0;
                }
                return Nodos.Count - 2;
            }
        }

        // Constructor que inicializa las listas internas
        public RutaResultado()
        {
            Nodos = new List<string>();
            Vuelos = new List<Vuelo>();
            ExisteRuta = false;
            CostoTotal = double.PositiveInfinity;
        }
    }
}
