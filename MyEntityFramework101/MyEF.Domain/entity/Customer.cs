using System;
using System.Collections.Generic;
using System.Text;

namespace MyEF.Domain.entity
{
    public class Customer : EntityBase
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}