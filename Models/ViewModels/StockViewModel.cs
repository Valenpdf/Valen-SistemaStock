namespace Valen_SistemaStock.Models.ViewModels
{
    public class StockViewModel
    {
        public int id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int StockTotal { get; set; }
        public List<StockDepositoViewModel> Stocks { get; set; } = new();
    }

    public class StockDepositoViewModel
    {
        public string DepositoNombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }
}
