namespace Talabat.Core.Entities.Identity
{
    public class Address//1 identity
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public string AppUserId { get; set; } //FK
        public AppUser User { get; set; } //Navidation property of 1 to 1

    }
}