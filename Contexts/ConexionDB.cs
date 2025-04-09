using BackEnd_SistemaCompra.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd_SistemaCompra.Contexts
{
    public class ConexionDB: DbContext
    {
        public ConexionDB(DbContextOptions<ConexionDB> options): base(options) {
             
        }

        public DbSet<Departamentos> Tbl_Departamentos { get; set;}
        public DbSet<UnidadesMedidas> Tbl_UnidadesMedidas { get; set; }
        public DbSet<Proveedores> Tbl_Proveedores { get; set; }
        public DbSet<Articulos> Tbl_Articulos { get; set; }
        public DbSet<OrdenCompra> Tbl_OrdenCompra { get; set; }
        public DbSet<DetalleOrdenCompra> Tbl_Detalle_OrdenCompra { get; set; }
        public DbSet<AsientoContable> Tbl_AsientosContables { get; set; }
    }
}
