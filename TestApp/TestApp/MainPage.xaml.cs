using Magnolia.Xamarin.Forms.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TestApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            Assembly assembly = Assembly.GetExecutingAssembly();
            this.regImage.Source = ImageSource.FromResource(ResizableImage.GetFullResourceName(assembly, "attach.png"));
        }
    }
}
