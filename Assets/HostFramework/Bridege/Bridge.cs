using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public abstract class BridgeMessage
{
    public string id { get; set; }
    public string method { get; set; }
    public object data { get; set; }
    public string type { get; set; }

    protected BridgeMessage(string method, object data = null)
    {
        this.id = Guid.NewGuid().ToString();
        this.method = method;
        this.data = data;
        this.type = GetMessageType();
    }

    protected abstract string GetMessageType();
}

public class HostToMiniGameRequest : BridgeMessage
{
    public HostToMiniGameRequest(string method, object data = null) : base(method, data) { }
    protected override string GetMessageType() => "request";
}

public class MiniGameToHostRequest : BridgeMessage
{
    public MiniGameToHostRequest(string method, object data = null) : base(method, data) { }
    protected override string GetMessageType() => "request";
}

public class BridgeResponse : BridgeMessage
{
    public bool success { get; set; }
    public string message { get; set; }

    public BridgeResponse(string originalRequestId, string method, bool success, object data = null, string message = "")
        : base(method, data)
    {
        this.id = originalRequestId;
        this.success = success;
        this.message = message;
        this.type = "response";
    }

    protected override string GetMessageType() => "response";
}

public interface IHostCallMiniGameHandler
{
    string Name { get; }
    void Handle(HostToMiniGameRequest request, Action<BridgeResponse> callback);
}

public interface IMiniGameCallHostHandler
{
    string Name { get; }
    void Handle(MiniGameToHostRequest request, Action<BridgeResponse> callback);
}



