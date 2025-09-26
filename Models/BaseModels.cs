namespace Models;

public class BaseModel
{
    public int Id { get; set; }
    public DateTime DateCreatedUTC { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? LatestDateUpdatedUTC { get; set; }
    public int? LatestUpdatedBy { get; set; }
}

