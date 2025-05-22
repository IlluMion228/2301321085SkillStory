using System.ComponentModel.DataAnnotations;

namespace SkillStoryBack.Models
{
    public class Skill
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Range(0, 100)]
        public int Level { get; set; }

        [Required]
        public int UserId { get; set; }
        public User? User { get; set; }

        public List<Comment> Comments { get; set; } = new();
    }
}
