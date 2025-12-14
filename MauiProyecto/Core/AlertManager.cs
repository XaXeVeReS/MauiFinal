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
            // 2. FILTRADO: Solo aceptamos alertas que NO estén en la lista de ignoradas
            // NOTA: Si tu Cls_Alerta tiene un 'Id', usa 'x.Id'. Si no, usa 'x.Mensaje'.
            var alertasFiltradas = nuevasAlertas
                .Where(x => !_alertasIgnoradas.Contains(x.Mensaje))
                .ToList();

            // Actualizamos la colección observable
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

            // 3. AGREGAR A LISTA NEGRA: Guardamos el identificador
            if (!_alertasIgnoradas.Contains(alerta.Mensaje))
            {
                _alertasIgnoradas.Add(alerta.Mensaje);
            }

            // 4. REMOVER DE LA UI: La quitamos visualmente
            if (Alertas.Contains(alerta))
            {
                Alertas.Remove(alerta);
            }
        }

        public bool TieneAlertas => Alertas.Count > 0;
    }
}