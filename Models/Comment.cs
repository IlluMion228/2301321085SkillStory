using System.ComponentModel.DataAnnotations;

namespace SkillStoryBack.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Text { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [Required]
        public int SkillId { get; set; }

        public User? User { get; set; }
        public Skill? Skill { get; set; }
    }
}
