using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Xamarin.Forms.Maps;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage
    {
        public MainPage()
        {
            var MY_MAP_KEY = "9diqx2Yylc2nGObz2vJe~72boTGI_Y95nNZXXwDeTfw~AgeKkBxU76GeWMtJT8L1T6pDZ48q_OmQj8jalKdp8GDIvs6oHnPWngqusYPsbTPV";
            this.InitializeComponent();

            Xamarin.FormsMaps.Init(MY_MAP_KEY);

            LoadApplication(new hutchmap.App());
        }
    }
}