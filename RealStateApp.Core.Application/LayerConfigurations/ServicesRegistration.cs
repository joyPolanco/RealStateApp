using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RealStateApp.Core.Application.Behaviors;
using RealStateApp.Core.Application.Features.Agents.Commands.ChangeStatus;
using RealStateApp.Core.Application.Features.Agents.Commands.DeleteAgent;
using RealStateApp.Core.Application.Features.Agents.Queries.GetAgentProperties;
using RealStateApp.Core.Application.Features.Agents.Queries.GetById;
using RealStateApp.Core.Application.Features.Improvement.Commands.CreateImprovement;
using RealStateApp.Core.Application.Features.Improvement.Commands.DeleteImprovement;
using RealStateApp.Core.Application.Features.Improvement.Commands.EditImprovement;
using RealStateApp.Core.Application.Features.Improvement.Queries.GetById;
using RealStateApp.Core.Application.Features.Login.Commands;
using RealStateApp.Core.Application.Features.Property.Queries.GetByCode;
using RealStateApp.Core.Application.Features.PropertyType.Commands.Create;
using RealStateApp.Core.Application.Features.PropertyType.Commands.Edit;
using RealStateApp.Core.Application.Features.PropertyType.Queries.GetById;
using RealStateApp.Core.Application.Features.SaleType.Commands.CreateSaleType;
using RealStateApp.Core.Application.Features.SaleType.Commands.DeleteSaleType;
using RealStateApp.Core.Application.Features.SaleType.Commands.EditSaleType;
using RealStateApp.Core.Application.Features.SaleType.Queries.GetById;
using RealStateApp.Core.Application.Interfaces;
using RealStateApp.Core.Application.Services;
using System.Reflection;



namespace RealStateApp.Core.Application.LayerConfigurations
{
    public static class ServicesRegistration
    {

        public static void AddApplicationLayerIOC(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => { }, AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));

            services.AddScoped<IPropertyService, PropertyService>();
            services.AddScoped<IPropertyTypeService, PropertyTypeService>();

            services.AddScoped<IAdministrationService, AdministrationService>();
            services.AddScoped<IImpromentService, ImprovementService>();
            services.AddScoped<ISaleTypeService, SaleTypeService>();


            services.AddScoped<IFavoritePropertyService, FavoritePropertyService>();
            services.AddScoped<IOfferService, OfferService>();
            services.AddScoped<IHomeClienteService, HomeClientService>();
            services.AddScoped<IMessageService, MessageService>();
            services.AddScoped<IAgentService, AgentService>();



            services.AddMediatR(opt => opt.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
            //Improvemt
            services.AddValidatorsFromAssembly(typeof(CreateImprovementCommandValidator).Assembly);
            services.AddValidatorsFromAssembly(typeof(EditImprovementCommandValidator).Assembly);
            services.AddValidatorsFromAssembly(typeof(DeleteImprovementCommandValidator).Assembly);
            services.AddValidatorsFromAssembly(typeof(GetImprovementByIdQueryValidator).Assembly);

            //Propertytpe

            services.AddValidatorsFromAssembly(typeof(CreatePropertyTypeCommandValidator).Assembly);
            services.AddValidatorsFromAssembly(typeof(DeletePropertyTypeCommandValidator).Assembly);
            services.AddValidatorsFromAssembly(typeof(EditPropertyTypeCommandValidator).Assembly);

            services.AddValidatorsFromAssembly(typeof(GetPropertyTypeByIdQueryValidator).Assembly);


            //SaleType
            services.AddValidatorsFromAssembly(typeof(CreateSaleTypeCommandValidator).Assembly);
            services.AddValidatorsFromAssembly(typeof(EditSaleTypeCommandValidator).Assembly);
            services.AddValidatorsFromAssembly(typeof(DeleteSaleTypeCommandValidator).Assembly); 
            services.AddValidatorsFromAssembly(typeof(GetSaleTypeByIdQueryValidator).Assembly);




            //Login
            services.AddValidatorsFromAssembly(typeof(LoginCommandValidator).Assembly);


            //Property
            services.AddValidatorsFromAssembly(typeof(GetPropertyTypeByIdQueryValidator).Assembly);
            services.AddValidatorsFromAssembly(typeof(GetPropertyByCodeQueryValidator).Assembly);




            //Agents
            services.AddValidatorsFromAssembly(typeof(ChangeAgentStatusCommandValidator).Assembly);
            services.AddValidatorsFromAssembly(typeof(DeleteAgentCommandValidator).Assembly);
            services.AddValidatorsFromAssembly(typeof(GetAgentPropertiesQueryValidator).Assembly);
            services.AddValidatorsFromAssembly(typeof(GetAgentByIdQueryValidator).Assembly);


            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));





        }
    }
}
