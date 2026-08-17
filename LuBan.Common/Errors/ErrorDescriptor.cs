namespace LuBan.Common.Errors;

public readonly struct ErrorDescriptor : IEquatable<ErrorDescriptor>
{
    public int Code { get; }
    public string Message { get; }
    public ErrorCategory Category { get; }

    public int HttpStatusCode => Category.ToHttpStatus();

    public ErrorDescriptor(int code, string message, ErrorCategory category)
    {
        Code = code;
        Message = message;
        Category = category;
    }

    public bool Equals(ErrorDescriptor other) => Code == other.Code;
    public override bool Equals(object? obj) => obj is ErrorDescriptor other && Equals(other);
    public override int GetHashCode() => Code;
    public override string ToString() => $"[{Code}] {Message}";

    public static bool operator ==(ErrorDescriptor left, ErrorDescriptor right) => left.Equals(right);
    public static bool operator !=(ErrorDescriptor left, ErrorDescriptor right) => !left.Equals(right);
}