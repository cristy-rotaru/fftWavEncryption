using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace fftWavEncryption
{
    class SoundPanel : Border
    {
        WavFormatManager soundObject;

        Button encryptButton, decryptButton, playButton;

        public SoundPanel(WavFormatManager wfm, String displayName)
        {
            this.soundObject = wfm;

            // todo:
            // add images to buttons
            // finish sound UI

            this.BorderThickness = new Thickness(1.5);
            this.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x91, 0x91, 0x91));

            Grid contentHolderGrid = new Grid();
            contentHolderGrid.Margin = new Thickness(5, 5, 5, 0);
            contentHolderGrid.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0));
            contentHolderGrid.Height = 100;
            contentHolderGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

            RowDefinition gridRow0 = new RowDefinition();
            RowDefinition gridRow1 = new RowDefinition();
            gridRow0.Height = new GridLength(1, GridUnitType.Auto);
            gridRow1.Height = new GridLength(1, GridUnitType.Auto);
            ColumnDefinition gridColumn0 = new ColumnDefinition();
            gridColumn0.Width = new GridLength(1, GridUnitType.Auto);

            contentHolderGrid.RowDefinitions.Add(gridRow0);
            contentHolderGrid.RowDefinitions.Add(gridRow1);
            contentHolderGrid.ColumnDefinitions.Add(gridColumn0);

            TextBlock soundName = new TextBlock();
            soundName.FontSize = 17;
            soundName.HorizontalAlignment = HorizontalAlignment.Left;
            soundName.Text = displayName;

            Grid buttonGrid = new Grid();
            buttonGrid.HorizontalAlignment = HorizontalAlignment.Center;

            RowDefinition buttonGridRow0 = new RowDefinition();
            buttonGridRow0.Height = new GridLength(1, GridUnitType.Auto);
            ColumnDefinition buttonGridColumn0 = new ColumnDefinition();
            ColumnDefinition buttonGridColumn1 = new ColumnDefinition();
            ColumnDefinition buttonGridColumn2 = new ColumnDefinition();
            buttonGridColumn0.Width = new GridLength(1, GridUnitType.Auto);
            buttonGridColumn1.Width = new GridLength(1, GridUnitType.Auto);
            buttonGridColumn2.Width = new GridLength(1, GridUnitType.Auto);

            buttonGrid.RowDefinitions.Add(buttonGridRow0);
            buttonGrid.ColumnDefinitions.Add(buttonGridColumn0);
            buttonGrid.ColumnDefinitions.Add(buttonGridColumn1);
            buttonGrid.ColumnDefinitions.Add(buttonGridColumn2);
        }
    }
}
