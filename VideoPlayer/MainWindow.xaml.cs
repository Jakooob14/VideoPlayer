using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using Unosquare.FFME.Common;

namespace VideoPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string ID = "Video Player";

        private bool _playing = false;

        public MainWindow()
        {
            Unosquare.FFME.Library.FFmpegDirectory = "./FFmpeg";

            InitializeComponent();

            MainVideoPlayerWindow.Title = ID;
            Controls.Visibility = Visibility.Hidden;
            VolumeLabel.Visibility = Visibility.Hidden;
            VolumeLabel.Content = Math.Round(Media.Volume * 100) + "%";

            Media.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "assets/videos/default.mkv", UriKind.Absolute));
            ToggleMediaPlayState(false);
            ToggleLoopState(MediaPlaybackState.Play);
        }

        private void ToggleMediaPlayState(bool? paused = null)
        {
            paused ??= Media.IsPlaying;

            if (paused.Value)
            {
                Media.Pause();
                ToggleMediaImage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "assets/icons/Play.png", 
                    UriKind.Absolute));
                _playing = false;
            }
            else
            {
                Media.Play();
                ToggleMediaImage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "assets/icons/Pause.png", 
                    UriKind.Absolute));
                _playing = true;
            }
        }

        private void ToggleLoopState(MediaPlaybackState? loop = null)
        {
            loop ??= Media.LoopingBehavior;

            if (loop == MediaPlaybackState.Play)
            {
                Media.LoopingBehavior = MediaPlaybackState.Manual;
                ToggleLoopImage.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "assets/icons/No Repeat.png", 
                    UriKind.Absolute));
            }
            else
            {
                Media.LoopingBehavior = MediaPlaybackState.Play;
                ToggleLoopImage.Source =
                    new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "assets/icons/Repeat.png",
                        UriKind.Absolute));
            }
        }

        public static Color ColorToRgbFromHsl(double hue, double saturation, double lightness)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            lightness = lightness * 255;
            byte v = Convert.ToByte(lightness);
            byte p = Convert.ToByte(lightness * (1 - saturation));
            byte q = Convert.ToByte(lightness * (1 - f * saturation));
            byte t = Convert.ToByte(lightness * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }





        private void MediaFailed(object sender, MediaFailedEventArgs e)
        {
            if (e.ErrorException.Message.Contains("Unable to load FFmpeg binaries from folder"))
            {
                MessageBox.Show("Please download FFmpeg and put it into an 'FFmpeg' folder", ID, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Open_OnClick(object sender, RoutedEventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() != true) return;
            Media.Open(new Uri(fileDialog.FileName, UriKind.Absolute));
        }

        private void Media_OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            ToggleMediaPlayState();
        }

        private void Media_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Controls.Visibility = Visibility.Visible;
        }

        private void Media_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Controls.Visibility = Visibility.Hidden;
        }

        private void Controls_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Controls.Visibility = Visibility.Visible;
        }

        private void Controls_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Controls.Visibility = Visibility.Hidden;
        }

        private void ToggleMedia_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleMediaPlayState();
        }

        private void Egg_OnClick(object sender, RoutedEventArgs e)
        {
            Media.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory + "assets/videos/( \u0361\u00b0 \u035cʖ \u0361\u00b0)", UriKind.Absolute));
        }

        DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private void Audio_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Media.Volume = e.NewValue;
            if (VolumeLabel != null)
            {
                VolumeLabel.Visibility = Visibility.Visible;
                VolumeLabel.Content = Math.Round(e.NewValue * 100) + "%";

                double coef = e.NewValue <= 1 ? e.NewValue : 1;
                VolumeLabel.Foreground = new SolidColorBrush(ColorToRgbFromHsl(100 - (coef * 100), coef is < 0.7 and > 0.3 ? coef + 0.3 : coef, 1));

                timer.Stop();
                timer.Start();
                timer.Tick += (sender, args) =>
                {
                    timer.Stop();
                    VolumeLabel.Visibility = Visibility.Hidden;
                };
            }
        }

        private bool seeking = false;

        private void Media_OnPositionChanged(object? sender, PositionChangedEventArgs e)
        {
            if (e.Position.TotalMilliseconds >= Media.NaturalDuration.Value.TotalMilliseconds)
            {
                ToggleMediaPlayState(true);
            }

            if (!seeking)
            {
                Seeker.Value = e.Position.TotalSeconds;
            }
        }

        private void Seeker_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!seeking) return;
            Media.Position = TimeSpan.FromSeconds(e.NewValue);
        }

        private void Media_OnMediaOpened(object? sender, MediaOpenedEventArgs e)
        {
            Seeker.Maximum = Media.NaturalDuration.Value.TotalSeconds;
        }

        private void Seeker_OnDragStarted(object sender, DragStartedEventArgs e)
        {
            seeking = true;
            Media.Pause();
        }

        private void Seeker_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            seeking = false;
            if (_playing)
            {
                Media.Play();
            }
        }

        private void SpeedControl_OnClick(object sender, RoutedEventArgs e)
        {
            Dialog dialog = new Dialog("Speed", "Input the speed modifier of the video. (1.5 / 2.9 / 1)", new Regex("[^\\d*\\.?\\d+$]+"));

            if (dialog.ShowDialog() != true) return;

            float speedRatio;
            try
            {
                speedRatio = Convert.ToSingle(dialog.Value);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                MessageBox.Show("Incorrect number. You can only use numbers here.", ID, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            Media.SpeedRatio = speedRatio;
        }

        private void ToggleLoop_OnClick(object sender, RoutedEventArgs e)
        {
            ToggleLoopState();
        }
    }
}