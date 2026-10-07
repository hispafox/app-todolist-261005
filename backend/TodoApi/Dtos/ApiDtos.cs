namespace TodoApi.Dtos;

public sealed record TodoRequest(
    string? Title,
    bool IsCompleted = false,
    int? CategoryId = null,
    DateTime? CreatedAt = null,
    int? UserId = null);

public sealed record TodoResponse(
    int Id,
    string Title,
    bool IsCompleted,
    DateTime CreatedAt,
    int? CategoryId,
    string? CategoryName,
    int? UserId,
    string? UserName);

public sealed record CategoryRequest(string? Name);

public sealed record CategoryResponse(int Id, string Name);

public sealed record UserRequest(string? Name);

public sealed record UserResponse(int Id, string Name);
