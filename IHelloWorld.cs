using Orleans;

namespace orleans_simple;

public interface IHelloWorld : IGrainWithStringKey
{
    Task<string> Hello();
    Task Write(string data);
}

public class HelloWorld : Grain<HelloState>, IHelloWorld
{
    /// <inheritdoc />
    public Task<string> Hello()
    {
        return Task.FromResult<string>($"Hello from {this.GetPrimaryKeyString()}: {State.Data}");
    }

    /// <inheritdoc />
    public Task Write(string data)
    {
        State.Data = data;
        WriteStateAsync();

        return Task.CompletedTask;
    }
}

public class HelloState
{
    public string Data { get; set; }
}