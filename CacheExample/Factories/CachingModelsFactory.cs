using System;
using System.Collections.Generic;
using System.Text;
using CacheExample.Models;

namespace CacheExample.Factories
{
    public class CachingModelsFactory
    {
        public static UserForCaching CreateFakeUser()
        {
            return new UserForCaching()
            {
                DateOfBirth = Faker.DateOfBirth.Next(),
                FirstName = Faker.Name.First(),
                LastName = Faker.Name.Last(),
                City = Faker.Address.City(),
                Id = Faker.Identification.UKNationalInsuranceNumber()
            };
        }
    }
}
