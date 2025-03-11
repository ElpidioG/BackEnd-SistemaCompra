using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackEnd_SistemaCompra.Models
{
    public class DetalleOrdenCompra
    {
        public int Id { get; set; }
        public int IdOrdenCompra { get; set; }
        [ForeignKey("IdOrdenCompra")]
        public virtual OrdenCompra? OrdenCompra { get; set; }
        public int IdArticulo { get; set; }
        [ForeignKey("IdArticulo")]
        public virtual Articulos? Articulo { get; set; }
        public int Cantidad { get; set; }
        public int IdUnidadMedida { get; set; }
        [ForeignKey("IdUnidadMedida")]
        public virtual UnidadesMedidas? UnidadMedida { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal CostoTotal { get; set; }



    }
}