namespace Pet_Shop_Project.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IAnimalRepository Animal { get; }
        ICoverTypeRepository CoverType { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IShoppingCartRepository shoppingCart { get; }
        IApplicationUserRepository applicationUser { get; }
        IOrderHeaderRepository orderHeader { get; }
        IOrderDetailRepository orderDetail { get; }
        void Save();
    }
}
