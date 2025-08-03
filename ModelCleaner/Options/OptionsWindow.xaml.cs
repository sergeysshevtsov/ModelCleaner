using ModelCleaner.Models;
using System.Windows;

namespace ModelCleaner.Options;
public partial class OptionsWindow : Window
{
    public OptionsWindow(Document document, List<ElementData> elementsData)
    {
        InitializeComponent();
        DataContext = new OptionsDataContext(document, elementsData);
    }
}
