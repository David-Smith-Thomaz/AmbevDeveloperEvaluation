using Ambev.DeveloperEvaluation.WebApi.Features.Sales.AddItemToSale;
using System.ComponentModel.DataAnnotations;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale
{
    public class CreateSaleRequest
    {
        [Required(ErrorMessage = "Sale number is required.")]
        [StringLength(50, ErrorMessage = "Sale number cannot exceed 50 characters.")]
        public string SaleNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sale date is required.")]
        public DateTime SaleDate { get; set; }

        [Required(ErrorMessage = "Customer ID is required.")]
        public string CustomerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Customer name is required.")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branch ID is required.")]
        public string BranchId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Branch name is required.")]
        public string BranchName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sale items are required.")]
        [MinLength(1, ErrorMessage = "At least one sale item is required.")]
        public List<AddItemToSaleRequest> Items { get; set; } = new List<AddItemToSaleRequest>();
    }
}