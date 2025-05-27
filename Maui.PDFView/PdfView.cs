using System.Windows.Input;
using Maui.PDFView.Helpers.DataSource;

namespace Maui.PDFView
{
    public class PdfView : View, IPdfView
    {
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
            nameof(Source),
            typeof(DataSource),
            typeof(PdfView)
        );

        public static readonly BindableProperty IsHorizontalProperty = BindableProperty.Create(
            propertyName: nameof(IsHorizontal),
            returnType: typeof(bool),
            declaringType: typeof(PdfView),
            defaultValue: false
        );
        
        public static readonly BindableProperty IsLoadingProperty = BindableProperty.Create(
            propertyName: nameof(IsLoading),
            returnType: typeof(bool),
            declaringType: typeof(PdfView),
            defaultValue: false
        );

        public static readonly BindableProperty MaxZoomProperty = BindableProperty.Create(
            propertyName: nameof(MaxZoom),
            returnType: typeof(float),
            declaringType: typeof(PdfView),
            defaultValue: 4f,
            propertyChanged: OnMaxZoomPropertyChanged
        );
        
        public static readonly BindableProperty PageAppearanceProperty = BindableProperty.Create(
            propertyName: nameof(PageAppearance),
            returnType: typeof(PageAppearance), 
            declaringType: typeof(PdfView),
            defaultValue: null
        );

        public static readonly BindableProperty PageChangedCommandProperty = BindableProperty.Create(
            propertyName: nameof(PageChangedCommand),
            returnType: typeof(ICommand),
            declaringType: typeof(PdfView),
            defaultValue: null
        );

        public static readonly BindableProperty PageIndexProperty = BindableProperty.Create(
            propertyName: nameof(PageIndex),
            returnType: typeof(uint),
            declaringType: typeof(PdfView),
            defaultValue: (uint)0,
            defaultBindingMode: BindingMode.TwoWay
        );
        
        public event Action<Exception> LoadingFailed;

        [System.ComponentModel.TypeConverter(typeof(DataSourceConverter))]
        public DataSource Source
        {
            get => (DataSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
        
        public bool IsHorizontal
        {
            get => (bool)GetValue(IsHorizontalProperty);
            set => SetValue(IsHorizontalProperty, value);
        }
        
        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public float MaxZoom
        {
            get => (float)GetValue(MaxZoomProperty);
            set => SetValue(MaxZoomProperty, value);
        }
        
        public PageAppearance? PageAppearance
        {
            get => (PageAppearance?)GetValue(PageAppearanceProperty);
            set => SetValue(PageAppearanceProperty, value);
        }

        public ICommand PageChangedCommand
        {
            get => (ICommand)GetValue(PageChangedCommandProperty);
            set => SetValue(PageChangedCommandProperty, value);
        }

        public uint PageIndex
        {
            get => (uint)GetValue(PageIndexProperty);
            set => SetValue(PageIndexProperty, value);
        }

        internal void OnLoadingFailed(Exception exception)
        {
            LoadingFailed?.Invoke(exception);
        }
        
        private static void OnMaxZoomPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if ((float)newValue < 1f)
            {
                throw new ArgumentException("PdfView: MaxZoom cannot be less than 1");
            }
        }
    }
}
