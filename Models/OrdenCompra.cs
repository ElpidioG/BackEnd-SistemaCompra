using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackEnd_SistemaCompra.Models
{
    public class OrdenCompra
    {
        public int Id { get; set; }

        public int idProveedor { get; set; }  // Solo guardamos el ID del proveedor



        [ForeignKey("idProveedor")]
        public virtual Proveedores? Proveedor { get; set; } // Solo para referencia

        public DateTime Fecha { get; set; } = DateTime.Now;
        public bool Estado { get; set; }
    }
}