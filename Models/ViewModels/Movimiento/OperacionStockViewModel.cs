using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Valen_SistemaStock.Models.ViewModels.Movimiento
{
    public class OperacionStockEntradaViewModel
    {
        [Display(Name = "Producto")]
        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        public int? idProducto { get; set; }
        public List<SelectListItem>? Productos { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un Deposito.")]
        [Display(Name = "Depósito")]
        public int? idDepositoOrigen { get; set; }
        public List<SelectListItem>? Depositos { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }

        [Display(Name = "Proveedor")]
        [Required(ErrorMessage = "Debe seleccionar un proveedor.")]
        public int? idProveedor { get; set; }
        public List<SelectListItem>? Proveedores { get; set; }

        [StringLength(300, ErrorMessage = "Máximo 300 caracteres")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }
    }

    public class OperacionStockSalidaViewModel
    {
        [Display(Name = "Producto")]
        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        public int? idProducto { get; set; }
        public List<SelectListItem>? Productos { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un Deposito.")]
        [Display(Name = "Depósito")]
        public int? idDepositoOrigen { get; set; }
        public List<SelectListItem>? Depositos { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }

        [Display(Name = "Cliente")]
        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        public int? idCliente { get; set; }
        public List<SelectListItem>? Clientes { get; set; }

        [StringLength(300, ErrorMessage = "Máximo 300 caracteres")]
        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }
    }

    public class TransferenciaViewModel
    {
        public int id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Display(Name = "Producto")]
        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        public int? idProducto { get; set; }
        public List<SelectListItem>? Productos { get; set; }
        public string? ProductoNombre { get; set; }

        [Required(ErrorMessage = "La cantidad es obligatoria.")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un Deposito.")]
        [Display(Name = "Depósito Origen")]
        public int? idDepositoOrigen { get; set; }
        public string? DepositoOrigenNombre { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un Deposito.")]
        [Display(Name = "Depósito Destino")]
        public int? idDepositoDestino { get; set; }
        public string? DepositoDestinoNombre { get; set; }

        public List<SelectListItem>? Depositos { get; set; }

        [StringLength(300, ErrorMessage = "Máximo 300 caracteres")]
        public string? Observaciones { get; set; }

        [Display(Name = "Stock Disponible")]
        public int StockDisponible { get; set; }
    }
}
