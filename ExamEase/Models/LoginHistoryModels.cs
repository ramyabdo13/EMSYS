using EMSYS.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EMSYS.Models
{
    public class LoginHistory
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public string AspNetUserId { get; set; }
        public DateTime? LoginDateTime { get; set; }
        public string IsoUtcLoginDateTime { get; set; }
    }

    public class LoginHistoryViewModel
    {
        public string Id { get; set; }
        public string AspNetUserId { get; set; }
        public string UserProfileId { get; set; }
        [Display(Name = "Username", ResourceType = typeof(Resource))]
        public string Username { get; set; }
        [Display(Name = "FullName", ResourceType = typeof(Resource))]
        public string FullName { get; set; }
        public DateTime LoginDateTime { get; set; }
        [Display(Name = "LoginDateTime", ResourceType = typeof(Resource))]
        public string FormattedLoginDateTime { get; set; }
        [Display(Name = "LoginDateTime", ResourceType = typeof(Resource))]
        public string IsoUtcLoginDateTime { get; set; }
        public int LoginDateTimeOrder { get; set; }
    }
}