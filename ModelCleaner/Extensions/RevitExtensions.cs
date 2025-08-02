namespace ModelCleaner.Extensions;
public static class RevitExtensions
{
    public static bool IsInUse(this FamilySymbol symbol) => 
        symbol.IsActive || symbol.GetDependentElements(null).Count > 0;

    public static bool IsInUse(this GroupType groupType) => 
        groupType.GetDependentElements(null).Count > 0;

    public static bool IsViewPlaced(this View view) => 
        view.GetDependentElements(new ElementClassFilter(typeof(Viewport))).Count > 0;

    public static bool IsUsedInDocument(this Category category, Document doc)
    {
        if (category == null || !category.AllowsBoundParameters)
            return true;

        var collector = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .OfCategoryId(category.Id);

        return collector.Any();
    }

    public static bool IsPlacedOnSheet(this View view, Document doc)
    {
        var viewportFilter = new ElementClassFilter(typeof(Viewport));
        var dependent = view.GetDependentElements(viewportFilter);
        return dependent != null && dependent.Count > 0;
    }
}
