const string query = @"t:model dir:Assets/FBX/Originals";

var candidates = new[]
{
    UnityEditor.Search.SearchService.GetProvider("asset"),
    UnityEditor.Search.SearchService.GetProvider("scene")
};
var preferredProviders = System.Linq.Enumerable.ToArray(
    System.Linq.Enumerable.Where(candidates, provider => provider != null));

var context = preferredProviders.Length > 0
    ? new UnityEditor.Search.SearchContext(preferredProviders, query)
    : new UnityEditor.Search.SearchContext(UnityEditor.Search.SearchService.GetActiveProviders(), query);

UnityEditor.Search.SearchService.ShowWindow(context);
return "Opened Unity Search with query: " + query;
