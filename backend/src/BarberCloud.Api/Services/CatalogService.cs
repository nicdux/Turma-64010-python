using BarberCloud.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace BarberCloud.Api.Services;

public sealed class CatalogService(BarberCloudDbContext dbContext)
{
    public async Task<IReadOnlyList<BarberView>> GetBarbersAsync(CancellationToken cancellationToken) =>
        await dbContext.Barbers.AsNoTracking()
            .OrderBy(item => item.DisplayName)
            .Select(item => new BarberView(item.BarberId, item.DisplayName, item.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<BarberView> CreateBarberAsync(
        CreateBarberRequest request,
        CancellationToken cancellationToken)
    {
        var barber = new Barber { DisplayName = request.DisplayName.Trim() };
        dbContext.Barbers.Add(barber);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new BarberView(barber.BarberId, barber.DisplayName, barber.IsActive);
    }

    public async Task<IReadOnlyList<ServiceView>> GetServicesAsync(CancellationToken cancellationToken) =>
        await dbContext.Services.AsNoTracking()
            .OrderBy(item => item.Name)
            .Select(item => new ServiceView(
                item.ServiceId,
                item.Name,
                item.DurationMinutes,
                item.Price,
                item.IsActive))
            .ToListAsync(cancellationToken);

    public async Task<ServiceView> CreateServiceAsync(
        CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var service = new ServiceOffering
        {
            Name = request.Name.Trim(),
            DurationMinutes = request.DurationMinutes,
            Price = request.Price
        };
        dbContext.Services.Add(service);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new ServiceView(service.ServiceId, service.Name, service.DurationMinutes, service.Price, service.IsActive);
    }
}

public sealed record CreateBarberRequest(string DisplayName);
public sealed record BarberView(Guid BarberId, string DisplayName, bool IsActive);
public sealed record CreateServiceRequest(string Name, short DurationMinutes, decimal Price);
public sealed record ServiceView(Guid ServiceId, string Name, short DurationMinutes, decimal Price, bool IsActive);
