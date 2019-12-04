using System;
using CacheExample.Interfaces;

namespace CacheExample.Models
{
    public class UserForCaching : ModelWithId<string>
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string City { get; set; }
    }
}
