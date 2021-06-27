using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities.DataTransferObjects
{
    public class CompanyForCreationDTO
    {
        [Required(ErrorMessage = "Company name field is required")]
        [MaxLength(60, ErrorMessage = "Maximum length for name is 60 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Company address is a required field.")]
        [MaxLength(60, ErrorMessage = "Maximum length for the Address " +
            "is 60 characters.")]
        public string Address { get; set; }
        public string Country { get; set; }
        public IEnumerable<EmployeeForCreationDTO> Employees { get; set; }
    }
}
