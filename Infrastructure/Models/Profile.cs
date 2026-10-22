using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Models
{
    public partial class Profile
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = null!;
    }
}
