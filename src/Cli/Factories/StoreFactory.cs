using Core;
using Core.Abstractions;
using Core.Storage;

namespace Cli.Factories;

public static class StoreFactory
{
    public static string DataPath => Path.Combine(
        AppContext.BaseDirectory,
        "data",
        "orders.json");

    public static IOrderStore Create(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        bool useFile = args.Contains("--file");
        bool useCache = args.Contains("--cache");

        IOrderStore store = useFile
            ? new FileOrderStore(DataPath)
            : new InMemoryOrderStore(SampleData.Orders());

        if (useCache)
        {
            store = new CachingOrderStore(store);
        }

        return store;
    }
}