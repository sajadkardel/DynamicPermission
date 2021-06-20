using System.Collections.Generic;
using System.Security.Claims;

namespace DynamicPermission.AspNetCore.Common.Models
{
    public class AreaViewModel
    {
        public Claim Area { get; set; }
        public List<ControllerViewModel> ControllerViewModels { get; set; } = new List<ControllerViewModel>();
    }

    public class ControllerViewModel
    {
        public Claim ParentArea { get; set; }
        public Claim Controller { get; set; }
        public List<ActionViewModel> ActionViewModels { get; set; } = new List<ActionViewModel>();
    }

    public class ActionViewModel
    {
        public Claim ParentController { get; set; }
        public Claim Action { get; set; }
        public bool Selected { get; set; }
    }
}