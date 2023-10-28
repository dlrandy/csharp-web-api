
using System;
using DefaultValueAttribute = System.ComponentModel.DefaultValueAttribute;
using System.ComponentModel.DataAnnotations;
using MyBGList.Attributes;

namespace MyBGList.DTO
{
    //public class RequestDTO<T>
    //{
    //	[DefaultValue(0)]
    //	public int PageIndex { get; set; } = 0;
    //	[DefaultValue(10)]
    //	[Range(1, 100)]
    //	public int PageSize { get; set; } = 10;
    //	[DefaultValue("Name")]
    //	[SortColumnValidator(typeof(T))]
    //	public string? SortColumn { get; set; } = "Name";
    //	[DefaultValue("ASC")]
    //	[SortOrderValidator]
    //	public string? SortOrder { get; set; } = "ASC";
    //	public string? FilterQuery { get; set; } = null;
    //}


    public class RequestDTO<T> : IValidatableObject
    {
        [DefaultValue(0)]
        public int PageIndex { get; set; } = 0;
        [DefaultValue(10)]
        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
        [DefaultValue("Name")]
        public string? SortColumn { get; set; } = "Name";
        [DefaultValue("ASC")]
        [SortOrderValidator]
        public string? SortOrder { get; set; } = "ASC";
        public string? FilterQuery { get; set; } = null;
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validator = new SortColumnValidatorAttribute(typeof(T));
            var result = validator
                    .GetValidationResult(SortColumn, validationContext);
            return (result != null)
                    ? new[] { result }
                    : new ValidationResult[0];
        }
    }


















}

