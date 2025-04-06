using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;

namespace Order_Service.Core
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public List<ProductInOrder> Products { get; set; }
    }

    public class UserId
    {
        public int UserID { get; set; }
    }

    public class ProductInOrder
    {
        
        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("product_name")]
        public string ProductName { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
        
    }

    public class CreateOrder : UserId
    {

        public required List<CreateProductinorder> createProductinorders { get; set; }

    }
    public class CreateProductinorder
    {

        public int productid { get; set; }

        public int quantity { get; set; }
    }





}
