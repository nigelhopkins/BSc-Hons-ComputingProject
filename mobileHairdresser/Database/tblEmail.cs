//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace mobileHairdresser.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class tblEmail
    {
        public int EmailID { get; set; }
        [Required]
        [Display(Name ="Customer Name")]
        public string CustName { get; set; }
        [Required]
        [Display(Name ="Customer Email")]
        public string custEmail { get; set; }
        [Required]
        [Display(Name ="Customer Phone Number")]
        public string custPhone { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        [Required]
        public string Read { get; set; }
        [Required]
        [Display(Name = "Date Sent")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public System.DateTime DateSent { get; set; }
    }
}