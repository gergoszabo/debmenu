namespace Debmenu.Tests.Helpers;

public sealed class ScriptedHttpMessageHandler : HttpMessageHandler
{
    public Func<HttpRequestMessage, HttpResponseMessage> Responder { get; }

    public ScriptedHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        Responder = responder;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Responder(request));
    }
}