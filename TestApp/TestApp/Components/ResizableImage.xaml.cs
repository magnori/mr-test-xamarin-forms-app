using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Reflection;
using Xamarin.Essentials;
using System.Windows.Input;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Magnolia.Xamarin.Forms.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ResizableImage : Image
    {
        #region BindableProperty: ResourceAssembly
        public Assembly ResourceAssembly {
            get
            {
                return (Assembly)this.GetValue(ResourceAssemblyProperty);
            }

            set
            {
                SetValue(ResourceAssemblyProperty, value);
            }
        }
        public static readonly BindableProperty ResourceAssemblyProperty = BindableProperty.Create(
            propertyName: "ResourceAssembly",
            returnType: typeof(Assembly),
            declaringType: typeof(ResizableImage),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: (BindableObject bindable, object oldValue, object newValue) =>
            {
                var self = (ResizableImage)bindable;
                self.CreateImage();
            }
        );
        #endregion

        #region BindableProperty: ResourceName
        // We need to pass in the resource name, not an ImageSource because
        // there doesn't seem to be a way to convert ImageSource to byte[]
        private string _resourceName;
        public string ResourceName
        {
            get
            {
                return (string)this.GetValue(ResourceNameProperty);
            }

            set
            {
                SetValue(ResourceNameProperty, value);
            }
        }
        public static readonly BindableProperty ResourceNameProperty = BindableProperty.Create(
            propertyName: "ResourceName",
            returnType: typeof(string),
            declaringType: typeof(ResizableImage),
            defaultValue: String.Empty,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: (BindableObject bindable, object oldValue, object newValue) =>
            {
                var self = (ResizableImage)bindable;
                self.CreateImage();
            }
        );
        #endregion

        #region BindableProperty: ImageWidth
        // BindableProperty: ResourceName
        private double _imageWidth;
        public double ImageWidth
        {
            get
            {
                return (double)this.GetValue(ImageWidthProperty);
            }

            set
            {
                SetValue(ImageWidthProperty, value);
            }
        }
        public static readonly BindableProperty ImageWidthProperty = BindableProperty.Create(
            propertyName: "ImageWidth",
            returnType: typeof(double),
            declaringType: typeof(ResizableImage),
            defaultValue: 0.0,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: (BindableObject bindable, object oldValue, object newValue) =>
            {
                var self = (ResizableImage)bindable;
                self.WidthRequest = (double)newValue;
                self.CreateImage();
            }
        );
        #endregion

        #region BindableProperty: ImageHeight
        private double _imageHeight;
        public double ImageHeight
        {
            get
            {
                return (double)this.GetValue(ImageHeightProperty);
            }

            set
            {
                SetValue(ImageHeightProperty, value);
            }
        }
        public static readonly BindableProperty ImageHeightProperty = BindableProperty.Create(
            propertyName: "ImageHeight",
            returnType: typeof(double),
            declaringType: typeof(ResizableImage),
            defaultValue: 0.0,
            defaultBindingMode: BindingMode.OneWay,
            propertyChanged: (BindableObject bindable, object oldValue, object newValue) =>
            {
                var self = (ResizableImage)bindable;
                self.HeightRequest = (double)newValue;
                self.CreateImage();
            }
        );
        #endregion

        #region Events
        public event EventHandler<EventArgs> Clicked;
        #endregion

        private bool _parentSourceSet = false;

        public ResizableImage()
        {
            InitializeComponent();

            // Add click event to image
            var tgr = new TapGestureRecognizer();
            tgr.Tapped += OnClicked;
            this.root.GestureRecognizers.Add(tgr);
        }

        protected virtual void OnClicked(object sender, EventArgs e)
        {
            Clicked?.Invoke(this, e);
        }


        //public static string GetResourceName(ImageSource value)
        //{
        //    string resourceName = string.Empty;
        //    if (value is FileImageSource) {
        //        var valueAs = value as FileImageSource;
        //        resourceName = valueAs.File;
                
        //    }
        //    else if (value is UriImageSource) {
        //        var valueAs = value as UriImageSource;
        //        resourceName = valueAs.Uri.ToString();
        //    }

        //    return resourceName;
        //}

        public async void CreateImage()
        {            
            // Make sure all properties are set before resizing image
            if ((this.ImageWidth > 0 || this.ImageHeight > 0) && !(this.ResourceName == String.Empty)) {

                Assembly assembly = Assembly.GetExecutingAssembly();
                var fullResourceName = ResizableImage.GetFullResourceName(assembly, this.ResourceName);

                var imageBytes = ResizableImage.ExtractResource(assembly, fullResourceName);


                await LoadFromBytes(imageBytes);
            }
        }
     

        public async Task LoadFromBytes(byte[] imageBytes)
        {
            if ((this.ImageWidth > 0 || this.ImageHeight > 0) && !(this.ResourceName == String.Empty)) { 
                //if (imageBytes == null)
                //{
                //    throw new Exception($"ResizableImage: Could not find resource '{ResourceName}'");
                //}

                // Resize image, taking into account pixel density, and set as image source
                var density = DeviceDisplay.MainDisplayInfo.Density;
                var width = this.ImageWidth * density;
                var height = this.ImageHeight * density;
                var resizedImageBytes = await ImageUtil.ResizeImage(imageBytes, (float)width, (float)height);
                
                this.Source = ImageSource.FromStream(() => new MemoryStream(resizedImageBytes));
                //base.Source = XamarinFormsUtil.ByteArrayToImageSource(resizedImageBytes);
                //this._parentSourceSet = true;
            }
        }

        public static byte[] ExtractResource(Assembly assembly, String fullResourceName)
        {            
            using (Stream resFilestream = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        public static string GetFullResourceName(Assembly assembly, String filename)
        {
            var resourceNames = assembly.GetManifestResourceNames();
            foreach (var resourceName in resourceNames)
            {
                if (resourceName.EndsWith(filename))
                {
                    return resourceName;
                }
            }

            return null;
        }

    }
}