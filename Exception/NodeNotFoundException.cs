namespace MVE;

public class NodeNotFoundException : Exception
{
    public NodeNotFoundException(string nodeName) : base($"Node not found: {nodeName}")
    {
    }
}