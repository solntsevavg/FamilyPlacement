using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FamilyPlacement.Abstractions;
using FamilyPlacement.Services;
using FamilyPlacement.ViewModels;
using FamilyPlacement.Views;
using Microsoft.Extensions.DependencyInjection;
using RxBim.Command.Revit;
using RxBim.Shared;
using System;

namespace FamilyPlacement
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Cmd : RxBimCommand
    {
        public PluginResult ExecuteCommand(IServiceProvider provider)
        {
            try
            {
                var uiApp = provider.GetRequiredService<UIApplication>();
                var document = uiApp.ActiveUIDocument?.Document;

                if (document == null)
                {
                    TaskDialog.Show("Ошибка", "Не открыт активный документ Revit");
                    return PluginResult.Failed;
                }

                var services = new ServiceCollection();
                ConfigureServices(services, document);

                var serviceProvider = services.BuildServiceProvider();
                var mainWindow = serviceProvider.GetRequiredService<MainWindow>();

                mainWindow.ShowDialog();

                return PluginResult.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка запуска плагина", ex.Message);
                return PluginResult.Failed;
            }
        }

        private void ConfigureServices(ServiceCollection services, Document document)
        {
            services.AddSingleton(document);
            services.AddSingleton<IPlacementService, PlacementService>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();
        }
    }
}
