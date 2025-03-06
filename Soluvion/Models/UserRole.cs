using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soluvion.Models
{
    public class UserRole
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }

        public static implicit operator string(UserRole v)
        {
            return v.RoleName;
        }
    }
}
