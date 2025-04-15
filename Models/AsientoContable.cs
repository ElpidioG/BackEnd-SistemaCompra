using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd_SistemaCompra.Models
{
    public class AsientoContable
    {
        public int Id{ get; set; }
        public int sistemaAuxiliarId { get; set; }
        public string Descripcion { get; set; }
        public int IdTipoInventario { get; set; }
        public string CuentaContable { get; set; }
        public string TipoMovimiento { get; set; }
        public DateTime FechaAsiento { get; set; }
        public decimal Monto { get; set; }
        public bool Estado { get; set; }

        public int IdOrdenCompra { get; set; }
    }

}
