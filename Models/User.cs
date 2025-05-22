using System.ComponentModel.DataAnnotations;

namespace SkillStoryBack.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        public List<Skill> Skills { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
    }
}
