namespace Product_Service.Core
{
    
    public class ProductDB : Product
    {
        public int Id { get; set; }
        

            


    }

    public class Product
    {
        public required string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public int quantity { get; set; }
    }


    public class ReservationRequest
    {
        public string Type { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
