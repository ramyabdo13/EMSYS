using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EMSYS.Models
{
    public class EmailTemplate
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Type { get; set; }
    }

}