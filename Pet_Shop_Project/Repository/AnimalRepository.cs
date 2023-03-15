using Microsoft.EntityFrameworkCore;
using Models;
using Pet_Shop_Project.Repository.IRepository;

namespace Pet_Shop_Project.Repository
{
    public class AnimalRepository : Repository<Animal>, IAnimalRepository
    {
        private readonly MyDbContext _db;

        public AnimalRepository(MyDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Animal obj)
        {
            var UpdateMoreThanOne = _db.Animals.FirstOrDefault(a => a.Id == obj.Id);
            if (UpdateMoreThanOne != null)
            {
                UpdateMoreThanOne.Age = obj.Age;
                UpdateMoreThanOne.AnimalName = obj.AnimalName;
                UpdateMoreThanOne.ShortDescription = obj.ShortDescription;
                UpdateMoreThanOne.Price = obj.Price;
                if (obj.ImageUrl != null)
                {
                    UpdateMoreThanOne.ImageUrl = obj.ImageUrl;
                }
                _db.Animals.Update(UpdateMoreThanOne);
            }
        }
    }
}