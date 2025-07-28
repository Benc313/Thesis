using ThesisBackend.Services.CarService.Models;

namespace ThesisBackend.Services.CarService.Interfaces;

public interface ICarService
{
    Task<CarOperationResult> addCar(Domain.Messages.CarRequest carRequest, int userId);
    Task<CarOperationResult> editCar(Domain.Messages.CarRequest carRequest, int carId);
    Task<AllCarOperationResult> getAllCars(int userId);
    Task<CarOperationResult> deleteCar(int carId);
}