using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace App4
{
    
    public partial class PresetPatternFromTab : ContentPage
    {
        private ObservableCollection<Mode> modes;
        private Mode currentMode;

        public PresetPatternFromTab()
        {
            InitializeComponent();

            modes = new ObservableCollection<Mode>
            {
                new Mode("Rainbow", "bg_1.png", "mode_selected_icon.png"),

                new Mode("Colou Strobe", "bg_2.png", "mode_deselected_icon.png"),

                new Mode("Colou Walk", "bg_3.png", "mode_deselected_icon.png"),

                new Mode("Fire Pixel", "bg_4.png", "mode_deselected_icon.png"),
            };

            ModeList.ItemsSource = modes;
            currentMode = modes.ElementAt(0);
            //ModeList.SelectedItem = modes.ElementAt(0);

            ModeList.ItemSelected += PatternList_ItemSelected;
        }

        private void PatternList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem == null)
            {
                return;
            }

            Mode mode = (Mode) e.SelectedItem;

            if(mode != currentMode)
            {
                mode.SelectedImageSrc = "mode_selected_icon.png";
                currentMode.SelectedImageSrc = "mode_deselected_icon.png";

                currentMode = mode;
            }
            
        }
    }
}
