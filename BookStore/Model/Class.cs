namespace BookStore.Model
{
    public record Response(
        int Error,
        string Message,
        object? Data
    );
}
