using System;
using System.Collections.Generic;

namespace SistemaVuelos.Models
{
    // Elemento que asocia un vertice del grafo con su costo acumulado actual
    public class ElementoPrioridad : IComparable<ElementoPrioridad>
    {
        // Codigo identificador del aeropuerto
        public string Vertice { get; set; }

        // Distancia o costo acumulado desde el origen
        public double Costo { get; set; }

        // Constructor que inicializa los valores del nodo
        public ElementoPrioridad(string vertice, double costo)
        {
            Vertice = vertice;
            Costo = costo;
        }

        // Metodo para ordenar de forma ascendente segun el costo monetario
        public int CompareTo(ElementoPrioridad otro)
        {
            if (otro == null)
            {
                return 1;
            }
            return Costo.CompareTo(otro.Costo);
        }
    }

    // Estructura de datos monticulo binario de minimos para garantizar la extraccion mas rapida
    public class MonticuloMinimo
    {
        // Arreglo dinamico que almacena los nodos del arbol binario completo
        private readonly List<ElementoPrioridad> _datos;

        // Propiedad que retorna el total de elementos en la cola
        public int Cantidad
        {
            get { return _datos.Count; }
        }

        // Constructor del monticulo
        public MonticuloMinimo()
        {
            _datos = new List<ElementoPrioridad>();
        }

        // Inserta un vertice con su costo y lo acomoda hacia la superficie
        public void Encolar(string vertice, double costo)
        {
            ElementoPrioridad nuevo = new ElementoPrioridad(vertice, costo);
            _datos.Add(nuevo);
            Flotar(_datos.Count - 1);
        }

        // Extrae el vertice de la raiz que posee el menor costo acumulado
        public ElementoPrioridad Desencolar()
        {
            if (_datos.Count == 0)
            {
                throw new InvalidOperationException("El monticulo de minimos esta vacio.");
            }

            ElementoPrioridad raiz = _datos[0];
            int ultimoIndice = _datos.Count - 1;
            _datos[0] = _datos[ultimoIndice];
            _datos.RemoveAt(ultimoIndice);

            if (_datos.Count > 0)
            {
                Hundir(0);
            }

            return raiz;
        }

        // Desplaza un nodo hacia arriba mientras su costo sea inferior al de su padre
        private void Flotar(int indice)
        {
            while (indice > 0)
            {
                int padre = (indice - 1) / 2;
                if (_datos[indice].CompareTo(_datos[padre]) < 0)
                {
                    Intercambiar(indice, padre);
                    indice = padre;
                }
                else
                {
                    break;
                }
            }
        }

        // Desplaza un nodo hacia abajo mientras alguno de sus hijos tenga menor costo
        private void Hundir(int indice)
        {
            int limite = _datos.Count;
            while (true)
            {
                int menor = indice;
                int hijoIzquierdo = 2 * indice + 1;
                int hijoDerecho = 2 * indice + 2;

                if (hijoIzquierdo < limite && _datos[hijoIzquierdo].CompareTo(_datos[menor]) < 0)
                {
                    menor = hijoIzquierdo;
                }

                if (hijoDerecho < limite && _datos[hijoDerecho].CompareTo(_datos[menor]) < 0)
                {
                    menor = hijoDerecho;
                }

                if (menor != indice)
                {
                    Intercambiar(indice, menor);
                    indice = menor;
                }
                else
                {
                    break;
                }
            }
        }

        // Realiza el intercambio fisico de dos posiciones dentro de la lista
        private void Intercambiar(int a, int b)
        {
            ElementoPrioridad temp = _datos[a];
            _datos[a] = _datos[b];
            _datos[b] = temp;
        }
    }
}
