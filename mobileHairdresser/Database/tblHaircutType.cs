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
    
    public partial class tblHaircutType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblHaircutType()
        {
            this.tblHaircuts = new HashSet<tblHaircut>();
        }
    
        public int TypeID { get; set; }
        public string TypeName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblHaircut> tblHaircuts { get; set; }
    }
}
