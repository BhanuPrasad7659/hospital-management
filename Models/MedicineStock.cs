using System.ComponentModel.DataAnnotations;

namespace CogMediHospitalManagementSystem.Models
{
    public class MedicineStock
    {
        [Key]
        [Required]
        [StringLength(100)]
        [Display(Name = "Medicine Name")]
        public string MedicineName { get; set; } = string.Empty;

        [Range(0, 100000)]
        [Display(Name = "Quantity in Stock")]
        public int Quantity { get; set; }
    }
}
