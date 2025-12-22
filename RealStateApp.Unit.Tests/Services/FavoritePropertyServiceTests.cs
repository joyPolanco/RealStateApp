
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RealStateApp.Core.Application.Dtos.FavoriteProperty;
using RealStateApp.Core.Application.Mappings.DtosAndViewModels;
using RealStateApp.Core.Application.Mappings.EntitiesAndDtos;
using RealStateApp.Core.Application.Services;
using RealStateApp.Core.Domain.Common.Enums;
using RealStateApp.Core.Domain.Entities;
using RealStateApp.Infraestructure.Identity.Contexts;
using RealStateApp.Infraestructure.Identity.Entities;
using RealStateApp.Infraestructure.Persistence.Contexts;
using RealStateApp.Infraestructure.Persistence.Repositories;

namespace RealStateApp.Unit.Tests.Services
{
    public class FavoritePropertyServiceTests
    {



        private readonly DbContextOptions<RealStateContext> dbContextOptions;
        private readonly DbContextOptions<IdentityContext> IdentitydbContextOptions;
        private readonly IMapper _mapper;




        public FavoritePropertyServiceTests()
        {

            dbContextOptions = new DbContextOptionsBuilder<RealStateContext>()
                .UseInMemoryDatabase(databaseName: $"dbTestFavoriteProperty_{Guid.NewGuid()}")
                .Options;



            IdentitydbContextOptions = new DbContextOptionsBuilder<IdentityContext>()
                .UseInMemoryDatabase(databaseName: $"IdentitydbTestFavoriteProperty_{Guid.NewGuid()}")
                .Options;



            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });


            var config = new MapperConfiguration(cf =>
            {

                cf.AddProfile<FavoritePropertyToDtoMappingProfile>();
                cf.AddProfile<FavoritePropertyDtosAndMappingProfile>();

            }, loggerFactory);


            _mapper = config.CreateMapper();
        }



        #region private methods CreateService
        private FavoritePropertyService CreateService()
        {

            var context = new RealStateContext(dbContextOptions);

            var repo = new FavoritePropertyRepoitory(context);
            var Propertyrepo = new PropertyRepository(context);

            var service = new FavoritePropertyService(repo, _mapper, Propertyrepo);
            return service;


        }

        #endregion





        #region private method creacion de usuario
        private async Task<List<AppUser>> CreateUsers()
        {
            var passwordHasher = new PasswordHasher<AppUser>();
            var Identitycontext = new IdentityContext(IdentitydbContextOptions);

            var users = new List<AppUser>
            {
                new() { Id = Guid.NewGuid().ToString(), FirstName = "Kelvin", LastName = "Ramirez", Email = "Kelvin@gmail.com", EmailConfirmed = true, IsActive = true, PhoneNumber = "58930003", UserName = "KIONoudes"},
                new() { Id = Guid.NewGuid().ToString(), FirstName = "Kodak", LastName = "Diaz", Email = "Kodak@gmail.com", EmailConfirmed = true, IsActive = true, PhoneNumber = "58930003", UserName = "KIONoudes", },
                new() { Id = Guid.NewGuid().ToString(), FirstName = "KIONoudes", LastName = "Ramirez", Email = "KIO@gmail.com", EmailConfirmed = true, IsActive = true, PhoneNumber = "829303224", UserName = "KIONousKey",},
                new() { Id = Guid.NewGuid().ToString(), FirstName = "Agent2", LastName = "UserAgent", Email = "Agent2@gmail.com", EmailConfirmed = true, IsActive = true, PhoneNumber = "8493034523", UserName = "AgentKey",},
            };


            foreach (var user in users)
            {
                int d = 1;
                user.PasswordHash = passwordHasher.HashPassword(user, user.PasswordHash = $"@kelvn3{d++}");
                d++;
            }



            await Identitycontext.Users.AddRangeAsync(users);
            await Identitycontext.SaveChangesAsync();
            var _UserInDatabase = await Identitycontext.Users.ToListAsync();


            //creamos roless
            var roles = new List<IdentityRole>
             {

                 new()  { Id = Guid.NewGuid().ToString(), Name = $"{AppRoles.AGENT.ToString()}", NormalizedName = $"{AppRoles.AGENT.ToString()}" },
                 new()  { Id = Guid.NewGuid().ToString(), Name = $"{AppRoles.CLIENT.ToString()}", NormalizedName = $"{AppRoles.CLIENT.ToString()}" },
             };


            await Identitycontext.Roles.AddRangeAsync(roles);
            await Identitycontext.SaveChangesAsync();
            var rolesInDatabase = await Identitycontext.Roles.ToListAsync();


            //asignamos roles
            var userRoles = new List<IdentityUserRole<string>>()
            {


                new() { RoleId = rolesInDatabase.FirstOrDefault(s => s.Name == "AGENT" )!.Id, UserId = _UserInDatabase.FirstOrDefault(s => s.FirstName == "Kelvin")!.Id},
                new() { RoleId = rolesInDatabase.FirstOrDefault(s => s.Name == "CLIENT")!.Id, UserId = _UserInDatabase.FirstOrDefault(s => s.FirstName == "Kodak")!.Id},
                new() { RoleId = rolesInDatabase.FirstOrDefault(s => s.Name == "CLIENT")!.Id, UserId = _UserInDatabase.FirstOrDefault(s => s.FirstName == "KIONoudes")!.Id},
                new() { RoleId = rolesInDatabase.FirstOrDefault(s => s.Name == "AGENT")!.Id, UserId = _UserInDatabase.FirstOrDefault(s => s.FirstName == "Agent2")!.Id},
            };


            await Identitycontext.UserRoles.AddRangeAsync(userRoles);
            await Identitycontext.SaveChangesAsync();
            var userRolesInDatabase = await Identitycontext.UserRoles.ToListAsync();
            return _UserInDatabase;
        }
        #endregion

        #region CreateProperty private method 
        public async Task<(List<Property>, List<AppUser>)> CreateProperty()
        {
            var context = new RealStateContext(dbContextOptions);
            var _UserInDatabase = await CreateUsers();

            //creamos tipo de propiedad
            var PropertyTypeRepo = new PropertyTypeRepository(context);
            var propertyType = await PropertyTypeRepo.AddAsync(new PropertyType() { Id = 0, Name = "Apartamento", Description = "Es una partamento amueblado...." });


            //creamos tipo de ventas
            var SaleTypeRepo = new SaleTypeRepository(context);
            var saleType = await SaleTypeRepo.AddAsync(new SaleType() { Id = 0, Name = "Venta", Description = "Venta de apartamento full, negociable" });


            //creamos mejoras que se le puede hacer a la propiedad
            var ImprovementRepo = new ImprovementRepository(context);
            var Improvements = new List<Improvement>
            {
                new() { Id = 0, Name = "Cocina", Description = "Se require hacer arreglo minimo en la cocina, por filtro de ...."},
                new() { Id = 0, Name = "Baño", Description = "Se require hacer arreglo minimo en la cocina, por filtro de agua...."},
                new() { Id = 0, Name = "Area de lavado", Description = "Se requiere cambiar algunas tuverias..."}
            };

            await ImprovementRepo.AddRangeAsync(Improvements);
            var entitiesImprovement = await ImprovementRepo.GetAllList();


            //creamo la propiedad
            var properties = new List<Property>
            {
                new()
                {

                Id = 0,
                AgentId = _UserInDatabase.FirstOrDefault(s => s.FirstName == "Kelvin")!.Id,
                Bathrooms = 3,
                Bedrooms = 6,
                Code = "P09DDOD",
                Description = "Apartamento con opcion de compra negociable.....",
                Price = 1500000000,
                SizeInMeters = 150.90,
                SaleTypeId = saleType.Id,
                PropertyTypeId = propertyType.Id,
                Status = PropertyStatus.Available

                },
                new()
                {

                 Id = 0,
                 AgentId = _UserInDatabase.FirstOrDefault(s => s.FirstName == "Kelvin")!.Id,
                 Bathrooms = 2,
                 Bedrooms = 6,
                 Code = "00PDOK",
                 Description = "A una casa para arquiler muy amplia y con opcion de compra.....",
                 Price = 100000000,
                 SizeInMeters = 150.90,
                 SaleTypeId = saleType.Id,
                 PropertyTypeId = propertyType.Id,
                 Status = PropertyStatus.Available
                },
                new()
                {


                 Id = 0,
                 AgentId = _UserInDatabase.FirstOrDefault(s => s.FirstName == "Kelvin")!.Id,
                 Bathrooms = 5,
                 Bedrooms = 10,
                 Code = "MAP-3920",
                 Description = "Es Una mansion.....",
                 Price = 400000000,
                 SizeInMeters = 150.90,
                 SaleTypeId = saleType.Id,
                 PropertyTypeId = propertyType.Id,
                 Status = PropertyStatus.Available

                },
                 new()
                {
                 Id = 0,
                 AgentId = _UserInDatabase.First(s => s.FirstName == "Agent2")!.Id,
                 Bathrooms = 2,
                 Bedrooms = 6,
                 Code = "MIO-9090",
                 Description = "Es un apartamento muy amplio con opcion de ventas.....",
                 Price = 1200000000,
                 SizeInMeters = 190.90,
                 SaleTypeId = saleType.Id,
                 PropertyTypeId = propertyType.Id,
                 Status = PropertyStatus.Available
                }

            };

            var repo = new PropertyRepository(context);
            await repo.AddRangeAsync(properties);
            var ListPropery = await repo.GetAllList();

            //asignamos mejoras a la propiedad

            var PropertyImprovement = new List<PropertyImprovement>
            {

                new() { ImprovementId = entitiesImprovement!.FirstOrDefault(s => s.Name == "Cocina")!.Id, PropertyId = ListPropery!.First(s => s.Code == "P09DDOD")!.Id},
                new() { ImprovementId = entitiesImprovement!.FirstOrDefault(s => s.Name == "Baño")!.Id, PropertyId =ListPropery!.First(s => s.Code == "P09DDOD")!.Id},
                new() { ImprovementId = entitiesImprovement!.FirstOrDefault(s => s.Name == "Area de lavado")!.Id, PropertyId =ListPropery!.First(s => s.Code == "P09DDOD")!.Id},



                new() { ImprovementId = entitiesImprovement!.FirstOrDefault(s => s.Name == "Cocina")!.Id, PropertyId = ListPropery!.First(s => s.Code == "00PDOK")!.Id},
                new() { ImprovementId = entitiesImprovement!.FirstOrDefault(s => s.Name == "Baño")!.Id, PropertyId =ListPropery!.First(s => s.Code == "00PDOK")!.Id},
                new() { ImprovementId = entitiesImprovement!.FirstOrDefault(s => s.Name == "Area de lavado")!.Id, PropertyId =ListPropery!.First(s => s.Code == "00PDOK")!.Id},



                new() { ImprovementId = entitiesImprovement!.FirstOrDefault(s => s.Name == "Cocina")!.Id, PropertyId = ListPropery!.First(s => s.Code == "MAP-3920")!.Id},
                new() { ImprovementId = entitiesImprovement!.FirstOrDefault(s => s.Name == "Baño")!.Id, PropertyId =ListPropery!.First(s => s.Code == "MAP-3920")!.Id},
                new() { ImprovementId = entitiesImprovement!.FirstOrDefault(s => s.Name == "Area de lavado")!.Id, PropertyId =ListPropery!.First(s => s.Code == "MAP-3920")!.Id},


                new() { ImprovementId = entitiesImprovement!.FirstOrDefault(s => s.Name == "Cocina")!.Id, PropertyId = ListPropery!.First(s => s.Code == "MIO-9090")!.Id},
                new() { ImprovementId = entitiesImprovement!.FirstOrDefault(s => s.Name == "Baño")!.Id, PropertyId =ListPropery!.First(s => s.Code == "MIO-9090")!.Id},
                new() { ImprovementId = entitiesImprovement!.FirstOrDefault(s => s.Name == "Area de lavado")!.Id, PropertyId =ListPropery!.First(s => s.Code == "MIO-9090")!.Id},

         
            };

            var PropertyImprovementRepo = new PropertyImprovementRepository(context);
            await PropertyImprovementRepo.AddRangeAsync(PropertyImprovement);

            var RepoPhoto = new PropertyPhotoRepository(context);
            var photos = new List<PropertyPhoto>
            {
                new() { Id = 0, ImageUrl = "https://cdn.pixabay.com/photo/2016/08/05/17/32/new-1572747_1280.jpg", PropertyId = ListPropery!.First(s => s.Code == "P09DDOD")!.Id},
                new() { Id = 0, ImageUrl = "https://img.freepik.com/fotos-premium/fondo-imagen-inmobiliaria-hermosa-vista-frontal-casa_800563-4673.jpg", PropertyId = ListPropery!.First(s => s.Code == "P09DDOD")!.Id},
                new() { Id = 0, ImageUrl = "https://cdn.pixabay.com/photo/2017/07/03/21/35/house-2469067_1280.jpg", PropertyId = ListPropery!.First(s => s.Code == "MIO-9090")!.Id},
                new() { Id = 0, ImageUrl = "https://cdn.pixabay.com/photo/2016/07/25/17/02/new-home-1540871_1280.jpg", PropertyId = ListPropery!.First(s => s.Code == "MIO-9090")!.Id},
            };

            await RepoPhoto.AddRangeAsync(photos);

            return (ListPropery!, _UserInDatabase);
        }
        #endregion



        [Fact]
        public async Task AddAsync_Should_Return_Dto_When_Added()
        {

            //arrange
            var service = CreateService();

            (var propertyInDatabase, var UserInDatabase) = await CreateProperty();

            var client = UserInDatabase.First(s => s.FirstName == "Kodak");
            var agent = UserInDatabase.First(s => s.FirstName == "Kelvin");
            var property = propertyInDatabase.First(s => s.AgentId == agent.Id);


            var favorite = new CreateFavoritePropertyDto() { Id = 0, ClientId = client.Id, PropertyId = property.Id }; 

            //act
            var result = await service.AddAsync(favorite);



            //assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);

        }



        [Fact]
        public async Task AddAsync_Should_Return_null_When_Not_Addede()
        {


            //arrange
            var service = CreateService();


            //act
            var result = await service.AddAsync(null!);

            //assert
            result.Should().BeNull();

        }




        [Fact]
        public async Task GetByIdAsync_Should_Return_FavoriteProperty_When_Exist()
        {


            //arrange
            var service = CreateService();


            (var propertyInDatabase, var UserInDatabase) = await CreateProperty();

            var client = UserInDatabase.First(s => s.FirstName == "Kodak");
            var agent = UserInDatabase.First(s => s.FirstName == "Kelvin");
            var property = propertyInDatabase.First(s => s.AgentId == agent.Id);


            var favorite = new CreateFavoritePropertyDto() { Id = 0, ClientId = client.Id, PropertyId = property.Id };


            //act
            var Savefavorite = await service.AddAsync(favorite);
            var result = await service.GetByIdAsync(Savefavorite!.Id);

            //assert
            result.Should().NotBeNull();
            result.Id.Should().Be(Savefavorite.Id);
            result.PropertyId.Should().Be(Savefavorite.PropertyId);

        }




        [Fact]
        public async Task GetByIdAsync_Should_Return_null_When_Not_Exist()
        {


            //arrange
            var service = CreateService();

            //act
            var result = await service.GetByIdAsync(999);


            //assert
            result.Should().BeNull();


        }




        [Fact]
        public async Task DeleteAsync_Should_Return_null_When_Exist()
        {


            //arrange
            var service = CreateService();


            (var propertyInDatabase, var UserInDatabase) = await CreateProperty();



            var client = UserInDatabase.First(s => s.FirstName == "Kodak");
            var agent = UserInDatabase.First(s => s.FirstName == "Kelvin");
            var property = propertyInDatabase.First(s => s.AgentId == agent.Id);


            var favorite = new CreateFavoritePropertyDto() { Id = 0, ClientId = client.Id, PropertyId = property.Id };


            //act
            var SaveFavorite = await service.AddAsync(favorite);
            await service.DeleteAsync(SaveFavorite!.Id);
            var result = await service.GetByIdAsync(SaveFavorite.Id);


            //assert
            result.Should().BeNull();


        }



        [Fact]
        public async Task DeleteAsync_Should_Return_Throw_When_Not_Exist()
        {
            // arrange
            var service = CreateService();


            // act
            Func<Task> act = async () => await service.DeleteAsync(999);

            // assert
            await act.Should().ThrowAsync<Exception>();


        }




        [Fact]
        public async Task UpdateAsync_Should_Return_DTo_When_Updated()
        {
            var service = CreateService();


            (var propertyInDatabase, var UserInDatabase) = await CreateProperty();



            var client = UserInDatabase.First(s => s.FirstName == "Kodak");
            var client2 = UserInDatabase.First(s => s.FirstName == "KIONoudes");
            var agent = UserInDatabase.First(s => s.FirstName == "Kelvin");
            var property = propertyInDatabase.First(s => s.AgentId == agent.Id);


            var favorite = new CreateFavoritePropertyDto() { Id = 0, ClientId = client.Id, PropertyId = property.Id };


            //act
            var SaveFavorite = await service.AddAsync(favorite);
            SaveFavorite!.ClientId = client2.Id;
            var result = await service.UpdateAsync(SaveFavorite.Id, SaveFavorite);



            //Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(SaveFavorite.Id);
            result.ClientId.Should().Be(SaveFavorite.ClientId);
        }





        [Fact]
        public async Task UpdateAsync_Should_Return_null_When_Not_Updated()
        {
            var service = CreateService();


            (var propertyInDatabase, var UserInDatabase) = await CreateProperty();


            var client = UserInDatabase.First(s => s.FirstName == "Kodak");
            var client2 = UserInDatabase.First(s => s.FirstName == "KIONoudes");
            var agent = UserInDatabase.First(s => s.FirstName == "Kelvin");
            var property = propertyInDatabase.First(s => s.AgentId == agent.Id);


            var favorite = new CreateFavoritePropertyDto() { Id = 0, ClientId = client.Id, PropertyId = property.Id };


            //act
            var result = await service.UpdateAsync(favorite.Id, favorite);

            //Assert
            result.Should().BeNull();

        }




        [Fact]
        public async Task GetAllLIst_Should_Return_FavoriteProperty_When_Exists()
        {
            var service = CreateService();


            (var propertyInDatabase, var UserInDatabase) = await CreateProperty();


            var client = UserInDatabase.First(s => s.FirstName == "Kodak");
            var client2 = UserInDatabase.First(s => s.FirstName == "KIONoudes");
            var agent = UserInDatabase.First(s => s.FirstName == "Kelvin");
            var property = propertyInDatabase.First(s => s.AgentId == agent.Id);



            var Favorities = new List<CreateFavoritePropertyDto>
            {
                new() {   Id = 0, ClientId = client.Id, PropertyId = property.Id },
                new() {   Id = 0, ClientId = client2.Id, PropertyId = property.Id }
              
            };



            //act
            foreach (var Favorite in Favorities)
            {
                var saveFavorite = await service.AddAsync(Favorite);
            }

            var result = await service.GetAllList();


            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().HaveCountGreaterThan(1);
        }





        [Fact]
        public async Task GetAllLIst_Should_Return_Empty_When_Not_Exists()
        {


            //Arrange
            var service = CreateService();

            //act

            var result = await service.GetAllList();


            //Assert
            result.Should().BeEmpty();
            result.Should().HaveCount(0);
        }




        ///// <summary>
        ///// Test que hay que darle revision
        ///// </summary>
   
        //[Fact]
        //public async Task GetListPropertyFavoriteAsync_Should_Return_all_Favority_When_Exists()
        //{


        //    //Arrange
        //    var service = CreateService();


        //    (var propertyInDatabase, var UserInDatabase) = await CreateProperty();

        //    var client = UserInDatabase.First(s => s.FirstName == "Kodak");
        //    var agent = UserInDatabase.First(s => s.FirstName == "Kelvin");
        //    var agent2 = UserInDatabase.First(s => s.FirstName == "Agent2");
        //    var property = propertyInDatabase.First(s => s.AgentId == agent.Id);
        //    var property2 = propertyInDatabase.First(s => s.AgentId == agent2.Id);



        //    var Favorities = new List<CreateFavoritePropertyDto>
        //    {
        //        new() {   Id = 0, ClientId = client.Id, PropertyId = property.Id },
        //        new() {   Id = 0, ClientId = client.Id, PropertyId = property2.Id }

        //    };



        //    //act

        //    foreach (var Favorite in Favorities)
        //    {
        //        var saveFavorite = await service.AddAsync(Favorite);
        //    }

        //    var result = await service.GetListPropertyFavoriteAsync(client.Id);


        //    //Assert
        //    result.Should().NotBeEmpty();
        //    result.Should().HaveCount(2);
        //    result.Should().HaveCountGreaterThan(0);

        //}





        [Fact]
        public async Task GetListPropertyFavoriteAsync_Should_Return_Empty__When_Not_Exists()
        {


            //Arrange
            var service = CreateService();


            (var propertyInDatabase, var UserInDatabase) = await CreateProperty();

            var client = UserInDatabase.First(s => s.FirstName == "Kodak");
            var agent = UserInDatabase.First(s => s.FirstName == "Kelvin");
            var agent2 = UserInDatabase.First(s => s.FirstName == "Agent2");
            var property = propertyInDatabase.First(s => s.AgentId == agent.Id);
            var property2 = propertyInDatabase.First(s => s.AgentId == agent2.Id);



  
            //act
            var result = await service.GetListPropertyFavoriteAsync(client.Id);


            //Assert
            result.Should().BeEmpty();
            result.Should().HaveCount(0);
         

        }


      








    }
}








    

