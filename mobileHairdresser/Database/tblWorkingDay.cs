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
    
    public partial class tblWorkingDay
    {
        public int DayID { get; set; }
        public string DayName { get; set; }
        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public Nullable<int> EmployeeID { get; set; }
    
        public virtual tblEmployee tblEmployee { get; set; }
    }
}