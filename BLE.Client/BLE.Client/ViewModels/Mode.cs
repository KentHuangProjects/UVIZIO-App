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
