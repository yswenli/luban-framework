namespace LuBan.Common.Errors;

public sealed class ErrorCodeRegistry
{
    private readonly Dictionary<int, ErrorDescriptor> _byCode = new();

    public ErrorCodeRegistry()
    {
        Register(FrameworkErrors.All);
    }

    public void Register(IEnumerable<ErrorDescriptor> descriptors)
    {
        foreach (var d in descriptors)
        {
            if (!_byCode.TryAdd(d.Code, d))
                throw new InvalidOperationException($"Duplicate error code: {d.Code}");
        }
    }

    public ErrorDescriptor? FindByCode(int code)
        => _byCode.GetValueOrDefault(code);
}