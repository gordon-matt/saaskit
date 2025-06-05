namespace AspNetStructureMapSample;

public interface IMessageService
{
    Guid Id { get; }

    string Format(string message);
}

public class MessageService : IMessageService, IDisposable
{
    private readonly ILogger<MessageService> log;

    public MessageService(ILogger<MessageService> log)
    {
        this.log = log;
    }

    public Guid Id { get; } = Guid.NewGuid();

    public void Dispose() => log.LogInformation("Disposing MessageSerivce:{id}", Id);

    public string Format(string message) => $"{Id}: {message}";
}

public class OtherMessageService : IMessageService
{
    public Guid Id { get; } = Guid.NewGuid();

    public string Format(string message) => "Other";
}