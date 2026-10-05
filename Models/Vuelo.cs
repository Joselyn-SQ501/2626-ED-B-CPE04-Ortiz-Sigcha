using System;

namespace SistemaVuelos.Models
{
    // Modelo que representa cada conexion aerea directa entre dos ciudades
    public class Vuelo
    {
        // Aeropuerto de salida con su codigo internacional
        public string Origen { get; set; }

        // Aeropuerto de llegada con su codigo internacional
        public string Destino { get; set; }

        // Tarifa monetaria del billete expresada en dolares americanos
        public double Precio { get; set; }

        // Nombre comercial de la empresa que opera el trayecto
        public string Aerolinea { get; set; }

        // Constructor que asigna las propiedades de la conexion
        public Vuelo(string origen, string destino, double precio, string aerolinea)
        {
            Origen = (origen != null) ? origen.Trim().ToUpper() : string.Empty;
            Destino = (destino != null) ? destino.Trim().ToUpper() : string.Empty;
            Precio = precio;
            Aerolinea = (aerolinea != null) ? aerolinea.Trim() : string.Empty;
        }

        // Formateo legible de la informacion del trayecto
        public override string ToString()
        {
            return string.Format("{0} -> {1} | Tarifa: ${2:F2} USD | Aerolinea: {3}", Origen, Destino, Precio, Aerolinea);
        }
    }
}
