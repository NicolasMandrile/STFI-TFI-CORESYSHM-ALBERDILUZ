using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using CoreSysHM.Application.DTOs.Ventas;
using CoreSysHM.Application.Interfaces.Services;
using CoreSysHM.Domain.Exceptions;
using CoreSysHM.UnitTests.TestInfrastructure;
using Xunit;

namespace CoreSysHM.UnitTests.Maestros;

public class ClienteServiceTests : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly SqliteConnection _connection;

    public ClienteServiceTests()
    {
        (_services, _connection) = TestServiceFactory.Create();
    }

    public void Dispose()
    {
        _services.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task Crear_ConDniDuplicado_LanzaDuplicadoException()
    {
        using var scope = _services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IClienteService>();

        (await svc.CreateAsync(new CreateClienteDto { Nombre = "Uno", Apellido = "Test", Dni = "30111222" }, null))
            .Exitoso.Should().BeTrue();

        var act = async () => await svc.CreateAsync(new CreateClienteDto { Nombre = "Otro", Apellido = "Test", Dni = "30111222" }, null);

        await act.Should().ThrowAsync<DuplicadoException>();
    }

    [Fact]
    public async Task Actualizar_ConCuitYaUsadoPorOtroCliente_LanzaDuplicadoException()
    {
        using var scope = _services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IClienteService>();

        await svc.CreateAsync(new CreateClienteDto { Nombre = "A", Apellido = "A", Cuit = "20-30111222-3" }, null);
        var segundo = await svc.CreateAsync(new CreateClienteDto { Nombre = "B", Apellido = "B", Cuit = "20-40111222-3" }, null);

        var act = async () => await svc.UpdateAsync(segundo.Data!.Id,
            new CreateClienteDto { Nombre = "B", Apellido = "B", Cuit = "20-30111222-3" }, null);

        await act.Should().ThrowAsync<DuplicadoException>();
    }

    [Fact]
    public async Task Actualizar_ConSuPropioDni_NoLanzaError()
    {
        using var scope = _services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IClienteService>();

        var creado = await svc.CreateAsync(new CreateClienteDto { Nombre = "A", Apellido = "A", Dni = "30999888" }, null);

        var result = await svc.UpdateAsync(creado.Data!.Id,
            new CreateClienteDto { Nombre = "A", Apellido = "A", Dni = "30999888", Telefono = "111" }, null);

        result.Exitoso.Should().BeTrue();
    }

    [Fact]
    public async Task Completitud_SubeAlCompletarCamposRelevantes()
    {
        using var scope = _services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IClienteService>();

        var incompleto = await svc.CreateAsync(new CreateClienteDto { Nombre = "A", Apellido = "A" }, null);
        var completo = await svc.CreateAsync(new CreateClienteDto
        {
            Nombre = "B", Apellido = "B", Dni = "1", Email = "b@test.com", Telefono = "1",
            Direccion = "x", Localidad = "y"
        }, null);

        completo.Data!.Completitud.Should().BeGreaterThan(incompleto.Data!.Completitud);
    }

    [Fact]
    public async Task DarDeBaja_ConservaElRegistroPeroDejaDeListarse()
    {
        using var scope = _services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IClienteService>();

        var creado = await svc.CreateAsync(new CreateClienteDto { Nombre = "A", Apellido = "A" }, null);
        (await svc.DeleteAsync(creado.Data!.Id, null)).Exitoso.Should().BeTrue();

        (await svc.GetByIdAsync(creado.Data!.Id)).Exitoso.Should().BeFalse();
        (await svc.GetAllAsync()).Data.Should().NotContain(c => c.Id == creado.Data!.Id);
    }

    [Fact]
    public async Task Historial_RegistraAltaYModificacion()
    {
        using var scope = _services.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IClienteService>();

        var creado = await svc.CreateAsync(new CreateClienteDto { Nombre = "A", Apellido = "A" }, null);
        await svc.UpdateAsync(creado.Data!.Id, new CreateClienteDto { Nombre = "A", Apellido = "A", Telefono = "123" }, null);

        var historial = (await svc.GetHistorialAsync(creado.Data!.Id)).Data!.ToList();

        historial.Should().Contain(h => h.Accion == "Alta");
        historial.Should().Contain(h => h.Accion == "Modificacion");
    }
}
