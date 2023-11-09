namespace kithub.api.Data
{
    public class KithubDbContext : DbContext
    {
        public KithubDbContext(DbContextOptions<KithubDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Users
            //modelBuilder.Entity<User>().HasData(new User
            //{
            //    Id = 1,
            //    UserName = "Nkseervi"
            //});
            //modelBuilder.Entity<User>().HasData(new User
            //{
            //    Id = 2,
            //    UserName = "TestUser"
            //});

            ////Create Shopping Cart for Users
            //modelBuilder.Entity<Cart>().HasData(new Cart
            //{
            //    Id = 1,
            //    UserId = 1

            //});
            //modelBuilder.Entity<Cart>().HasData(new Cart
            //{
            //    Id = 2,
            //    UserId = 2

            //});

            #endregion

            #region Products
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 1,
                Name = "Cumin Seed Gold",
                Description = "Best quality whole Jeera",
                ImageURL = "/Images/SpicesW/Jeera1.png",
                Price = 270,
                Qty = 500,
                CategoryId = 1
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 2,
                Name = "Fennel Seeds Gold",
                Description = "Best quality whole Fennel seeds",
                ImageURL = "/Images/SpicesW/Fennel1.png",
                Price = 150,
                Qty = 500,
                CategoryId = 1
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 3,
                Name = "Cumin Seeds Premium",
                Description = "Premium quality whole Jeera seeds",
                ImageURL = "/Images/SpicesW/Jeera2.png",
                Price = 250,
                Qty = 500,
                CategoryId = 1
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 4,
                Name = "Cumin Powder",
                Description = "Premium quality Jeera powder",
                ImageURL = "/Images/SpicesP/Jeera1.png",
                Price = 200,
                Qty = 500,
                CategoryId = 2
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 5,
                Name = "Kasuri Methi",
                Description = "Best quality Kasuri Methi",
                ImageURL = "/Images/Herbs/MethiLeaves1.png",
                Price = 150,
                Qty = 500,
                CategoryId = 3
            }); 
            #endregion

            #region Product Categories
            modelBuilder.Entity<ProductCategory>().HasData(new ProductCategory
            {
                Id = 1,
                Name = "Whole Spices",
                IconCSS = "fas fa-clover"
            });
            modelBuilder.Entity<ProductCategory>().HasData(new ProductCategory
            {
                Id = 2,
                Name = "Powder Spices",
                IconCSS = "fas fa-mortar-pestle"
            });
            modelBuilder.Entity<ProductCategory>().HasData(new ProductCategory
            {
                Id = 3,
                Name = "Herbs",
                IconCSS = "fas fa-leaf"
            }); 
            #endregion

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
    }
}
