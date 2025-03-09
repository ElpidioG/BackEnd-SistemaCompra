using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BackEnd_SistemaCompra.Models
{
    public class Articulos
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public string Marca { get; set; }
        public int Existencia { get; set; }
        public bool Estado { get; set; } = true;
        public int IdUnidadMedidas { get; set; }
        public virtual UnidadesMedidas UnidadMedida { get; set; }
    }
}
