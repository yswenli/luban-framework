using LuBan.AIAgent.Hosting.Commands;

namespace LuBan.XTestProject;

[TestClass]
public class CommandCatalogUnitTest
{
    private readonly CommandCatalog _catalog = new();

    [TestMethod]
    public void All_ContainsCoreCommands()
    {
        var names = _catalog.All.Select(c => c.Name).ToList();
        CollectionAssert.Contains(names, "/model");
        CollectionAssert.Contains(names, "/help");
    }

    [TestMethod]
    public void MatchByPrefix_EmptyPrefix_ReturnsOnlyClientSupported()
    {
        var cli = _catalog.MatchByPrefix("/", SlashCommandClient.Cli);

        Assert.IsTrue(cli.All(c => c.Supports(SlashCommandClient.Cli)));
    }

    [TestMethod]
    public void MatchByPrefix_FiltersByText()
    {
        var result = _catalog.MatchByPrefix("/mo", SlashCommandClient.Both);

        Assert.IsTrue(result.Any(c => c.Name == "/model"));
        Assert.IsFalse(result.Any(c => c.Name == "/help"));
    }
}