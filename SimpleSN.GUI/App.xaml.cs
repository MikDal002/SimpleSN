using Ninject;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleSN.GUI
{
    internal class SiteModule : NinjectModule
    {
        public override void Load()
        {
            //Set up ninject bindings here.
            Bind<MainWindow>().ToSelf();
            Bind<MainWindowViewModel>().ToSelf();
        }
    }
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var kernel = new StandardKernel(new SiteModule());
            MainWindow = kernel.Get<MainWindow>();
            MainWindow.Show();
        }
    }
}
