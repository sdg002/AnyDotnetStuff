using System;
using System.Collections.Generic;
using System.Text;

namespace MyEF.Domain.entity
{
    public class Address : EntityBase
    {
        public string PostCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
    }
}