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

    public partial class tblGallery
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblGallery()
        {
            this.tblPhotos = new HashSet<tblPhoto>();
        }
    
        public int GalleryID { get; set; }
        [Required]
        [Display(Name ="Gallery Name")]
        public string GalleryName { get; set; }
        [Required]
        [Display(Name = "Gallery Image")]
        public string GalleryImg { get; set; }
        [Display(Name ="Gallery Discription")]
        public string GalleryDiscription { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblPhoto> tblPhotos { get; set; }
    }
}