using System.Windows.Input;
using Maui.PDFView.Helpers.DataSource;

namespace Maui.PDFView
{
    public interface IPdfView : IView
    {
        DataSource Source  { get; set; }
        
        bool IsHorizontal { get; set; }
        float MaxZoom { get; set; }
        PageAppearance? PageAppearance { get; set; }
        uint PageIndex { get; set; }

        ICommand PageChangedCommand { get; set; }
    }
}
