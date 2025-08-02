using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using ModelCleaner.Extensions;
using ModelCleaner.Models;
using Nice3point.Revit.Toolkit.External;

namespace ModelCleaner.Commands;

[UsedImplicitly]
[Transaction(TransactionMode.Manual)]
public class CmdModelCleaner : ExternalCommand
{
    public override void Execute()
    {
        List<Type> typesToCheck =
        [
            typeof(Family),
            typeof(View),
            typeof(GroupType),
            typeof(Material),
            typeof(ParameterFilterElement),
            typeof(FillPatternElement)
        ];

        List<ElementData> elementsToDelete = [];
        using Transaction tr = new(Document, "Clean unused elemenets");
        tr.Start();

        foreach (Type type in typesToCheck)
        {
            var collector = new FilteredElementCollector(Document)
                .OfClass(type)
                .ToList();

            foreach (Element element in collector)
            {
                bool isUsed = false;
                if (element is Family family)
                {
                    foreach (ElementId id in family.GetFamilySymbolIds())
                    {
                        if (Document.GetElement(id) is FamilySymbol symbol && symbol.IsInUse())
                        {
                            isUsed = true;
                            break;
                        }
                    }
                }
                else if (element is View view)
                    if (!view.IsTemplate && !view.IsAssemblyView && view.CanBePrinted && view.ViewType != ViewType.ProjectBrowser)
                        if (!view.IsPlacedOnSheet(Document) && !view.IsViewPlaced())
                            isUsed = true;
                        else if (element is GroupType groupType)
                            isUsed = groupType.IsInUse();
                        else if (element is Material material)
                            isUsed = material.GetDependentElements(null).Count > 0;
                        else if (element is ParameterFilterElement filter)
                            isUsed = filter.GetDependentElements(null).Count > 0;
                        else if (element is FillPatternElement pattern)
                            isUsed = pattern.GetDependentElements(null).Count > 0;

                if (!isUsed)
                    elementsToDelete.Add(new ElementData() { ElementId = element.Id, ElementType = type.Name });
            }
        }

        var categories = Document.Settings.Categories;
        Category linesCategory = categories.get_Item(BuiltInCategory.OST_Lines);
        CategoryNameMap categoryNameMaps = linesCategory.SubCategories;

        foreach (Category category in categoryNameMaps)
            if (!category.IsUsedInDocument(Document))
                elementsToDelete.Add(new ElementData() { ElementId = category.Id, ElementType = category.Name });

        if (elementsToDelete.Count > 0)
        {
            var grouped = elementsToDelete
                .GroupBy(e => e.ElementType)
                .Select(g => $"{g.Key}: {g.Count()}");

            string summary = string.Join(Environment.NewLine, grouped);
            string message = $"ModelCleaner found {elementsToDelete.Count} unused elements:\n\n{summary}\n\nDo you want to delete them?";
            TaskDialogResult result = TaskDialog.Show(
                "ModelCleaner",
                message,
                TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No,
                TaskDialogResult.No
            );

            if (result != TaskDialogResult.Yes)
                return;

            //Document.Delete([.. elementsToDelete.Select(e => e.ElementId)]);
            foreach(ElementData elementData in elementsToDelete)
            {
                try
                {
                    var elementId = elementData.ElementId;
                    Document.Delete(elementId);
                }
                catch { }
            }
            TaskDialog.Show("ModelCleaner", $"{elementsToDelete.Count} unused elements deleted.");
        }
        else
            TaskDialog.Show("ModelCleaner", "No unused elements found.");

        tr.Commit();
    }
}