using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using WCF_Apl_Dis;

namespace APP_MAUI_Apl_Dis_2025_II.Core
{
    public class AlertManager
    {
        private static AlertManager _instancia;
        public static AlertManager Instancia => _instancia ??= new AlertManager();

        public ObservableCollection<Cls_Alerta> Alertas { get; private set; }
            = new ObservableCollection<Cls_Alerta>();

        // 1. LISTA NEGRA: Aquí guardamos los IDs o Mensajes de las alertas cerradas
        private HashSet<string> _alertasIgnoradas = new HashSet<string>();

        public event Action<List<Cls_Alerta>> OnAlertasActualizadas;

        private AlertManager() { }

        /// <summary>
        /// Recibe las alertas del servidor, filtra las ignoradas y actualiza la UI
        /// </summary>
        public void EstablecerAlertas(List<Cls_Alerta> nuevasAlertas)
        {
            // 1. Filtrar las ignoradas (igual que antes)
            var alertasFiltradas = nuevasAlertas
                .Where(x => !_alertasIgnoradas.Contains(x.Mensaje))
                .ToList();

            // --- NUEVA LÓGICA DE OPTIMIZACIÓN ---

            // 2. Verificar si la lista nueva es IDÉNTICA a la que ya tenemos mostrada.
            // Comparamos cantidad y contenido (Mensaje y Detalle)
            bool sonIdenticas = Alertas.Count == alertasFiltradas.Count &&
                                !Alertas.Where((alertaActual, index) =>
                                {
                                    var alertaNueva = alertasFiltradas[index];
                                    return alertaActual.Mensaje != alertaNueva.Mensaje ||
                                           alertaActual.Detalle != alertaNueva.Detalle;
                                }).Any();

            // 3. SI SON IGUALES, NO HACEMOS NADA (Salimos del método)
            if (sonIdenticas)
            {
                return;
            }
            // ------------------------------------

            // 4. Solo si hay cambios, actualizamos la UI
            Alertas.Clear();
            foreach (var alerta in alertasFiltradas)
            {
                Alertas.Add(alerta);
            }

            OnAlertasActualizadas?.Invoke(alertasFiltradas);
        }

        /// <summary>
        /// Método para cerrar una alerta permanentemente en esta sesión
        /// </summary>
        public void CerrarAlerta(Cls_Alerta alerta)
        {
            if (alerta == null) return;

            // 1. Agregar a lista negra (Ya lo tienes)
            if (!_alertasIgnoradas.Contains(alerta.Mensaje))
            {
                _alertasIgnoradas.Add(alerta.Mensaje);
            }

            // 2. Remover de la UI (Ya lo tienes)
            if (Alertas.Contains(alerta))
            {
                Alertas.Remove(alerta);
            }

            // 3. ¡NUEVO! Notificar a todos (incluida la campanita) que la lista cambió
            // Esto hará que el Encabezado revise si quedan alertas (count > 0)
            OnAlertasActualizadas?.Invoke(Alertas.ToList());
        }

        public bool TieneAlertas => Alertas.Count > 0;
    }
}