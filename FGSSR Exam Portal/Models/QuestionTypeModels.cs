using System.ComponentModel.DataAnnotations;

namespace EMSYS.Models
{
    public class QuestionType
    {
        [Key]
        [MaxLength(128)]
        public string Id { get; set; }
        [MaxLength(128)]
        public string Code { get; set; }
        [MaxLength(256)]
        public string Name { get; set; }
    }
}
