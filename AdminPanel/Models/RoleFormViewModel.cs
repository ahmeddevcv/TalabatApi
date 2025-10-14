using System.ComponentModel.DataAnnotations;

namespace AdminPanel.Models
{
    public class RoleFormViewModel
    {
        [Required]
        [StringLength(256, MinimumLength = 2)]
        public string Name { get; set; }
    }
}
