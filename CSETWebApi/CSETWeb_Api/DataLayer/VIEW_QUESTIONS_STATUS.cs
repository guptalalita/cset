//////////////////////////////// 
// 
//   Copyright 2018 Battelle Energy Alliance, LLC  
// 
// 
//////////////////////////////// 
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataLayer
{
    using System;
    using System.Collections.Generic;
    
    public partial class VIEW_QUESTIONS_STATUS
    {
        public int Question_Or_Requirement_Id { get; set; }
        public Nullable<bool> HasComment { get; set; }
        public Nullable<bool> MarkForReview { get; set; }
        public Nullable<bool> HasDocument { get; set; }
        public int docnum { get; set; }
        public Nullable<bool> HasDiscovery { get; set; }
        public int findingnum { get; set; }
        public int Assessment_Id { get; set; }
        public int Answer_Id { get; set; }
    }
}


