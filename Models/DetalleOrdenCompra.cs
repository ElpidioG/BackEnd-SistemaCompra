using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackEnd_SistemaCompra.Models
{
    public class DetalleOrdenCompra
    {
        public int Id { get; set; }
        public int IdOrdenCompra { get; set; }
        public int IdArticulo { get; set; }
        public int Cantidad { get; set; }
        public int IdUnidadMedida { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal CostoTotal { get; set; }
        public virtual OrdenCompra OrdenCompra { get; set; }
        public virtual Articulos Articulo { get; set; }
        public virtual UnidadesMedidas UnidadMedida { get; set; }
    }
}
