using System.ComponentModel.DataAnnotations;

namespace Hitorus.Data.Entities {
    public class SequenceGenerator {
        [Key]
        public int NextValue { get; set; }
    }
}
