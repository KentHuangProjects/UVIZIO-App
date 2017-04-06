using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BLE.Client.ViewModels
{
    public class Mode : INotifyPropertyChanged
    {

        public static string OFF = "00";
        public static string STATIC = "01";
        public static string BLINK = "02";
        public static string FADE = "03";
        public static string FRAMES = "04";
        public static string RAINBOW = "05";
        public static string COLOR_STROBE = "06";
        public static string COLOR_WALK = "07";
        public static string FIRE_PIXEL = "08";

        public static string STATIC_RED = STATIC + " 00 00 00 FF 00 00";
        public static string STATIC_GREEN = STATIC + " 00 00 00 00 FF 00";
        public static string STATIC_BLUE = STATIC + " 00 00 00 00 00 FF";

        public static string BLINK_PURPLE = BLINK + " 00 FF 01 FF 00 FF";

        private string name;
        private string backgroundImageSrc;
        private string selectedImageSrc;
        private string bleWritingText;
        

        public event PropertyChangedEventHandler PropertyChanged;

        public Mode(string name, string backgroundImageSrc, string selectedImageSrc,string bleWritingText)
        {
            this.name = name;
            this.backgroundImageSrc = backgroundImageSrc;
            this.selectedImageSrc = selectedImageSrc;
            this.bleWritingText = bleWritingText;
        }

        public string BleWritingText
        {
            get
            {
                return bleWritingText;
            }
            set
            {
                bleWritingText = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        public string BackgroundImageSrc
        {
            get
            {
                return backgroundImageSrc;
            }
            set
            {
                backgroundImageSrc = value;
            }
        }

        public string SelectedImageSrc
        {
            get
            {
                return selectedImageSrc;
            }
            set
            {
                if(selectedImageSrc != value)
                {
                    selectedImageSrc = value;

                    if(PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SelectedImageSrc"));
                    }
                }
            }
        }
    }
}
