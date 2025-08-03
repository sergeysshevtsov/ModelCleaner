using Autodesk.Revit.UI;
using ModelCleaner.Models;
using ModelCleaner.Options.Models;
using Serilog;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ModelCleaner.Options;
internal class OptionsDataContext : INotifyPropertyChanged
{
    private readonly Document document;
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

    public OptionsDataContext(Document document, List<ElementData> elementsData)
    {
        Items = [.. elementsData
            .GroupBy(e => e.ElementType)
            .Select(g => new OptionsData() { ElementType = g.Key, ElementCount = g.Count(), IsChecked = true, ElementIds = [.. g.Select(d => d.ElementId)] })];

        IsAllSelected = true;
        this.document = document;
    }

    public ICommand CloseCommand => new RelayCommand<Window>(
       window =>
       {
           IsCleaned = false;
           window?.Close();
       });

    public ICommand CleanCommand => new RelayCommand<Window>(
        window =>
        {
            IsCleaned = true;
            var selectedElementTypesToDelete = Items.Where(x => x.IsChecked).ToList();
            if (selectedElementTypesToDelete.Count == 0)
            {
                TaskDialog.Show("ModelCleaner", "No element types selected.");
                return;
            }

            var i = 0;
            foreach (OptionsData optionsData in selectedElementTypesToDelete)
            {
                foreach (ElementId elementId in optionsData.ElementIds)
                {
                    try
                    {
                        Element element = document.GetElement(elementId);
                        if (element != null)
                        {
                            document.Delete(elementId);
                            Log.Information($"{element.Name} - {elementId} - is deleted.");
                            i++;
                        }
                        else 
                        {
                            Log.Information($"Element is NULL - {elementId} - can't delete.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to delete {optionsData.ElementType}: {ex.Message}");
                    }
                }
            }

            TaskDialog.Show("ModelCleaner", $"{i} unused elements deleted.");
            window?.Close();
        });

    public List<OptionsData> Items { get; set; }

    private bool _isAllSelected;
   

    public bool IsAllSelected
    {
        get => _isAllSelected;
        set
        {
            if (_isAllSelected != value)
            {
                _isAllSelected = value;
                OnPropertyChanged(nameof(IsAllSelected));

                foreach (var item in Items)
                    item.IsChecked = value;
            }
        }
    }

    public bool IsCleaned { get; set; }
}
