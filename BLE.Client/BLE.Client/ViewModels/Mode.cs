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
        public string name;
        public string backgroundImageSrc;
        public string selectedImageSrc;

        public event PropertyChangedEventHandler PropertyChanged;

        public Mode(string name, string backgroundImageSrc, string selectedImageSrc)
        {
            this.name = name;
            this.backgroundImageSrc = backgroundImageSrc;
            this.selectedImageSrc = selectedImageSrc;
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
