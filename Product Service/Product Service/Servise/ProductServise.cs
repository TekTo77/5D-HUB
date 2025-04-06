using Product_Service.Core;
using Product_Service.SQL.Repository;

namespace Product_Service.Servise
{
    public class ProductServise
    {
        public async Task<ProductDB> GetProductDBAsync(int id) => await Repos.GetProductAsync(id);

        public async Task<bool> CreateProductAsync(Product product) => await Repos.CreateProduct(product);

        public async Task<bool> changeQuntityProductAsync(int id, int quantity) => await Repos.changeQuantity( id, quantity);

        public async Task<bool> Reservation(int id, int quntyti)=> await Repos.Reservation( id, quntyti);


        public async Task<bool> CheckProductAvailability(int id, int quantity)
        {
          int result =  await Repos.CheckQuntuty(id);
            if (result >= quantity)
            {
                return true;
            }
            return false;
        }




    }
}
