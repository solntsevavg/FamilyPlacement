using Autodesk.Revit.DB;
using CSharpFunctionalExtensions;
using FamilyPlacement.Abstractions;
using FamilyPlacement.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FamilyPlacement.Services
{
    public class PlacementService : IPlacementService
    {
        private readonly Document _document;
        private const double DEFAULT_SPACING_METERS = 3.0; // Расстояние между деревьями

        public PlacementService(Document document)
        {
            _document = document;
        }

        public Result Place(TreeType treeType, int count)
        {
            return Validate(count)
                .Bind(() => FindFamily(treeType))
                .Bind(symbol => PlaceInstancesInGrid(symbol, count));
        }

        private Result Validate(int count)
        {
            if (count <= 0)
                return Result.Failure("Количество должно быть больше 0");
            if (count > 100)
                return Result.Failure("Количество не должно превышать 100");
            return Result.Success();
        }

        private Result<FamilySymbol> FindFamily(TreeType treeType)
        {
            string treeName = GetTreeFamilyName(treeType);

            FamilySymbol familySymbol = new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_Planting)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .FirstOrDefault(x => x.FamilyName.IndexOf(treeName, StringComparison.OrdinalIgnoreCase) >= 0);

            if (familySymbol == null)
            {
                // Если не нашли по имени, попробуем любой символ посадки
                familySymbol = new FilteredElementCollector(_document)
                    .OfCategory(BuiltInCategory.OST_Planting)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .FirstOrDefault();

                if (familySymbol == null)
                    return Result.Failure<FamilySymbol>("Не найдено семейство деревьев в проекте");
            }

            return familySymbol;
        }

        private string GetTreeFamilyName(TreeType treeType)
        {
            switch (treeType)
            {
                case TreeType.Oak: return "Дуб";
                case TreeType.Birch: return "Береза";
                case TreeType.Pine: return "Сосна";
                case TreeType.Spruce: return "Ель";
                default: return "Дерево";
            }
        }

        private Result PlaceInstancesInGrid(FamilySymbol familySymbol, int count)
        {
            try
            {
                double spacing = UnitUtils.ConvertToInternalUnits(DEFAULT_SPACING_METERS, DisplayUnitType.DUT_METERS);

                // Рассчитываем размер сетки (квадрат)
                int gridSize = (int)Math.Ceiling(Math.Sqrt(count));

                var points = CalculateGridPoints(count, gridSize, spacing);

                var level = GetLevel();
                if (level == null)
                    return Result.Failure("Не удалось определить уровень для размещения");

                using (Transaction transaction = new Transaction(_document, "Размещение деревьев"))
                {
                    transaction.Start();

                    if (!familySymbol.IsActive)
                    {
                        familySymbol.Activate();
                    }

                    foreach (var point in points)
                    {
                        try
                        {
                            _document.Create.NewFamilyInstance(
                                point,
                                familySymbol,
                                level,
                                Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        }
                        catch (Exception ex)
                        {
                            // Продолжаем размещать остальные деревья
                            continue;
                        }
                    }

                    transaction.Commit();
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Ошибка при размещении: {ex.Message}");
            }
        }

        private List<XYZ> CalculateGridPoints(int count, int gridSize, double spacing)
        {
            var points = new List<XYZ>();

            // Начинаем с центра
            double startX = -((gridSize - 1) * spacing) / 2;
            double startY = -((gridSize - 1) * spacing) / 2;

            int placed = 0;
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    if (placed >= count) break;

                    double x = startX + col * spacing;
                    double y = startY + row * spacing;
                    points.Add(new XYZ(x, y, 0));
                    placed++;
                }

                if (placed >= count) break;
            }

            return points;
        }

        private Level GetLevel()
        {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .FirstOrDefault();
        }
    }
}
