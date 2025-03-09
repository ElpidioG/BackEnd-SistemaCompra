using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackEnd_SistemaCompra.Models
{
    public class OrdenCompra
    {
        public int Id { get; set; }
        public int IdProveedor { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;
        public bool Estado { get; set; }
        public virtual Proveedores Proveedor { get; set; }
        public virtual ICollection<DetalleOrdenCompra> DetallesOrden { get; set; }

    }
}
