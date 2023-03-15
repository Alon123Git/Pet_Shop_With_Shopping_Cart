using Models;

namespace Pet_Shop_Project.Repository.IRepository
{
    public interface IAnimalRepository : IRepository<Animal>
    {
        void Update(Animal obj);
    }
}