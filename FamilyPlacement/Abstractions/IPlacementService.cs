using CSharpFunctionalExtensions;
using FamilyPlacement.Models;

namespace FamilyPlacement.Abstractions
{
    public interface IPlacementService
    {
        Result Place(TreeType selectedFurnitureType, int count);
    }
}
