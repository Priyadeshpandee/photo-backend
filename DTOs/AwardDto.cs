namespace PhotographyCMS.DTOs;

public class AwardDto
{
    public int Id { get; set; }
    public string AwardName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Competition { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public class CreateUpdateAwardDto
{
    public string AwardName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Competition { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}