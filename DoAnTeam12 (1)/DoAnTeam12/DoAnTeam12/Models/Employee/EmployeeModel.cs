using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnTeam12.Models.Employee
{
    public class EmployeeModel
    {
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = "Employee name is required.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Employee name must be between 4 and 100 characters long.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [CustomValidation(typeof(EmployeeModel), "ValidateEmployeeAge", ErrorMessage = "Employee must be between 18 and 60 years old.")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^(0|\+84)(3[2-9]|8[6|8|9]|7[0|6-9]|8[1-5|8]|9[0-4|6-9])[0-9]{7}$", ErrorMessage = "Please enter a valid Vietnamese phone number.")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Hire Date is required.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [CustomValidation(typeof(EmployeeModel), "ValidateHireDate", ErrorMessage = "Hire Date cannot be in the future.")]
        public DateTime? HireDate { get; set; }

        [Required(ErrorMessage = "Department is required.")]
        public int? DepartmentID { get; set; }
        public string DepartmentName { get; set; }

        [Required(ErrorMessage = "Position is required.")]
        public int? PositionID { get; set; }
        public string PositionName { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; }

        
        public DateTime? CreatedAt { get; set; }

       
        public DateTime? UpdatedAt { get; set; }

        public static ValidationResult ValidateEmployeeAge(DateTime? dateOfBirth, ValidationContext context)
        {
            if (dateOfBirth == null)
            {
                return ValidationResult.Success;
            }

            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Value.Year;
            if (dateOfBirth.Value.Date > today.AddYears(-age))
            {
                age--;
            }

            if (age < 18 || age > 60)
            {
                return new ValidationResult("Employee's age must be between 18 and 60.", new[] { context.MemberName });
            }

            return ValidationResult.Success;
        }

        public static ValidationResult ValidateHireDate(DateTime? hireDate, ValidationContext context)
        {
            if (hireDate == null)
            {
                return ValidationResult.Success;
            }

            if (hireDate.Value.Date > DateTime.Today)
            {
                return new ValidationResult("Hire Date cannot be set for a future date.", new[] { context.MemberName });
            }
            var employeeModel = context.ObjectInstance as EmployeeModel;
            if (employeeModel != null && employeeModel.DateOfBirth != null)
            {
                var dateOfBirth = employeeModel.DateOfBirth.Value;
                var ageAtHire = hireDate.Value.Year - dateOfBirth.Year;

                if (dateOfBirth.Date > hireDate.Value.AddYears(-ageAtHire))
                {
                    ageAtHire--;
                }

                if (ageAtHire < 18)
                {
                    return new ValidationResult("Employee must be at least 18 years old at the time of hiring.", new[] { context.MemberName });
                }
            }
            return ValidationResult.Success;
        }

    }
}