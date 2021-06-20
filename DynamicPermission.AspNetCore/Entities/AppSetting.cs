using System;
using System.ComponentModel.DataAnnotations;

namespace DynamicPermission.AspNetCore.Entities
{
    public class AppSetting
    {
        [Key]
        public string Key { get; set; }
        public string Value { get; set; }
        public DateTime? LastTimeChanged { get; set; }
    }
}
