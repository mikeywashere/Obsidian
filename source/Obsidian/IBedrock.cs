namespace Obsidian
{
    public interface IBedrock
    {
        Task<IEnumerable<BedrockVersion>> Versions();
    }
}
