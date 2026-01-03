
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CSharpFunctionalExtensions;
using FamilyPlacement.Abstractions;
using FamilyPlacement.Models;
using System;
using System.Collections.ObjectModel;

namespace FamilyPlacement.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly IPlacementService _placementService;
        private TreeType _selectedTreeType;
        private int _count = 9; // Значение по умолчанию
        private string _statusMessage;
        private string _gridInfo;

        public MainWindowViewModel(IPlacementService placementService)
        {
            PlaceCommand = new RelayCommand(PlaceTrees, CanPlaceTrees);

            TreeTypes = new ObservableCollection<TreeType>
            {
                TreeType.Oak,
                TreeType.Birch,
                TreeType.Pine,
                TreeType.Spruce
            };

            SelectedTreeType = TreeType.Oak;
            _placementService = placementService;

            UpdateGridInfo();
        }

        public ObservableCollection<TreeType> TreeTypes { get; }

        public TreeType SelectedTreeType
        {
            get => _selectedTreeType;
            set
            {
                SetProperty(ref _selectedTreeType, value);
                PlaceCommand.NotifyCanExecuteChanged();
            }
        }

        public int Count
        {
            get => _count;
            set
            {
                if (SetProperty(ref _count, value))
                {
                    PlaceCommand.NotifyCanExecuteChanged();
                    UpdateGridInfo();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string GridInfo
        {
            get => _gridInfo;
            set => SetProperty(ref _gridInfo, value);
        }

        public RelayCommand PlaceCommand { get; }

        private bool CanPlaceTrees()
        {
            return Count > 0 && Count <= 100;
        }

        private void PlaceTrees()
        {
            CSharpFunctionalExtensions.Result result = _placementService.Place(SelectedTreeType, Count);

            if (result.IsSuccess)
            {
                StatusMessage = $"Размещено {Count} деревьев в виде квадратной сетки";
                TaskDialog.Show("Размещение деревьев",
                    $"Успешно размещено {Count} деревьев!\n" +
                    $"Тип: {SelectedTreeType}\n" +
                    $"Сетка: {GetGridDescription(Count)}");
            }
            else
            {
                StatusMessage = $"Ошибка: {result.Error}";
                TaskDialog.Show("Ошибка размещения", result.Error);
            }
        }

        private void UpdateGridInfo()
        {
            GridInfo = GetGridDescription(Count);
        }

        private string GetGridDescription(int count)
        {
            if (count <= 0) return "Неверное количество";

            int gridSize = (int)Math.Ceiling(Math.Sqrt(count));
            int fullRows = count / gridSize;
            int remainder = count % gridSize;

            if (remainder == 0)
            {
                return $"Сетка {gridSize}×{gridSize} ({count} деревьев)";
            }
            else
            {
                return $"Сетка {gridSize}×{gridSize} ({fullRows} полных рядов + {remainder} в последнем)";
            }
        }
    }
}
