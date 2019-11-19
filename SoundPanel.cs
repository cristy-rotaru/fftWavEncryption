using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace fftWavEncryption
{
    class SoundPanel : Border
    {
        private WavFormatManager soundObject;
        private String displayText;

        private Button encryptButton, decryptButton, playButton, saveButton;

        private Action encryptHandler, decryptHandler, playHandler, saveHandler;

        public SoundPanel(WavFormatManager wfm, String displayName)
        {
            this.encryptHandler = null;
            this.decryptHandler = null;
            this.playHandler = null;

            this.soundObject = wfm;
            this.displayText = displayName;

            Image encryptImage = new Image();
            Image decryptImage = new Image();
            Image playImage = new Image();
            Image saveImage = new Image();
            encryptImage.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/encrypt.png"));
            decryptImage.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/decrypt.png"));
            playImage.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/play.png"));
            saveImage.Source = new BitmapImage(new Uri(@"pack://application:,,,/Resources/save.png"));
            encryptImage.Stretch = Stretch.Uniform;
            decryptImage.Stretch = Stretch.Uniform;
            playImage.Stretch = Stretch.Uniform;
            saveImage.Stretch = Stretch.Uniform;
            encryptImage.Height = 45;
            decryptImage.Height = 45;
            playImage.Height = 45;
            saveImage.Height = 45;

            this.encryptButton = new Button();
            this.decryptButton = new Button();
            this.playButton = new Button();
            this.saveButton = new Button();
            this.encryptButton.Content = encryptImage;
            this.decryptButton.Content = decryptImage;
            this.playButton.Content = playImage;
            this.saveButton.Content = saveImage;
            this.encryptButton.ToolTip = "Encrypt this sound";
            this.decryptButton.ToolTip = "Decrypt this sound";
            this.playButton.ToolTip = "Play this sound";
            this.saveButton.ToolTip = "Save as .wav";
            this.encryptButton.Style = (Style)FindResource("ButtonStyleDarkMode");
            this.decryptButton.Style = (Style)FindResource("ButtonStyleDarkMode");
            this.playButton.Style = (Style)FindResource("ButtonStyleDarkMode");
            this.saveButton.Style = (Style)FindResource("ButtonStyleDarkMode");
            this.encryptButton.Margin = new Thickness(4);
            this.decryptButton.Margin = new Thickness(4);
            this.playButton.Margin = new Thickness(4);
            this.saveButton.Margin = new Thickness(4);
            this.encryptButton.Click += this.EncryptButton_Click;
            this.decryptButton.Click += this.DecryptButton_Click;
            this.playButton.Click += this.PlayButton_Click;
            this.saveButton.Click += this.SaveButton_Click;

            RowDefinition buttonGridRow0 = new RowDefinition();
            buttonGridRow0.Height = new GridLength(1, GridUnitType.Auto);
            ColumnDefinition buttonGridColumn0 = new ColumnDefinition();
            ColumnDefinition buttonGridColumn1 = new ColumnDefinition();
            ColumnDefinition buttonGridColumn2 = new ColumnDefinition();
            ColumnDefinition buttonGridColumn3 = new ColumnDefinition();
            buttonGridColumn0.Width = new GridLength(1, GridUnitType.Auto);
            buttonGridColumn1.Width = new GridLength(1, GridUnitType.Auto);
            buttonGridColumn2.Width = new GridLength(1, GridUnitType.Auto);
            buttonGridColumn3.Width = new GridLength(1, GridUnitType.Auto);

            Grid buttonGrid = new Grid();
            buttonGrid.HorizontalAlignment = HorizontalAlignment.Center;
            buttonGrid.RowDefinitions.Add(buttonGridRow0);
            buttonGrid.ColumnDefinitions.Add(buttonGridColumn0);
            buttonGrid.ColumnDefinitions.Add(buttonGridColumn1);
            buttonGrid.ColumnDefinitions.Add(buttonGridColumn2);
            buttonGrid.ColumnDefinitions.Add(buttonGridColumn3);
            Grid.SetRow(this.encryptButton, 0);
            Grid.SetRow(this.decryptButton, 0);
            Grid.SetRow(this.playButton, 0);
            Grid.SetRow(this.saveButton, 0);
            Grid.SetColumn(this.encryptButton, 0);
            Grid.SetColumn(this.decryptButton, 1);
            Grid.SetColumn(this.playButton, 2);
            Grid.SetColumn(this.saveButton, 3);
            buttonGrid.Children.Add(this.encryptButton);
            buttonGrid.Children.Add(this.decryptButton);
            buttonGrid.Children.Add(this.playButton);
            buttonGrid.Children.Add(this.saveButton);

            DockPanel dockPanelButtons = new DockPanel();
            dockPanelButtons.HorizontalAlignment = HorizontalAlignment.Stretch;
            dockPanelButtons.Children.Add(buttonGrid);

            TextBlock soundName = new TextBlock();
            soundName.FontSize = 20;
            soundName.HorizontalAlignment = HorizontalAlignment.Left;
            soundName.Margin = new Thickness(4);
            soundName.Text = displayName;

            RowDefinition gridRow0 = new RowDefinition();
            RowDefinition gridRow1 = new RowDefinition();
            gridRow0.Height = new GridLength(1, GridUnitType.Auto);
            gridRow1.Height = new GridLength(1, GridUnitType.Auto);
            ColumnDefinition gridColumn0 = new ColumnDefinition();
            gridColumn0.Width = new GridLength(1, GridUnitType.Star);

            Grid contentHolderGrid = new Grid();
            contentHolderGrid.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0));
            contentHolderGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            contentHolderGrid.RowDefinitions.Add(gridRow0);
            contentHolderGrid.RowDefinitions.Add(gridRow1);
            contentHolderGrid.ColumnDefinitions.Add(gridColumn0);
            Grid.SetRow(soundName, 0);
            Grid.SetRow(dockPanelButtons, 1);
            Grid.SetColumn(soundName, 0);
            Grid.SetColumn(dockPanelButtons, 0);
            contentHolderGrid.Children.Add(soundName);
            contentHolderGrid.Children.Add(dockPanelButtons);

            this.BorderThickness = new Thickness(1.5);
            this.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x91, 0x91, 0x91));
            this.Margin = new Thickness(5, 5, 5, 0);
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.Child = contentHolderGrid;
        }

        public WavFormatManager GetFormatManager()
        {
            return this.soundObject;
        }

        public String GetDisplayName()
        {
            return this.displayText;
        }

        private void EncryptButton_Click(object sender, RoutedEventArgs e)
        {
            this.encryptHandler?.Invoke();
        }

        private void DecryptButton_Click(object sender, RoutedEventArgs e)
        {
            this.decryptHandler?.Invoke();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            this.playHandler?.Invoke();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.saveHandler?.Invoke();
        }

        public void SetEncryptButtonHandler(Action handler)
        {
            this.encryptHandler = handler;
        }

        public void SetDecryptButtonHandler(Action handler)
        {
            this.decryptHandler = handler;
        }

        public void SetPlayButtonHandler(Action handler)
        {
            this.playHandler = handler;
        }

        public void SetSaveButtonHandler(Action handler)
        {
            this.saveHandler = handler;
        }
    }
}
