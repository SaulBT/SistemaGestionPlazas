using Moq;
using SGPla.Models;
using SGPla.Models.DTOs.PeriodoEscolar;
using SGPla.Repositories.Interfaces;
using SGPla.Services.Implementations;
using SGPla.Validations.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SGPla.Tests.Services
{

    public class PeriodoEscolarServiceTest
    {
        private readonly PeriodoEscolarService _periodoEscolarService;
        private readonly Mock<IPeriodoEscolarRepository> _periodoEscolarRepositoryMock;
        private readonly Mock<IPeriodoEscolarValidator> _periodoEscolarValidatorMock;

        public PeriodoEscolarServiceTest()
        {
            _periodoEscolarRepositoryMock = new Mock<IPeriodoEscolarRepository>();
            _periodoEscolarValidatorMock = new Mock<IPeriodoEscolarValidator>();
            _periodoEscolarService = new PeriodoEscolarService(_periodoEscolarRepositoryMock.Object, _periodoEscolarValidatorMock.Object);
        }

        //CP-07-01
        [Fact]

        public async Task CrearPeriodoEscolar()
        {
            var dto = new CrearPeriodoEscolarDTO
            {
                Anio = 2024,
                Periodo = "Febrero-Julio"
            };

            var result = new Periodo
            {
                Codigo = "202551"
            };

            _periodoEscolarRepositoryMock.Setup(r => r.CrearAsync(It.IsAny<Periodo>()))
            .Callback<Periodo>(p => result = p)
            .ReturnsAsync(result);

            _periodoEscolarValidatorMock.Setup(v => v.ValidarCreacionAsync(dto)).Returns(Task.CompletedTask);

            var periodoEscolar = await _periodoEscolarService.CrearAsync(dto);

            Assert.NotNull(periodoEscolar);
            Assert.Equal(dto.Anio, periodoEscolar.Anio);
            Assert.Equal(dto.Periodo, periodoEscolar.Periodo);
            Assert.Equal(result.Codigo, periodoEscolar.Codigo);
        }

        //CP-07-06
        [Fact]

        public async Task EditarPeriodoEscolar()
        {
            var dto = new EditarPeriodoEscolarDTO
            {
                Codigo = "202551",
                Anio = 2024,
                Periodo = "Febrero-Julio"
            };
            var result = new Periodo
            {
                Codigo = "202551",

            };
            _periodoEscolarRepositoryMock.Setup(r => r.ActualizarAsync(It.IsAny<Periodo>()))
            .Callback<Periodo>(p => result = p)
            .ReturnsAsync(result);
            _periodoEscolarValidatorMock.Setup(v => v.ValidarEdicionAsync(dto)).Returns(Task.CompletedTask);
            var periodoEscolar = await _periodoEscolarService.EditarAsync(dto);
            Assert.NotNull(periodoEscolar);
            Assert.Equal(dto.Codigo, periodoEscolar.Codigo);
            Assert.Equal(dto.Anio, periodoEscolar.Anio);
            Assert.Equal(dto.Periodo, periodoEscolar.Periodo);
        }

        //CP-07-15
        [Fact]
        public async Task EliminarPeriodoEscolar()
        {
            int id = 1;
            _periodoEscolarRepositoryMock.Setup(r => r.EliminarAsync(id)).ReturnsAsync(true);
            _periodoEscolarValidatorMock.Setup(v => v.ValidarEliminarAsync(id)).Returns(Task.CompletedTask);
            var result = await _periodoEscolarService.EliminarAsync(id);
            Assert.True(result);
        }

        //CP-07-14
        [Fact]
        public async Task ObtenerPeriodoEscolarPorId()
        {
            int id = 1;
            var result = new Periodo
            {
                Codigo = "202551",

            };
            _periodoEscolarRepositoryMock.Setup(r => r.ObtenerPorIdAsync(id)).ReturnsAsync(result);
            _periodoEscolarValidatorMock.Setup(v => v.ValidarObtenerPorIdAsync(id)).Returns(Task.CompletedTask);
            var periodoEscolar = await _periodoEscolarService.ObtenerPorIdAsync(id);
            Assert.NotNull(periodoEscolar);
            Assert.Equal(result.Codigo, periodoEscolar.Codigo);

        }

        //CP-07-13
        [Fact]
        public async Task BuscarPeriodoEscolarPorFiltro()
        {
            var filtro = new BuscarPeriodoEscolarDTO
            {
                Anio = 2024,
                Periodo = "Febrero-Julio"
            };
            var result = new List<Periodo>
            {
                new Periodo
                {
                    Codigo = "202551",

                }
            };
            _periodoEscolarRepositoryMock.Setup(r => r.ObtenerPorFiltroAsync(filtro)).ReturnsAsync(result);
            _periodoEscolarValidatorMock.Setup(v => v.ValidarBusquedaPorFiltroAsync(filtro)).Returns(Task.CompletedTask);
            var periodosEscolares = await _periodoEscolarService.BuscarPorFiltroAsync(filtro);
            Assert.NotNull(periodosEscolares);
            Assert.Single(periodosEscolares);
            Assert.Equal(result[0].Codigo, periodosEscolares[0].Codigo);

        }

        //CP-07-12
        [Fact]
        public async Task ObtenerTodos()
        {
            var result = new List<Periodo>
            {
                new Periodo
                {
                    Codigo = "202551",
                },
                new Periodo
                {
                    Codigo = "202552",
                }
            };
            _periodoEscolarRepositoryMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(result);
            var periodosEscolares = await _periodoEscolarService.ObtenerTodosAsync();
            Assert.NotNull(periodosEscolares);
            Assert.Equal(2, periodosEscolares.Count);
            Assert.Equal(result[0].Codigo, periodosEscolares[0].Codigo);
            Assert.Equal(result[1].Codigo, periodosEscolares[1].Codigo);
        }

    }

}
