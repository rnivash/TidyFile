namespace TidyFile.Models;

/// <summary>
/// Represents a custom category for file organization.
/// </summary>
public class Category
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Category() { }

    public Category(string name, string description = "")
    {
        Name = name;
        Description = description;
        CreatedAt = DateTime.Now;
    }

    public override string ToString()
    {
        return $"{Name} {(string.IsNullOrEmpty(Description) ? "" : $"- {Description}")}";
    }

    public override bool Equals(object? obj)
    {
        return obj is Category category && category.Name == Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}
