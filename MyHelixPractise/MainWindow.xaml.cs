using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using MahApps.Metro.Controls;
using MyHelixPractise.Data;
using System;
using System.Windows;


namespace MyHelixPractise
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        
        private MainViewModel mainViewModel;


        public MainWindow()
        {
            InitializeComponent();

            mainViewModel = new MainViewModel(this);
            DataContext = mainViewModel;

            Closed += (s, e) => {

                if (DataContext is IDisposable)
                {
                    (DataContext as IDisposable).Dispose();
                }

            };

            mainViewPort.AddHandler(Element3D.MouseDown3DEvent, new RoutedEventHandler((s, e) =>
            {
                var arg = e as MouseDown3DEventArgs;

                if (arg.HitTestResult == null)
                {
                    return;
                }

                
                if (arg.HitTestResult.ModelHit is SceneNode node && node.Tag is CustomDataModel vm)
                {
                    vm.Selected = !vm.Selected;
                }
                

            }));
            
        }           

    }
}
