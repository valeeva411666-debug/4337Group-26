using System;

namespace Group4337
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public DateTime CreationDate { get; set; }
        public string ClientCode { get; set; }
        public string Services { get; set; }

        public string GetRentalCategory()
        {
            TimeSpan rentalTime = DateTime.Now - CreationDate;

            if (rentalTime.TotalHours < 24)
                return "Менее суток";
            else if (rentalTime.TotalHours < 72)
                return "1-3 дня";
            else if (rentalTime.TotalHours < 168)
                return "4-7 дней";
            else
                return "Более недели";
        }
    }
}