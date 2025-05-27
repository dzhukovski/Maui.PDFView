using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maui.PDFView.Events;
using System.Diagnostics;
using Example.Business.Collections;

namespace Example.Business.UI.ViewModels
{
    internal partial class MainPageViewModel : ObservableObject
    {
        private readonly LoopedList<string> _pdfs = new(
            "PDF/pdf1.pdf",
            "PDF/pdf2.pdf",
            "https://www.orimi.com/pdf-test.pdf",
            "file:///sdcard/Download/Agreement Prolongation Instruction - TR_05 Oct 2023.pdf"
        );

        [ObservableProperty] private string _pdfSource;
        [ObservableProperty] private bool _isHorizontal;
        [ObservableProperty] private float _maxZoom = 4;
        [ObservableProperty] private string _pagePosition;
        [ObservableProperty] private uint _pageIndex = 0;
        [ObservableProperty] private uint _maxPageIndex = uint.MaxValue;

        [RelayCommand] private void Appearing()
        {
            ChangeUri();
        }

        [RelayCommand] private void ChangeUri()
        {
            PdfSource = _pdfs.Next();
        }

        [RelayCommand] private void PageChanged(PageChangedEventArgs args)
        {
            MaxPageIndex = (uint)args.TotalPages - 1;
            PagePosition = $"{args.CurrentPage} of {args.TotalPages}";
            Debug.WriteLine($"Current page: {args.CurrentPage} of {args.TotalPages}");
        }
    }
}
