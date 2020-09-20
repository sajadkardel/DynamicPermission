using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DynamicPermission.AspNetCore.Models
{
    public class AppSetting
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime? LastTimeChanged { get; set; }
    }
}
