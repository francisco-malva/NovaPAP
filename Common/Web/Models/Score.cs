using System.ComponentModel.DataAnnotations;

namespace Common.Web.Models;

public class Score
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public int Time { get; set; }
}