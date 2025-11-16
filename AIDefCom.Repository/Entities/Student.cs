using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDefCom.Repository.Entities
{
    public class Student : AppUser
    {
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
    }
}   