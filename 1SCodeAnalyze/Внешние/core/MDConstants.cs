using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace V8Reader.Core
{

    //static class MDConstants
    //{
    //    public const String AttributeCollection = "ec6bb5e5-b7a8-4d75-bec9-658107a699cf";
    //    public const String TablesCollection = "2bcef0d1-0981-11d6-b9b8-0050bae0a95d";
    //    public const String FormCollection = "d5b0e5ed-256d-401c-9c36-f630cafd8a62";
    //    public const String TemplatesCollection = "3daea016-69b7-4ed4-9453-127911372fe6";
    //    public const String ExternalProcessingClass = "c3831ec8-d8d5-4f93-8a22-f9bfae07327f";
    //    public const String ExternalReportClass = "e41aff26-25cf-4bb6-b6c1-3f478a75f374";
    //}

    static class IconCollections
    {

        static IconCollections()
        {
            MDObjects = new NamedIcons(new WPFImageArray(new Uri("pack://application:,,,/Resources/MD_Pictures.png"), 16, 16));

            MDObjects.NameIndexMap.Add("Report", 63);
            MDObjects.NameIndexMap.Add("DataProcessor", 64);
            MDObjects.NameIndexMap.Add("Attribute", 80);
            MDObjects.NameIndexMap.Add("AttributesCollection", 80);
            MDObjects.NameIndexMap.Add("Table", 79);
            MDObjects.NameIndexMap.Add("TablesCollection", 78);
            MDObjects.NameIndexMap.Add("Form", 51);
            MDObjects.NameIndexMap.Add("FormsCollection", 17);
            MDObjects.NameIndexMap.Add("TemplatesCollection", 18);
            MDObjects.NameIndexMap.Add("Template", 53);

            ManagedForm = new NamedIcons(new WPFImageArray(new Uri("pack://application:,,,/Resources/mngdfrm.png"), 16, 16));
            ManagedForm.NameIndexMap.Add("Element", 0);
            ManagedForm.NameIndexMap.Add("Button", 1);
            ManagedForm.NameIndexMap.Add("Group", 2);
            ManagedForm.NameIndexMap.Add("Toolbar", 3);
            ManagedForm.NameIndexMap.Add("Table", 4);
            ManagedForm.NameIndexMap.Add("Attribute", 5);
            ManagedForm.NameIndexMap.Add("TableAttribute", 6);
            ManagedForm.NameIndexMap.Add("Form", 7);

        }

        public static NamedIcons MDObjects;
        public static NamedIcons ManagedForm;
    }
    
    class NamedIcons
    {
        public NamedIcons(WPFImageArray imgArray)
        {
            m_imgArray = imgArray;
            m_NameIndexMap = new Dictionary<string,int>();
        }

        public AbstractImage this[String Name]
        {
            get
            {
                return new Image<ImageSource>(m_imgArray[NameIndexMap[Name]]);
            }
        }

        public Dictionary<String, int> NameIndexMap
        {
            get
            {
                return m_NameIndexMap;
            }
        }

        WPFImageArray m_imgArray;
        private Dictionary<String, int> m_NameIndexMap;
    }

    abstract class AbstractImage
    {
        public AbstractImage(object Image)
        {
            m_Image = Image;
        }

        public virtual object GetImage()
        {
            return m_Image;
        }

        protected object m_Image;
    }

    class IconSet
    {
        private WPFImageArray _arr;
        private Uri _uri;

        public Uri ImageUri
        {
            get
            {
                return _uri;
            }

            set
            {
                _uri = value;
                _arr = new WPFImageArray(value, 16, 16);
            }
        }

        public ImageSource this[int index]
        {
            get
            {
                return _arr[index];
            }
        }

    }
    
    class IconSetItem
    {

        public IconSet Icons { get; set; }
        
        public int Index { get; set; }

        public ImageSource Item 
        {
            get
            {
                return Icons[Index];
            }
        }

    }

    class Image<TImage> : AbstractImage
    {
        public Image(TImage image) : base(image)
        {

        }

        public TImage GetPlatformImage()
        {
            return (TImage)m_Image;
        }
    }

    [ValueConversion(typeof(AbstractImage), typeof(ImageSource))]
    internal class IconTypeConverter : IValueConverter 
    {

        public IconTypeConverter() { }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is AbstractImage))
                return null;

            Image<ImageSource> source = (Image<ImageSource>)value;

            return source.GetPlatformImage();

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }

}