namespace kithub.api.Repositories
{
    public class ShoppingCartRepository : IShoppingCartRepository
    {
        private readonly KithubDbContext kesarjotShopDbContext;

        public ShoppingCartRepository(KithubDbContext kesarjotShopDbContext)
        {
            this.kesarjotShopDbContext = kesarjotShopDbContext;
        }

        private async Task<bool> CartItemExists(int cartId, int productId)
        {
            return await this.kesarjotShopDbContext.CartItems.AnyAsync(c => c.CartId == cartId &&
                                                                     c.ProductId == productId);

        }
        public async Task<CartItem> AddItem(CartItemToAddDto cartItemToAddDto)
        {
            if (await CartItemExists(cartItemToAddDto.CartId, cartItemToAddDto.ProductId) == false)
            {
                var item = await (from product in this.kesarjotShopDbContext.Products
                                  where product.Id == cartItemToAddDto.ProductId
                                  select new CartItem
                                  {
                                      CartId = cartItemToAddDto.CartId,
                                      ProductId = product.Id,
                                      Qty = cartItemToAddDto.Qty
                                  }).SingleOrDefaultAsync();

                if (item != null)
                {
                    var result = await this.kesarjotShopDbContext.CartItems.AddAsync(item);
                    await this.kesarjotShopDbContext.SaveChangesAsync();
                    return result.Entity;
                }
            }

            return null;

        }

        public async Task<CartItem> DeleteItem(int id)
        {
            var item = await this.kesarjotShopDbContext.CartItems.FindAsync(id);

            if (item != null)
            {
                this.kesarjotShopDbContext.CartItems.Remove(item);
                await this.kesarjotShopDbContext.SaveChangesAsync();
            }

            return item;

        }

        public async Task<CartItem> GetItem(int id)
        {
            return await (from cart in this.kesarjotShopDbContext.Carts
                          join cartItem in this.kesarjotShopDbContext.CartItems
                          on cart.Id equals cartItem.CartId
                          where cartItem.Id == id
                          select new CartItem
                          {
                              Id = cartItem.Id,
                              ProductId = cartItem.ProductId,
                              Qty = cartItem.Qty,
                              CartId = cartItem.CartId
                          }).SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<CartItem>> GetItems(string userId)
        {
            return await (from cart in this.kesarjotShopDbContext.Carts
                          join cartItem in this.kesarjotShopDbContext.CartItems
                          on cart.Id equals cartItem.CartId
                          where cart.UserId == userId
                          select new CartItem
                          {
                              Id = cartItem.Id,
                              ProductId = cartItem.ProductId,
                              Qty = cartItem.Qty,
                              CartId = cartItem.CartId
                          }).ToListAsync();
        }

        public async Task<CartItem> UpdateQty(int id, CartItemQtyUpdateDto cartItemQtyUpdateDto)
        {
            var item = await this.kesarjotShopDbContext.CartItems.FindAsync(id);

            if (item != null)
            {
                item.Qty = cartItemQtyUpdateDto.Qty;
                await this.kesarjotShopDbContext.SaveChangesAsync();
                return item;
            }

            return null;
        }
    }
}
