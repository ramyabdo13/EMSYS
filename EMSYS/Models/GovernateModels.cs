using EMSYS.Data;
using EMSYS.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;

namespace EMSYS.Models
{
    public class Governate
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
