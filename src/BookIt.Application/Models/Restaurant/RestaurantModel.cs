﻿namespace BookIt.Application.Models.Restaurant
{
    public class RestaurantModel : BaseResponseModel
    {
        public string RestaurantOwner { get; set; }
        public string RestaurantName { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string ManagerId { get; set; }
        public string ManagerFirstName { get; set; }
        public string ManagerLastName { get; set; }
        public string ManagerUserName { get; set; }
    }
}
