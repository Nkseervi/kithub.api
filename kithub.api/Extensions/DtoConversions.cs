using kithub.api.Areas.Identity.Data;

namespace kithub.api.Extensions
{
    public static class DtoConversions
    {
        public static IEnumerable<ProductCategoryDto> ConvertToDto(this IEnumerable<ProductCategory> productCategories)
        {
            return (from productCategory in productCategories
                    select new ProductCategoryDto
                    {
                        Id = productCategory.Id,
                        Name = productCategory.Name,
                        IconCSS = productCategory.IconCSS
                    }).ToList();
        }
        public static IEnumerable<ProductDto> ConvertToDto(this IEnumerable<Product> products)
        {
            return (from product in products
                    select new ProductDto
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Description = product.Description,
                        ImageURL = product.ImageURL,
                        Price = product.Price,
                        Qty = product.Qty,
                        CategoryId = product.ProductCategory.Id,
                        CategoryName = product.ProductCategory.Name
                    }).ToList();

        }
        public static ProductDto ConvertToDto(this Product product)

        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                ImageURL = product.ImageURL,
                Price = product.Price,
                Qty = product.Qty,
                CategoryId = product.ProductCategory.Id,
                CategoryName = product.ProductCategory.Name

            };

        }

        public static IEnumerable<CartItemDto> ConvertToDto(this IEnumerable<CartItem> cartItems,
                                                            IEnumerable<Product> products)
        {
            return (from cartItem in cartItems
                    join product in products
                    on cartItem.ProductId equals product.Id
                    select new CartItemDto
                    {
                        Id = cartItem.Id,
                        ProductId = cartItem.ProductId,
                        ProductName = product.Name,
                        ProductDescription = product.Description,
                        ProductImageURL = product.ImageURL,
                        Price = product.Price,
                        CartId = cartItem.CartId,
                        Qty = cartItem.Qty,
                        TotalPrice = product.Price * cartItem.Qty
                    }).ToList();
        }
        public static CartItemDto ConvertToDto(this CartItem cartItem,
                                                    Product product)
        {
            return new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = product.Name,
                ProductDescription = product.Description,
                ProductImageURL = product.ImageURL,
                Price = product.Price,
                CartId = cartItem.CartId,
                Qty = cartItem.Qty,
                TotalPrice = product.Price * cartItem.Qty
            };
        }

        public static IEnumerable<UserDto> ConvertToDto(this IEnumerable<KithubUser> users, IEnumerable<string> roles)
        {
            return (from user in users
                    select new UserDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        CreateDate = user.CreateDate,
                        EmailAddress = user.EmailAddress,
                        Roles = roles
                    }).ToList();

        }
		public static LoggedInUserDto ConvertToDto(this KithubUser user, Cart cart)
		{
			return new LoggedInUserDto
					{
						Id = user.Id,
                        CartId = cart.Id,
						FirstName = user.FirstName,
						LastName = user.LastName,
						CreateDate = user.CreateDate,
						EmailAddress = user.EmailAddress,
					};

		}

        public static IEnumerable<OrderDto> ConvertToDto(this IEnumerable<Order> orders)
        {
            return (from order in orders
                    select new OrderDto
                    {
                        Id = order.Id,
                        Amount = order.Amount,
                        CreatedOn = order.CreatedOn,
                        Status = order.Status,
                        UpdatedOn = order.UpdatedOn,
                        OrderItems = (from ordItem in order.OrderItems
                                      select new OrderItemDto
                                      {
                                          ProductName = ordItem.ProductName,
                                          ProductDescription = ordItem.ProductDescription,
                                          Qty = ordItem.Qty,
                                          SellingPrice = ordItem.SellingPrice,
                                          TotalPrice = ordItem.TotalPrice
                                      }).ToList()
                    }).ToList();
        }

        public static OrderDto ConvertToDto(this Order order)
        {
            List<OrderItemDto> orderItemDtos =
                (from orderItem in order.OrderItems
                    select new OrderItemDto
                    {
                        ProductName = orderItem.ProductName,
                        DiscountPercent = orderItem.Discount,
                        ListedPrice = orderItem.ListedPrice,
                        Qty = orderItem.Qty,
                        SellingPrice = orderItem.SellingPrice,
                        GstRate = orderItem.GstRate,
                        ProductDescription = orderItem.ProductDescription,
                        TotalPrice = orderItem.TotalPrice
                    }).ToList();

            return new OrderDto
            {
                Id = order.Id,
                Amount = order.Amount,
                CreatedOn = order.CreatedOn,
                Status = order.Status,
                UpdatedOn = order.UpdatedOn,
                OrderItems = orderItemDtos
            };
        }
    }
}
